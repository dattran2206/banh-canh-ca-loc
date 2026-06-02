# BanhCanhCaLoc — Cấu Trúc Backend .NET Hoàn Chỉnh

> Clean Architecture + DDD + CQRS + Vertical Slice sẵn sàng scale

---

## Sơ đồ dependency

```
API  →  Application  →  Domain
              ↑
       Infrastructure (implements interfaces từ Domain)
```

---

## Toàn bộ cấu trúc

```
BanhCanhCaLoc/
│
├── BanhCanhCaLoc.sln
│
├── src/
│   │
│   ├── BanhCanhCaLoc.Domain/
│   │   ├── BanhCanhCaLoc.Domain.csproj
│   │   │
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs                  # Id, DomainEvents
│   │   │   ├── AggregateRoot.cs               # kế thừa BaseEntity
│   │   │   ├── ValueObject.cs                 # abstract, equality theo value
│   │   │   ├── IDomainEvent.cs                # marker interface
│   │   │   └── IAuditableEntity.cs            # CreatedAt, UpdatedAt, CreatedBy
│   │   │
│   │   ├── Entities/
│   │   │   ├── Order.cs                       # Aggregate Root
│   │   │   ├── OrderItem.cs                   # Entity con của Order
│   │   │   ├── MenuItem.cs                    # Aggregate Root
│   │   │   ├── Table.cs                       # Bàn ăn
│   │   │   └── Customer.cs
│   │   │
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs                       # Amount + Currency
│   │   │   ├── Address.cs
│   │   │   └── PhoneNumber.cs
│   │   │
│   │   ├── Enums/
│   │   │   ├── OrderStatus.cs                 # Pending, Confirmed, Served, Paid, Cancelled
│   │   │   ├── PaymentMethod.cs               # Cash, Transfer, QRCode
│   │   │   └── MenuItemCategory.cs            # BanhCanh, Topping, Drink
│   │   │
│   │   ├── Events/
│   │   │   ├── OrderPlacedEvent.cs
│   │   │   ├── OrderCancelledEvent.cs
│   │   │   ├── OrderPaidEvent.cs
│   │   │   └── MenuItemOutOfStockEvent.cs
│   │   │
│   │   ├── Errors/
│   │   │   ├── OrderErrors.cs                 # static Error definitions
│   │   │   ├── MenuItemErrors.cs
│   │   │   └── CustomerErrors.cs
│   │   │
│   │   └── Repositories/                      # Interfaces chỉ khai báo ở đây
│   │       ├── IOrderRepository.cs
│   │       ├── IMenuItemRepository.cs
│   │       ├── ICustomerRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   ├── BanhCanhCaLoc.Application/
│   │   ├── BanhCanhCaLoc.Application.csproj   # ref: Domain
│   │   │
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs      # FluentValidation pipeline
│   │   │   │   ├── LoggingBehavior.cs         # log mọi request/response
│   │   │   │   ├── TransactionBehavior.cs     # wrap command trong transaction
│   │   │   │   └── CachingBehavior.cs         # cache query result
│   │   │   │
│   │   │   ├── Interfaces/
│   │   │   │   ├── ICurrentUserService.cs     # UserId, UserName, Roles
│   │   │   │   ├── IDateTimeProvider.cs       # UtcNow — dễ test
│   │   │   │   ├── IEmailService.cs
│   │   │   │   └── INotificationService.cs    # push notification
│   │   │   │
│   │   │   └── Models/
│   │   │       ├── Result.cs                  # Result<T> / Result pattern
│   │   │       ├── Error.cs                   # Code + Description
│   │   │       └── PagedList.cs
│   │   │
│   │   ├── Features/                          # Vertical Slice: mỗi feature 1 folder
│   │   │   │
│   │   │   ├── Orders/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── PlaceOrder/
│   │   │   │   │   │   ├── PlaceOrderCommand.cs
│   │   │   │   │   │   ├── PlaceOrderCommandHandler.cs
│   │   │   │   │   │   └── PlaceOrderCommandValidator.cs
│   │   │   │   │   ├── CancelOrder/
│   │   │   │   │   │   ├── CancelOrderCommand.cs
│   │   │   │   │   │   ├── CancelOrderCommandHandler.cs
│   │   │   │   │   │   └── CancelOrderCommandValidator.cs
│   │   │   │   │   └── PayOrder/
│   │   │   │   │       ├── PayOrderCommand.cs
│   │   │   │   │       └── PayOrderCommandHandler.cs
│   │   │   │   │
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── GetOrderById/
│   │   │   │   │   │   ├── GetOrderByIdQuery.cs
│   │   │   │   │   │   ├── GetOrderByIdQueryHandler.cs
│   │   │   │   │   │   └── OrderResponse.cs   # DTO trả về
│   │   │   │   │   └── GetOrdersByTable/
│   │   │   │   │       ├── GetOrdersByTableQuery.cs
│   │   │   │   │       └── GetOrdersByTableQueryHandler.cs
│   │   │   │   │
│   │   │   │   └── EventHandlers/
│   │   │   │       ├── OrderPlacedEventHandler.cs   # gửi notification cho bếp
│   │   │   │       └── OrderPaidEventHandler.cs     # cập nhật doanh thu
│   │   │   │
│   │   │   ├── MenuItems/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateMenuItem/
│   │   │   │   │   └── UpdateMenuItemPrice/
│   │   │   │   └── Queries/
│   │   │   │       ├── GetAllMenuItems/
│   │   │   │       └── GetMenuItemsByCategory/
│   │   │   │
│   │   │   └── Customers/
│   │   │       ├── Commands/
│   │   │       │   └── RegisterCustomer/
│   │   │       └── Queries/
│   │   │           └── GetCustomerOrderHistory/
│   │   │
│   │   └── DependencyInjection.cs             # AddApplication(this IServiceCollection)
│   │
│   ├── BanhCanhCaLoc.Infrastructure/
│   │   ├── BanhCanhCaLoc.Infrastructure.csproj  # ref: Domain + Application
│   │   │
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs                # EF Core DbContext + Outbox
│   │   │   ├── AppDbContextFactory.cs         # IDesignTimeDbContextFactory (migration)
│   │   │   │
│   │   │   ├── Configurations/                # IEntityTypeConfiguration<T>
│   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   ├── OrderItemConfiguration.cs
│   │   │   │   ├── MenuItemConfiguration.cs
│   │   │   │   └── CustomerConfiguration.cs
│   │   │   │
│   │   │   ├── Migrations/                    # EF Core migrations
│   │   │   │
│   │   │   └── Seed/
│   │   │       └── DataSeeder.cs              # seed menu mặc định
│   │   │
│   │   ├── Repositories/                      # Implement interfaces từ Domain
│   │   │   ├── BaseRepository.cs              # Generic CRUD
│   │   │   ├── OrderRepository.cs
│   │   │   ├── MenuItemRepository.cs
│   │   │   ├── CustomerRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   │
│   │   ├── Outbox/                            # Đảm bảo domain events không bị mất
│   │   │   ├── OutboxMessage.cs
│   │   │   ├── OutboxMessageConfiguration.cs
│   │   │   └── ProcessOutboxMessagesJob.cs    # Quartz.NET background job
│   │   │
│   │   ├── Caching/
│   │   │   ├── CacheService.cs                # IDistributedCache wrapper
│   │   │   └── CacheKeys.cs                   # static string constants
│   │   │
│   │   ├── Services/
│   │   │   ├── CurrentUserService.cs          # ICurrentUserService
│   │   │   ├── DateTimeProvider.cs            # IDateTimeProvider
│   │   │   ├── EmailService.cs                # SendGrid / SMTP
│   │   │   └── NotificationService.cs         # Firebase / SignalR
│   │   │
│   │   ├── Authentication/
│   │   │   ├── JwtTokenGenerator.cs
│   │   │   ├── JwtSettings.cs
│   │   │   └── PermissionAuthorizationHandler.cs
│   │   │
│   │   └── DependencyInjection.cs             # AddInfrastructure(this IServiceCollection)
│   │
│   ├── BanhCanhCaLoc.Contracts/
│   │   ├── BanhCanhCaLoc.Contracts.csproj     # Không ref project nào
│   │   │
│   │   ├── Orders/
│   │   │   ├── PlaceOrderRequest.cs
│   │   │   ├── PlaceOrderResponse.cs
│   │   │   └── OrderSummaryResponse.cs
│   │   │
│   │   ├── MenuItems/
│   │   │   ├── CreateMenuItemRequest.cs
│   │   │   └── MenuItemResponse.cs
│   │   │
│   │   └── Authentication/
│   │       ├── LoginRequest.cs
│   │       └── AuthResponse.cs
│   │
│   └── BanhCanhCaLoc.API/
│       ├── BanhCanhCaLoc.API.csproj           # ref: Application + Infrastructure + Contracts
│       │
│       ├── Controllers/
│       │   ├── ApiController.cs               # BaseController: xử lý Result → IActionResult
│       │   ├── OrdersController.cs
│       │   ├── MenuItemsController.cs
│       │   ├── TablesController.cs
│       │   └── AuthController.cs
│       │
│       ├── Middleware/
│       │   ├── GlobalExceptionHandler.cs      # IExceptionHandler (.NET 8)
│       │   └── RequestLoggingMiddleware.cs
│       │
│       ├── Extensions/
│       │   ├── ServiceCollectionExtensions.cs # Swagger, CORS, Auth config
│       │   └── WebApplicationExtensions.cs    # Middleware pipeline
│       │
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs                         # Minimal setup: AddX() rồi UseX()
│
└── tests/
    │
    ├── BanhCanhCaLoc.UnitTests/
    │   ├── Domain/
    │   │   ├── OrderTests.cs                  # test business rules trong entity
    │   │   └── MoneyTests.cs                  # test value object equality
    │   └── Application/
    │       ├── PlaceOrderCommandHandlerTests.cs
    │       └── GetOrderByIdQueryHandlerTests.cs
    │
    ├── BanhCanhCaLoc.IntegrationTests/
    │   ├── WebAppFactory.cs                   # TestServer + in-memory DB
    │   ├── Orders/
    │   │   ├── PlaceOrderTests.cs             # test full HTTP request
    │   │   └── GetOrderTests.cs
    │   └── MenuItems/
    │       └── GetMenuItemsTests.cs
    │
    └── BanhCanhCaLoc.ArchTests/
        ├── LayerDependencyTests.cs            # Domain không được ref Infrastructure
        ├── NamingConventionTests.cs           # Handler, Command, Query đúng tên
        └── RepositoryInterfaceTests.cs        # Interfaces phải nằm trong Domain
```

---

## Thứ tự đăng ký DI (Program.cs)

```csharp
builder.Services
    .AddApplication()       // MediatR, FluentValidation, Behaviors
    .AddInfrastructure()    // EF Core, Repos, Services, Auth
    .AddPresentation();     // Controllers, Swagger, CORS
```

---

## Công nghệ sử dụng

| Mục đích             | Package                          |
|----------------------|----------------------------------|
| CQRS / Mediator      | MediatR                          |
| Validation           | FluentValidation                 |
| Mapping              | Mapster                          |
| ORM                  | Entity Framework Core            |
| Caching              | Redis (StackExchange.Redis)      |
| Background Jobs      | Quartz.NET                       |
| Auth                 | ASP.NET Core Identity + JWT      |
| Logging              | Serilog + Seq                    |
| API Docs             | Scalar (thay Swagger UI)         |
| Architecture Test    | ArchUnitNET                      |
| Unit Test            | xUnit + FluentAssertions + NSubstitute |
| Integration Test     | Microsoft.AspNetCore.Mvc.Testing |

---

## Quy tắc dependency (bắt buộc kiểm tra bằng ArchTests)

- `Domain` → **không phụ thuộc gì**
- `Application` → chỉ được phụ thuộc `Domain`
- `Infrastructure` → được phụ thuộc `Domain` + `Application`
- `API` → được phụ thuộc tất cả, nhưng không chứa business logic
- `Contracts` → **không phụ thuộc gì** (DTO thuần)
