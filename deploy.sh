#!/bin/bash

# 阿里云服务器手动部署脚本（CI 走 .github/workflows/deploy.yml，本脚本用于应急/回滚）
# 使用方法：chmod +x deploy.sh && ./deploy.sh
# 回滚指定版本：IMAGE_TAG=<git-sha> ./deploy.sh

set -eu

echo "开始部署 PortfolioAdmin..."

# 检查 .env 文件是否存在
if [ ! -f .env ]; then
    echo "错误: .env 文件不存在，请从 .env.example 复制并配置"
    exit 1
fi

# 加载环境变量
# 不能用 `source .env`：连接串里的分号会被 shell 当作命令分隔符，
# 例如 DB_CONNECTION_STRING=Server=x;Port=3306;User=root 会被拆成三条命令执行，
# 变量只剩 "Server=x"，与 compose 注入容器的真实值不一致，前置校验就失去意义。
# 这里逐项按行提取，值全程只当作数据处理，不经 shell 解析。
read_env() {
    _key="$1"
    _line=$(grep -m1 "^[[:space:]]*${_key}=" .env 2>/dev/null || true)
    if [ -z "$_line" ]; then
        return 1
    fi
    _value=${_line#*=}
    # 去掉成对的首尾引号（.env 允许用引号包裹含特殊字符的值）
    case "$_value" in
        \"*\") _value=${_value#\"}; _value=${_value%\"} ;;
        \'*\') _value=${_value#\'}; _value=${_value%\'} ;;
    esac
    # 去掉行尾可能的 CR（Windows 换行符）
    printf '%s' "$_value" | tr -d '\r'
}

ALIYUN_REGISTRY=$(read_env ALIYUN_REGISTRY || true)
DB_CONNECTION_STRING=$(read_env DB_CONNECTION_STRING || true)
JWT_SECRET=$(read_env JWT_SECRET || true)
REDIS_CONNECTION_STRING=$(read_env REDIS_CONNECTION_STRING || true)
export ALIYUN_REGISTRY DB_CONNECTION_STRING JWT_SECRET REDIS_CONNECTION_STRING
export IMAGE_TAG="${IMAGE_TAG:-latest}"

# 前置校验：缺项会让容器拿到空连接串/空密钥
if [ -z "$ALIYUN_REGISTRY" ]; then echo "错误: .env 缺少 ALIYUN_REGISTRY"; exit 1; fi
if [ -z "$DB_CONNECTION_STRING" ]; then echo "错误: .env 缺少 DB_CONNECTION_STRING"; exit 1; fi
if [ -z "$JWT_SECRET" ]; then echo "错误: .env 缺少 JWT_SECRET"; exit 1; fi
if [ -z "$REDIS_CONNECTION_STRING" ]; then echo "错误: .env 缺少 REDIS_CONNECTION_STRING"; exit 1; fi

# 连接串必须含账号与密码段：只判非空不够，残缺的串（例如只剩 Server=xxx）
# 后端照样能启动，首个请求才 500，而流水线会误判为部署成功。
# 覆盖 User= / User ID= / UserID= / Uid= 四种 MySqlConnector 合法写法
if ! printf '%s' "$DB_CONNECTION_STRING" | grep -qiE '(user[[:space:]]*id|uid|user)[[:space:]]*=[[:space:]]*[^;]+'; then
    echo "错误: DB_CONNECTION_STRING 缺少有效的账号段（User / User ID / Uid）"
    echo "      实际取到：$(printf '%s' "$DB_CONNECTION_STRING" | sed -E 's/(Password|password|Pwd|pwd)=[^;]*/\1=***/g')"
    exit 1
fi
if ! printf '%s' "$DB_CONNECTION_STRING" | grep -qiE '(password|pwd)[[:space:]]*=[[:space:]]*[^;]+'; then
    echo "错误: DB_CONNECTION_STRING 缺少有效的密码段（Password/Pwd）"
    exit 1
fi

# 前置校验：外部网络 cache-net 必须已接入 redis
if ! docker network inspect cache-net >/dev/null 2>&1; then
    echo "错误: 外部网络 cache-net 不存在"
    echo "      请先执行：docker network create cache-net && docker network connect cache-net redis"
    exit 1
fi

# 前置校验：compose 配置合法性
docker compose -f docker-compose.prod.yml config -q

# 拉取最新镜像
echo "正在拉取最新镜像..."
docker compose -f docker-compose.prod.yml pull

# 只替换有变化的容器，不整体 down（down 会删除网络并造成不必要的中断）
echo "正在启动容器..."
docker compose -f docker-compose.prod.yml up -d

# 等待服务启动
echo "等待服务启动..."
sleep 10

# 冒烟校验
attached=$(docker network inspect cache-net --format '{{range .Containers}}{{.Name}} {{end}}')
echo "cache-net 已连接容器: ${attached:-<无>}"
case "$attached" in
    *redis*) ;;
    *) echo "警告: redis 未接入 cache-net，缓存失效不可用" ;;
esac

# 检查服务状态
echo "检查服务状态..."
docker compose -f docker-compose.prod.yml ps

# 查看日志
echo "最近的日志："
docker compose -f docker-compose.prod.yml logs --tail=50

echo "部署完成！"
echo "本次镜像标签: $IMAGE_TAG"
echo "前端访问地址: http://your-server-ip:5001"
