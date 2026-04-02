using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace e_learning_app.Views
{
    public partial class SemesterSettingsView : UserControl
    {
        // ─── Model ───────────────────────────────────────────────────
        public class SemesterConfig
        {
            public string SemesterName { get; set; } = "Học kỳ 2 – Năm học 2024-2025";
            public string SchoolName { get; set; } = "THPT Nguyễn Thị Minh Khai – TP.HCM";
            public string AcademicYear { get; set; } = "2024 – 2025";
            public DateTime StartDate { get; set; } = new DateTime(2025, 1, 1);
            public DateTime EndDate { get; set; } = new DateTime(2025, 5, 31);

            // Grading
            public double MinPassScore { get; set; } = 5.0;
            public bool RoundToHalf { get; set; } = true;
            public bool ShowGradeLabel { get; set; } = true;
            public bool AllowStudentView { get; set; } = true;

            // Score weights (%)
            public int WeightOral { get; set; } = 15;
            public int Weight15Min { get; set; } = 15;
            public int WeightMidterm { get; set; } = 30;
            public int WeightFinal { get; set; } = 40;

            // Attendance
            public int MaxAbsences { get; set; } = 3;
            public bool EmailParentOnAbsent { get; set; } = true;
            public bool QRAttendance { get; set; } = true;
            public bool AllowOnlineExcuse { get; set; } = false;

            // Integrations
            public bool WeeklyEmailSummary { get; set; } = true;
            public bool GoogleClassroom { get; set; } = false;
            public bool GoogleCalendar { get; set; } = true;
            public bool MobilePush { get; set; } = true;
            public string Language { get; set; } = "Tiếng Việt";
        }

        // ─── State ───────────────────────────────────────────────────
        private SemesterConfig _config = new();
        private SemesterConfig _originalConfig = new();
        private bool _isDirty = false;

        // ─── Named UI elements (must match x:Name in XAML) ───────────
        // Grading weights
        private TextBox? _txtWeightOral, _txtWeight15, _txtWeightMid, _txtWeightFinal;

        // ─── Constructor ─────────────────────────────────────────────
        public SemesterSettingsView()
        {
            InitializeComponent();
            CloneConfig(_config, _originalConfig);
        }

        // ─── Deep clone helper ────────────────────────────────────────
        private static void CloneConfig(SemesterConfig src, SemesterConfig dst)
        {
            dst.SemesterName = src.SemesterName;
            dst.SchoolName = src.SchoolName;
            dst.AcademicYear = src.AcademicYear;
            dst.StartDate = src.StartDate;
            dst.EndDate = src.EndDate;
            dst.MinPassScore = src.MinPassScore;
            dst.RoundToHalf = src.RoundToHalf;
            dst.ShowGradeLabel = src.ShowGradeLabel;
            dst.AllowStudentView = src.AllowStudentView;
            dst.WeightOral = src.WeightOral;
            dst.Weight15Min = src.Weight15Min;
            dst.WeightMidterm = src.WeightMidterm;
            dst.WeightFinal = src.WeightFinal;
            dst.MaxAbsences = src.MaxAbsences;
            dst.EmailParentOnAbsent = src.EmailParentOnAbsent;
            dst.QRAttendance = src.QRAttendance;
            dst.AllowOnlineExcuse = src.AllowOnlineExcuse;
            dst.WeeklyEmailSummary = src.WeeklyEmailSummary;
            dst.GoogleClassroom = src.GoogleClassroom;
            dst.GoogleCalendar = src.GoogleCalendar;
            dst.MobilePush = src.MobilePush;
            dst.Language = src.Language;
        }

        // ─── Validation ───────────────────────────────────────────────
        private (bool ok, string error) Validate()
        {
            if (string.IsNullOrWhiteSpace(_config.SemesterName))
                return (false, "Tên học kỳ không được để trống.");

            if (_config.StartDate >= _config.EndDate)
                return (false, "Ngày bắt đầu phải trước ngày kết thúc.");

            int totalWeight = _config.WeightOral + _config.Weight15Min
                            + _config.WeightMidterm + _config.WeightFinal;
            if (totalWeight != 100)
                return (false, $"Tổng trọng số điểm phải bằng 100% (hiện tại: {totalWeight}%).");

            if (_config.MinPassScore < 0 || _config.MinPassScore > 10)
                return (false, "Điểm đạt tối thiểu phải trong khoảng 0 – 10.");

            return (true, "");
        }

        // ─── Collect values from XAML into _config ────────────────────
        private void CollectFromUI()
        {
            // In a binding-based app, _config properties update automatically.
            // For code-behind, read x:Name TextBox values here:
            // _config.SemesterName = TxtSemesterName.Text;
            // _config.WeightOral   = int.TryParse(TxtWeightOral.Text.Replace("%",""), out int v) ? v : _config.WeightOral;
            // etc.
        }

        // ─── Push _config values back to XAML elements ───────────────
        private void PushToUI()
        {
            // In a binding-based app this is automatic.
            // TxtSemesterName.Text = _config.SemesterName;
            // TxtWeightOral.Text   = $"{_config.WeightOral}%";
        }

        // ─── Event handlers ──────────────────────────────────────────

        /// <summary>Mỗi khi người dùng thay đổi bất kỳ field nào.</summary>
        private void Field_Changed(object sender, EventArgs e)
        {
            _isDirty = true;
            // Optionally highlight the Save button
        }

        /// <summary>Toggle checkboxes cho Attendance / Integration.</summary>
        private void Toggle_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb) return;
            bool val = cb.IsChecked == true;

            switch (cb.Tag?.ToString())
            {
                case "emailParent": _config.EmailParentOnAbsent = val; break;
                case "qrAttendance": _config.QRAttendance = val; break;
                case "onlineExcuse": _config.AllowOnlineExcuse = val; break;
                case "weeklyEmail": _config.WeeklyEmailSummary = val; break;
                case "googleClassroom": _config.GoogleClassroom = val; break;
                case "googleCalendar": _config.GoogleCalendar = val; break;
                case "mobilePush": _config.MobilePush = val; break;
                case "roundHalf": _config.RoundToHalf = val; break;
                case "gradeLabel": _config.ShowGradeLabel = val; break;
                case "studentView": _config.AllowStudentView = val; break;
            }
            _isDirty = true;
        }

        /// <summary>Nút "Lưu cài đặt".</summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            CollectFromUI();

            var (ok, err) = Validate();
            if (!ok)
            {
                MessageBox.Show(err, "Kiểm tra dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Persist: serialize to JSON / DB here
            // e.g. File.WriteAllText("settings.json", JsonSerializer.Serialize(_config));
            CloneConfig(_config, _originalConfig);
            _isDirty = false;

            MessageBox.Show("Đã lưu cài đặt học kỳ thành công! ✅",
                            "Lưu thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Nút "Hủy thay đổi".</summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!_isDirty) return;

            var res = MessageBox.Show("Bạn có thay đổi chưa được lưu. Hủy bỏ tất cả thay đổi?",
                                      "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                CloneConfig(_originalConfig, _config);
                PushToUI();
                _isDirty = false;
            }
        }

        /// <summary>Validate trọng số realtime khi nhập.</summary>
        private void WeightBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            string raw = tb.Text.Replace("%", "").Trim();
            if (!int.TryParse(raw, out int val) || val < 0 || val > 100)
            {
                tb.Foreground = Brushes.Red;
                return;
            }
            tb.Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B));

            switch (tb.Tag?.ToString())
            {
                case "oral": _config.WeightOral = val; break;
                case "15min": _config.Weight15Min = val; break;
                case "midterm": _config.WeightMidterm = val; break;
                case "final": _config.WeightFinal = val; break;
            }

            // Live weight sum indicator
            int sum = _config.WeightOral + _config.Weight15Min
                    + _config.WeightMidterm + _config.WeightFinal;
            // TxtWeightSum.Text      = $"{sum}%";
            // TxtWeightSum.Foreground = sum == 100 ? Brushes.Green : Brushes.Red;
            _isDirty = true;
        }

        /// <summary>Reset toàn bộ về mặc định.</summary>
        private void BtnResetDefaults_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Đặt lại tất cả cài đặt về mặc định?",
                                      "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;

            _config = new SemesterConfig();
            PushToUI();
            _isDirty = true;
            MessageBox.Show("Đã đặt lại cài đặt về mặc định.", "Thành công",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>Ngăn người dùng đóng/navigate đi khi có thay đổi chưa lưu.</summary>
        public bool CanNavigateAway()
        {
            if (!_isDirty) return true;
            var res = MessageBox.Show("Bạn có thay đổi chưa được lưu. Tiếp tục mà không lưu?",
                                      "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return res == MessageBoxResult.Yes;
        }

        // ─── Public API ───────────────────────────────────────────────
        public SemesterConfig CurrentConfig => _config;
    }
}
