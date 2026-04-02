using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace e_learning_app.Views
{
    public partial class ReportsView : UserControl
    {
        // ─── Models ──────────────────────────────────────────────────
        public class ClassReport
        {
            public string Subject      { get; set; }
            public string ClassName    { get; set; }
            public string AccentColor  { get; set; }
            public int    StudentCount { get; set; }
            public double AvgScore     { get; set; }
            public double PassRate     { get; set; }   // %
            public double ExcellentRate{ get; set; }   // %
            public string Semester     { get; set; }
        }

        public class StudentRank
        {
            public int    Rank        { get; set; }
            public string Name        { get; set; }
            public string ClassName   { get; set; }
            public double AvgScore    { get; set; }
            public string Medal       { get; set; }
        }

        public class ScoreDistribution
        {
            public string Range  { get; set; }
            public int    Count  { get; set; }
            public double Pct    { get; set; }
            public string Color  { get; set; }
        }

        // ─── State ───────────────────────────────────────────────────
        private List<ClassReport>       _reports      = new();
        private List<StudentRank>       _topStudents  = new();
        private List<ScoreDistribution> _distribution = new();
        private string _currentSemester = "Học kỳ 2 – 2024";

        // ─── Constructor ─────────────────────────────────────────────
        public ReportsView()
        {
            InitializeComponent();
            LoadData();
        }

        // ─── Data loading ─────────────────────────────────────────────
        private void LoadData()
        {
            _reports = new List<ClassReport>
            {
                new() { Subject="Toán Giải Tích",    ClassName="12A1", AccentColor="#3B82F6",
                        StudentCount=38, AvgScore=7.9, PassRate=89, ExcellentRate=24, Semester="Học kỳ 2 – 2024" },
                new() { Subject="Vật Lý Đại Cương",  ClassName="11B3", AccentColor="#8B5CF6",
                        StudentCount=42, AvgScore=7.4, PassRate=83, ExcellentRate=19, Semester="Học kỳ 2 – 2024" },
                new() { Subject="Hóa Hữu Cơ",        ClassName="12C2", AccentColor="#F59E0B",
                        StudentCount=35, AvgScore=8.1, PassRate=91, ExcellentRate=28, Semester="Học kỳ 2 – 2024" },
                new() { Subject="Sinh Học Phân Tử",   ClassName="11A2", AccentColor="#10B981",
                        StudentCount=40, AvgScore=7.6, PassRate=85, ExcellentRate=20, Semester="Học kỳ 2 – 2024" },
            };

            _topStudents = new List<StudentRank>
            {
                new() { Rank=1, Name="Nguyễn Minh Anh",  ClassName="12A1", AvgScore=9.7, Medal="🥇" },
                new() { Rank=2, Name="Trần Bảo Châu",    ClassName="12C2", AvgScore=9.4, Medal="🥈" },
                new() { Rank=3, Name="Lê Quốc Hùng",     ClassName="11B3", AvgScore=9.2, Medal="🥉" },
                new() { Rank=4, Name="Phạm Thị Lan",     ClassName="11A2", AvgScore=9.0, Medal="" },
                new() { Rank=5, Name="Đỗ Minh Tuấn",     ClassName="12A1", AvgScore=8.9, Medal="" },
            };

            _distribution = new List<ScoreDistribution>
            {
                new() { Range="9–10",  Count=55,  Pct=22, Color="#22C55E" },
                new() { Range="7–8.9", Count=89,  Pct=36, Color="#3B82F6" },
                new() { Range="5–6.9", Count=69,  Pct=28, Color="#F59E0B" },
                new() { Range="3–4.9", Count=22,  Pct=9,  Color="#FB923C" },
                new() { Range="0–2.9", Count=12,  Pct=5,  Color="#EF4444" },
            };
        }

        // ─── Computed summary ─────────────────────────────────────────
        private double OverallAvg   => _reports.Any() ? _reports.Average(r => r.AvgScore)      : 0;
        private double OverallPass  => _reports.Any() ? _reports.Average(r => r.PassRate)      : 0;
        private double OverallExcel => _reports.Any() ? _reports.Average(r => r.ExcellentRate) : 0;
        private int    TotalStudents=> _reports.Sum(r => r.StudentCount);
        private int    WeakStudents => _distribution.Where(d => d.Range is "3–4.9" or "0–2.9").Sum(d => d.Count);

        // ─── Event handlers ──────────────────────────────────────────

        /// <summary>Chọn học kỳ từ dropdown.</summary>
        private void SemesterSelector_Click(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu();
            foreach (var sem in new[] { "Học kỳ 2 – 2024", "Học kỳ 1 – 2024", "Học kỳ 2 – 2023" })
            {
                var item = new MenuItem { Header = sem };
                item.Click += (_, _) =>
                {
                    _currentSemester = sem;
                    // TxtCurrentSemester.Text = sem;
                    // LoadData for the semester, then refresh
                    RefreshUI();
                };
                menu.Items.Add(item);
            }
            menu.PlacementTarget = sender as UIElement;
            menu.IsOpen = true;
        }

        private void RefreshUI()
        {
            // In a full implementation: rebind TextBlocks and re-render bars
            // For now, the XAML static content reflects the loaded data
        }

        /// <summary>Xuất báo cáo ra file CSV.</summary>
        private void BtnExportReport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName   = $"BaoCao_{_currentSemester.Replace(" ","_").Replace("–","")}.csv",
                DefaultExt = ".csv",
                Filter     = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Môn học,Lớp,Sĩ số,Điểm TB,Tỷ lệ đạt,Tỷ lệ xuất sắc,Học kỳ");
                foreach (var r in _reports)
                    sb.AppendLine($"{r.Subject},{r.ClassName},{r.StudentCount},{r.AvgScore:0.0},{r.PassRate}%,{r.ExcellentRate}%,{r.Semester}");

                sb.AppendLine();
                sb.AppendLine("=== TOP HỌC SINH ===");
                sb.AppendLine("Hạng,Tên học sinh,Lớp,Điểm TB");
                foreach (var s in _topStudents)
                    sb.AppendLine($"{s.Rank},{s.Name},{s.ClassName},{s.AvgScore:0.0}");

                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Đã xuất báo cáo thành công!\n{dlg.FileName}",
                                "Xuất báo cáo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>Click "Xem chi tiết" trong bảng điểm.</summary>
        private void BtnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string className = btn.Tag?.ToString() ?? "";
            var report = _reports.FirstOrDefault(r => r.ClassName == className);
            if (report == null) return;

            var msg = $"Chi tiết lớp: {report.Subject} – {report.ClassName}\n" +
                      $"Sĩ số: {report.StudentCount}  |  Điểm TB: {report.AvgScore:0.0}\n" +
                      $"Tỷ lệ đạt: {report.PassRate}%  |  Xuất sắc: {report.ExcellentRate}%\n\n" +
                      "(Tích hợp StudentDetailView tại đây)";
            MessageBox.Show(msg, "Chi tiết lớp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>In báo cáo (PrintDialog).</summary>
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var printDlg = new PrintDialog();
            if (printDlg.ShowDialog() == true)
                printDlg.PrintVisual(this, $"Báo cáo {_currentSemester}");
        }
    }
}
