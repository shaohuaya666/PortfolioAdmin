# 部署指南

本文档介绍如何将 PortfolioAdmin 项目部署到阿里云服务器并配置 CI/CD。

## 方案概述

- **容器化**: 使用 Docker 和 Docker Compose
- **CI/CD**: GitHub Actions 自动构建和部署
- **镜像仓库**: 阿里云容器镜像服务
- **反向代理**: Nginx
- **数据库**: 使用现有的阿里云 MySQL (116.62.61.222)

## 前置准备

### 1. 阿里云服务器要求

- **操作系统**: Ubuntu 20.04+ / CentOS 7+
- **配置**: 2核4GB 起步（建议 4核8GB）
- **端口**: 开放 80, 443, 22

### 2. 阿里云容器镜像服务

1. 登录 [阿里云容器镜像服务](https://cr.console.aliyun.com/)
2. 创建个人实例（免费）或企业实例
3. 创建命名空间 `portfolio-admin`
4. 创建两个镜像仓库：
   - `portfolio-admin/backend`
   - `portfolio-admin/frontend`
5. 记录以下信息：
   - 仓库地址：`registry.cn-hangzhou.aliyuncs.com` (根据你的区域调整)
   - 用户名：阿里云账号
   - 密码：在"访问凭证"中设置

## 服务器初始化

### 1. 登录服务器并安装 Docker

```bash
# 登录服务器
ssh root@your-server-ip

# 安装 Docker
curl -fsSL https://get.docker.com | bash -s docker
sudo systemctl start docker
sudo systemctl enable docker

# 安装 Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 验证安装
docker --version
docker-compose --version
```

### 2. 创建项目目录

```bash
sudo mkdir -p /opt/portfolio-admin
cd /opt/portfolio-admin

# 创建必要的子目录
mkdir -p nginx/logs nginx/ssl
```

### 3. 配置项目文件

将以下文件上传到服务器的 `/opt/portfolio-admin` 目录：

- `docker-compose.prod.yml`
- `nginx/nginx.conf`
- `.env` (从 `.env.example` 复制并修改)
- `deploy.sh`

```bash
# 配置环境变量
cp .env.example .env
vim .env  # 修改数据库连接、JWT 密钥等配置

# 赋予部署脚本执行权限
chmod +x deploy.sh
```

### 4. 登录阿里云容器镜像服务

```bash
docker login --username=your_aliyun_username registry.cn-hangzhou.aliyuncs.com
# 输入密码
```

## GitHub Actions CI/CD 配置

### 1. 配置 GitHub Secrets

在 GitHub 仓库设置中添加以下 Secrets (Settings → Secrets and variables → Actions):

| Secret 名称 | 说明 | 示例值 |
|------------|------|--------|
| `ALIYUN_REGISTRY` | 阿里云镜像仓库地址 | `registry.cn-hangzhou.aliyuncs.com` |
| `ALIYUN_REGISTRY_USERNAME` | 阿里云账号 | `your_username@aliyun.com` |
| `ALIYUN_REGISTRY_PASSWORD` | 镜像仓库密码 | `your_password` |
| `ALIYUN_HOST` | 服务器 IP | `116.62.61.222` |
| `ALIYUN_USERNAME` | SSH 用户名 | `root` |
| `ALIYUN_SSH_KEY` | SSH 私钥 | `-----BEGIN RSA PRIVATE KEY-----...` |

### 2. 生成 SSH 密钥对（如果还没有）

在本地运行：

```bash
# 生成新的密钥对
ssh-keygen -t rsa -b 4096 -C "deploy@portfolio" -f ~/.ssh/portfolio_deploy

# 复制公钥到服务器
ssh-copy-id -i ~/.ssh/portfolio_deploy.pub root@your-server-ip

# 查看私钥（复制到 GitHub Secrets 的 ALIYUN_SSH_KEY）
cat ~/.ssh/portfolio_deploy
```

### 3. 触发部署

```bash
# 推送到 main 分支自动触发部署
git add .
git commit -m "feat: 配置 CI/CD"
git push origin main

# 或在 GitHub Actions 页面手动触发
```

## 手动部署

如果不使用 CI/CD，可以手动部署：

### 1. 本地构建镜像并推送

```bash
# 构建后端镜像
cd backend
docker build -t registry.cn-hangzhou.aliyuncs.com/portfolio-admin/portfolio-admin-api:latest .
docker push registry.cn-hangzhou.aliyuncs.com/portfolio-admin/portfolio-admin-api:latest

# 构建前端镜像
cd ../frontend
docker build -t registry.cn-hangzhou.aliyuncs.com/portfolio-admin/portfolio-admin-web:latest .
docker push registry.cn-hangzhou.aliyuncs.com/portfolio-admin/portfolio-admin-web:latest
```

### 2. 在服务器上部署

```bash
# SSH 到服务器
ssh root@your-server-ip
cd /opt/portfolio-admin

# 执行部署脚本
./deploy.sh
```

## SSL/HTTPS 配置（可选但推荐）

### 使用 Let's Encrypt 免费证书

```bash
# 安装 Certbot
sudo apt-get update
sudo apt-get install certbot

# 获取证书（需要先停止 nginx）
docker-compose -f docker-compose.prod.yml stop nginx

sudo certbot certonly --standalone \
  -d your-domain.com \
  --agree-tos \
  --email your-email@example.com

# 复制证书到项目目录
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /opt/portfolio-admin/nginx/ssl/
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /opt/portfolio-admin/nginx/ssl/

# 修改 nginx.conf 启用 HTTPS 配置块
vim nginx/nginx.conf  # 取消 HTTPS server 块的注释

# 重启服务
docker-compose -f docker-compose.prod.yml up -d
```

### 自动续期证书

```bash
# 添加定时任务
sudo crontab -e

# 每月 1 号凌晨 2 点自动续期
0 2 1 * * certbot renew --quiet && docker-compose -f /opt/portfolio-admin/docker-compose.prod.yml restart nginx
```

## 日常运维

### 查看日志

```bash
cd /opt/portfolio-admin

# 查看所有服务日志
docker-compose -f docker-compose.prod.yml logs -f

# 查看特定服务日志
docker-compose -f docker-compose.prod.yml logs -f backend
docker-compose -f docker-compose.prod.yml logs -f frontend
docker-compose -f docker-compose.prod.yml logs -f nginx
```

### 重启服务

```bash
# 重启所有服务
docker-compose -f docker-compose.prod.yml restart

# 重启特定服务
docker-compose -f docker-compose.prod.yml restart backend
```

### 更新服务

```bash
# 拉取最新镜像
docker-compose -f docker-compose.prod.yml pull

# 重新启动
docker-compose -f docker-compose.prod.yml up -d
```

### 清理旧镜像

```bash
# 清理未使用的镜像
docker image prune -f

# 清理所有未使用的资源
docker system prune -a -f
```

### 数据备份

```bash
# 备份 MySQL 数据库（在 MySQL 服务器上执行）
mysqldump -h 116.62.61.222 -u your_user -p BlazorPortfolio > backup_$(date +%Y%m%d).sql

# 定期备份脚本
cat > /opt/backup.sh << 'EOF'
#!/bin/bash
BACKUP_DIR="/opt/backups"
mkdir -p $BACKUP_DIR
DATE=$(date +%Y%m%d_%H%M%S)
mysqldump -h 116.62.61.222 -u your_user -pyour_password BlazorPortfolio > $BACKUP_DIR/db_$DATE.sql
# 保留最近 7 天的备份
find $BACKUP_DIR -name "db_*.sql" -mtime +7 -delete
EOF

chmod +x /opt/backup.sh

# 添加定时任务：每天凌晨 3 点备份
crontab -e
# 添加: 0 3 * * * /opt/backup.sh
```

## 监控和告警

### 使用 Portainer 管理容器（可选）

```bash
docker volume create portainer_data

docker run -d -p 9000:9000 --name portainer --restart=always \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v portainer_data:/data \
  portainer/portainer-ce:latest
```

访问 `http://your-server-ip:9000` 进行容器管理。

## 故障排查

### 容器无法启动

```bash
# 查看容器状态
docker-compose -f docker-compose.prod.yml ps

# 查看详细日志
docker-compose -f docker-compose.prod.yml logs backend
docker-compose -f docker-compose.prod.yml logs frontend

# 检查配置
docker-compose -f docker-compose.prod.yml config
```

### 后端连接数据库失败

1. 检查数据库连接字符串是否正确
2. 确认阿里云 MySQL 安全组已开放 3306 端口
3. 测试数据库连接：

```bash
docker exec -it portfolio-backend bash
# 在容器内测试连接
apt-get update && apt-get install -y mysql-client
mysql -h 116.62.61.222 -u your_user -p
```

### 前端访问后端 API 失败

1. 检查 nginx 配置是否正确
2. 确认后端容器正常运行
3. 查看 nginx 日志：

```bash
docker-compose -f docker-compose.prod.yml logs nginx
```

## 性能优化

### 1. 启用 Redis 缓存（可选）

在 `docker-compose.prod.yml` 中添加 Redis 服务，然后在后端配置中启用缓存。

### 2. CDN 加速

将前端静态资源上传到阿里云 OSS，配置 CDN 加速。

### 3. 数据库优化

- 定期执行 `OPTIMIZE TABLE`
- 添加适当的索引
- 配置连接池

## 安全建议

1. **更改默认端口**: 修改 SSH 默认端口 22
2. **配置防火墙**: 只开放必要端口
3. **定期更新**: 及时更新系统和 Docker 镜像
4. **使用强密码**: 数据库和服务账号使用复杂密码
5. **定期备份**: 自动化数据库备份
6. **监控日志**: 定期检查异常日志

## 相关链接

- [阿里云容器镜像服务](https://cr.console.aliyun.com/)
- [Docker 官方文档](https://docs.docker.com/)
- [GitHub Actions 文档](https://docs.github.com/en/actions)
- [Nginx 官方文档](https://nginx.org/en/docs/)
