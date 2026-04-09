using FirebaseIntegration;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace e_learning_app.Views
{
    public partial class TeacherDashboardView : UserControl
    {
        private readonly DatabaseManager _dbManager;
        // ── Models ───────────────────────────────────────────────────
        public class TodoItem
        {
            public int Index { get; set; }
            public string Text { get; set; }
            public string Deadline { get; set; }
            public bool IsDone { get; set; }
            public bool IsUrgent { get; set; }
        }

        public class ScheduleItem
        {
            public string ClassName { get; set; }
            public string Subject { get; set; }
            public string TimeSlot { get; set; }
            public string Room { get; set; }
            public int Students { get; set; }
            public string Status { get; set; }  // "ongoing" | "upcoming" | "afternoon"
            public string Color { get; set; }
        }

        public class NotifItem
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Time { get; set; }
            public bool IsUnread { get; set; }
        }

        // ── Data ─────────────────────────────────────────────────────
        private readonly List<TodoItem> _todos = new();
        private readonly List<ScheduleItem> _schedule = new();
        private readonly List<NotifItem> _notifs = new();

        private int _totalStudents = 247;
        private int _totalClasses = 8;
        private int _pendingGrading = 12;
        private double _avgScore = 7.8;

        // ── Constructor ──────────────────────────────────────────────
        public TeacherDashboardView(DatabaseManager dbManager)
        {
            InitializeComponent();
            LoadData();
            _dbManager = dbManager;
        }

        // ── Loaded ───────────────────────────────────────────────────
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RenderAll();
        }

        // ── Data ─────────────────────────────────────────────────────
        private void LoadData()
        {
            _todos.AddRange(new[]
            {
                new TodoItem { Index=0, Text="Chấm bài kiểm tra 1 tiết – 12A1", Deadline="Hôm nay",  IsDone=false, IsUrgent=true  },
                new TodoItem { Index=1, Text="Cập nhật điểm danh tuần 14",       Deadline="05/04",    IsDone=false, IsUrgent=false },
                new TodoItem { Index=2, Text="Soạn đề thi giữa kỳ – 11B3",       Deadline="Hoàn thành", IsDone=true, IsUrgent=false },
                new TodoItem { Index=3, Text="Gửi thông báo họp phụ huynh",      Deadline="07/04",    IsDone=false, IsUrgent=false },
            });

            _schedule.AddRange(new[]
            {
                new ScheduleItem { ClassName="12A1", Subject="Toán Giải Tích",    TimeSlot="🕐 07:30 – 09:00", Room="B201", Students=38, Status="ongoing",   Color="#3B82F6" },
                new ScheduleItem { ClassName="11B3", Subject="Vật Lý Đại Cương", TimeSlot="🕙 09:30 – 11:00", Room="A105", Students=42, Status="upcoming",  Color="#8B5CF6" },
                new ScheduleItem { ClassName="12C2", Subject="Hóa Hữu Cơ",       TimeSlot="🕑 13:00 – 14:30", Room="C304", Students=35, Status="afternoon", Color="#F59E0B" },
            });

            _notifs.AddRange(new[]
            {
                new NotifItem { Id=1, Title="Học sinh Minh Anh nộp bài trễ",     Time="2 phút trước", IsUnread=true  },
                new NotifItem { Id=2, Title="Lịch họp khoa đã được cập nhật",    Time="1 giờ trước",  IsUnread=false },
                new NotifItem { Id=3, Title="Kết quả thi học kỳ đã có",          Time="Hôm qua 15:30",IsUnread=false },
            });
        }

        // ── Render ───────────────────────────────────────────────────
        private void RenderAll()
        {
            // Header
            var hour = DateTime.Now.Hour;
            string greeting = hour < 12 ? "buổi sáng" : hour < 18 ? "buổi chiều" : "buổi tối";
            TxtGreeting.Text = $"👋  Chào {greeting}, Trọng Nguyên!";
            TxtTodayDate.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy");
            int pendingTasks = _todos.FindAll(t => !t.IsDone).Count;
            TxtTodayInfo.Text = $"Hôm nay bạn có {_schedule.Count} lớp học và {pendingTasks} việc cần làm.";

            // Stat cards
            TxtTotalStudents.Text = _totalStudents.ToString();
            TxtTotalClasses.Text = _totalClasses.ToString();
            TxtPendingGrading.Text = _pendingGrading.ToString();
            TxtAvgScore.Text = _avgScore.ToString("0.0");
            TxtStudentTrend.Text = "↑ 4%";
            TxtSemesterBadge.Text = "Học kỳ 2";
            TxtScoreTrend.Text = "↑ 0.3";

            // Schedule cards
            BindScheduleCard(0, BarClass1, TxtClass1Name, TxtClass1Time, TxtClass1Room, TxtClass1Students, PillClass1, TxtClass1Status);
            BindScheduleCard(1, BarClass2, TxtClass2Name, TxtClass2Time, TxtClass2Room, TxtClass2Students, PillClass2, TxtClass2Status);
            BindScheduleCard(2, BarClass3, TxtClass3Name, TxtClass3Time, TxtClass3Room, TxtClass3Students, PillClass3, TxtClass3Status);

            // Todo list
            RenderTodos();

            // Notifications
            RenderNotifs();
        }

        private void BindScheduleCard(int i, Border bar,
            TextBlock name, TextBlock time, TextBlock room, TextBlock students,
            Border pill, TextBlock status)
        {
            if (i >= _schedule.Count) return;
            var s = _schedule[i];
            var clr = (SolidColorBrush)new BrushConverter().ConvertFromString(s.Color)!;

            bar.Background = clr;
            name.Text = $"{s.Subject} – {s.ClassName}";
            time.Text = s.TimeSlot;
            room.Text = $"Phòng {s.Room}";
            students.Text = $"{s.Students} học sinh";

            var (bg, fg, label) = s.Status switch
            {
                "ongoing" => ("#DCFCE7", "#16A34A", "Đang diễn ra"),
                "upcoming" => ("#EDE9FE", "#7C3AED", "Sắp bắt đầu"),
                _ => ("#FEF3C7", "#D97706", "Chiều"),
            };
            pill.Background = (Brush)new BrushConverter().ConvertFromString(bg)!;
            status.Text = label;
            status.Foreground = (Brush)new BrushConverter().ConvertFromString(fg)!;
        }

        private void RenderTodos()
        {
            TodoPanel.Children.Clear();
            for (int i = 0; i < _todos.Count; i++)
            {
                var t = _todos[i];
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, i < _todos.Count - 1 ? 14 : 0)
                };

                var cb = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    IsChecked = t.IsDone,
                    Tag = i
                };
                cb.Checked += TodoCheckBox_Changed;
                cb.Unchecked += TodoCheckBox_Changed;

                var inner = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };

                var txtMain = new TextBlock
                {
                    Text = t.Text,
                    FontSize = 13,
                    Foreground = t.IsDone
                        ? new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8))
                        : new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)),
                    TextDecorations = t.IsDone ? TextDecorations.Strikethrough : null
                };
                var txtDead = new TextBlock
                {
                    Text = t.IsDone ? "Hoàn thành" : $"Hạn: {t.Deadline}",
                    FontSize = 11,
                    Foreground = t.IsDone
                        ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
                        : t.IsUrgent
                            ? new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44))
                            : new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B))
                };
                inner.Children.Add(txtMain);
                inner.Children.Add(txtDead);
                row.Children.Add(cb);
                row.Children.Add(inner);
                TodoPanel.Children.Add(row);
            }
        }

        private void RenderNotifs()
        {
            NotifPanel.Children.Clear();
            for (int i = 0; i < _notifs.Count; i++)
            {
                var n = _notifs[i];
                var isLast = i == _notifs.Count - 1;

                var btn = new Button
                {
                    Background = n.IsUnread
                        ? new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF))
                        : new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC)),
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Margin = new Thickness(0, 0, 0, isLast ? 0 : 10),
                    Tag = n.Id
                };
                btn.Click += BtnNotifItem_Click;

                var tpl = new ControlTemplate(typeof(Button));
                var f = new FrameworkElementFactory(typeof(Border));
                f.SetBinding(Border.BackgroundProperty,
                    new System.Windows.Data.Binding("Background")
                    { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
                f.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
                f.SetValue(Border.PaddingProperty, new Thickness(12));
                var cp = new FrameworkElementFactory(typeof(ContentPresenter));
                f.AppendChild(cp);
                tpl.VisualTree = f;
                btn.Template = tpl;

                var sp = new StackPanel();
                sp.Children.Add(new TextBlock
                {
                    Text = n.Title,
                    FontSize = 12,
                    FontWeight = n.IsUnread ? FontWeights.SemiBold : FontWeights.Normal,
                    Foreground = n.IsUnread
                        ? new SolidColorBrush(Color.FromRgb(0x1E, 0x40, 0xAF))
                        : new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69)),
                    TextWrapping = TextWrapping.Wrap
                });
                sp.Children.Add(new TextBlock
                {
                    Text = n.Time,
                    FontSize = 11,
                    Foreground = n.IsUnread
                        ? new SolidColorBrush(Color.FromRgb(0x93, 0xC5, 0xFD))
                        : new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8))
                });
                btn.Content = sp;
                NotifPanel.Children.Add(btn);
            }
        }

        // ── Event handlers ───────────────────────────────────────────
        private void BtnCreateClass_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new MyClassesView(_dbManager));
        }

        private void BtnCreateExam_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new QuestionBankView());
        }

        private void BtnScheduleClass_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string cls = btn.Tag?.ToString() ?? "";
            var item = _schedule.Find(s => s.ClassName == cls);
            if (item == null) return;
            MessageBox.Show(
                $"Mở lớp: {item.Subject} – {item.ClassName}\nPhòng {item.Room}  |  {item.TimeSlot}",
                "Vào lớp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TodoCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb || cb.Tag is not int idx) return;
            if (idx < 0 || idx >= _todos.Count) return;
            _todos[idx].IsDone = cb.IsChecked == true;
            RenderTodos();
        }

        private void BtnNotifItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            var n = _notifs.Find(x => x.Id == id);
            if (n == null) return;
            n.IsUnread = false;
            RenderNotifs();
            MessageBox.Show(n.Title, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnViewAllNotif_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new NotificationsView());
        }

        // ── Public API ───────────────────────────────────────────────
        public void UpdatePendingGrading(int count) { _pendingGrading = count; RenderAll(); }
    }
}
