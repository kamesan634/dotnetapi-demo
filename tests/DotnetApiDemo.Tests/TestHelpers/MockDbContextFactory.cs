using Microsoft.EntityFrameworkCore;
using DotnetApiDemo.Data;
using DotnetApiDemo.Models.Entities;
using DotnetApiDemo.Models.Enums;

namespace DotnetApiDemo.Tests.TestHelpers;

/// <summary>
/// Mock DbContext 工廠
/// </summary>
public static class MockDbContextFactory
{
    /// <summary>
    /// 建立 InMemory DbContext
    /// </summary>
    public static ApplicationDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName ?? $"TestDb_{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// 建立並填充測試資料的 DbContext
    /// </summary>
    public static ApplicationDbContext CreateWithSeedData(string? dbName = null)
    {
        var context = Create(dbName);
        SeedTestData(context);
        return context;
    }

    /// <summary>
    /// 填充測試資料
    /// </summary>
    public static void SeedTestData(ApplicationDbContext context)
    {
        // 單位
        var unit = new Unit { Id = 1, Code = "PCS", Name = "件", IsActive = true };
        if (!context.Units.Any())
        {
            context.Units.Add(unit);
        }

        // 分類
        var category = new Category
        {
            Id = 1,
            Code = "CAT001",
            Name = "測試分類",
            IsActive = true
        };
        if (!context.Categories.Any())
        {
            context.Categories.Add(category);
        }

        // 門市
        var store = new Store
        {
            Id = 1,
            Code = "STORE001",
            Name = "測試門市",
            IsActive = true,
            IsDefault = true
        };
        if (!context.Stores.Any())
        {
            context.Stores.Add(store);
        }

        // 倉庫
        var warehouse = new Warehouse
        {
            Id = 1,
            Code = "WH001",
            Name = "測試倉庫",
            IsActive = true,
            IsDefault = true,
            StoreId = 1
        };
        if (!context.Warehouses.Any())
        {
            context.Warehouses.Add(warehouse);
        }

        // 商品
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    Id = 1,
                    Sku = "PROD001",
                    Barcode = "1234567890123",
                    Name = "測試商品 1",
                    CategoryId = 1,
                    UnitId = 1,
                    CostPrice = 100,
                    SellingPrice = 150,
                    SafetyStock = 10,
                    IsActive = true
                },
                new Product
                {
                    Id = 2,
                    Sku = "PROD002",
                    Barcode = "1234567890124",
                    Name = "測試商品 2",
                    CategoryId = 1,
                    UnitId = 1,
                    CostPrice = 200,
                    SellingPrice = 300,
                    SafetyStock = 5,
                    IsActive = true
                }
            );
        }

        // 庫存
        if (!context.Inventories.Any())
        {
            context.Inventories.AddRange(
                new Inventory { ProductId = 1, WarehouseId = 1, Quantity = 100, ReservedQuantity = 0 },
                new Inventory { ProductId = 2, WarehouseId = 1, Quantity = 3, ReservedQuantity = 0 } // 低於安全庫存
            );
        }

        // 客戶等級
        var customerLevel = new CustomerLevel
        {
            Id = 1,
            Code = "NORMAL",
            Name = "一般會員",
            DiscountRate = 100,
            PointRate = 1,
            IsDefault = true,
            IsActive = true
        };
        if (!context.CustomerLevels.Any())
        {
            context.CustomerLevels.Add(customerLevel);
        }

        // 客戶
        if (!context.Customers.Any())
        {
            context.Customers.Add(new Customer
            {
                Id = 1,
                MemberNo = "M00001",
                Name = "測試客戶",
                Phone = "0912345678",
                Email = "customer@test.com",
                CustomerLevelId = 1,
                TotalSpent = 0,
                CurrentPoints = 0,
                IsActive = true
            });
        }

        // 供應商
        if (!context.Suppliers.Any())
        {
            context.Suppliers.Add(new Supplier
            {
                Id = 1,
                Code = "SUP001",
                Name = "測試供應商",
                ContactPerson = "聯絡人",
                Phone = "02-12345678",
                IsActive = true
            });
        }

        // 付款方式
        if (!context.PaymentMethods.Any())
        {
            context.PaymentMethods.AddRange(
                new PaymentMethod { Id = 1, Code = "CASH", Name = "現金", IsActive = true, IsDefault = true },
                new PaymentMethod { Id = 2, Code = "CARD", Name = "信用卡", IsActive = true }
            );
        }

        // 促銷活動
        if (!context.Promotions.Any())
        {
            context.Promotions.Add(new Promotion
            {
                Id = 1,
                Code = "PROMO001",
                Name = "測試促銷活動",
                PromotionType = PromotionType.Discount,
                Status = PromotionStatus.Active,
                DiscountValue = 90,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddMonths(1),
                CreatedAt = DateTime.UtcNow
            });
        }

        context.SaveChanges();
    }

    /// <summary>
    /// 建立測試訂單
    /// </summary>
    public static Order CreateTestOrder(ApplicationDbContext context, int? customerId = null)
    {
        var order = new Order
        {
            OrderNo = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}",
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StoreId = 1,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            SubTotal = 150,
            DiscountAmount = 0,
            TaxAmount = 0,
            TotalAmount = 150,
            PaidAmount = 0,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        context.SaveChanges();

        context.OrderItems.Add(new OrderItem
        {
            OrderId = order.Id,
            ProductId = 1,
            ProductName = "測試商品 1",
            Quantity = 1,
            UnitPrice = 150,
            DiscountAmount = 0,
            SubTotal = 150
        });

        context.SaveChanges();
        return order;
    }
}
