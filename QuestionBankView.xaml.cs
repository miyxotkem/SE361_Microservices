using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace e_learning_app.Views
{
    public partial class QuestionBankView : UserControl
    {
        // ─── Models ──────────────────────────────────────────────────
        public class Question
        {
            public int Id { get; set; }
            public string Subject { get; set; }   // "Toán học" | "Vật lý" | "Hóa học" | "Sinh học" | "Ngữ văn"
            public string Level { get; set; }   // "Dễ" | "Trung bình" | "Khó"
            public string Type { get; set; }   // "Trắc nghiệm" | "Tự luận" | "Điền vào chỗ trống"
            public string Content { get; set; }
            public int UsedInExams { get; set; }
            public DateTime AddedDate { get; set; }
        }

        // ─── Filter state ─────────────────────────────────────────────
        private List<Question> _allQuestions = new();
        private string _selectedSubject = "Tất cả";
        private HashSet<string> _selectedLevels = new() { "Dễ", "Trung bình", "Khó" };
        private HashSet<string> _selectedTypes = new() { "Trắc nghiệm", "Tự luận", "Điền vào chỗ trống" };
        private string _searchText = "";
        private Question? _editingQuestion = null;

        // ─── Level/type color maps ────────────────────────────────────
        private static readonly Dictionary<string, (string bg, string fg)> LevelColors = new()
        {
            { "Dễ",        ("#DCFCE7", "#16A34A") },
            { "Trung bình",("#FEF3C7", "#D97706") },
            { "Khó",       ("#FEE2E2", "#DC2626") },
        };
        private static readonly Dictionary<string, (string bg, string fg)> SubjectColors = new()
        {
            { "Toán học", ("#EFF6FF", "#3B82F6") },
            { "Vật lý",   ("#EDE9FE", "#7C3AED") },
            { "Hóa học",  ("#ECFDF5", "#059669") },
            { "Sinh học", ("#F0FDF4", "#16A34A") },
            { "Ngữ văn",  ("#FFF7ED", "#D97706") },
        };

        // ─── Constructor ─────────────────────────────────────────────
        public QuestionBankView()
        {
            InitializeComponent();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            _allQuestions = new List<Question>
            {
                new() { Id=1, Subject="Toán học", Level="Trung bình", Type="Trắc nghiệm",
                        Content="Tìm giới hạn của hàm số f(x) = (x² - 4)/(x - 2) khi x tiến tới 2.",
                        UsedInExams=3, AddedDate=new DateTime(2025,3,14) },
                new() { Id=2, Subject="Vật lý", Level="Khó", Type="Tự luận",
                        Content="Một vật khối lượng 2kg chuyển động đều trên mặt phẳng nghiêng góc 30°. Tính lực ma sát tác dụng lên vật. Cho g = 10 m/s².",
                        UsedInExams=5, AddedDate=new DateTime(2025,2,2) },
                new() { Id=3, Subject="Hóa học", Level="Dễ", Type="Trắc nghiệm",
                        Content="Phản ứng nào sau đây là phản ứng oxi hóa – khử? A. NaOH + HCl → NaCl + H₂O  B. Fe + CuSO₄ → FeSO₄ + Cu",
                        UsedInExams=7, AddedDate=new DateTime(2025,1,10) },
                new() { Id=4, Subject="Toán học", Level="Khó", Type="Tự luận",
                        Content="Chứng minh rằng với mọi số nguyên n, biểu thức n³ - n luôn chia hết cho 6.",
                        UsedInExams=2, AddedDate=new DateTime(2025,3,20) },
                new() { Id=5, Subject="Sinh học", Level="Trung bình", Type="Trắc nghiệm",
                        Content="Quá trình nào xảy ra trong pha sáng của quang hợp? A. Tổng hợp ATP  B. Cố định CO₂  C. Tạo glucose  D. Tổng hợp protein",
                        UsedInExams=4, AddedDate=new DateTime(2025,2,28) },
                new() { Id=6, Subject="Ngữ văn", Level="Dễ", Type="Điền vào chỗ trống",
                        Content="Điền từ thích hợp: \"Văn học là _______ của cuộc sống.\" (gương, bức tranh, tiếng vang)",
                        UsedInExams=1, AddedDate=new DateTime(2025,3,5) },
            };
        }

        // ─── Filter logic ─────────────────────────────────────────────
        private IEnumerable<Question> GetFiltered()
        {
            var q = _allQuestions.AsEnumerable();
            if (_selectedSubject != "Tất cả")
                q = q.Where(x => x.Subject == _selectedSubject);
            q = q.Where(x => _selectedLevels.Contains(x.Level));
            q = q.Where(x => _selectedTypes.Contains(x.Type));
            if (!string.IsNullOrWhiteSpace(_searchText))
                q = q.Where(x => x.Content.Contains(_searchText, StringComparison.OrdinalIgnoreCase)
                               || x.Subject.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
            return q;
        }

        private void RefreshList()
        {
            var results = GetFiltered().ToList();

            // Update summary stats
            // TxtTotalCount.Text = _allQuestions.Count.ToString();
            // TxtHardCount.Text  = _allQuestions.Count(q => q.Level == "Khó").ToString();
            // etc.

            // Re-render question panel (if using ItemsControl, set ItemsSource here)
            // For static XAML, we rely on the filter state only
        }

        // ─── Search ───────────────────────────────────────────────────
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
                _searchText = tb.Text;
            RefreshList();
        }

        // ─── Subject radio buttons ────────────────────────────────────
        private void SubjectRadio_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.IsChecked == true)
                _selectedSubject = rb.Content?.ToString() ?? "Tất cả";
            RefreshList();
        }

        // ─── Level checkboxes ─────────────────────────────────────────
        private void LevelCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb) return;
            string level = cb.Content?.ToString() ?? "";
            if (cb.IsChecked == true) _selectedLevels.Add(level);
            else _selectedLevels.Remove(level);
            RefreshList();
        }

        // ─── Type checkboxes ──────────────────────────────────────────
        private void TypeCheck_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox cb) return;
            string type = cb.Content?.ToString() ?? "";
            if (cb.IsChecked == true) _selectedTypes.Add(type);
            else _selectedTypes.Remove(type);
            RefreshList();
        }

        // ─── Apply filter button ──────────────────────────────────────
        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
            var count = GetFiltered().Count();
            MessageBox.Show($"Tìm thấy {count} câu hỏi phù hợp.",
                            "Kết quả lọc", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ─── Add question ─────────────────────────────────────────────
        private void BtnAddQuestion_Click(object sender, RoutedEventArgs e)
        {
            ShowQuestionDialog(null);
        }

        // ─── Edit question ────────────────────────────────────────────
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            var q = _allQuestions.FirstOrDefault(x => x.Id == id);
            ShowQuestionDialog(q);
        }

        // ─── Delete question ──────────────────────────────────────────
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            var q = _allQuestions.FirstOrDefault(x => x.Id == id);
            if (q == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa câu hỏi này?\n\n\"{q.Content[..Math.Min(60, q.Content.Length)]}...\"",
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _allQuestions.Remove(q);
                RefreshList();
            }
        }

        // ─── Import questions ─────────────────────────────────────────
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Chọn file câu hỏi để import"
            };
            if (dlg.ShowDialog() == true)
            {
                // TODO: parse file and add to _allQuestions
                MessageBox.Show($"Đã chọn file: {dlg.FileName}\n(Tính năng import đang được phát triển)",
                                "Import câu hỏi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ─── Question dialog (add / edit) ─────────────────────────────
        private void ShowQuestionDialog(Question? existing)
        {
            bool isEdit = existing != null;
            var dlg = new Window
            {
                Title = isEdit ? "Chỉnh sửa câu hỏi" : "Thêm câu hỏi mới",
                Width = 520,
                Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(0xF8, 0xFA, 0xFC))
            };

            var root = new StackPanel { Margin = new Thickness(28) };

            // Content
            root.Children.Add(new TextBlock { Text = "Nội dung câu hỏi:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
            var txtContent = new TextBox
            {
                Text = existing?.Content ?? "",
                Height = 90,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Padding = new Thickness(10, 8, 10, 8),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0)),
                FontSize = 13
            };
            root.Children.Add(txtContent);

            // Row: subject + level + type
            var row = new Grid { Margin = new Thickness(0, 14, 0, 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            row.ColumnDefinitions.Add(new ColumnDefinition());

            ComboBox MakeCb(int col, string[] items, string selected)
            {
                var cb = new ComboBox { ItemsSource = items, SelectedItem = selected, Height = 36, FontSize = 13, Padding = new Thickness(8, 0, 8, 0) };
                Grid.SetColumn(cb, col);
                return cb;
            }

            var cbSubject = MakeCb(0, new[] { "Toán học", "Vật lý", "Hóa học", "Sinh học", "Ngữ văn" }, existing?.Subject ?? "Toán học");
            var cbLevel = MakeCb(2, new[] { "Dễ", "Trung bình", "Khó" }, existing?.Level ?? "Trung bình");
            var cbType = MakeCb(4, new[] { "Trắc nghiệm", "Tự luận", "Điền vào chỗ trống" }, existing?.Type ?? "Trắc nghiệm");
            row.Children.Add(cbSubject);
            row.Children.Add(cbLevel);
            row.Children.Add(cbType);
            root.Children.Add(row);

            // Buttons
            var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 24, 0, 0) };
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

            var btnSave = new Button
            {
                Content = isEdit ? "Lưu thay đổi" : "Thêm câu hỏi",
                Width = 130,
                Height = 38,
                Background = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(10, 0, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnSave.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtContent.Text))
                {
                    MessageBox.Show("Vui lòng nhập nội dung câu hỏi."); return;
                }
                if (isEdit && existing != null)
                {
                    existing.Content = txtContent.Text;
                    existing.Subject = cbSubject.SelectedItem?.ToString() ?? existing.Subject;
                    existing.Level = cbLevel.SelectedItem?.ToString() ?? existing.Level;
                    existing.Type = cbType.SelectedItem?.ToString() ?? existing.Type;
                }
                else
                {
                    _allQuestions.Add(new Question
                    {
                        Id = _allQuestions.Count + 1,
                        Content = txtContent.Text,
                        Subject = cbSubject.SelectedItem?.ToString() ?? "Toán học",
                        Level = cbLevel.SelectedItem?.ToString() ?? "Trung bình",
                        Type = cbType.SelectedItem?.ToString() ?? "Trắc nghiệm",
                        UsedInExams = 0,
                        AddedDate = DateTime.Today
                    });
                }
                RefreshList();
                dlg.Close();
            };
            btnRow.Children.Add(btnCancel);
            btnRow.Children.Add(btnSave);
            root.Children.Add(btnRow);

            dlg.Content = root;
            dlg.ShowDialog();
        }
    }
}
