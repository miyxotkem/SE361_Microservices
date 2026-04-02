using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace e_learning_app.Views
{
    public partial class MyClassesView : UserControl
    {
        // ─── Data model ──────────────────────────────────────────────
        public class ClassInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ClassName { get; set; }
            public string Semester { get; set; }
            public string Emoji { get; set; }
            public string AccentColor { get; set; }   // hex
            public int StudentCount { get; set; }
            public double AvgScore { get; set; }
            public int AttendanceRate { get; set; }   // %
            public bool IsActive { get; set; }
        }

        // ─── State ───────────────────────────────────────────────────
        private List<ClassInfo> _allClasses = new();
        private string _filterMode = "all";
        private string _searchText = "";

        // ─── Constructor ─────────────────────────────────────────────
        public MyClassesView()
        {
            InitializeComponent();
            LoadSampleData();
        }

        // ─── Lifecycle ───────────────────────────────────────────────
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        // ─── Sample data ─────────────────────────────────────────────
        private void LoadSampleData()
        {
            _allClasses = new List<ClassInfo>
            {
                new() { Id=1, Name="Toán Giải Tích",    ClassName="12A1", Semester="Học kỳ 2 – 2024",
                        Emoji="📐", AccentColor="#3B82F6", StudentCount=38, AvgScore=7.9, AttendanceRate=94, IsActive=true  },
                new() { Id=2, Name="Vật Lý Đại Cương",  ClassName="11B3", Semester="Học kỳ 2 – 2024",
                        Emoji="⚡", AccentColor="#8B5CF6", StudentCount=42, AvgScore=7.4, AttendanceRate=91, IsActive=true  },
                new() { Id=3, Name="Hóa Hữu Cơ",        ClassName="12C2", Semester="Học kỳ 2 – 2024",
                        Emoji="🧪", AccentColor="#F59E0B", StudentCount=35, AvgScore=8.1, AttendanceRate=96, IsActive=true  },
                new() { Id=4, Name="Sinh Học Phân Tử",   ClassName="11A2", Semester="Học kỳ 2 – 2024",
                        Emoji="🌿", AccentColor="#10B981", StudentCount=40, AvgScore=7.6, AttendanceRate=89, IsActive=true  },
                new() { Id=5, Name="Ngữ Văn 12",         ClassName="12B1", Semester="Học kỳ 1 – 2023",
                        Emoji="📖", AccentColor="#EC4899", StudentCount=37, AvgScore=7.2, AttendanceRate=92, IsActive=false },
                new() { Id=6, Name="Lịch Sử Việt Nam",   ClassName="11C4", Semester="Học kỳ 1 – 2023",
                        Emoji="🏛️", AccentColor="#64748B", StudentCount=41, AvgScore=6.9, AttendanceRate=88, IsActive=false },
            };
        }

        // ─── Filtering & rendering ────────────────────────────────────
        private void ApplyFilter()
        {
            var filtered = _allClasses.AsEnumerable();

            // Status filter
            if (_filterMode == "active") filtered = filtered.Where(c => c.IsActive);
            else if (_filterMode == "ended") filtered = filtered.Where(c => !c.IsActive);

            // Search filter
            if (!string.IsNullOrWhiteSpace(_searchText))
                filtered = filtered.Where(c =>
                    c.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                    c.ClassName.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

            var results = filtered.ToList();

            // Update subtitle
            int total = _allClasses.Count(c => c.IsActive);
            TxtSubtitle.Text = $"Bạn đang phụ trách {total} lớp học đang hoạt động.";

            // Render
            ClassesPanel.Children.Clear();
            foreach (var cls in results)
                ClassesPanel.Children.Add(BuildCard(cls));

            // Empty state
            EmptyState.Visibility = results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ClassesPanel.Visibility = results.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // ─── Card builder (code-behind avoids StackPanel Padding issue) ──
        private Border BuildCard(ClassInfo c)
        {
            var accent = new BrushConverter().ConvertFromString(c.AccentColor) as SolidColorBrush
                             ?? Brushes.SteelBlue;
            var accentLight = new SolidColorBrush(Color.FromArgb(30, accent.Color.R, accent.Color.G, accent.Color.B));

            // ── Card root ──────────────────────────────────
            var card = new Border
            {
                Width = 310,
                Margin = new Thickness(0, 0, 16, 16),
                CornerRadius = new CornerRadius(20),
                Background = Brushes.White,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                { BlurRadius = 16, ShadowDepth = 2, Opacity = 0.06, Color = Colors.Black }
            };

            var root = new StackPanel();
            card.Child = root;

            // ── Coloured header ────────────────────────────
            var header = new Border
            {
                CornerRadius = new CornerRadius(20, 20, 0, 0),
                Height = 90,
                Background = accent
            };

            var headerGrid = new Grid { Margin = new Thickness(20, 16, 20, 16) };
            headerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            headerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Status pill
            var statusPill = new Border
            {
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(8, 3, 8, 3),
                Background = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            statusPill.Child = new TextBlock
            {
                Text = c.IsActive ? "Đang học" : "Kết thúc",
                FontSize = 11,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(statusPill, 0);

            // Emoji
            var emojiBlock = new TextBlock
            {
                Text = c.Emoji,
                FontSize = 32,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            Grid.SetRow(emojiBlock, 1);

            headerGrid.Children.Add(statusPill);
            headerGrid.Children.Add(emojiBlock);
            header.Child = headerGrid;
            root.Children.Add(header);

            // ── Card body (Border replaces StackPanel Padding) ─────
            var bodyBorder = new Border { Padding = new Thickness(20, 16, 20, 16) };
            var body = new StackPanel();
            bodyBorder.Child = body;

            body.Children.Add(new TextBlock
            {
                Text = c.Name,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B))
            });
            body.Children.Add(new TextBlock
            {
                Text = $"Lớp {c.ClassName}  ·  {c.Semester}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B)),
                Margin = new Thickness(0, 4, 0, 12)
            });

            // Stats row
            var statsGrid = new Grid();
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

            void AddStat(int col, string val, string label)
            {
                var sp = new StackPanel();
                sp.Children.Add(new TextBlock
                {
                    Text = val,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x29, 0x3B))
                });
                sp.Children.Add(new TextBlock
                {
                    Text = label,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B))
                });
                Grid.SetColumn(sp, col);
                statsGrid.Children.Add(sp);
            }
            AddStat(0, c.StudentCount.ToString(), "Học sinh");
            AddStat(1, c.AvgScore.ToString("0.0"), "Điểm TB");
            AddStat(2, $"{c.AttendanceRate}%", "Đi học");
            body.Children.Add(statsGrid);

            body.Children.Add(new Separator { Margin = new Thickness(0, 14, 0, 14), Background = new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9)) });

            // Buttons row
            var btnRow = new StackPanel { Orientation = Orientation.Horizontal };

            var btnEnter = CreateCardButton("Vào lớp", accent, Brushes.White, FontWeights.SemiBold);
            btnEnter.Tag = c.Id;
            btnEnter.Click += BtnEnterClass_Click;

            var btnAttend = CreateCardButton("Điểm danh",
                new SolidColorBrush(Color.FromRgb(0xF1, 0xF5, 0xF9)),
                new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69)),
                FontWeights.Normal);
            btnAttend.Tag = c.Id;
            btnAttend.Click += BtnAttendance_Click;
            btnAttend.Margin = new Thickness(8, 0, 0, 0);

            btnRow.Children.Add(btnEnter);
            btnRow.Children.Add(btnAttend);
            body.Children.Add(btnRow);

            root.Children.Add(bodyBorder);
            return card;
        }

        private static Button CreateCardButton(string text, Brush bg, Brush fg, FontWeight fw)
        {
            var btn = new Button
            {
                Content = text,
                Width = 110,
                Height = 34,
                Background = bg,
                Foreground = fg,
                FontSize = 12,
                FontWeight = fw,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            var tpl = new ControlTemplate(typeof(Button));
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background")
            { RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            factory.AppendChild(cp);
            tpl.VisualTree = factory;
            btn.Template = tpl;
            return btn;
        }

        // ─── Filter button helpers ────────────────────────────────────
        private void SetActiveFilterButton(Button active)
        {
            foreach (var btn in new[] { BtnFilterAll, BtnFilterActive, BtnFilterEnded })
            {
                btn.Background = Brushes.White;
                btn.Foreground = new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B));
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0xE2, 0xE8, 0xF0));
                btn.FontWeight = FontWeights.Normal;
            }
            active.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
            active.Foreground = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6));
            active.BorderBrush = new SolidColorBrush(Color.FromRgb(0xBF, 0xDB, 0xFE));
            active.FontWeight = FontWeights.SemiBold;
        }

        // ─── Event handlers ──────────────────────────────────────────
        private void UserControl_Loaded_EventHandler(object sender, RoutedEventArgs e)
            => ApplyFilter();

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            _filterMode = btn.Tag?.ToString() ?? "all";
            SetActiveFilterButton(btn);
            ApplyFilter();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = TxtSearch.Text;
            TxtSearchPlaceholder.Visibility = string.IsNullOrEmpty(_searchText)
                                              ? Visibility.Visible : Visibility.Collapsed;
            BtnClearSearch.Visibility = string.IsNullOrEmpty(_searchText)
                                              ? Visibility.Collapsed : Visibility.Visible;
            ApplyFilter();
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
            => TxtSearchPlaceholder.Visibility = Visibility.Collapsed;

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtSearch.Text))
                TxtSearchPlaceholder.Visibility = Visibility.Visible;
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = "";
            TxtSearch.Focus();
        }

        private void BtnEnterClass_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            var cls = _allClasses.FirstOrDefault(c => c.Id == id);
            if (cls == null) return;
            MessageBox.Show($"Mở lớp: {cls.Name} – {cls.ClassName}\n\n(Tích hợp ClassDetailView tại đây)",
                            "Vào lớp", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int id) return;
            var cls = _allClasses.FirstOrDefault(c => c.Id == id);
            if (cls == null) return;
            MessageBox.Show($"Mở điểm danh: {cls.Name} – {cls.ClassName}\n\n(Tích hợp AttendanceView tại đây)",
                            "Điểm danh", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCreateClass_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for create-class dialog
            var dlg = new Window
            {
                Title = "Tạo lớp học mới",
                Width = 420,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                ResizeMode = ResizeMode.NoResize
            };
            var sp = new StackPanel { Margin = new Thickness(24) };
            sp.Children.Add(new TextBlock { Text = "Tên lớp:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
            var txtName = new TextBox
            {
                Height = 36,
                FontSize = 13,
                Padding = new Thickness(10, 0, 10, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            sp.Children.Add(txtName);
            sp.Children.Add(new TextBlock
            {
                Text = "Môn học:",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 14, 0, 6)
            });
            var txtSubject = new TextBox
            {
                Height = 36,
                FontSize = 13,
                Padding = new Thickness(10, 0, 10, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            sp.Children.Add(txtSubject);

            var btnSave = new Button
            {
                Content = "Tạo lớp",
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 20, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnSave.Click += (s, _) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text)) { MessageBox.Show("Vui lòng nhập tên lớp."); return; }
                _allClasses.Add(new ClassInfo
                {
                    Id = _allClasses.Count + 1,
                    Name = txtSubject.Text,
                    ClassName = txtName.Text,
                    Semester = "Học kỳ 2 – 2024",
                    Emoji = "📚",
                    AccentColor = "#3B82F6",
                    StudentCount = 0,
                    AvgScore = 0,
                    AttendanceRate = 0,
                    IsActive = true
                });
                ApplyFilter();
                dlg.Close();
            };
            sp.Children.Add(btnSave);
            dlg.Content = sp;
            dlg.ShowDialog();
        }

        // ─── Public API (called by MainWindow if needed) ──────────────
        public void RefreshData() => ApplyFilter();
    }
}
