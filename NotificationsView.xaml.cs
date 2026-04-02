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
        // ── Model ────────────────────────────────────────────────────
        public enum NotifType { Submission, Schedule, Result, Achievement, Warning, System }

        public class Notification
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public string Source { get; set; }
            public DateTime Time { get; set; }
            public bool IsRead { get; set; }
            public bool IsUrgent { get; set; }
            public NotifType Type { get; set; }
        }

        // ── State ────────────────────────────────────────────────────
        private List<Notification> _all = new();
        private string _filterMode = "all";

        // ── Emoji / color per type ────────────────────────────────────
        static (string emoji, string bg) TypeStyle(NotifType t) => t switch
        {
            NotifType.Submission => ("📝", "#EFF6FF"),
            NotifType.Schedule => ("📅", "#FFF7ED"),
            NotifType.Result => ("✅", "#F1F5F9"),
            NotifType.Achievement => ("🎉", "#ECFDF5"),
            NotifType.Warning => ("⚠️", "#FFF1F2"),
            _ => ("⚙️", "#F1F5F9"),
        };

        static string RelTime(DateTime dt)
        {
            var d = DateTime.Now - dt;
            if (d.TotalMinutes < 1) return "Vừa xong";
            if (d.TotalMinutes < 60) return $"{(int)d.TotalMinutes} phút trước";
            if (d.TotalHours < 24) return $"{(int)d.TotalHours} giờ trước";
            if (d.TotalDays < 2) return "Hôm qua, " + dt.ToString("HH:mm");
            return dt.ToString("dd/MM/yyyy");
        }

        // ── Constructor ──────────────────────────────────────────────
        public NotificationsView() { InitializeComponent(); LoadSampleData(); }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => Refresh();

        // ── Sample data ──────────────────────────────────────────────
        private void LoadSampleData()
        {
            _all = new List<Notification>
            {
                new() { Id=1, Title="Học sinh nộp bài trễ hạn",
                        Body="Nguyễn Minh Anh (12A1) nộp bài tập chương 5 trễ 2 ngày.",
                        Source="Lớp 12A1", Time=DateTime.Now.AddMinutes(-2),
                        IsRead=false, IsUrgent=true,  Type=NotifType.Submission },
                new() { Id=2, Title="Lịch thi giữa kỳ đã được cập nhật",
                        Body="Phòng đào tạo đã cập nhật lịch thi giữa kỳ học kỳ 2.",
                        Source="Phòng đào tạo", Time=DateTime.Now.AddMinutes(-45),
                        IsRead=false, IsUrgent=false, Type=NotifType.Schedule },
                new() { Id=3, Title="Kết quả thi học kỳ 1 đã được phê duyệt",
                        Body="Điểm thi HK1 của tất cả lớp đã được phê duyệt và công bố.",
                        Source="Phòng đào tạo", Time=DateTime.Now.AddHours(-3),
                        IsRead=true,  IsUrgent=false, Type=NotifType.Result },
                new() { Id=4, Title="Học sinh đạt thành tích cao – HSG Toán",
                        Body="5 học sinh lớp 12A1 đạt giải kỳ thi HSG cấp thành phố môn Toán.",
                        Source="Ban giám hiệu", Time=DateTime.Now.AddDays(-1).AddHours(-7),
                        IsRead=true,  IsUrgent=false, Type=NotifType.Achievement },
                new() { Id=5, Title="Cảnh báo: Tỷ lệ vắng học 11B3 tăng cao",
                        Body="Tỷ lệ vắng học lớp 11B3 tuần này đạt 18%, cao hơn ngưỡng cho phép.",
                        Source="Hệ thống", Time=DateTime.Now.AddDays(-1).AddHours(-15),
                        IsRead=true,  IsUrgent=true,  Type=NotifType.Warning },
                new() { Id=6, Title="Nâng cấp hệ thống – Phiên bản 2.4.1",
                        Body="Hệ thống đã được nâng cấp. Một số tính năng mới đã được thêm vào.",
                        Source="Hệ thống", Time=DateTime.Now.AddDays(-2),
                        IsRead=true,  IsUrgent=false, Type=NotifType.System },
            };
        }

        // ── Refresh UI ───────────────────────────────────────────────
        private void Refresh()
        {
            // Update tab labels
            int unread = _all.Count(n => !n.IsRead);
            BtnFilterAll.Content = $"Tất cả  ({_all.Count})";
            BtnFilterUnread.Content = $"Chưa đọc  ({unread})";
            BtnFilterSent.Content = "Đã gửi";
            BtnFilterSystem.Content = "Hệ thống";

            // Update BtnMarkAllRead visibility
            BtnMarkAllRead.Visibility = unread > 0 ? Visibility.Visible : Visibility.Collapsed;

            // Build list
            var items = _filterMode switch
            {
                "unread" => _all.Where(n => !n.IsRead),
                "sent" => _all.Where(n => n.Type != NotifType.System),
                "system" => _all.Where(n => n.Type == NotifType.System),
                _ => _all.AsEnumerable()
            };

            NotifListPanel.Children.Clear();
            string lastSection = "";
            foreach (var n in items)
            {
                // Section header (Today / Yesterday / earlier)
                string section = (DateTime.Now - n.Time).TotalHours < 24
                    ? "Hôm nay"
                    : (DateTime.Now - n.Time).TotalHours < 48
                        ? "Hôm qua"
                        : n.Time.ToString("dd/MM/yyyy");

                if (section != lastSection)
                {
                    NotifListPanel.Children.Add(new TextBlock
                    {
                        Text = section,
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)),
                        Margin = new Thickness(0, lastSection == "" ? 0 : 16, 0, 10)
                    });
                    lastSection = section;
                }

                NotifListPanel.Children.Add(BuildNotifRow(n));
            }

            if (!items.Any())
                NotifListPanel.Children.Add(new TextBlock
                {
                    Text = "\n📭  Không có thông báo nào.",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
        }

        private Border BuildNotifRow(Notification n)
        {
            var (emoji, bgHex) = TypeStyle(n.Type);
            var cardBg = n.IsRead
                ? Color.FromRgb(0xFA, 0xFA, 0xFA)
                : Colors.White;
            var border = new Border
            {
                Background = new SolidColorBrush(cardBg),
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(18, 16, 18, 16),
                Margin = new Thickness(0, 0, 0, 10),
                BorderBrush = n.IsRead ? new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9)) : null,
                BorderThickness = n.IsRead ? new Thickness(1) : new Thickness(0),
            };
            if (!n.IsRead)
                border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                { BlurRadius = 12, ShadowDepth = 1, Opacity = 0.06, Color = Colors.Black };

            var root = new Grid();
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            root.ColumnDefinitions.Add(new ColumnDefinition());
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            var iconWrap = new Grid { Width = 46, Height = 46, Margin = new Thickness(0, 0, 16, 0) };
            var iconBg = new Border
            {
                CornerRadius = new CornerRadius(14),
                Background = (Brush)new BrushConverter().ConvertFromString(bgHex)!
            };
            var iconTxt = new TextBlock { Text = emoji, FontSize = 22, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            iconBg.Child = iconTxt;
            iconWrap.Children.Add(iconBg);
            if (!n.IsRead)
            {
                var dot = new Border
                {
                    Width = 10,
                    Height = 10,
                    CornerRadius = new CornerRadius(5),
                    Background = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, -2, -2, 0)
                };
                iconWrap.Children.Add(dot);
            }
            Grid.SetColumn(iconWrap, 0);

            // Content
            var content = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var titleRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            titleRow.Children.Add(new TextBlock
            {
                Text = n.Title,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = n.IsRead ? FontWeights.Normal : FontWeights.SemiBold,
                Foreground = n.IsRead ? new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69))
                                    : new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B))
            });
            if (n.IsUrgent)
            {
                var urgentPill = new Border
                {
                    CornerRadius = new CornerRadius(20),
                    Padding = new Thickness(8, 3, 8, 3),
                    Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)),
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                urgentPill.Child = new TextBlock
                {
                    Text = "Khẩn",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26))
                };
                titleRow.Children.Add(urgentPill);
            }
            content.Children.Add(titleRow);
            content.Children.Add(new TextBlock
            {
                Text = n.Body,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69))
            });
            content.Children.Add(new TextBlock
            {
                Text = $"{RelTime(n.Time)}  ·  {n.Source}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb((byte)(n.IsRead ? 0xCB : 0x94), (byte)(n.IsRead ? 0xD5 : 0xA3), (byte)(n.IsRead ? 0xE1 : 0xB8))),
                Margin = new Thickness(0, 6, 0, 0)
            });
            Grid.SetColumn(content, 1);

            // Action button
            var actionBtn = new Button
            {
                Content = n.IsUrgent ? "Xử lý" : "Xem",
                Background = n.IsUrgent ? new SolidColorBrush(Color.FromRgb(0xFF, 0xF1, 0xF2))
                                         : n.IsRead ? new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9))
                                                    : new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF)),
                Foreground = n.IsUrgent ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                                         : n.IsRead ? new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B))
                                                    : new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(16, 0, 0, 0),
                Tag = n.Id,
                FontSize = 12,
                FontWeight = n.IsUrgent ? FontWeights.SemiBold : FontWeights.Normal
            };
            var tpl = new ControlTemplate(typeof(Button));
            var f = new FrameworkElementFactory(typeof(Border));
            f.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background")
            { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            f.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            f.SetValue(Border.PaddingProperty, new Thickness(14, 7, 14, 7));
            f.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));
            tpl.VisualTree = f;
            actionBtn.Template = tpl;
            actionBtn.Click += BtnViewNotif_Click;
            Grid.SetColumn(actionBtn, 2);

            root.Children.Add(iconWrap);
            root.Children.Add(content);
            root.Children.Add(actionBtn);
            border.Child = root;
            return border;
        }

        // ── Filter buttons ───────────────────────────────────────────
        private void SetActiveTab(Button active)
        {
            foreach (var b in new[] { BtnFilterAll, BtnFilterUnread, BtnFilterSent, BtnFilterSystem })
            {
                b.Background = Brushes.Transparent;
                b.Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B));
                b.FontWeight = FontWeights.Normal;
            }
            active.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
            active.Foreground = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6));
            active.FontWeight = FontWeights.SemiBold;
        }

        // ── Event handlers ───────────────────────────────────────────
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            _filterMode = btn.Tag?.ToString() ?? "all";
            SetActiveTab(btn);
            Refresh();
        }

        private void BtnMarkAllRead_Click(object sender, RoutedEventArgs e)
        {
            foreach (var n in _all) n.IsRead = true;
            Refresh();
        }

        private void BtnViewNotif_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            var n = _all.FirstOrDefault(x => x.Id == id);
            if (n == null) return;
            n.IsRead = true;
            Refresh();
            MessageBox.Show($"{n.Title}\n\n{n.Body}\n\nNguồn: {n.Source}  |  {RelTime(n.Time)}",
                            "Chi tiết thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCreateNotif_Click(object sender, RoutedEventArgs e)
        {
            var w = new Window
            {
                Title = "Tạo thông báo mới",
                Width = 500,
                Height = 380,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC))
            };
            var sp = new StackPanel { Margin = new Thickness(28) };
            sp.Children.Add(new TextBlock { Text = "Tiêu đề:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
            var txtTitle = new TextBox
            {
                Height = 38,
                FontSize = 13,
                Padding = new Thickness(10, 0, 10, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0))
            };
            sp.Children.Add(txtTitle);
            sp.Children.Add(new TextBlock { Text = "Nội dung:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 14, 0, 6) });
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
            sp.Children.Add(txtBody);
            sp.Children.Add(new TextBlock { Text = "Gửi tới:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 14, 0, 6) });
            var cbTarget = new ComboBox
            {
                Height = 36,
                FontSize = 13,
                ItemsSource = new[] { "Tất cả học sinh", "Lớp 12A1", "Lớp 11B3", "Lớp 12C2", "Lớp 11A2" },
                SelectedIndex = 0
            };
            sp.Children.Add(cbTarget);

            var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 24, 0, 0) };
            var cancel = new Button
            {
                Content = "Hủy",
                Width = 90,
                Height = 38,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9)),
                Foreground = new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69)),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            cancel.Click += (_, _) => w.Close();
            var send = new Button
            {
                Content = "📤  Gửi",
                Width = 130,
                Height = 38,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                Foreground = Brushes.White,
                Margin = new Thickness(10, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontWeight = FontWeights.SemiBold
            };
            send.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtTitle.Text)) { MessageBox.Show("Vui lòng nhập tiêu đề."); return; }
                _all.Insert(0, new Notification
                {
                    Id = _all.Count + 1,
                    Title = txtTitle.Text,
                    Body = txtBody.Text,
                    Source = cbTarget.SelectedItem?.ToString() ?? "Tất cả",
                    Time = DateTime.Now,
                    IsRead = false,
                    IsUrgent = false,
                    Type = NotifType.System
                });
                Refresh(); w.Close();
            };
            btnRow.Children.Add(cancel); btnRow.Children.Add(send);
            sp.Children.Add(btnRow);
            w.Content = sp; w.ShowDialog();
        }

        public int UnreadCount => _all.Count(n => !n.IsRead);
    }
}
