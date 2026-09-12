# 快速部署指南

5 分钟完成阿里云部署和 CI/CD 配置。

## 📋 准备清单

- [ ] 阿里云 ECS 服务器（2核4GB 起步）
- [ ] 阿里云容器镜像服务账号
- [ ] GitHub 仓库
- [ ] MySQL 数据库（已有：116.62.61.222）

## 🚀 部署步骤

### 步骤 1：配置阿里云容器镜像服务

1. 访问 https://cr.console.aliyun.com/
2. 创建命名空间：`portfolio-admin`
3. 创建两个镜像仓库：
   - 仓库名：`portfolio-admin-api`（命名空间选择 `portfolio-admin`）
   - 仓库名：`portfolio-admin-web`（命名空间选择 `portfolio-admin`）
4. 记录：
   - 仓库地址：`registry.cn-hangzhou.aliyuncs.com`（根据你的区域）
   - 用户名和密码（在"访问凭证"设置）

### 步骤 2：初始化服务器

```bash
# SSH 登录服务器
ssh root@your-server-ip

# 一键安装 Docker 和 Docker Compose
curl -fsSL https://get.docker.com | bash
sudo systemctl start docker && sudo systemctl enable docker

sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 创建项目目录
sudo mkdir -p /opt/portfolio-admin/{nginx/logs,nginx/ssl}
cd /opt/portfolio-admin

# 登录阿里云镜像仓库
docker login --username=your_username registry.cn-hangzhou.aliyuncs.com
```

### 步骤 3：上传配置文件到服务器

将以下文件上传到 `/opt/portfolio-admin/`：

```bash
# 在本地项目目录执行
scp docker-compose.prod.yml root@your-server-ip:/opt/portfolio-admin/
scp .env.example root@your-server-ip:/opt/portfolio-admin/.env
scp deploy.sh root@your-server-ip:/opt/portfolio-admin/
scp -r nginx root@your-server-ip:/opt/portfolio-admin/

# SSH 到服务器配置环境变量
ssh root@your-server-ip
cd /opt/portfolio-admin
vim .env  # 修改数据库连接、JWT 密钥等
chmod +x deploy.sh
```

**重要：编辑 `.env` 文件，设置以下变量：**

```env
ALIYUN_REGISTRY=registry.cn-hangzhou.aliyuncs.com  # 你的镜像仓库地址
DB_CONNECTION_STRING=Server=116.62.61.222;Port=3306;Database=BlazorPortfolio;User=你的用户名;Password=你的密码;
JWT_SECRET=至少32位的随机字符串
JWT_ISSUER=PortfolioAdmin
JWT_AUDIENCE=PortfolioAdmin
```

### 步骤 4：配置 GitHub Secrets

在 GitHub 仓库中添加以下 Secrets（Settings → Secrets and variables → Actions）：

```
ALIYUN_REGISTRY = registry.cn-hangzhou.aliyuncs.com
ALIYUN_REGISTRY_USERNAME = your_username@aliyun.com
ALIYUN_REGISTRY_PASSWORD = your_password
ALIYUN_HOST = your-server-ip
ALIYUN_USERNAME = root
ALIYUN_SSH_KEY = <SSH私钥内容>
```

**生成 SSH 密钥：**

```bash
# 在本地执行
ssh-keygen -t rsa -b 4096 -C "deploy@portfolio" -f ~/.ssh/portfolio_deploy

# 复制公钥到服务器
ssh-copy-id -i ~/.ssh/portfolio_deploy.pub root@your-server-ip

# 查看私钥（复制全部内容到 GitHub Secrets 的 ALIYUN_SSH_KEY）
cat ~/.ssh/portfolio_deploy
```

### 步骤 5：首次手动部署（测试）

```bash
# 在服务器上执行首次部署
cd /opt/portfolio-admin
./deploy.sh
```

等待 1-2 分钟，然后访问：
- 前端：http://your-server-ip
- API 文档：http://your-server-ip/api/swagger

### 步骤 6：启用 CI/CD

```bash
# 在本地项目目录
git add .
git commit -m "feat: 配置 CI/CD 部署"
git push origin main
```

推送到 main 分支后，GitHub Actions 会自动：
1. 构建 Docker 镜像
2. 推送到阿里云镜像仓库
3. SSH 到服务器拉取镜像并重启服务

查看部署进度：GitHub → Actions

## 🔧 常用命令

### 查看服务状态

```bash
cd /opt/portfolio-admin
docker-compose -f docker-compose.prod.yml ps
```

### 查看日志

```bash
# 所有服务
docker-compose -f docker-compose.prod.yml logs -f

# 特定服务
docker-compose -f docker-compose.prod.yml logs -f backend
docker-compose -f docker-compose.prod.yml logs -f frontend
```

### 重启服务

```bash
docker-compose -f docker-compose.prod.yml restart
```

### 更新部署

```bash
# 方式 1：使用部署脚本
./deploy.sh

# 方式 2：手动执行
docker-compose -f docker-compose.prod.yml pull
docker-compose -f docker-compose.prod.yml up -d
```

### 清理旧镜像

```bash
docker image prune -f
```

## 🔒 配置 HTTPS（可选）

### 使用 Let's Encrypt 免费证书

```bash
# 安装 Certbot
sudo apt-get update && sudo apt-get install -y certbot

# 临时停止 nginx
docker-compose -f docker-compose.prod.yml stop nginx

# 获取证书（替换为你的域名）
sudo certbot certonly --standalone \
  -d your-domain.com \
  --agree-tos \
  --email your-email@example.com

# 复制证书
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem /opt/portfolio-admin/nginx/ssl/
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem /opt/portfolio-admin/nginx/ssl/

# 编辑 nginx 配置，取消 HTTPS server 块的注释
vim nginx/nginx.conf

# 重启服务
docker-compose -f docker-compose.prod.yml up -d
```

### 自动续期

```bash
# 添加定时任务
sudo crontab -e

# 添加以下行（每月 1 号凌晨 2 点）
0 2 1 * * certbot renew --quiet && docker-compose -f /opt/portfolio-admin/docker-compose.prod.yml restart nginx
```

## 📊 监控和维护

### 安装 Portainer（容器管理界面）

```bash
docker volume create portainer_data
docker run -d -p 9000:9000 --name portainer --restart=always \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v portainer_data:/data \
  portainer/portainer-ce:latest
```

访问 `http://your-server-ip:9000` 管理容器。

### 数据库备份

```bash
# 创建备份脚本
cat > /opt/backup.sh << 'EOF'
#!/bin/bash
BACKUP_DIR="/opt/backups"
mkdir -p $BACKUP_DIR
DATE=$(date +%Y%m%d_%H%M%S)
mysqldump -h 116.62.61.222 -u your_user -p'your_password' BlazorPortfolio > $BACKUP_DIR/db_$DATE.sql
find $BACKUP_DIR -name "db_*.sql" -mtime +7 -delete
echo "Backup completed: db_$DATE.sql"
EOF

chmod +x /opt/backup.sh

# 定时备份（每天凌晨 3 点）
crontab -e
# 添加: 0 3 * * * /opt/backup.sh >> /opt/backups/backup.log 2>&1
```

## ❓ 故障排查

### 容器启动失败

```bash
# 查看详细日志
docker-compose -f docker-compose.prod.yml logs backend
docker-compose -f docker-compose.prod.yml logs frontend

# 检查配置
docker-compose -f docker-compose.prod.yml config
```

### 后端连接数据库失败

```bash
# 进入后端容器测试连接
docker exec -it portfolio-backend bash
apt-get update && apt-get install -y mysql-client
mysql -h 116.62.61.222 -u your_user -p
```

检查：
- 数据库连接字符串是否正确
- 阿里云 MySQL 安全组是否开放 3306 端口
- 数据库用户是否有远程访问权限

### 前端无法访问后端 API

```bash
# 查看 nginx 日志
docker-compose -f docker-compose.prod.yml logs nginx

# 测试后端服务
curl http://localhost:5273/api/health  # 在服务器上执行
```

检查：
- nginx 配置是否正确
- 后端容器是否正常运行
- 防火墙/安全组是否开放 80 端口

## 📚 更多文档

详细文档请查看：
- [DEPLOYMENT.md](DEPLOYMENT.md) - 完整部署文档
- [README.md](README.md) - 项目说明

## 🎉 完成！

现在你的项目已经部署完成，每次推送代码到 main 分支都会自动部署到服务器。

**默认登录账号：**
- 用户名：`audience`
- 密码：`123456`

**建议在生产环境中立即修改默认密码！**
