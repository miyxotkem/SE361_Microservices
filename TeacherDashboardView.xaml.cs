using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace e_learning_app.Views
{
    public partial class TeacherDashboardView : UserControl
    {
        // ─── Models ──────────────────────────────────────────────────
        public class TodoItem
        {
            public string Text { get; set; }
            public string Deadline { get; set; }
            public bool IsDone { get; set; }
            public bool IsUrgent { get; set; }
        }

        public class ScheduleItem
        {
            public string Subject { get; set; }
            public string ClassName { get; set; }
            public string TimeSlot { get; set; }
            public string Room { get; set; }
            public int Students { get; set; }
            public string Status { get; set; }  // "ongoing" | "upcoming" | "afternoon"
        }

        // ─── State ───────────────────────────────────────────────────
        private List<TodoItem> _todos = new();
        private List<ScheduleItem> _schedule = new();
        private int _pendingGrading = 12;
        private int _totalStudents = 247;
        private int _totalClasses = 8;
        private double _avgScore = 7.8;

        // ─── Constructor ─────────────────────────────────────────────
        public TeacherDashboardView()
        {
            InitializeComponent();
            LoadData();
            UpdateStatCards();
        }

        // ─── Data loading ─────────────────────────────────────────────
        private void LoadData()
        {
            _todos = new List<TodoItem>
            {
                new() { Text="Chấm bài kiểm tra 1 tiết – 12A1", Deadline="Hôm nay",  IsDone=false, IsUrgent=true  },
                new() { Text="Cập nhật điểm danh tuần 14",       Deadline="05/04",    IsDone=false, IsUrgent=false },
                new() { Text="Soạn đề thi giữa kỳ – 11B3",       Deadline="Hoàn thành", IsDone=true,  IsUrgent=false },
                new() { Text="Gửi thông báo họp phụ huynh",      Deadline="07/04",    IsDone=false, IsUrgent=false },
            };

            _schedule = new List<ScheduleItem>
            {
                new() { Subject="Toán Giải Tích",   ClassName="12A1", TimeSlot="07:30 – 09:00",
                        Room="B201", Students=38, Status="ongoing"   },
                new() { Subject="Vật Lý Đại Cương", ClassName="11B3", TimeSlot="09:30 – 11:00",
                        Room="A105", Students=42, Status="upcoming"  },
                new() { Subject="Hóa Hữu Cơ",       ClassName="12C2", TimeSlot="13:00 – 14:30",
                        Room="C304", Students=35, Status="afternoon" },
            };
        }

        // ─── UI update helpers ────────────────────────────────────────
        private void UpdateStatCards()
        {
            // The XAML already has static values for demo.
            // In a real app, bind TextBlock x:Name properties here, e.g.:
            // TxtTotalStudents.Text  = _totalStudents.ToString();
            // TxtTotalClasses.Text   = _totalClasses.ToString();
            // TxtPendingGrading.Text = _pendingGrading.ToString();
            // TxtAvgScore.Text       = _avgScore.ToString("0.0");

            // Count pending tasks
            int pendingTasks = _todos.FindAll(t => !t.IsDone).Count;
            int todayClasses = _schedule.Count;
        }

        // ─── Event handlers ──────────────────────────────────────────

        /// <summary>Checkbox trong danh sách việc cần làm.</summary>
        private void TodoCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb) return;
            int index = (int)(cb.Tag ?? -1);
            if (index < 0 || index >= _todos.Count) return;

            _todos[index].IsDone = cb.IsChecked == true;

            // Update visual: strike-through text sibling
            if (cb.Parent is StackPanel sp && sp.Children.Count > 1
                && sp.Children[1] is StackPanel inner)
            {
                if (inner.Children[0] is TextBlock tb)
                    tb.TextDecorations = _todos[index].IsDone
                        ? TextDecorations.Strikethrough
                        : null;
            }
        }

        /// <summary>Nút "Tạo lớp học" trên header.</summary>
        private void BtnCreateClass_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to MyClassesView and open create dialog
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new MyClassesView());
        }

        /// <summary>Nút "Tạo bài kiểm tra" trên header.</summary>
        private void BtnCreateExam_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new QuestionBankView());
        }

        /// <summary>Click vào lịch dạy hôm nay → mở lớp học tương ứng.</summary>
        private void ClassScheduleItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string classId = btn.Tag?.ToString() ?? "";
            MessageBox.Show($"Mở lớp: {classId}\n(Tích hợp ClassDetailView tại đây)",
                            "Vào lớp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Link "Xem tất cả thông báo".</summary>
        private void BtnViewAllNotif_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.NavigateTo(new NotificationsView());
        }

        // ─── Public API ───────────────────────────────────────────────
        /// <summary>Cập nhật số bài chờ chấm từ ngoài.</summary>
        public void UpdatePendingCount(int count)
        {
            _pendingGrading = count;
            UpdateStatCards();
        }
    }
}
