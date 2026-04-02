using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace e_learning_app.Views
{
    public partial class NotificationsView : UserControl
    {
        // ─── Model ───────────────────────────────────────────────────
        public enum NotifType { Submission, Schedule, Result, Achievement, Warning, System }

        public class Notification
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public string Source { get; set; }   // e.g. "Lớp 12A1", "Hệ thống"
            public DateTime Time { get; set; }
            public bool IsRead { get; set; }
            public bool IsUrgent { get; set; }
            public NotifType Type { get; set; }
        }

        // ─── State ───────────────────────────────────────────────────
        private List<Notification> _allNotifs = new();
        private string _filterMode = "all";   // "all" | "unread" | "sent" | "system"

        // ─── Constructor ─────────────────────────────────────────────
        public NotificationsView()
        {
            InitializeComponent();
            LoadSampleData();
            UpdateFilterTabs();
        }

        // ─── Sample data ──────────────────────────────────────────────
        private void LoadSampleData()
        {
            _allNotifs = new List<Notification>
            {
                new() { Id=1, Title="Học sinh nộp bài trễ hạn",
                        Body="Nguyễn Minh Anh (12A1) đã nộp bài tập chương 5 trễ 2 ngày. Vui lòng xem xét và xử lý.",
                        Source="Lớp 12A1", Time=DateTime.Now.AddMinutes(-2),
                        IsRead=false, IsUrgent=true, Type=NotifType.Submission },

                new() { Id=2, Title="Nhắc nhở: Lịch thi giữa kỳ đã được cập nhật",
                        Body="Lịch thi giữa kỳ học kỳ 2 đã được phòng đào tạo cập nhật. Kiểm tra ngay để thông báo cho học sinh.",
                        Source="Phòng đào tạo", Time=DateTime.Now.AddMinutes(-45),
                        IsRead=false, IsUrgent=false, Type=NotifType.Schedule },

                new() { Id=3, Title="Kết quả thi học kỳ 1 đã được phê duyệt",
                        Body="Điểm thi học kỳ 1 của tất cả các lớp đã được phòng đào tạo phê duyệt và công bố chính thức.",
                        Source="Phòng đào tạo", Time=DateTime.Now.AddHours(-3),
                        IsRead=true, IsUrgent=false, Type=NotifType.Result },

                new() { Id=4, Title="Học sinh đạt thành tích cao trong kỳ thi HSG",
                        Body="5 học sinh lớp 12A1 đã đạt giải trong kỳ thi học sinh giỏi cấp thành phố môn Toán.",
                        Source="Ban giám hiệu", Time=DateTime.Now.AddDays(-1).AddHours(-7.7),
                        IsRead=true, IsUrgent=false, Type=NotifType.Achievement },

                new() { Id=5, Title="Cảnh báo: Tỷ lệ nghỉ học lớp 11B3 tăng cao",
                        Body="Tỷ lệ vắng học lớp 11B3 trong tuần này đạt 18%, cao hơn ngưỡng cho phép.",
                        Source="Hệ thống", Time=DateTime.Now.AddDays(-1).AddHours(-15),
                        IsRead=true, IsUrgent=true, Type=NotifType.Warning },

                new() { Id=6, Title="Cập nhật hệ thống – Phiên bản 2.4.1",
                        Body="Hệ thống đã được nâng cấp lên phiên bản mới. Một số tính năng mới đã được thêm vào.",
                        Source="Hệ thống", Time=DateTime.Now.AddDays(-2),
                        IsRead=true, IsUrgent=false, Type=NotifType.System },
            };
        }

        // ─── Filter & render ─────────────────────────────────────────
        private IEnumerable<Notification> GetFiltered()
        {
            return _filterMode switch
            {
                "unread" => _allNotifs.Where(n => !n.IsRead),
                "sent" => _allNotifs.Where(n => n.Type != NotifType.System),
                "system" => _allNotifs.Where(n => n.Type == NotifType.System),
                _ => _allNotifs
            };
        }

        private void UpdateFilterTabs()
        {
            int unreadCount = _allNotifs.Count(n => !n.IsRead);
            int totalCount = _allNotifs.Count;

            // Update button content with counts
            // BtnFilterAll.Content    = $"Tất cả  ({totalCount})";
            // BtnFilterUnread.Content = $"Chưa đọc  ({unreadCount})";
            // etc.
        }

        // ─── Helpers ─────────────────────────────────────────────────
        private static string RelativeTime(DateTime dt)
        {
            var diff = DateTime.Now - dt;
            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 2) return "Hôm qua, " + dt.ToString("HH:mm");
            return dt.ToString("dd/MM/yyyy");
        }

        private static (string emoji, string bgColor) GetNotifStyle(NotifType type) => type switch
        {
            NotifType.Submission => ("📝", "#EFF6FF"),
            NotifType.Schedule => ("📅", "#FFF7ED"),
            NotifType.Result => ("✅", "#F1F5F9"),
            NotifType.Achievement => ("🎉", "#ECFDF5"),
            NotifType.Warning => ("⚠️", "#FFF1F2"),
            NotifType.System => ("⚙️", "#F1F5F9"),
            _ => ("🔔", "#F1F5F9")
        };

        // ─── Event handlers ──────────────────────────────────────────

        /// <summary>Tab filter buttons (Tất cả / Chưa đọc / Đã gửi / Hệ thống).</summary>
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            _filterMode = btn.Tag?.ToString() ?? "all";
            UpdateFilterTabs();
            // Re-render notification list here in a full binding implementation
        }

        /// <summary>Click "Xem" / "Xử lý" trên từng thông báo.</summary>
        private void BtnViewNotif_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            var notif = _allNotifs.FirstOrDefault(n => n.Id == id);
            if (notif == null) return;

            // Mark as read
            notif.IsRead = true;
            UpdateFilterTabs();

            MessageBox.Show($"{notif.Title}\n\n{notif.Body}\n\nNguồn: {notif.Source}\n{RelativeTime(notif.Time)}",
                            "Chi tiết thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Đánh dấu tất cả đã đọc.</summary>
        private void BtnMarkAllRead_Click(object sender, RoutedEventArgs e)
        {
            foreach (var n in _allNotifs) n.IsRead = true;
            UpdateFilterTabs();
            MessageBox.Show("Đã đánh dấu tất cả thông báo là đã đọc.",
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Tạo thông báo mới.</summary>
        private void BtnCreateNotif_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Window
            {
                Title = "Tạo thông báo mới",
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC))
            };

            var root = new StackPanel { Margin = new Thickness(28) };

            root.Children.Add(new TextBlock { Text = "Tiêu đề:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
            var txtTitle = new TextBox
            {
                Height = 38,
                FontSize = 13,
                Padding = new Thickness(10, 0, 10, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0))
            };
            root.Children.Add(txtTitle);

            root.Children.Add(new TextBlock { Text = "Nội dung:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 14, 0, 6) });
            var txtBody = new TextBox
            {
                Height = 100,
                FontSize = 13,
                Padding = new Thickness(10, 8, 10, 8),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0))
            };
            root.Children.Add(txtBody);

            root.Children.Add(new TextBlock { Text = "Gửi tới:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 14, 0, 6) });
            var cbTarget = new ComboBox
            {
                Height = 36,
                FontSize = 13,
                ItemsSource = new[] { "Tất cả học sinh", "Lớp 12A1", "Lớp 11B3", "Lớp 12C2", "Lớp 11A2" },
                SelectedIndex = 0
            };
            root.Children.Add(cbTarget);

            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 24, 0, 0)
            };
            var btnCancel = new Button
            {
                Content = "Hủy",
                Width = 90,
                Height = 38,
                Background = new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9)),
                Foreground = new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69)),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnCancel.Click += (_, _) => dlg.Close();

            var btnSend = new Button
            {
                Content = "📤  Gửi thông báo",
                Width = 150,
                Height = 38,
                Background = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(10, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnSend.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text))
                { MessageBox.Show("Vui lòng nhập tiêu đề thông báo."); return; }

                _allNotifs.Insert(0, new Notification
                {
                    Id = _allNotifs.Count + 1,
                    Title = txtTitle.Text,
                    Body = txtBody.Text,
                    Source = cbTarget.SelectedItem?.ToString() ?? "Tất cả",
                    Time = DateTime.Now,
                    IsRead = false,
                    IsUrgent = false,
                    Type = NotifType.System
                });
                UpdateFilterTabs();
                dlg.Close();
                MessageBox.Show("Thông báo đã được gửi thành công!",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            btnRow.Children.Add(btnCancel);
            btnRow.Children.Add(btnSend);
            root.Children.Add(btnRow);

            dlg.Content = root;
            dlg.ShowDialog();
        }

        /// <summary>Xóa thông báo.</summary>
        private void BtnDeleteNotif_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            var notif = _allNotifs.FirstOrDefault(n => n.Id == id);
            if (notif == null) return;

            var res = MessageBox.Show($"Xóa thông báo: \"{notif.Title}\"?",
                                      "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                _allNotifs.Remove(notif);
                UpdateFilterTabs();
            }
        }

        // ─── Public API ───────────────────────────────────────────────
        public int UnreadCount => _allNotifs.Count(n => !n.IsRead);
    }
}
