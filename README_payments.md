# Thông tin Thẻ Test VNPay (Sandbox)

Dưới đây là thông tin thẻ test được cung cấp để thực hiện thanh toán thử nghiệm qua cổng thanh toán VNPay Sandbox:

| Trường thông tin | Giá trị kiểm thử (Test Value) |
| :--- | :--- |
| **Ngân hàng** | **NCB** |
| **Số thẻ** | `9704198526191432198` |
| **Tên chủ thẻ** | `NGUYEN VAN A` |
| **Ngày phát hành** | `07/15` |
| **Mật khẩu OTP** | `123456` |

---

### Hướng dẫn kiểm thử thanh toán VNPay:
1. Khi ứng dụng yêu cầu thanh toán khóa học, bạn sẽ được chuyển hướng đến trang thanh toán của VNPay Sandbox.
2. Chọn thanh toán qua **Thẻ nội địa và tài khoản ngân hàng**.
3. Chọn ngân hàng **NCB**.
4. Nhập các thông tin thẻ như bảng trên (Số thẻ, Tên chủ thẻ, Ngày phát hành).
5. Khi hệ thống hiển thị màn hình xác thực mã OTP, nhập mật khẩu OTP là `123456` để hoàn tất thanh toán thành công.

# Thông tin Thẻ Test MoMo (Sandbox)

Sử dụng Thử Nghiệm Thanh Toán ATM để hiểu cách hoạt động.

* **Bước 1**: Tạo yêu cầu thanh toán qua Cổng thanh toán MoMo.
* **Bước 2**: Chọn "Xác Nhận" => Điều hướng tới trang thanh toán của Ngân hàng/Napas tạo bởi MoMo.
* **Bước 3**: Sử dụng tài khoản test để thanh toán thử.

| No | Tên | Số thẻ | Hạn ghi trên thẻ | OTP | Trường hợp test |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | NGUYEN VAN A | 9704 0000 0000 0018 | 03/07 | OTP | Thành công |
| 2 | NGUYEN VAN A | 9704 0000 0000 0026 | 03/07 | OTP | Thẻ bị khóa |
| 3 | NGUYEN VAN A | 9704 0000 0000 0034 | 03/07 | OTP | Nguồn tiền không đủ |
| 4 | NGUYEN VAN A | 9704 0000 0000 0042 | 03/07 | OTP | Hạn mức thẻ |

