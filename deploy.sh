#!/bin/bash

# 阿里云服务器部署脚本
# 使用方法：chmod +x deploy.sh && ./deploy.sh

set -e

echo "开始部署 PortfolioAdmin..."

# 检查 .env 文件是否存在
if [ ! -f .env ]; then
    echo "错误: .env 文件不存在，请从 .env.example 复制并配置"
    exit 1
fi

# 加载环境变量
export $(cat .env | xargs)

# 拉取最新镜像
echo "正在拉取最新镜像..."
docker compose -f docker-compose.prod.yml pull

# 停止旧容器
echo "正在停止旧容器..."
docker compose -f docker-compose.prod.yml down

# 启动新容器
echo "正在启动新容器..."
docker compose -f docker-compose.prod.yml up -d

# 等待服务启动
echo "等待服务启动..."
sleep 10

# 检查服务状态
echo "检查服务状态..."
docker compose -f docker-compose.prod.yml ps

# 查看日志
echo "最近的日志："
docker compose -f docker-compose.prod.yml logs --tail=50

echo "部署完成！"
echo "前端访问地址: http://your-server-ip"
echo "后端 Swagger: http://your-server-ip/api/swagger"
