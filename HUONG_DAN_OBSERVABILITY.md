# Hướng Dẫn Kiểm Thử & Giám Sát Hệ Thống Bằng Công Cụ Observability & RabbitMQ

Tài liệu này cung cấp các kịch bản kiểm thử (Test Scenarios) thực tế và hướng dẫn chi tiết cách sử dụng các công cụ giám sát: **RabbitMQ Management Dashboard**, **Jaeger**, **Prometheus**, **Loki**, và **Grafana** để theo dõi, đo lường hiệu năng và phân tích lỗi của hệ thống Microservices **SmartEdu**.

---

## 1. Địa Chỉ Truy Cập Các Công Cụ Giám Sát

Khi chạy hệ thống thông qua Docker Compose, hãy truy cập các công cụ bằng các đường dẫn sau:

| Công cụ | Địa chỉ truy cập (URL) | Tài khoản mặc định | Mục đích chính |
| :--- | :--- | :--- | :--- |
| **Yarp API Gateway** | `http://localhost:7000` | - | Điểm nhận HTTP Request |
| **RabbitMQ Management** | `http://localhost:15672` | `guest` / `guest` | Giám sát hàng đợi tin nhắn (Message Queue) |
| **Jaeger UI** | `http://localhost:16686` | - | Giám sát vết cuộc gọi phân tán (Tracing) |
| **Prometheus UI** | `http://localhost:9090` | - | Truy vấn các chỉ số hiệu năng (Metrics) |
| **Grafana Dashboard** | `http://localhost:3000` | `admin` / `admin` | Trực quan hóa Metrics & Logs tập trung |

---

## 2. Kịch Bản Kiểm Thử 1: Luồng Thanh Toán Đồng Bộ & Không Đồng Bộ (E2E Payment Flow)

**Mục tiêu**: Kiểm tra tính đúng đắn của luồng thanh toán khi có sự kết hợp giữa **HTTP Request (API)**, **Message Broker (RabbitMQ)**, **gRPC** và **NoSQL Database (Firestore)**.

```
Postman (Client) ──> Yarp Gateway (7000) ──> Payment.API (Postgres)
                                                 │
                                           (Publish Event)
                                                 │
                                                 ▼
                                            [RabbitMQ]
                                           /          \
                                 (Consume)              (Consume)
                                    /                        \
                                   ▼                          ▼
                           Course.API (Firestore)     Notification.API (gRPC & Firestore)
```

### Bước 1.1: Gửi Request tạo thanh toán từ Postman
1. Đăng nhập tài khoản Học sinh trên Postman để lấy `token`.
2. Chạy request **1. Khởi tạo thanh toán VNPay Sandbox** (hoặc PayPal).
3. Ghi nhận mã **`CorrelationId`** trả về ở kết quả (ví dụ: `8a7f23bb-bc2f-4c12-9214-411a00a12e34`). Đây chính là mã định danh giao dịch liên thông.

---

### Bước 1.2: Giám sát trên RabbitMQ Management Console
1. Truy cập `http://localhost:15672` và đăng nhập (`guest`/`guest`).
2. Chọn tab **Exchanges**:
   * Kiểm tra xem các Exchange dạng Event của MassTransit đã được tự động tạo chưa, ví dụ: `BuildingBlocks.Messaging.Events:PaymentInitiatedEvent`, `BuildingBlocks.Messaging.Events:PaymentCompletedEvent`.
3. Chọn tab **Queues**:
   * Kiểm tra các hàng đợi (Queues) nhận tin nhắn của các service:
     * `course-api-payment-completed` (Đăng ký học viên sau khi thanh toán).
     * `notification-api-payment-completed` (Gửi thông báo sau khi thanh toán).
4. **Giả lập sự kiện Thanh toán thành công**:
   * Quay lại Postman, chạy request **3. Giả lập VNPay Webhook (Thanh toán thành công)** bằng cách truyền đúng `transactionId` là mã `CorrelationId` đã lấy ở Bước 1.1.
5. F5 lại trang **Queues** của RabbitMQ:
   * Bạn sẽ thấy biểu đồ sóng hiển thị số lượng tin nhắn (Message Rate) tăng lên 1 ở cột `Publish` và giảm ngay về 0 ở cột `Deliver` (nghĩa là tin nhắn đã được gửi và các consumer đã tiêu thụ thành công lập tức).

---

### Bước 1.3: Phân tích vết phân tán trên Jaeger
1. Truy cập `http://localhost:16686`.
2. Tại khung **Search** bên trái:
   * **Service**: Chọn `yarpapigateway` hoặc `Payment.API`.
   * **Operation**: Chọn `POST api/payment/webhook/{method}`.
   * Nhấn **Find Traces**.
3. Chọn Trace vừa thực hiện ở Bước 1.2 để xem biểu đồ Gantt Chart phân tán:
   * Bạn sẽ thấy vết bắt đầu từ Yarp Gateway -> đi vào controller của `Payment.API`.
   * Bên dưới có Span tên là `PaymentCompletedEvent` (biểu thị sự kiện publish lên RabbitMQ).
   * Tiếp đó là hai nhánh con chạy **song song**:
     * Một nhánh thuộc `Course.API` (Consume `PaymentCompletedEvent` -> thực hiện lưu thông tin học viên vào Firebase Firestore).
     * Một nhánh thuộc `Notification.API` (Consume `PaymentCompletedEvent` -> gọi gRPC sang `Course.API` lấy thông tin và lưu log thông báo trên Firestore).

---

### Bước 1.4: Truy vấn Log liên thông trên Grafana Loki
1. Truy cập `http://localhost:3000` (đăng nhập `admin`/`admin`).
2. Chọn biểu tượng la bàn **Explore** ở menu bên trái -> Chọn data source là **Loki**.
3. Sao chép mã **`TraceId`** từ Jaeger (nằm ở góc trên cùng bên phải của trang chi tiết trace trong Jaeger).
4. Nhập câu lệnh LogQL sau vào ô tìm kiếm của Loki để xem toàn bộ log của tất cả các service liên quan đến request này:
    ```logql
    {service_name=~".+"} | trace_id="MÃ_TRACE_ID_CỦA_BẠN"
    ```
5. Bấm **Run query**. Bạn sẽ thấy các log của `Payment.API`, `Course.API`, và `Notification.API` hiển thị đan xen theo trình tự thời gian thực tế, giúp bạn đọc hiểu chính xác luồng dữ liệu chạy qua code.

---

## 3. Kịch Bản Kiểm Thử 2: Kiểm Tra Tải & Phân Tích Metrics (Load Test & Performance Metrics)

**Mục tiêu**: Kiểm tra khả năng chịu tải của hệ thống, đo lường thời gian phản hồi (Latency) và giám sát lượng sử dụng tài nguyên (CPU, RAM) của các container.

### Bước 2.1: Khởi động Load Test
1. Mở terminal tại thư mục gốc của project (nơi có file `loadtest.js`).
2. Chạy lệnh kiểm thử tải bằng **k6** (nếu máy bạn đã cài k6) hoặc giả lập lặp lại nhiều request bằng Runner của Postman:
   ```bash
   k6 run loadtest.js
   ```
   *(Kịch bản k6 này sẽ tạo ra 50 người dùng giả lập truy cập liên tục vào hệ thống trong vòng 2 phút).*

---

### Bước 2.2: Truy vấn số liệu thời gian thực trên Prometheus UI
1. Truy cập `http://localhost:9090`.
2. Gõ các câu lệnh truy vấn (PromQL) sau vào ô tìm kiếm để phân tích hiệu năng:
   * **Đo lường thời gian phản hồi trung bình (HTTP Latency)** của dịch vụ Course API:
     ```promql
     rate(http_server_request_duration_seconds_sum{exported_job="Course.API"}[1m]) 
     / 
     rate(http_server_request_duration_seconds_count{exported_job="Course.API"}[1m])
     ```
   * **Tính số lượng Request mỗi giây (RPS)** đi qua API Gateway:
     ```promql
     rate(http_server_request_duration_seconds_count{exported_job="YarpApiGateway"}[1m])
     ```
   * **Kiểm tra số lượng kết nối đang hoạt động tới Kestrel (Active Connections)** của các container:
     ```promql
     kestrel_active_connections
     ```
3. Chọn tab **Graph** để xem biểu đồ thay đổi tải lượng và tài nguyên trực quan trong suốt quá trình chạy k6.

---

### Bước 2.3: Giám sát Dashboard tập trung trên Grafana
1. Truy cập `http://localhost:3000`.
2. Chọn **Dashboards** -> chọn **Import**.
3. Nhập ID Dashboard của cộng đồng OpenTelemetry (ví dụ: nhập ID `17706` hoặc `20568` để giám sát các ứng dụng .NET Core qua OpenTelemetry) -> Nhấn **Load**.
4. Chọn Data Source là **Prometheus** -> Nhấn **Import**.
5. Màn hình Dashboard trực quan sẽ xuất hiện hiển thị:
   * Tỉ lệ CPU/RAM sử dụng của từng microservice.
   * Số lượng luồng (Active Threads) đang hoạt động.
   * Tỉ lệ lỗi HTTP (`2xx`, `4xx`, `5xx`).
   * Phân bổ thời gian phản hồi ở phân vị P95 và P99.

---

## 4. Kịch Bản Kiểm Thử 3: Điều Tra Lỗi Hệ Thống (Troubleshooting a Fail Scenario)

**Mục tiêu**: Giả lập lỗi kết nối cơ sở dữ liệu hoặc lỗi cấu hình để học cách sử dụng Observability tìm ra dòng code gây crash.

### Bước 3.1: Giả lập lỗi
1. Tạm dừng container Redis để làm đứt kết nối của các dịch vụ:
   ```bash
   docker compose stop redis
   ```
2. Mở Postman, thực hiện gọi request **1. Lấy danh sách khóa học** (API `GET /api/courses`).
3. Request sẽ quay đều và trả về lỗi **HTTP 500 (Internal Server Error)** sau khoảng 5 giây.

---

### Bước 3.2: Định vị lỗi bằng Jaeger
1. Mở Jaeger (`http://localhost:16686`), tìm các trace lỗi màu đỏ (Error traces) của dịch vụ `Course.API`.
2. Nhấp vào trace bị lỗi:
   * Bạn sẽ thấy Span của API `GET /api/courses` hiển thị màu đỏ cùng nhãn biểu tượng dấu chấm than.
   * Nhấp vào Span đó và xem mục **Tags** và **Logs**:
     * Bạn sẽ thấy thuộc tính `error = true`.
     * Trong mục Logs hiển thị thông báo: `StackExchange.Redis.RedisConnectionException: It was not possible to connect to the redis server(s)`.

---

### Bước 3.3: Điều tra chi tiết lỗi bằng Loki
1. Copy `TraceId` của request lỗi từ Jaeger.
2. Vào Grafana Loki (`http://localhost:3000` -> Explore -> Loki).
3. Nhập câu lệnh lọc theo Trace ID:
   ```logql
   {service_name="Course.API"} | trace_id="MÃ_TRACE_ID"
   ```
4. Loki sẽ trả về chính xác dòng log xảy ra lỗi kèm theo Stack Trace đầy đủ của C#, chỉ ra lỗi xảy ra tại hàm `Handle` của `GetAllCoursesQueryHandler.cs` khi gọi lệnh:
   ```csharp
   var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
   ```
5. Khởi động lại Redis để hệ thống hoạt động bình thường trở lại:
   ```bash
   docker compose start redis
   ```

---

## 5. Các Mẹo Cần Lưu Ý Khi Kiểm Thử
* **Correlation ID**: Trong các sự kiện gửi qua RabbitMQ (MassTransit), luôn giữ thuộc tính `CorrelationId` nhất quán. Điều này giúp Jaeger tự động nhóm các sự kiện không đồng bộ vào chung một Trace duy nhất.
* **Trace-Log Correlation**: Thư viện Serilog trong dự án đã cấu hình tự động chèn `trace_id` và `span_id` vào mọi dòng log ghi ra. Do đó, bạn luôn có thể tìm kiếm chéo giữa Jaeger và Loki thông qua `TraceId`.
* **Clean data trên RabbitMQ**: Nếu hàng đợi RabbitMQ bị tắc nghẽn tin nhắn cũ trong quá trình test, bạn có thể vào tab **Queues** -> chọn hàng đợi bị kẹt -> cuộn xuống dưới cùng chọn **Purge Messages** để xóa sạch tin nhắn rác.
