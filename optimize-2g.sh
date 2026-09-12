#!/bin/bash

# 2GB 内存服务器优化脚本

echo "开始优化服务器配置（2GB 内存）..."

# 1. 配置 Swap 交换空间（增加虚拟内存）
echo "配置 Swap 交换空间..."
if [ ! -f /swapfile ]; then
    sudo fallocate -l 2G /swapfile
    sudo chmod 600 /swapfile
    sudo mkswap /swapfile
    sudo swapon /swapfile
    echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
    echo "Swap 配置完成（2GB）"
else
    echo "Swap 已存在，跳过"
fi

# 2. 调整 Swap 使用策略（降低 swappiness）
echo "vm.swappiness=10" | sudo tee -a /etc/sysctl.conf
sudo sysctl -p

# 3. 优化 Docker 配置
echo "优化 Docker 配置..."
sudo mkdir -p /etc/docker
cat > /tmp/daemon.json << EOF
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  },
  "storage-driver": "overlay2"
}
EOF
sudo mv /tmp/daemon.json /etc/docker/daemon.json
sudo systemctl restart docker

# 4. 设置系统资源限制
echo "fs.file-max = 65535" | sudo tee -a /etc/sysctl.conf
sudo sysctl -p

# 5. 清理不必要的服务
echo "禁用不必要的服务..."
sudo systemctl disable snapd 2>/dev/null || true
sudo systemctl stop snapd 2>/dev/null || true

echo "优化完成！"
echo ""
echo "内存使用情况："
free -h
echo ""
echo "Swap 状态："
swapon --show
