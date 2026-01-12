# 使用 .NET 8 SDK 作為建置環境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# 複製專案檔案並還原相依套件
COPY src/DotnetApiDemo/*.csproj ./src/DotnetApiDemo/
RUN dotnet restore ./src/DotnetApiDemo/DotnetApiDemo.csproj

# 複製所有原始碼並建置
COPY src/ ./src/
RUN dotnet publish ./src/DotnetApiDemo/DotnetApiDemo.csproj -c Release -o /app --no-restore

# 使用 .NET 8 Runtime 作為執行環境
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 安裝 curl 用於健康檢查
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# 複製建置結果
COPY --from=build /app ./

# 設定環境變數
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# 開放 port
EXPOSE 8080

# 執行應用程式
ENTRYPOINT ["dotnet", "DotnetApiDemo.dll"]
