# ---------- 构建阶段 ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 先拷贝 csproj 做依赖还原（利用层缓存）
COPY PortfolioAdmin.Api/PortfolioAdmin.Api.csproj ./PortfolioAdmin.Api/
RUN dotnet restore PortfolioAdmin.Api/PortfolioAdmin.Api.csproj

# 拷贝全部源码并发布
COPY PortfolioAdmin.Api/ ./PortfolioAdmin.Api/
RUN dotnet publish PortfolioAdmin.Api/PortfolioAdmin.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---------- 运行阶段 ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "PortfolioAdmin.Api.dll"]
