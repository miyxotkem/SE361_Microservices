using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace e_learning_app
{
    public partial class TeachingScheduleView : UserControl
    {
        public class ScheduleEvent
        {
            public int DayOfWeek { get; set; } // 1: Monday, ..., 7: Sunday
            public string Subject { get; set; }
            public string ClassName { get; set; }
            public string Room { get; set; }
            public string Time { get; set; }
            public string ColorHex { get; set; }
        }

        private List<ScheduleEvent> _mockSchedule = new();

        public TeachingScheduleView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMockData();
            RenderTimetable();
        }

        private void LoadMockData()
        {
            // Create some fake classes scattered across the week
            _mockSchedule = new List<ScheduleEvent>
            {
                // Monday
                new() { DayOfWeek = 1, Subject = "Toán Giải Tích", ClassName = "12A1", Room = "B201", Time = "07:30 - 09:00", ColorHex = "#DBEAFE" },
                new() { DayOfWeek = 1, Subject = "Toán Hình Học", ClassName = "12A2", Room = "B202", Time = "09:30 - 11:00", ColorHex = "#DBEAFE" },
                
                // Tuesday
                new() { DayOfWeek = 2, Subject = "Vật Lý Đại Cương", ClassName = "11B3", Room = "A105", Time = "13:00 - 14:30", ColorHex = "#F3E8FF" },
                new() { DayOfWeek = 2, Subject = "Vật Lý Đại Cương", ClassName = "11B4", Room = "A106", Time = "15:00 - 16:30", ColorHex = "#F3E8FF" },
                
                // Wednesday
                new() { DayOfWeek = 3, Subject = "Hóa Hữu Cơ", ClassName = "12C2", Room = "C304", Time = "07:30 - 09:00", ColorHex = "#FEF3C7" },
                
                // Thursday
                new() { DayOfWeek = 4, Subject = "Toán Giải Tích", ClassName = "12A1", Room = "B201", Time = "09:30 - 11:00", ColorHex = "#DBEAFE" },
                new() { DayOfWeek = 4, Subject = "Toán Hình Học", ClassName = "12A2", Room = "B202", Time = "13:00 - 14:30", ColorHex = "#DBEAFE" },
                new() { DayOfWeek = 4, Subject = "Sinh Học Phân Tử", ClassName = "11A2", Room = "C101", Time = "15:00 - 16:30", ColorHex = "#D1FAE5" },

                // Friday
                new() { DayOfWeek = 5, Subject = "Vật Lý Nâng Cao", ClassName = "12B1", Room = "Lab Lý", Time = "07:30 - 09:45", ColorHex = "#FCE7F3" },
                new() { DayOfWeek = 5, Subject = "Hóa Phân Tích", ClassName = "12C1", Room = "Lab Hóa", Time = "13:00 - 15:15", ColorHex = "#FEF3C7" },

                // Saturday
                new() { DayOfWeek = 6, Subject = "Ôn thi Đại học", ClassName = "Lớp bồi dưỡng", Room = "Hội trường", Time = "18:00 - 20:30", ColorHex = "#FEE2E2" }
                
                // Sunday empty
            };
        }

        private void RenderTimetable()
        {
            // Clear current items
            IcMonday.Items.Clear();
            IcTuesday.Items.Clear();
            IcWednesday.Items.Clear();
            IcThursday.Items.Clear();
            IcFriday.Items.Clear();
            IcSaturday.Items.Clear();
            IcSunday.Items.Clear();

            var columns = new[] { IcMonday, IcTuesday, IcWednesday, IcThursday, IcFriday, IcSaturday, IcSunday };

            // Group by day and order by time
            for (int day = 1; day <= 7; day++)
            {
                var eventsForDay = _mockSchedule
                    .Where(e => e.DayOfWeek == day)
                    .OrderBy(e => e.Time)
                    .ToList();

                foreach (var ev in eventsForDay)
                {
                    var card = BuildScheduleCard(ev);
                    columns[day - 1].Items.Add(card);
                }
            }
        }

        private UIElement BuildScheduleCard(ScheduleEvent ev)
        {
            var border = new Border
            {
                Background = (Brush)new BrushConverter().ConvertFromString(ev.ColorHex),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 12),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var stack = new StackPanel();

            // Time
            var txtTime = new TextBlock
            {
                Text = ev.Time,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69)),
                Margin = new Thickness(0, 0, 0, 8)
            };
            stack.Children.Add(txtTime);

            // Subject
            var txtSubject = new TextBlock
            {
                Text = ev.Subject,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 4)
            };
            stack.Children.Add(txtSubject);

            // Details (Class & Room)
            var detailStack = new StackPanel { Orientation = Orientation.Horizontal };
            var txtClass = new TextBlock
            {
                Text = $"🎓 {ev.ClassName}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)),
                Margin = new Thickness(0, 0, 8, 0)
            };
            var txtRoom = new TextBlock
            {
                Text = $"📍 P.{ev.Room}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B))
            };
            detailStack.Children.Add(txtClass);
            detailStack.Children.Add(txtRoom);

            stack.Children.Add(detailStack);
            border.Child = stack;

            // Click event
            border.MouseLeftButtonUp += (s, e) =>
            {
                MessageBox.Show(
                    $"Thông tin lớp học:\n\nMôn: {ev.Subject}\nLớp: {ev.ClassName}\nPhòng: {ev.Room}\nGiờ học: {ev.Time}",
                    "Chi tiết Lịch Dạy", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            return border;
        }

        private void BtnPrevWeek_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đang được phát triển...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnNextWeek_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng đang được phát triển...", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                pd.PrintVisual(this, "Thời khóa biểu giảng dạy");
            }
        }
    }
}
