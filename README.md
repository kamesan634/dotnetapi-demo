# 龜三的ERP Demo - .NET Core Web API 專案

![CI](https://github.com/kamesan634/dotnetapi-demo/actions/workflows/ci.yml/badge.svg)

基於 .NET 8 + ASP.NET Core Web API + Entity Framework Core 的零售業 ERP 系統 RESTful API。

## 技能樹 請點以下技能

| 技能 | 版本 | 說明 |
|------|------|------|
| .NET | 8 | 核心框架 |
| ASP.NET Core | 8 | Web API 框架 |
| Entity Framework Core | 8 | ORM 框架 |
| ASP.NET Core Identity | 8 | JWT 認證 |
| MySQL | 8.4 | 資料庫 |
| Redis | 7 | 快取/Token 黑名單 |
| Serilog | - | 結構化日誌 |
| Swagger | - | API 文件 |
| xUnit | - | 單元測試 |
| Docker | - | 容器化佈署 |

## 功能模組

- **auth** - 認證管理（登入、註冊、Token 刷新）
- **users** - 使用者管理（CRUD、角色指派）
- **roles** - 角色權限管理
- **stores** - 門市/倉庫管理
- **products** - 商品管理（商品、分類、單位）
- **customers** - 客戶管理（會員、會員等級、點數）
- **suppliers** - 供應商管理
- **inventory** - 庫存管理（庫存查詢、異動記錄、調撥）
- **orders** - 銷售管理（訂單、促銷、優惠券）
- **purchasing** - 採購管理（採購單、驗收）
- **reports** - 報表管理（Dashboard、統計報表、匯出）

## 快速開始

### 環境需求

- Docker & Docker Compose
- 或 .NET 8 SDK + MySQL 8.4 + Redis

### 使用 Docker 佈署（推薦）

```bash
# 啟動所有服務
docker-compose up -d

# 查看服務狀態
docker-compose ps

# 查看日誌
docker-compose logs -f api

# 停止服務
docker-compose down
```

### 本地開發

```bash
# 還原套件
dotnet restore

# 執行應用程式
cd src/DotnetApiDemo
dotnet run

# 或使用 watch 模式
dotnet watch run
```

## Port

| 服務 | Port | 說明 |
|------|------|------|
| ASP.NET Core API | 8004 | Swagger UI |
| MySQL | 3304 | 資料庫 |
| Redis | 6384 | 快取 |

## API 文件

啟動服務後，訪問：http://localhost:8004

### 主要 API 端點

| 模組 | 路徑 | 說明 |
|------|------|------|
| 認證 | /api/v1/auth | 登入、註冊、Token |
| 商品 | /api/v1/products | 商品 CRUD |
| 分類 | /api/v1/categories | 分類 CRUD |
| 客戶 | /api/v1/customers | 客戶 CRUD |
| 供應商 | /api/v1/suppliers | 供應商 CRUD |
| 庫存 | /api/v1/inventory | 庫存查詢、調撥 |
| 訂單 | /api/v1/orders | 訂單 CRUD |
| 採購 | /api/v1/purchase-orders | 採購單 CRUD |
| 報表 | /api/v1/reports | Dashboard、統計 |

## 測試資訊

### 測試帳號

| 帳號 | 密碼 | 角色 | 說明 |
|------|------|------|------|
| admin | Admin123! | Admin | 系統管理員 |
| manager | Manager123! | Manager | 門市經理 |
| staff01 | Staff123! | Staff | 門市員工 |
| warehouse01 | Warehouse123! | Warehouse | 倉管人員 |
| purchaser01 | Purchaser123! | Purchaser | 採購人員 |

### 測試資料

系統已預載以下種子資料：

| 資料類型 | 數量 | 說明 |
|----------|------|------|
| 角色 | 5 | Admin, Manager, Staff, Warehouse, Purchaser |
| 門市/倉庫 | 7 | 3 門市 + 4 倉庫（含總倉） |
| 使用者 | 6 | 含各角色使用者 |
| 商品分類 | 11 | 3 大類 + 8 子類 |
| 計量單位 | 5 | 個、箱、公斤、公升、包 |
| 商品 | 17 | 含條碼 |
| 會員等級 | 5 | 一般、銀卡、金卡、白金、VIP |
| 會員 | 5 | 不同等級的會員 |
| 供應商 | 4 | 各類供應商 |
| 庫存 | - | 所有商品在所有倉庫的初始庫存 |
| 促銷活動 | 3 | 各類促銷活動 |

## 專案結構

```
dotnetapi-demo/
├── docker-compose.yml          # Docker Compose 配置
├── Dockerfile                  # Docker 映像配置
├── DotnetApiDemo.sln           # Solution 檔案
├── src/
│   └── DotnetApiDemo/
│       ├── Controllers/        # API 控制器
│       ├── Models/
│       │   ├── Entities/       # EF Core 實體
│       │   ├── DTOs/           # 資料傳輸物件
│       │   └── Enums/          # 列舉型別
│       ├── Data/               # DbContext、種子資料
│       ├── Services/           # 商業邏輯層
│       │   ├── Interfaces/     # 服務介面
│       │   └── Implementations/# 服務實作
│       ├── Middleware/         # 中介軟體
│       ├── BackgroundServices/ # 背景服務
│       ├── Hubs/               # SignalR Hub
│       └── Program.cs          # 應用程式進入點
└── tests/
    └── DotnetApiDemo.Tests/    # 測試專案
```

## 資料庫連線

### Docker 環境

- Host: `localhost`
- Port: `3304`
- Database: `dotnetdemo_db`
- Username: `root`
- Password: `dev123`

```bash
# 使用 MySQL 客戶端連線
mysql -h localhost -P 3304 -uroot -pdev123 dotnetdemo_db

# 或進入 Docker 容器
docker exec -it dotnetapi-mysql mysql -uroot -pdev123 dotnetdemo_db
```

## 健康檢查

```bash
# 檢查應用程式健康狀態
curl http://localhost:8004/health
```

## 執行測試

```bash
# 執行所有測試
dotnet test

# 執行特定測試類別
dotnet test --filter "FullyQualifiedName~AuthControllerTests"

# 產生測試覆蓋率報告
dotnet test --collect:"XPlat Code Coverage"
```

## License

MIT License
我一開始以為是Made In Taiwan 咧！(羞
