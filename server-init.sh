#!/bin/bash
# 服务器快速初始化脚本 - 在阿里云服务器上执行

set -e

echo "=========================================="
echo "PortfolioAdmin 服务器初始化"
echo "=========================================="

# 1. 创建项目目录
echo "[1/5] 创建项目目录..."
mkdir -p /opt/portfolio-admin/{nginx/logs,nginx/ssl}
cd /opt/portfolio-admin
echo "✓ 目录创建完成"

# 2. 登录阿里云容器镜像服务
echo ""
echo "[2/5] 登录阿里云容器镜像服务..."
echo "请输入阿里云镜像仓库地址（例如：registry.cn-hangzhou.aliyuncs.com）："
read REGISTRY
echo "请输入阿里云账号用户名："
read USERNAME
docker login --username=$USERNAME $REGISTRY
echo "✓ 登录成功"

# 3. 保存配置到 .env 文件
echo ""
echo "[3/5] 配置环境变量..."
cat > .env << EOF
# 阿里云容器镜像服务配置
ALIYUN_REGISTRY=$REGISTRY

# 数据库连接字符串
DB_CONNECTION_STRING=Server=116.62.61.222;Port=3306;Database=BlazorPortfolio;User=你的数据库用户名;Password=你的数据库密码;

# JWT 配置（请修改为随机字符串）
JWT_SECRET=your_super_secret_key_min_32_characters_please_change_this
JWT_ISSUER=PortfolioAdmin
JWT_AUDIENCE=PortfolioAdmin
EOF
echo "✓ 环境变量文件已创建: /opt/portfolio-admin/.env"
echo "⚠️  请立即编辑 .env 文件修改数据库密码和 JWT 密钥："
echo "    vim /opt/portfolio-admin/.env"

# 4. 测试 Docker
echo ""
echo "[4/5] 测试 Docker..."
docker --version
docker compose version
echo "✓ Docker 运行正常"

# 5. 优化服务器（2GB 内存）
echo ""
echo "[5/5] 是否优化服务器配置？（推荐2GB内存服务器执行）[y/N]"
read -r OPTIMIZE
if [[ "$OPTIMIZE" =~ ^[Yy]$ ]]; then
    echo "创建 2GB Swap..."
    if [ ! -f /swapfile ]; then
        fallocate -l 2G /swapfile
        chmod 600 /swapfile
        mkswap /swapfile
        swapon /swapfile
        echo '/swapfile none swap sw 0 0' >> /etc/fstab
        echo "vm.swappiness=10" >> /etc/sysctl.conf
        sysctl -p
        echo "✓ Swap 配置完成"
    else
        echo "✓ Swap 已存在"
    fi
fi

echo ""
echo "=========================================="
echo "初始化完成！"
echo "=========================================="
echo ""
echo "下一步："
echo "1. 编辑环境变量：vim /opt/portfolio-admin/.env"
echo "2. 从本地上传配置文件："
echo "   scp docker-compose.prod.yml root@你的服务器IP:/opt/portfolio-admin/"
echo "   scp deploy.sh root@你的服务器IP:/opt/portfolio-admin/"
echo "   scp -r nginx root@你的服务器IP:/opt/portfolio-admin/"
echo "3. 执行部署：cd /opt/portfolio-admin && chmod +x deploy.sh && ./deploy.sh"
echo ""
