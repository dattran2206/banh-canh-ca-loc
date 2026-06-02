# 🗄️ Hướng Dẫn Sao Lưu & Khôi Phục Cơ Sở Dữ Liệu (SQL Server)

Thư mục này được sử dụng để chứa các bản sao lưu (.bak) và các công cụ hỗ trợ sao lưu/khôi phục cơ sở dữ liệu `BanhCanhCaLoc` của hệ thống.

---

## 📁 Cấu Trúc Thư Mục

```text
database/
├── backups/                  # Thư mục lưu trữ các file backup (.bak) (Đã được cấu hình bỏ qua trên Git)
├── backup.ps1                # Script PowerShell tự động sao lưu database
├── restore.ps1               # Script PowerShell tự động khôi phục database từ file .bak
└── README.md                 # Hướng dẫn chi tiết (File này)
```

---

## ⚡ Hướng Dẫn Sử Dụng Script Tự Động (PowerShell)

Các script này tự động kết nối vào SQL Server và thực hiện sao lưu/khôi phục. Mặc định các script sẽ đọc cấu hình kết nối từ `backend/BanhCanhCaLoc.Api/appsettings.json`.

### 1. Sao lưu cơ sở dữ liệu (Backup)
Mở cửa sổ **PowerShell** (nên chạy với quyền Administrator) tại thư mục gốc dự án và chạy:
```powershell
./database/backup.ps1
```
*   **Hoạt động**: Script sẽ kết nối tới SQL Server, sao lưu cơ sở dữ liệu và lưu vào thư mục `database/backups/BanhCanhCaLoc_YYYYMMDD_HHMMSS.bak`.
*   **Tham số tùy chọn**: Bạn có thể truyền thủ công Server, Username, Password nếu muốn:
    ```powershell
    ./database/backup.ps1 -ServerName "localhost" -Username "sa" -Password "dattt@2206"
    ```

### 2. Khôi phục cơ sở dữ liệu (Restore)
Để khôi phục dữ liệu từ một bản sao lưu trong thư mục `backups/`:
```powershell
./database/restore.ps1 -BackupFile "database/backups/BanhCanhCaLoc_xxxx.bak"
```
*   **Lưu ý**: Khôi phục database sẽ ghi đè lên dữ liệu hiện tại. Hãy đảm bảo không có kết nối nào đang hoạt động tới database (nên tắt ứng dụng Backend trước khi restore).

---

## 🛠️ Hướng Dẫn Thủ Công Bằng SQL Server Management Studio (SSMS)

Nếu không sử dụng script, bạn có thể thực hiện trực tiếp bằng giao diện đồ họa SSMS:

### 1. Sao lưu (Backup)
1. Mở **SSMS** và kết nối tới SQL Server của bạn.
2. Tại cửa sổ **Object Explorer**, mở rộng thư mục **Databases**.
3. Nhấp chuột phải vào cơ sở dữ liệu `BanhCanhCaLoc` -> Chọn **Tasks** -> **Back Up...**.
4. Tại mục **Destination**, chọn **Disk**, chọn đường dẫn lưu file mong muốn (ví dụ: trỏ tới thư mục `c:\dattt\LEARN\banh-canh-ca-loc\database\backups\BanhCanhCaLoc.bak`).
5. Nhấn **OK** để hoàn tất sao lưu.

### 2. Khôi phục (Restore)
1. Nhấp chuột phải vào thư mục **Databases** trong SSMS -> Chọn **Restore Database...**.
2. Tại phần **Source**, chọn **Device** -> Nhấp vào nút `...` bên cạnh.
3. Trong hộp thoại hiện ra, nhấn **Add** và trỏ tới file `.bak` đã sao lưu trong thư mục `database/backups/`.
4. Tại tab **Options** bên trái, chọn **Overwrite the existing database (WITH REPLACE)** và tích chọn **Close existing connections to destination database** (để đóng các kết nối đang chạy từ backend).
5. Nhấn **OK** để bắt đầu khôi phục.
