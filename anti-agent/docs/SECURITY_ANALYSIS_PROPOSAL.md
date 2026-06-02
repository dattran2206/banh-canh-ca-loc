# Security Analysis & Hardening Proposal - Stub Server Project

## 1. Lỗ hổng Thực thi SQL tùy ý (SQL Injection / RCE)
Vấn đề nghiêm trọng nhất nằm ở việc Server nhận trực tiếp lệnh SQL từ Client và thực thi với quyền Root.

| Trạng thái hiện tại | Rủi ro | Giải pháp 1: API-Driven (Khuyên dùng) | Giải pháp 2: SQL Whitelist |
| :--- | :--- | :--- | :--- |
| File `web/routes/updateValue.js` và `staging/server.js` chấp nhận `req.body.sql`. | Kẻ tấn công có thể xóa toàn bộ Database (`DROP DATABASE`) hoặc đọc dữ liệu nhạy cảm. | **Loại bỏ hoàn toàn tham số `sql`**. Chỉ nhận các tham số cụ thể (`id`, `field`, `value`) và sử dụng ORM để cập nhật. | Chỉ cho phép các câu lệnh SQL bắt đầu bằng `SELECT` hoặc `UPDATE` trên một số bảng nhất định (Phức tạp và khó duy trì). |
| **Đánh giá** | **CRITICAL** | **An toàn nhất, dễ bảo trì.** | **Vẫn tiềm ẩn rủi ro lách luật.** |

---

## 2. Lỗ hổng Chiếm quyền điều khiển Host (Docker Escape)
Việc mount Docker socket vào container `web` tạo ra một "đường tắt" để kiểm soát máy chủ vật lý.

| Trạng thái hiện tại | Rủi ro | Giải pháp 1: Remove Socket (Khuyên dùng) | Giải pháp 2: Docker Socket Proxy |
| :--- | :--- | :--- | :--- |
| `docker-compose.yml` mount `/var/run/docker.sock`. | Nếu ứng dụng Web bị tấn công, kẻ gian có thể tạo container mới với quyền root trên máy host. | **Gỡ bỏ mount socket**. Chuyển sang dùng Docker API thông qua mạng nội bộ hoặc các script kiểm tra sức khỏe độc lập. | Sử dụng `docker-socket-proxy` để chỉ cho phép lệnh `GET` (Read-only) và chặn mọi lệnh can thiệp container. |
| **Đánh giá** | **CRITICAL** | **Triệt tiêu hoàn toàn rủi ro.** | **Giữ được tính năng monitor nhưng an toàn hơn.** |

---

## 3. Cơ chế Xác thực & Kiểm soát truy cập (Authentication)
Dự án hiện đang mở hoàn toàn (Wide Open) cho bất kỳ ai có URL.

| Trạng thái hiện tại | Rủi ro | Giải pháp 1: API Key + IP Whitelist | Giải pháp 2: Basic Auth + VPN |
| :--- | :--- | :--- | :--- |
| Không có Middleware kiểm tra danh tính. Header `Authorization` có khai báo nhưng không dùng. | Dữ liệu thật (clone) bị lộ ra ngoài internet/VPN. Ai cũng có thể sửa dữ liệu. | **Bắt buộc Header `x-api-key`**. Chỉ cho phép các dải IP nội bộ của công ty truy cập. | Sử dụng Basic Auth (User/Pass) ở tầng Nginx/Proxy và yêu cầu kết nối VPN để thấy server. |
| **Đánh giá** | **HIGH** | **Linh hoạt cho việc gọi API tự động.** | **Bảo mật tầng hạ tầng tốt hơn.** |

---

## 4. Dữ liệu nhạy cảm (Sensitive Data Protection)
Dữ liệu được clone từ hệ thống thật chứa thông tin khách hàng và có thể cả tokens/credentials.

| Trạng thái hiện tại | Rủi ro | Giải pháp 1: Data Masking (Khuyên dùng) | Giải pháp 2: Static Mock Data |
| :--- | :--- | :--- | :--- |
| Dữ liệu thô từ Prod được đưa vào Stub. | Lộ lọt thông tin cá nhân (PII), vi phạm chính sách bảo mật dữ liệu. | **Tự động hóa việc làm sạch dữ liệu**: Thay thế email, số điện thoại, tokens bằng dữ liệu giả sau khi clone. | Không dùng dữ liệu thật, chỉ dùng dữ liệu mẫu được sinh ra từ script. |
| **Đánh giá** | **HIGH** | **Giữ được tính đa dạng của dữ liệu nhưng an toàn.** | **An toàn tuyệt đối nhưng thiếu case thực tế.** |

---

## 5. Cấu hình Hạ tầng (Infrastructure Hardening)
Các mật khẩu mặc định và cổng dịch vụ đang bị phơi nhiễm.

| Trạng thái hiện tại | Rủi ro | Giải pháp 1: Internal Networking | Giải pháp 2: Strong Secrets Management |
| :--- | :--- | :--- | :--- |
| Mật khẩu MySQL `aA@123456`, port 3306 và 8080 (PMA) mở ra ngoài. | Dễ dàng bị tấn công Brute-force hoặc khai thác qua phpMyAdmin. | **Đóng toàn bộ port ra ngoài**. Chỉ mở port 3000/3001/3002. Mọi truy cập vào DB phải qua SSH Tunnel. | Đổi toàn bộ password sang chuỗi ngẫu nhiên dài. Sử dụng Docker Secrets hoặc file `.env` không đẩy lên Git. |
| **Đánh giá** | **MEDIUM** | **Giảm bề mặt tấn công tối đa.** | **Yêu cầu quy trình quản lý key chặt chẽ.** |

---

## 🚀 Đề xuất Ưu tiên thực hiện (Priority)
1. **Priority 1 (Ngay lập tức):** Vá lỗi SQL Injection & Gỡ Docker Socket.
2. **Priority 2 (Trong 24h):** Triển khai API Key Authentication.
3. **Priority 3 (Tiếp theo):** Đóng các cổng Database và đổi mật khẩu hệ thống.

---
**Bạn vui lòng xem qua và cho tôi biết lựa chọn của bạn cho từng mục (ví dụ: 1.1, 2.1, 3.1...). Sau đó tôi sẽ tiến hành thực hiện.**
