# Hướng Dẫn Sử Dụng Antigravity Kit (Phiên bản Cuối cùng - Level 5)

Chào mừng bạn đến với **Antigravity OS** – Hệ thống AI Coding không còn là những đoạn chat hỏi đáp thông thường. Nó đã được cơ cấu lại thành một "Tập đoàn Kỹ sư" (Agentic Architecture - Level 5) có khả năng: Điều phối, Tự lập kế hoạch, Quản lý bộ nhớ ngầm, Chống ảo giác bằng PageIndex, và Bắt buộc nộp bằng chứng khi code.

Dưới đây là cẩm nang toàn tập về cách "lái" cỗ máy này!

---

## Danh Sách Lõi: Các Câu Lệnh (Slash Commands) 

Chỉ cần gõ `@[<tên lệnh>] <Yêu cầu>`, hệ thống sẽ tự nổ máy vận hành đúng quy chuẩn.

### 1. Dòng Lệnh Thực Thi (Thợ Xây Code)
- **`@[/orchestrate]`**: Lệnh **Tổng Tư Lệnh (Supervisor)**. Đây là lệnh bạn sẽ dùng 90% thời gian. 
  - *Cách hoạt động:* Đừng khai báo Agent thủ công. Cứ đưa yêu cầu vào lệnh này. Supervisor sẽ tự đọc yêu cầu của bạn, tự lên Plan, tự phân công đúng 3+ Agent chuyên gia (Frontend, Backend, Security) để chúng nó làm việc đan xen (Parallel) hoặc tuần tự (Sequential) tùy độ khó.
- **`@[/fe-pro]` & `@[/be-pro]`**: Lệnh **Xây dựng Tính Năng Lớn (Deep Execution)**. 
  - *Cách hoạt động:* Nếu bạn cần code cả 1 luồng Authentication từ đầu đến cuối, dùng lệnh này. Nó sẽ nhốt Agent vào 3 Trạm Kiểm Duyệt (Biên dịch Yêu Cầu Viết Code Chạy Script Test & Trình duyệt). *Không vượt qua trạm = Không được kết thúc công việc.*

### 2. Dòng Lệnh Thu Thập Thông Tin (Mắt Thần)
- **`@[/research-protocol]`**: **Đội Đặc Nhiệm Mạng**. 
  - *Sử dụng khi:* Bạn cần AI lên mạng cào data thời gian thực thay vì "chém gió" dựa vào dữ liệu cũ. Hoặc bạn cần một bài Report so sánh công nghệ A vs B.
  - *Quy tắc:* Mọi kết quả trả về bắt buộc phải có [1] URL trích dẫn và Thời báo hằng ngày.
- **`@[/index-repo]`**: **Kỹ Sư Trắc Địa (PageIndex/Vectorless RAG)**.
  - *Sử dụng khi:* Bạn mới kéo một Source Code lạ về máy, hoặc bạn vừa quăng vào thư mục 1 file PDF Dài hàng trăm trang.
  - *Biết để hiểu:* Chạy lệnh này, AI sẽ lập một "Cây Mục Lục" (`page_index.json`). Những lần hỏi sau, AI sẽ nhìn vào Mục Lục này để chui vào đúng nơi phát sinh vấn đề, tuyệt đối 100% không bị "ảo giác" (Hallucination) do đọc lộn xộn các mẩu code.

### 3. Dòng Lệnh Quản Trị Hệ Thống
- **`@[/context]`**: Mở bảng điểu khiển **Bộ Nhớ Ngầm (Persistent Memory)**. Xem xem hệ thống đang nhớ lỗi gì của ngày hôm qua.

---

## Các Kiến Trúc Đang Chạy Ngầm (Bạn nên biết)

Bạn không cần ra lệnh, nhưng hệ thống luôn ép AI phải tuân thủ điều này:

1. **Khóa Tệp (Mutex File Locking):** Khi 2 Agent cùng lao vào code, Supervisor sẽ cấp "Khóa cửa". Backend mượn file cấu hình thì Frontend phải đứng chờ. Chấm dứt cảnh AI xóa nhầm code của chính nó vừa viết ở dòng chat trước.
2. **Double Evidence (Bằng Chứng Kép):** Lời nói của AI là vô giá trị. Khi Agent xin nộp bài, hệ thống bắt nó phải kẹp kèm 1 trong 2 thứ: LOG báo Pass của Script, HOẶC Hình ảnh chụp màn hình trình duyệt Subagent tự chạy.
3. **Phân Luồng Tuần Tự (Sequential Back-first):** Nếu bạn yêu cầu làm 1 tính năng Fullstack từ số 0. `/orchestrate` sẽ đóng băng Frontend lại, bắt Backend viết xong Data, ra được API Schema, chốt file thì mới mở khóa cho Frontend vào gõ React. Tránh tình trạng UI gọi API chưa tồn tại.

---

## Ví Dụ Mẫu (Best Practices)

**Tình huống 1: Mới làm dự án to**
- **Bạn:** `@[/index-repo]` *(Chạy trước để AI lập bản đồ Source code).*
- **Bạn:** `@[/orchestrate] Làm cho anh tính năng Đăng Nhập bằng Google.` *(Nó tự biết gọi FE làm nút, BE làm api, DB làm cột).*

**Tình huống 2: Code đã xong, nhưng hay bị lỗi nhỏ lắt nhắt**
- **Bạn:** Cứ paste lỗi Terminal vào thẳng kênh chat. AI sẽ tự gọi `debugger` ra xử lý song song.

**Tình huống 3: Muốn áp dụng thư viện mới nhưng chưa biết**
- **Bạn:** `@[/research-protocol] Xem trên mạng xem hiện giờ giữa Drizzle ORM và Prisma cái nào tốc độ build nhanh hơn trong tháng 4/2026. Lập bảng so sánh.`

---

## Tổng Kết
Hệ thống này được sinh ra không chỉ để viết code nhanh, mà là viết code **Chống Lỗi Rườm Rà**, **Có Trí Nhớ Lâu Dài**, và **Làm Việc Theo Kỷ Luật Quân Đội**. Chúc bạn có những giờ phút đứng lớp chỉ huy 20 Đặc vụ AI nhàn nhã!
