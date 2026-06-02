# Hướng Dẫn Chuyển Giao & Tiếp Tục Dự Án (Execution Flow)

Tài liệu này hướng dẫn cách thiết lập môi trường và các bước chi tiết để tiếp tục triển khai dự án **Bánh Canh Cá Lóc** (di động từ LocalStorage sang ASP.NET Core & SQL Server) khi chuyển sang máy tính mới.

---

## 1. Trạng Thái Hiện Tại (Current Status)
- **Frontend**:
  - Giao diện đã được tối ưu hóa hiển thị Responsive trên mobile và máy tính bảng (Tablet).
  - Đã chạy kiểm thử linter (`npm run lint`) và kiểm tra kiểu dữ liệu (`npm run typecheck`) hoàn toàn sạch lỗi.
- **Backend**:
  - Đã khởi tạo thư mục `backend/` chứa dotnet solution `BanhCanhCaLoc.sln` và dự án trống `BanhCanhCaLoc.Api` (`dotnet new webapi`).
  - Chưa định nghĩa database model, migrations, Controllers, SignalR Hub và chưa tích hợp apiClient ở frontend.

---

## 2. Điều Kiện Tiền Đề Trên Máy Mới (Prerequisites)
Hãy đảm bảo máy tính mới đã cài đặt các phần mềm sau:
1. **.NET 8.0 SDK** trở lên.
2. **Node.js** (Phiên bản v18+) & **npm**.
3. **Microsoft SQL Server** (Bản Express, Developer hoặc LocalDB) + **SQL Server Management Studio (SSMS)** để quản trị.
4. IDE khuyến nghị: **Visual Studio 2022** hoặc **VS Code** (kèm extension *C# Dev Kit*).

---

## 3. Các Bước Thực Hiện Chi Tiết (Step-by-Step)

### BƯỚC 1: Cấu hình Kết nối Cơ sở dữ liệu (SQL Server)
1. Mở SQL Server và tạo một database trống tên là `BanhCanhCaLoc`.
2. Mở file [appsettings.json](file:///c:/dattt/LEARN/banh-canh-ca-loc/backend/BanhCanhCaLoc.Api/appsettings.json) trong dự án API và cập nhật Connection String:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=BanhCanhCaLoc;User Id=sa;Password=dattt@2206;TrustServerCertificate=True;"
   }
   ```

### BƯỚC 2: Cài đặt NuGet Packages cho Backend
Mở Terminal tại thư mục `backend/BanhCanhCaLoc.Api/` và chạy các lệnh cài đặt các gói NuGet sau:
```bash
# 1. Thư viện kết nối database SQL Server
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# 2. Thư viện phục vụ migrations và thiết kế database (Design & Tools)
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools

# 3. Thư viện mã hóa mật khẩu ở Server
dotnet add package BCrypt.Net-Next

# 4. Thư viện xác thực JWT Bearer Token
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```
*(Lưu ý: SignalR đã được tích hợp sẵn trong framework ASP.NET Core Web API của .NET 8.0/9.0, do đó không cần cài đặt thêm gói NuGet ở Backend. Ở Frontend chúng ta sẽ cần cài đặt thư viện `@microsoft/signalr` ở Bước 5 để kết nối).*

*(Cài đặt công cụ EF Core nếu máy mới chưa có: `dotnet tool install --global dotnet-ef`)*

### BƯỚC 3: Xây dựng Database Models & DbContext
1. Tạo thư mục `Models/` trong dự án API và viết các class Entity:
   - Các bảng tĩnh/danh mục (Khóa chính `int` tự tăng): `Area`, `Table`, `Category`, `MenuItem`, `Ingredient`, `RecipeItem` (composite PK `MenuItemId` + `IngredientId`), `ShopInfo`.
   - Các bảng động/giao dịch (Khóa chính `Guid`): `User`, `Shift`, `Order`, `OrderItem`, `Payment`, `StockEntry`, `WasteRecord`, `StockTake`, `ActivityLog`.
2. Tạo file `Data/BanhCanhCaLocDbContext.cs` để ánh xạ quan hệ khóa ngoại (Foreign Keys) và cấu hình Fluent API.
3. Tạo migration và cập nhật database:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
4. Viết file `Data/DbInitializer.cs` để gieo dữ liệu mẫu ban đầu (nhân viên mặc định: admin, waiter, cashier, kitchen, các khu vực và danh sách món ăn định lượng từ file `seedData.js` cũ).

### BƯỚC 4: Triển khai APIs & SignalR Realtime Hub
1. Viết `Services/TokenService.cs` để mã hóa và tạo JWT Token cho đăng nhập.
2. Viết các Controllers trong thư mục `Controllers/`:
   - `AuthController`: Đăng nhập (hash BCrypt tại server), quản lý ca làm việc (`Shift`).
   - `TablesController` & `MenuController`: Quản lý sơ đồ bàn, khu vực, món ăn, công thức nấu ăn.
   - `InventoryController`: Quản lý nguyên liệu kho, nhập kho, hủy kho và kiểm kê.
   - `OrdersController`: 
     - Quản lý order mới và cập nhật trạng thái.
     - Viết logic tự động trừ kho nguyên liệu trong database dựa theo công thức cấu hình khi bếp đánh dấu món chín (`ready`).
     - Tích hợp SignalR Hub (`OrderHub`) để đẩy sự kiện `OrderUpdated` realtime cho toàn bộ client.
   - `PaymentsController`: Thanh toán hóa đơn, cập nhật ca làm việc và thống kê doanh thu.
3. Cấu hình Cors, Authentication JwtBearer, DI Services và SignalR Endpoint `/hub/orders` trong `Program.cs`.

### BƯỚC 5: Tải dữ liệu bất đồng bộ & Tích hợp SignalR trên Frontend
1. Cài đặt `axios` trên thư mục client `app/`:
   ```bash
   npm install axios
   ```
2. Tạo file [apiClient.js](file:///c:/dattt/LEARN/banh-canh-ca-loc/app/src/api/apiClient.js) bọc Axios với Interceptor tự động thêm Bearer token vào Header.
3. Tạo file [DataContext.jsx](file:///c:/dattt/LEARN/banh-canh-ca-loc/app/src/lib/DataContext.jsx) đóng gói toàn bộ trạng thái trong React Context. Sử dụng `@microsoft/signalr` kết nối tới `/hub/orders` để đồng bộ dữ liệu order tự động khi nhận sự kiện từ backend.
4. Sửa [appAuth.jsx](file:///c:/dattt/LEARN/banh-canh-ca-loc/app/src/lib/appAuth.jsx) kết nối các API đăng nhập, bắt đầu ca, kết thúc ca của backend. Loại bỏ dependency `bcryptjs` trên client.
5. Cấu trúc lại các trang hiển thị (`Dashboard.jsx`, `Tables.jsx`, `Orders.jsx`, `Menu.jsx`, `Inventory.jsx`, `Reports.jsx`, `Staff.jsx`) sang dạng đọc dữ liệu qua hook `const { ... } = useData()`.

### BƯỚC 6: Nghiệm thu toàn trình (E2E Test)
1. Chạy Backend: `dotnet run --project backend/BanhCanhCaLoc.Api/BanhCanhCaLoc.Api.csproj`
2. Chạy Frontend: `npm run dev` tại thư mục `app/`
3. Tiến hành thử nghiệm: Đăng nhập -> Gọi món -> Bếp nhận đơn realtime -> Làm xong -> Kho tự động trừ -> Thanh toán hóa đơn -> Báo cáo doanh thu ca.
