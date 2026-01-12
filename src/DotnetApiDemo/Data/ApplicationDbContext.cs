using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Models.Entities;

namespace DotnetApiDemo.Data;

/// <summary>
/// 應用程式資料庫上下文
/// </summary>
/// <remarks>
/// 繼承自 IdentityDbContext 以支援 ASP.NET Core Identity 功能
/// </remarks>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="options">資料庫上下文選項</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    #region 系統管理模組

    /// <summary>
    /// 門市資料表
    /// </summary>
    public DbSet<Store> Stores { get; set; } = null!;

    /// <summary>
    /// 倉庫資料表
    /// </summary>
    public DbSet<Warehouse> Warehouses { get; set; } = null!;

    /// <summary>
    /// 使用者門市關聯資料表
    /// </summary>
    public DbSet<Models.Entities.UserStore> UserStores { get; set; } = null!;

    #endregion

    #region 基礎資料模組

    /// <summary>
    /// 商品分類資料表
    /// </summary>
    public DbSet<Category> Categories { get; set; } = null!;

    /// <summary>
    /// 計量單位資料表
    /// </summary>
    public DbSet<Unit> Units { get; set; } = null!;

    /// <summary>
    /// 商品資料表
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// 商品條碼資料表
    /// </summary>
    public DbSet<ProductBarcode> ProductBarcodes { get; set; } = null!;

    /// <summary>
    /// 會員等級資料表
    /// </summary>
    public DbSet<CustomerLevel> CustomerLevels { get; set; } = null!;

    /// <summary>
    /// 客戶/會員資料表
    /// </summary>
    public DbSet<Customer> Customers { get; set; } = null!;

    /// <summary>
    /// 供應商資料表
    /// </summary>
    public DbSet<Supplier> Suppliers { get; set; } = null!;

    /// <summary>
    /// 供應商價格資料表
    /// </summary>
    public DbSet<SupplierPrice> SupplierPrices { get; set; } = null!;

    #endregion

    #region 銷售模組

    /// <summary>
    /// 訂單資料表
    /// </summary>
    public DbSet<Order> Orders { get; set; } = null!;

    /// <summary>
    /// 訂單明細資料表
    /// </summary>
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    /// <summary>
    /// 付款記錄資料表
    /// </summary>
    public DbSet<Payment> Payments { get; set; } = null!;

    /// <summary>
    /// 促銷活動資料表
    /// </summary>
    public DbSet<Promotion> Promotions { get; set; } = null!;

    /// <summary>
    /// 優惠券資料表
    /// </summary>
    public DbSet<Coupon> Coupons { get; set; } = null!;

    /// <summary>
    /// 付款方式資料表
    /// </summary>
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;

    /// <summary>
    /// 銷售退貨單資料表
    /// </summary>
    public DbSet<SalesReturn> SalesReturns { get; set; } = null!;

    /// <summary>
    /// 銷售退貨明細資料表
    /// </summary>
    public DbSet<SalesReturnItem> SalesReturnItems { get; set; } = null!;

    /// <summary>
    /// 掛單資料表
    /// </summary>
    public DbSet<SuspendedOrder> SuspendedOrders { get; set; } = null!;

    /// <summary>
    /// 掛單明細資料表
    /// </summary>
    public DbSet<SuspendedOrderItem> SuspendedOrderItems { get; set; } = null!;

    #endregion

    #region 庫存模組

    /// <summary>
    /// 庫存資料表
    /// </summary>
    public DbSet<Inventory> Inventories { get; set; } = null!;

    /// <summary>
    /// 庫存異動記錄資料表
    /// </summary>
    public DbSet<InventoryMovement> InventoryMovements { get; set; } = null!;

    /// <summary>
    /// 庫存調撥單資料表
    /// </summary>
    public DbSet<StockTransfer> StockTransfers { get; set; } = null!;

    /// <summary>
    /// 庫存調撥單明細資料表
    /// </summary>
    public DbSet<StockTransferItem> StockTransferItems { get; set; } = null!;

    /// <summary>
    /// 庫存調整單資料表
    /// </summary>
    public DbSet<StockAdjustment> StockAdjustments { get; set; } = null!;

    /// <summary>
    /// 庫存盤點單資料表
    /// </summary>
    public DbSet<StockCount> StockCounts { get; set; } = null!;

    /// <summary>
    /// 庫存盤點單明細資料表
    /// </summary>
    public DbSet<StockCountItem> StockCountItems { get; set; } = null!;

    #endregion

    #region 採購模組

    /// <summary>
    /// 採購單資料表
    /// </summary>
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;

    /// <summary>
    /// 採購單明細資料表
    /// </summary>
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } = null!;

    /// <summary>
    /// 採購驗收單資料表
    /// </summary>
    public DbSet<PurchaseReceipt> PurchaseReceipts { get; set; } = null!;

    /// <summary>
    /// 採購驗收單明細資料表
    /// </summary>
    public DbSet<PurchaseReceiptItem> PurchaseReceiptItems { get; set; } = null!;

    /// <summary>
    /// 採購退貨單資料表
    /// </summary>
    public DbSet<PurchaseReturn> PurchaseReturns { get; set; } = null!;

    /// <summary>
    /// 採購退貨單明細資料表
    /// </summary>
    public DbSet<PurchaseReturnItem> PurchaseReturnItems { get; set; } = null!;

    #endregion

    #region 系統設定模組

    /// <summary>
    /// 系統設定資料表
    /// </summary>
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

    /// <summary>
    /// 稽核日誌資料表
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    /// <summary>
    /// 編號規則資料表
    /// </summary>
    public DbSet<NumberRule> NumberRules { get; set; } = null!;

    #endregion

    #region 商品擴充模組

    /// <summary>
    /// 商品規格資料表
    /// </summary>
    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;

    /// <summary>
    /// 商品組合資料表
    /// </summary>
    public DbSet<ProductCombo> ProductCombos { get; set; } = null!;

    /// <summary>
    /// 商品組合項目資料表
    /// </summary>
    public DbSet<ProductComboItem> ProductComboItems { get; set; } = null!;

    #endregion

    #region 收銀模組

    /// <summary>
    /// 收銀班別資料表
    /// </summary>
    public DbSet<CashierShift> CashierShifts { get; set; } = null!;

    /// <summary>
    /// 發票資料表
    /// </summary>
    public DbSet<Invoice> Invoices { get; set; } = null!;

    #endregion

    #region 點數模組

    /// <summary>
    /// 點數交易資料表
    /// </summary>
    public DbSet<PointTransaction> PointTransactions { get; set; } = null!;

    #endregion

    #region 標籤列印模組

    /// <summary>
    /// 標籤模板資料表
    /// </summary>
    public DbSet<LabelTemplate> LabelTemplates { get; set; } = null!;

    /// <summary>
    /// 列印任務資料表
    /// </summary>
    public DbSet<PrintJob> PrintJobs { get; set; } = null!;

    /// <summary>
    /// 列印任務項目資料表
    /// </summary>
    public DbSet<PrintJobItem> PrintJobItems { get; set; } = null!;

    #endregion

    #region 自訂報表模組

    /// <summary>
    /// 自訂報表資料表
    /// </summary>
    public DbSet<CustomReport> CustomReports { get; set; } = null!;

    /// <summary>
    /// 排程報表資料表
    /// </summary>
    public DbSet<ScheduledReport> ScheduledReports { get; set; } = null!;

    /// <summary>
    /// 排程報表執行記錄資料表
    /// </summary>
    public DbSet<ScheduledReportHistory> ScheduledReportHistories { get; set; } = null!;

    #endregion

    /// <summary>
    /// 配置模型
    /// </summary>
    /// <param name="modelBuilder">模型建構器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 套用所有 Entity 配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // 配置 Identity 表格名稱
        modelBuilder.Entity<ApplicationUser>().ToTable("users");
        modelBuilder.Entity<ApplicationRole>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("role_claims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("user_tokens");

        // UserStore 複合主鍵配置
        modelBuilder.Entity<Models.Entities.UserStore>()
            .HasKey(us => new { us.UserId, us.StoreId });

        modelBuilder.Entity<Models.Entities.UserStore>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserStores)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Models.Entities.UserStore>()
            .HasOne(us => us.Store)
            .WithMany(s => s.UserStores)
            .HasForeignKey(us => us.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Inventory 唯一索引配置
        modelBuilder.Entity<Inventory>()
            .HasIndex(i => new { i.ProductId, i.WarehouseId })
            .IsUnique();

        // Category 自關聯配置
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // StockTransfer 雙外鍵配置
        modelBuilder.Entity<StockTransfer>()
            .HasOne(st => st.FromWarehouse)
            .WithMany()
            .HasForeignKey(st => st.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransfer>()
            .HasOne(st => st.ToWarehouse)
            .WithMany()
            .HasForeignKey(st => st.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // 設定所有 decimal 欄位的精度
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }

        // 設定所有字串欄位使用 utf8mb4
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(string)))
        {
            property.SetCharSet("utf8mb4");
        }
    }
}
