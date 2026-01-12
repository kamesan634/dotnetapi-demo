using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Data.Seeds;

/// <summary>
/// 資料種子產生器
/// </summary>
/// <remarks>
/// 負責初始化測試資料，可重複執行
/// </remarks>
public static class DataSeeder
{
    /// <summary>
    /// 執行種子資料初始化
    /// </summary>
    /// <param name="serviceProvider">服務提供者</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // 執行資料庫遷移
            await context.Database.MigrateAsync();

            // 初始化角色
            await SeedRolesAsync(roleManager, logger);

            // 初始化使用者
            await SeedUsersAsync(userManager, logger);

            // 初始化門市與倉庫
            await SeedStoresAndWarehousesAsync(context, logger);

            // 初始化計量單位
            await SeedUnitsAsync(context, logger);

            // 初始化會員等級
            await SeedCustomerLevelsAsync(context, logger);

            // 初始化商品分類
            await SeedCategoriesAsync(context, logger);

            // 初始化供應商
            await SeedSuppliersAsync(context, logger);

            // 初始化商品
            await SeedProductsAsync(context, logger);

            // 初始化客戶
            await SeedCustomersAsync(context, logger);

            // 初始化庫存
            await SeedInventoriesAsync(context, logger);

            // 初始化促銷活動
            await SeedPromotionsAsync(context, logger);

            logger.LogInformation("資料種子初始化完成");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "資料種子初始化失敗");
            throw;
        }
    }

    /// <summary>
    /// 初始化角色
    /// </summary>
    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        var roles = new[]
        {
            new ApplicationRole { Name = "Admin", Description = "系統管理員", IsSystem = true, SortOrder = 1 },
            new ApplicationRole { Name = "Manager", Description = "門市經理", IsSystem = true, SortOrder = 2 },
            new ApplicationRole { Name = "Staff", Description = "門市員工", IsSystem = true, SortOrder = 3 },
            new ApplicationRole { Name = "Warehouse", Description = "倉管人員", IsSystem = true, SortOrder = 4 },
            new ApplicationRole { Name = "Purchaser", Description = "採購人員", IsSystem = true, SortOrder = 5 }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                await roleManager.CreateAsync(role);
                logger.LogInformation("建立角色: {RoleName}", role.Name);
            }
        }
    }

    /// <summary>
    /// 初始化使用者
    /// </summary>
    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var users = new[]
        {
            new { User = new ApplicationUser { UserName = "admin", Email = "admin@demo.com", RealName = "系統管理員", EmployeeNo = "EMP001", EmailConfirmed = true }, Password = "Admin@123", Role = "Admin" },
            new { User = new ApplicationUser { UserName = "manager", Email = "manager@demo.com", RealName = "王經理", EmployeeNo = "EMP002", EmailConfirmed = true }, Password = "Manager@123", Role = "Manager" },
            new { User = new ApplicationUser { UserName = "staff01", Email = "staff01@demo.com", RealName = "李小明", EmployeeNo = "EMP003", EmailConfirmed = true }, Password = "Staff@123", Role = "Staff" },
            new { User = new ApplicationUser { UserName = "staff02", Email = "staff02@demo.com", RealName = "陳小華", EmployeeNo = "EMP004", EmailConfirmed = true }, Password = "Staff@123", Role = "Staff" },
            new { User = new ApplicationUser { UserName = "warehouse", Email = "warehouse@demo.com", RealName = "林倉管", EmployeeNo = "EMP005", EmailConfirmed = true }, Password = "Warehouse@123", Role = "Warehouse" },
            new { User = new ApplicationUser { UserName = "purchaser", Email = "purchaser@demo.com", RealName = "張採購", EmployeeNo = "EMP006", EmailConfirmed = true }, Password = "Purchaser@123", Role = "Purchaser" }
        };

        foreach (var userData in users)
        {
            if (await userManager.FindByNameAsync(userData.User.UserName!) == null)
            {
                var result = await userManager.CreateAsync(userData.User, userData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(userData.User, userData.Role);
                    logger.LogInformation("建立使用者: {UserName}", userData.User.UserName);
                }
            }
        }
    }

    /// <summary>
    /// 初始化門市與倉庫
    /// </summary>
    private static async Task SeedStoresAndWarehousesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Stores.AnyAsync()) return;

        var stores = new[]
        {
            new Store { Code = "STORE001", Name = "台北旗艦店", ShortName = "台北店", Address = "台北市信義區信義路五段7號", Phone = "02-12345678", OpenTime = new TimeOnly(10, 0), CloseTime = new TimeOnly(22, 0), SortOrder = 1 },
            new Store { Code = "STORE002", Name = "台中門市", ShortName = "台中店", Address = "台中市西屯區台灣大道三段251號", Phone = "04-12345678", OpenTime = new TimeOnly(10, 0), CloseTime = new TimeOnly(21, 0), SortOrder = 2 },
            new Store { Code = "STORE003", Name = "高雄門市", ShortName = "高雄店", Address = "高雄市前鎮區中華五路789號", Phone = "07-12345678", OpenTime = new TimeOnly(10, 0), CloseTime = new TimeOnly(21, 0), SortOrder = 3 }
        };

        await context.Stores.AddRangeAsync(stores);
        await context.SaveChangesAsync();

        var warehouses = new[]
        {
            new Warehouse { Code = "WH001", Name = "總倉庫", Address = "新北市三重區重新路100號", Phone = "02-87654321", WarehouseType = "中央倉", IsDefault = true, SortOrder = 1 },
            new Warehouse { Code = "WH002", Name = "台北門市倉", StoreId = stores[0].Id, WarehouseType = "門市倉", SortOrder = 2 },
            new Warehouse { Code = "WH003", Name = "台中門市倉", StoreId = stores[1].Id, WarehouseType = "門市倉", SortOrder = 3 },
            new Warehouse { Code = "WH004", Name = "高雄門市倉", StoreId = stores[2].Id, WarehouseType = "門市倉", SortOrder = 4 }
        };

        await context.Warehouses.AddRangeAsync(warehouses);
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {StoreCount} 個門市與 {WarehouseCount} 個倉庫", stores.Length, warehouses.Length);
    }

    /// <summary>
    /// 初始化計量單位
    /// </summary>
    private static async Task SeedUnitsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Units.AnyAsync()) return;

        var units = new[]
        {
            new Unit { Code = "PCS", Name = "件", IsSystem = true, SortOrder = 1 },
            new Unit { Code = "SET", Name = "組", IsSystem = true, SortOrder = 2 },
            new Unit { Code = "BOX", Name = "盒", IsSystem = true, SortOrder = 3 },
            new Unit { Code = "PAIR", Name = "雙", IsSystem = true, SortOrder = 4 },
            new Unit { Code = "EA", Name = "個", IsSystem = true, SortOrder = 5 }
        };

        await context.Units.AddRangeAsync(units);
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {UnitCount} 個計量單位", units.Length);
    }

    /// <summary>
    /// 初始化會員等級
    /// </summary>
    private static async Task SeedCustomerLevelsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.CustomerLevels.AnyAsync()) return;

        var levels = new[]
        {
            new CustomerLevel { Code = "NORMAL", Name = "一般會員", MinSpendAmount = 0, DiscountRate = 100, PointsMultiplier = 1, Color = "#808080", IsDefault = true, SortOrder = 1 },
            new CustomerLevel { Code = "SILVER", Name = "銀卡會員", MinSpendAmount = 10000, DiscountRate = 95, PointsMultiplier = 1.2m, Color = "#C0C0C0", SortOrder = 2 },
            new CustomerLevel { Code = "GOLD", Name = "金卡會員", MinSpendAmount = 50000, DiscountRate = 90, PointsMultiplier = 1.5m, Color = "#FFD700", SortOrder = 3 },
            new CustomerLevel { Code = "PLATINUM", Name = "白金會員", MinSpendAmount = 100000, DiscountRate = 85, PointsMultiplier = 2, Color = "#E5E4E2", SortOrder = 4 },
            new CustomerLevel { Code = "VIP", Name = "VIP會員", MinSpendAmount = 200000, DiscountRate = 80, PointsMultiplier = 3, Color = "#800080", SortOrder = 5 }
        };

        await context.CustomerLevels.AddRangeAsync(levels);
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {LevelCount} 個會員等級", levels.Length);
    }

    /// <summary>
    /// 初始化商品分類
    /// </summary>
    private static async Task SeedCategoriesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Categories.AnyAsync()) return;

        // 第一層分類
        var topCategories = new[]
        {
            new Category { Code = "CAT01", Name = "服飾", Level = 1, Path = "", SortOrder = 1 },
            new Category { Code = "CAT02", Name = "鞋類", Level = 1, Path = "", SortOrder = 2 },
            new Category { Code = "CAT03", Name = "配件", Level = 1, Path = "", SortOrder = 3 }
        };

        await context.Categories.AddRangeAsync(topCategories);
        await context.SaveChangesAsync();

        // 更新路徑
        foreach (var cat in topCategories)
        {
            cat.Path = cat.Id.ToString();
        }
        await context.SaveChangesAsync();

        // 第二層分類
        var subCategories = new[]
        {
            new Category { Code = "CAT0101", Name = "上衣", ParentId = topCategories[0].Id, Level = 2, SortOrder = 1 },
            new Category { Code = "CAT0102", Name = "褲子", ParentId = topCategories[0].Id, Level = 2, SortOrder = 2 },
            new Category { Code = "CAT0103", Name = "外套", ParentId = topCategories[0].Id, Level = 2, SortOrder = 3 },
            new Category { Code = "CAT0201", Name = "運動鞋", ParentId = topCategories[1].Id, Level = 2, SortOrder = 1 },
            new Category { Code = "CAT0202", Name = "休閒鞋", ParentId = topCategories[1].Id, Level = 2, SortOrder = 2 },
            new Category { Code = "CAT0301", Name = "帽子", ParentId = topCategories[2].Id, Level = 2, SortOrder = 1 },
            new Category { Code = "CAT0302", Name = "皮帶", ParentId = topCategories[2].Id, Level = 2, SortOrder = 2 },
            new Category { Code = "CAT0303", Name = "皮夾", ParentId = topCategories[2].Id, Level = 2, SortOrder = 3 }
        };

        await context.Categories.AddRangeAsync(subCategories);
        await context.SaveChangesAsync();

        // 更新路徑
        foreach (var cat in subCategories)
        {
            cat.Path = $"{cat.ParentId}/{cat.Id}";
        }
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {CategoryCount} 個商品分類", topCategories.Length + subCategories.Length);
    }

    /// <summary>
    /// 初始化供應商
    /// </summary>
    private static async Task SeedSuppliersAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Suppliers.AnyAsync()) return;

        var suppliers = new[]
        {
            new Supplier { Code = "SUP001", Name = "ABC服飾批發商", ShortName = "ABC批發", TaxId = "12345678", ContactName = "王先生", Phone = "02-11112222", Email = "abc@supplier.com", Address = "台北市中山區南京東路100號", PaymentTerms = PaymentTerms.Net30, LeadTimeDays = 7 },
            new Supplier { Code = "SUP002", Name = "DEF服飾商行", ShortName = "DEF商行", TaxId = "23456789", ContactName = "李小姐", Phone = "02-33334444", Email = "def@supplier.com", Address = "台北市大同區承德路200號", PaymentTerms = PaymentTerms.Net30, LeadTimeDays = 5 },
            new Supplier { Code = "SUP003", Name = "GHI鞋業公司", ShortName = "GHI鞋業", TaxId = "34567890", ContactName = "陳先生", Phone = "04-55556666", Email = "ghi@supplier.com", Address = "台中市西區民生路300號", PaymentTerms = PaymentTerms.Net60, LeadTimeDays = 14 },
            new Supplier { Code = "SUP004", Name = "JKL配件行", ShortName = "JKL配件", TaxId = "45678901", ContactName = "林小姐", Phone = "07-77778888", Email = "jkl@supplier.com", Address = "高雄市三民區建國路400號", PaymentTerms = PaymentTerms.Net30, LeadTimeDays = 7 }
        };

        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {SupplierCount} 個供應商", suppliers.Length);
    }

    /// <summary>
    /// 初始化商品
    /// </summary>
    private static async Task SeedProductsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Products.AnyAsync()) return;

        var categories = await context.Categories.Where(c => c.Level == 2).ToListAsync();
        var units = await context.Units.ToListAsync();
        var suppliers = await context.Suppliers.ToListAsync();

        var pcsUnit = units.First(u => u.Code == "PCS");
        var pairUnit = units.First(u => u.Code == "PAIR");
        var eaUnit = units.First(u => u.Code == "EA");

        var products = new List<Product>
        {
            // 上衣
            new Product { Sku = "PRD001", Name = "白色T-Shirt", CategoryId = categories.First(c => c.Code == "CAT0101").Id, UnitId = pcsUnit.Id, CostPrice = 150, ListPrice = 399, SellingPrice = 299, MemberPrice = 279, SafetyStock = 50, PrimarySupplierId = suppliers[0].Id },
            new Product { Sku = "PRD002", Name = "黑色T-Shirt", CategoryId = categories.First(c => c.Code == "CAT0101").Id, UnitId = pcsUnit.Id, CostPrice = 150, ListPrice = 399, SellingPrice = 299, MemberPrice = 279, SafetyStock = 50, PrimarySupplierId = suppliers[0].Id },
            new Product { Sku = "PRD003", Name = "藍色Polo衫", CategoryId = categories.First(c => c.Code == "CAT0101").Id, UnitId = pcsUnit.Id, CostPrice = 200, ListPrice = 599, SellingPrice = 499, MemberPrice = 459, SafetyStock = 30, PrimarySupplierId = suppliers[0].Id },

            // 褲子
            new Product { Sku = "PRD004", Name = "黑色長褲", CategoryId = categories.First(c => c.Code == "CAT0102").Id, UnitId = pcsUnit.Id, CostPrice = 450, ListPrice = 1290, SellingPrice = 990, MemberPrice = 890, SafetyStock = 30, PrimarySupplierId = suppliers[1].Id },
            new Product { Sku = "PRD005", Name = "牛仔褲", CategoryId = categories.First(c => c.Code == "CAT0102").Id, UnitId = pcsUnit.Id, CostPrice = 400, ListPrice = 1190, SellingPrice = 890, MemberPrice = 790, SafetyStock = 30, PrimarySupplierId = suppliers[1].Id },
            new Product { Sku = "PRD006", Name = "卡其短褲", CategoryId = categories.First(c => c.Code == "CAT0102").Id, UnitId = pcsUnit.Id, CostPrice = 250, ListPrice = 690, SellingPrice = 590, MemberPrice = 530, SafetyStock = 25, PrimarySupplierId = suppliers[1].Id },

            // 外套
            new Product { Sku = "PRD007", Name = "連帽外套", CategoryId = categories.First(c => c.Code == "CAT0103").Id, UnitId = pcsUnit.Id, CostPrice = 550, ListPrice = 1590, SellingPrice = 1290, MemberPrice = 1090, SafetyStock = 20, PrimarySupplierId = suppliers[0].Id },
            new Product { Sku = "PRD008", Name = "防風夾克", CategoryId = categories.First(c => c.Code == "CAT0103").Id, UnitId = pcsUnit.Id, CostPrice = 650, ListPrice = 1890, SellingPrice = 1490, MemberPrice = 1290, SafetyStock = 15, PrimarySupplierId = suppliers[0].Id },

            // 運動鞋
            new Product { Sku = "PRD009", Name = "輕量運動鞋", CategoryId = categories.First(c => c.Code == "CAT0201").Id, UnitId = pairUnit.Id, CostPrice = 600, ListPrice = 1790, SellingPrice = 1490, MemberPrice = 1290, SafetyStock = 20, PrimarySupplierId = suppliers[2].Id },
            new Product { Sku = "PRD010", Name = "籃球鞋", CategoryId = categories.First(c => c.Code == "CAT0201").Id, UnitId = pairUnit.Id, CostPrice = 800, ListPrice = 2590, SellingPrice = 1990, MemberPrice = 1790, SafetyStock = 15, PrimarySupplierId = suppliers[2].Id },

            // 休閒鞋
            new Product { Sku = "PRD011", Name = "白色休閒鞋", CategoryId = categories.First(c => c.Code == "CAT0202").Id, UnitId = pairUnit.Id, CostPrice = 500, ListPrice = 1490, SellingPrice = 1190, MemberPrice = 990, SafetyStock = 20, PrimarySupplierId = suppliers[2].Id },
            new Product { Sku = "PRD012", Name = "帆布鞋", CategoryId = categories.First(c => c.Code == "CAT0202").Id, UnitId = pairUnit.Id, CostPrice = 300, ListPrice = 890, SellingPrice = 690, MemberPrice = 590, SafetyStock = 25, PrimarySupplierId = suppliers[2].Id },

            // 帽子
            new Product { Sku = "PRD013", Name = "棒球帽", CategoryId = categories.First(c => c.Code == "CAT0301").Id, UnitId = eaUnit.Id, CostPrice = 120, ListPrice = 390, SellingPrice = 290, MemberPrice = 250, SafetyStock = 30, PrimarySupplierId = suppliers[3].Id },
            new Product { Sku = "PRD014", Name = "漁夫帽", CategoryId = categories.First(c => c.Code == "CAT0301").Id, UnitId = eaUnit.Id, CostPrice = 150, ListPrice = 490, SellingPrice = 390, MemberPrice = 350, SafetyStock = 25, PrimarySupplierId = suppliers[3].Id },

            // 皮帶
            new Product { Sku = "PRD015", Name = "真皮皮帶", CategoryId = categories.First(c => c.Code == "CAT0302").Id, UnitId = eaUnit.Id, CostPrice = 200, ListPrice = 690, SellingPrice = 490, MemberPrice = 450, SafetyStock = 20, PrimarySupplierId = suppliers[3].Id },

            // 皮夾
            new Product { Sku = "PRD016", Name = "短夾", CategoryId = categories.First(c => c.Code == "CAT0303").Id, UnitId = eaUnit.Id, CostPrice = 350, ListPrice = 990, SellingPrice = 790, MemberPrice = 690, SafetyStock = 15, PrimarySupplierId = suppliers[3].Id },
            new Product { Sku = "PRD017", Name = "長夾", CategoryId = categories.First(c => c.Code == "CAT0303").Id, UnitId = eaUnit.Id, CostPrice = 450, ListPrice = 1290, SellingPrice = 990, MemberPrice = 890, SafetyStock = 15, PrimarySupplierId = suppliers[3].Id }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // 建立條碼
        var barcodes = products.Select((p, i) => new ProductBarcode
        {
            ProductId = p.Id,
            Barcode = $"47100880{(12345 + i):D5}",
            BarcodeType = "EAN13",
            IsPrimary = true
        }).ToList();

        await context.ProductBarcodes.AddRangeAsync(barcodes);
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {ProductCount} 個商品", products.Count);
    }

    /// <summary>
    /// 初始化客戶
    /// </summary>
    private static async Task SeedCustomersAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Customers.AnyAsync()) return;

        var levels = await context.CustomerLevels.ToListAsync();
        var stores = await context.Stores.ToListAsync();

        var customers = new[]
        {
            new Customer { MemberNo = "M00001", Name = "張三", Gender = "M", Phone = "0912345678", Email = "zhang3@test.com", CustomerLevelId = levels.First(l => l.Code == "GOLD").Id, TotalSpent = 65000, TotalOrders = 15, CurrentPoints = 3250, JoinStoreId = stores[0].Id },
            new Customer { MemberNo = "M00002", Name = "李四", Gender = "F", Phone = "0923456789", Email = "li4@test.com", CustomerLevelId = levels.First(l => l.Code == "SILVER").Id, TotalSpent = 25000, TotalOrders = 8, CurrentPoints = 1250, JoinStoreId = stores[0].Id },
            new Customer { MemberNo = "M00003", Name = "王五", Gender = "M", Phone = "0934567890", Email = "wang5@test.com", CustomerLevelId = levels.First(l => l.Code == "NORMAL").Id, TotalSpent = 5000, TotalOrders = 3, CurrentPoints = 250, JoinStoreId = stores[1].Id },
            new Customer { MemberNo = "M00004", Name = "趙六", Gender = "F", Phone = "0945678901", Email = "zhao6@test.com", CustomerLevelId = levels.First(l => l.Code == "PLATINUM").Id, TotalSpent = 120000, TotalOrders = 25, CurrentPoints = 6000, JoinStoreId = stores[1].Id },
            new Customer { MemberNo = "M00005", Name = "陳七", Gender = "M", Phone = "0956789012", Email = "chen7@test.com", CustomerLevelId = levels.First(l => l.Code == "VIP").Id, TotalSpent = 250000, TotalOrders = 50, CurrentPoints = 12500, JoinStoreId = stores[2].Id }
        };

        await context.Customers.AddRangeAsync(customers);
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {CustomerCount} 個客戶", customers.Length);
    }

    /// <summary>
    /// 初始化庫存
    /// </summary>
    private static async Task SeedInventoriesAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Inventories.AnyAsync()) return;

        var products = await context.Products.ToListAsync();
        var warehouses = await context.Warehouses.ToListAsync();

        var inventories = new List<Inventory>();
        var random = new Random(42);

        foreach (var product in products)
        {
            foreach (var warehouse in warehouses)
            {
                // 中央倉庫庫存較多
                var baseQuantity = warehouse.IsDefault ? 200 : 50;
                var quantity = random.Next(baseQuantity / 2, baseQuantity * 2);

                inventories.Add(new Inventory
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = quantity,
                    SafetyStock = product.SafetyStock,
                    LastMovementDate = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                });
            }
        }

        await context.Inventories.AddRangeAsync(inventories);
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {InventoryCount} 筆庫存記錄", inventories.Count);
    }

    /// <summary>
    /// 初始化促銷活動
    /// </summary>
    private static async Task SeedPromotionsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Promotions.AnyAsync()) return;

        var promotions = new[]
        {
            new Promotion { Code = "NEWYEAR2026", Name = "新年特惠", Description = "全館商品 9 折優惠", PromotionType = PromotionType.Discount, Status = PromotionStatus.Active, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31, 23, 59, 59), DiscountValue = 90, MinPurchaseAmount = 500, MaxUsageCount = 0 },
            new Promotion { Code = "VIP500OFF", Name = "VIP滿額折抵", Description = "滿 5000 折 500", PromotionType = PromotionType.AmountOff, Status = PromotionStatus.Active, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31, 23, 59, 59), DiscountValue = 500, MinPurchaseAmount = 5000, ApplicableLevelIds = "[4,5]" },
            new Promotion { Code = "FREESHIP", Name = "免運費", Description = "滿 2000 免運費", PromotionType = PromotionType.AmountOff, Status = PromotionStatus.Active, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31, 23, 59, 59), DiscountValue = 100, MinPurchaseAmount = 2000 }
        };

        await context.Promotions.AddRangeAsync(promotions);
        await context.SaveChangesAsync();

        logger.LogInformation("建立 {PromotionCount} 個促銷活動", promotions.Length);
    }
}
