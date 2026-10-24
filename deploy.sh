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

# 加载环境变量（不用 xargs 分词，避免含空格/#/$ 的密钥被破坏）
set -a
. ./.env
set +a
export IMAGE_TAG="${IMAGE_TAG:-latest}"

# 前置校验：缺项会让容器拿到空连接串/空密钥
for v in ALIYUN_REGISTRY DB_CONNECTION_STRING JWT_SECRET REDIS_CONNECTION_STRING; do
    if [ -z "$(eval echo "\${$v:-}")" ]; then
        echo "错误: .env 缺少或为空: $v"
        exit 1
    fi
done

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
