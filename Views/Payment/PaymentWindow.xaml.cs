using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using e_learning_app;
using e_learning_app.Class;

namespace e_learning_app.Views.Payment
{
    public partial class PaymentWindow : Window
    {
        private string _courseId;
        private string _userId;
        private decimal _amount;
        private DatabaseManager _db;
        private System.Net.HttpListener _currentListener;

        public PaymentWindow(string courseId, string courseName, decimal amount, string userId)
        {
            InitializeComponent();
            _courseId = courseId;
            _userId = userId;
            _amount = amount;
            _db = new DatabaseManager();

            txtCourseName.Text = courseName;
            txtAmount.Text = amount.ToString("N0") + " VNĐ";
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_currentListener != null && _currentListener.IsListening)
            {
                try { _currentListener.Stop(); _currentListener.Close(); } catch { }
            }
            base.OnClosed(e);
        }

        private async void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            var voucher = txtVoucher.Text.Trim();
            
            string paymentMethod = "VNPay";
            if (rbMoMo.IsChecked == true) paymentMethod = "MoMo";
            else if (rbPayPal.IsChecked == true) paymentMethod = "PayPal";

            try
            {
                // Hủy phiên nghe trước đó nếu user bấm lại
                if (_currentListener != null && _currentListener.IsListening)
                {
                    try { _currentListener.Abort(); } catch { }
                    _currentListener = null;
                }

                if (sender is Button btn) btn.IsEnabled = false;

                // Tìm port trống động
                int port = 8080;
                var tcpListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
                tcpListener.Start();
                port = ((System.Net.IPEndPoint)tcpListener.LocalEndpoint).Port;
                tcpListener.Stop();
                
                string localReturnUrl = $"http://127.0.0.1:{port}/payment-return/";
                
                var paymentUrl = await _db.CreatePaymentAsync(_courseId, _userId, _amount, paymentMethod, voucher, localReturnUrl);
                
                if (!string.IsNullOrEmpty(paymentUrl))
                {
                    txtMessage.Text = "Đang chờ thanh toán trên trình duyệt... (Bấm nút lại nếu lỡ tắt trang web)";
                    
                    _currentListener = new System.Net.HttpListener();
                    _currentListener.Prefixes.Add(localReturnUrl);
                    _currentListener.Start();

                    // Bật lại nút để user có thể nhấn lại
                    if (sender is Button btn2) btn2.IsEnabled = true;

                    // Open browser for payment
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = paymentUrl,
                        UseShellExecute = true
                    });

                    // Bắt kết quả trả về từ trình duyệt
                    var context = await _currentListener.GetContextAsync();
                    var req = context.Request;
                    var res = context.Response;

                    // Lấy mã phản hồi tùy theo cổng thanh toán
                    bool isSuccess = false;
                    string transactionId = "";

                    if (paymentMethod == "VNPay")
                    {
                        string responseCode = req.QueryString["vnp_ResponseCode"];
                        isSuccess = responseCode == "00";
                        transactionId = req.QueryString["vnp_TxnRef"] ?? "";
                    }
                    else if (paymentMethod == "MoMo")
                    {
                        string resultCode = req.QueryString["resultCode"];
                        isSuccess = resultCode == "0";
                        transactionId = req.QueryString["orderId"] ?? "";
                    }

                    // Trả về HTML cho trình duyệt
                    string responseString = isSuccess 
                        ? "<html><head><meta charset=\"UTF-8\"></head><body style='font-family:sans-serif;text-align:center;margin-top:50px;'><h1 style='color:green;'>Thanh toán thành công!</h1><p>Vui lòng đóng tab này và quay lại ứng dụng.</p></body></html>"
                        : "<html><head><meta charset=\"UTF-8\"></head><body style='font-family:sans-serif;text-align:center;margin-top:50px;'><h1 style='color:red;'>Thanh toán thất bại!</h1><p>Vui lòng đóng tab này và quay lại ứng dụng.</p></body></html>";
                    
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    res.ContentLength64 = buffer.Length;
                    await res.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    res.Close();
                    
                    try { _currentListener.Abort(); } catch { }

                    if (isSuccess)
                    {
                        try 
                        {
                            var payload = new { UserId = _userId, CourseId = _courseId, TransactionId = transactionId, Amount = _amount };
                            string webhookPath = paymentMethod == "VNPay" ? "payment/webhook/VNPay" : "payment/webhook/MoMo";
                            await ApiService.PostAsync(webhookPath, payload);
                        } 
                        catch (Exception ex) 
                        {
                            // Webhook trigger failed but payment was successful
                            Console.WriteLine("Webhook trigger failed: " + ex.Message);
                        }
                        
                        // Bỏ popup thông báo để giảm bớt click, chỉ cần tự động tắt và refresh
                        // CustomDialog.Show("Thanh toán thành công! Giao dịch hoàn tất.", "Thông báo", DialogType.Success);
                        
                        // Mainactive cái màn hình chính lên
                        Application.Current.MainWindow?.Activate();
                        
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        txtMessage.Text = "Lỗi: Thanh toán thất bại hoặc đã bị hủy.";
                    }
                }
                else
                {
                    txtMessage.Text = "Lỗi: Không thể tạo yêu cầu thanh toán.";
                }
            }
            catch (System.Net.HttpListenerException ex)
            {
                // Nếu bị lỗi "Access is denied" lúc Start, hiện ra
                if (ex.ErrorCode != 995) // 995 = The I/O operation has been aborted
                {
                    txtMessage.Text = "Lỗi mạng cục bộ: " + ex.Message;
                }
            }
            catch (ObjectDisposedException)
            {
                // Bị hủy do đóng cửa sổ hoặc tạo request mới, bỏ qua lỗi này
            }
            catch (Exception ex)
            {
                txtMessage.Text = "Lỗi: " + ex.Message;
            }
            finally
            {
                // Đảm bảo luôn bật lại nút dù bất cứ lỗi gì xảy ra
                if (sender is Button btnFinal) btnFinal.IsEnabled = true;
            }
        }
    }
}
