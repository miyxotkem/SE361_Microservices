using FirebaseIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace e_learning_app.Views
{
    public partial class CourseDetailView : UserControl
    {
        private readonly DatabaseManager _dbManager;
        private readonly Course _course;

        public CourseDetailView(DatabaseManager dbManager, Course course)
        {
            InitializeComponent();
            _dbManager = dbManager;
            _course = course;

            InitializeYearComboBox();
            UpdateUI();
        }

        private void InitializeYearComboBox()
        {
            int currentYear = DateTime.Now.Year;
            EditYearInput.Items.Clear();
            for (int i = currentYear - 2; i <= currentYear + 3; i++)
            {
                EditYearInput.Items.Add($"{i}-{i + 1}");
            }
        }

        private void UpdateUI()
        {
            if (_course == null) return;

            TxtTitle.Text = _course.Title;
            TxtEmoji.Text = _course.Emoji;
            TxtClassInfo.Text = $"{_course.ClassName}  •  {_course.Semester}";
            TxtCategory.Text = string.IsNullOrWhiteSpace(_course.Category) ? "Chung" : _course.Category;
            TxtCourseType.Text = string.IsNullOrWhiteSpace(_course.CourseType) ? "Đại cương" : _course.CourseType;
            TxtDescription.Text = string.IsNullOrWhiteSpace(_course.Description) ? "Chưa có mô tả chi tiết." : _course.Description;
            TxtStudentCount.Text = _course.StudentCount.ToString();

            var converter = new BrushConverter();
            try
            {
                CoverPhoto.Background = (SolidColorBrush)converter.ConvertFromString(_course.AccentColor) ?? Brushes.SlateBlue;
            }
            catch
            {
                CoverPhoto.Background = Brushes.SlateBlue;
            }

            if (_course.IsActive)
            {
                MenuToggleStatus.Header = "Kết thúc lớp học";
                MenuToggleIcon.Text = "⏸️";
                MenuToggleStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
            }
            else
            {
                MenuToggleStatus.Header = "Kích hoạt lại lớp";
                MenuToggleIcon.Text = "▶️";
                MenuToggleStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            }
        }

        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            EditTitleInput.Text = _course.Title;
            EditDescInput.Text = _course.Description;
            EditClassInput.Text = _course.ClassName;
            EditEmojiInput.Text = _course.Emoji;

            // Added Category Binding
            EditCategoryInput.Text = _course.Category;

            // Set the correct RadioButton color based on the saved course color
            SetSelectedColor(_course.AccentColor);

            SetComboBoxByContent(EditTypeInput, _course.CourseType);

            if (!string.IsNullOrEmpty(_course.Semester) && _course.Semester.Contains(" - "))
            {
                string[] parts = _course.Semester.Split(new[] { " - " }, StringSplitOptions.None);
                SetComboBoxByContent(EditSemesterInput, parts[0]);
                EditYearInput.SelectedItem = parts.Length > 1 ? parts[1] : null;
            }

            MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
            EditDrawer.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(_course.Semester) && _course.Semester.Contains(" - "))
            {
                // Use a more robust split to handle potential spacing issues
                string[] parts = _course.Semester.Split(new[] { " - " }, StringSplitOptions.None);

                // Set Semester (Hoc ky 1, 2, etc.)
                SetComboBoxByContent(EditSemesterInput, parts[0].Trim());

                // Set Year (2024-2025)
                if (parts.Length > 1)
                {
                    string yearValue = parts[1].Trim();

                    // Loop through items to find the match manually to avoid reference issues
                    foreach (var item in EditYearInput.Items)
                    {
                        if (item.ToString() == yearValue)
                        {
                            EditYearInput.SelectedItem = item;
                            break;
                        }
                    }
                }
            }

            MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
            EditDrawer.Visibility = Visibility.Visible;
        }

        private void SetComboBoxByContent(ComboBox cb, string value)
        {
            foreach (ComboBoxItem item in cb.Items)
            {
                if (item.Content.ToString() == value)
                {
                    cb.SelectedItem = item;
                    return;
                }
            }
        }

        private async void ConfirmEdit_Click(object sender, RoutedEventArgs e)
        {
            _course.Title = EditTitleInput.Text;
            _course.Description = EditDescInput.Text;
            _course.ClassName = EditClassInput.Text;
            _course.Emoji = EditEmojiInput.Text;

            // Save Category
            _course.Category = EditCategoryInput.Text;

            // Save Color from RadioButtons
            _course.AccentColor = GetSelectedColor();

            _course.CourseType = (EditTypeInput.SelectedItem as ComboBoxItem)?.Content.ToString();

            string sem = (EditSemesterInput.SelectedItem as ComboBoxItem)?.Content.ToString();
            string year = EditYearInput.SelectedItem?.ToString();
            _course.Semester = $"{sem} - {year}";

            if (await _dbManager.UpdateCourseAsync(_course))
            {
                UpdateUI();
                CloseEditDrawer_Click(null, null);
            }
        }

        private void CloseEditDrawer_Click(object sender, RoutedEventArgs e)
        {
            EditDrawer.Visibility = Visibility.Collapsed;
            MainScrollViewer.Effect = null;
        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
            DeleteOverlay.Visibility = Visibility.Visible;
        }

        private async void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (await _dbManager.DeleteCourseAsync(_course.Id)) NavigateBack();
        }

        private void CloseDeleteModal_Click(object sender, RoutedEventArgs e)
        {
            DeleteOverlay.Visibility = Visibility.Collapsed;
            MainScrollViewer.Effect = null;
        }

        private async void MenuToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            _course.IsActive = !_course.IsActive;
            if (await _dbManager.UpdateCourseAsync(_course)) UpdateUI();
        }

        private void BtnMoreActions_Click(object sender, RoutedEventArgs e)
        {
            BtnMoreActions.ContextMenu.PlacementTarget = BtnMoreActions;
            BtnMoreActions.ContextMenu.IsOpen = true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => NavigateBack();

        private void NavigateBack()
        {
            var mainWin = Window.GetWindow(this) as MainWindow;
            if (mainWin != null) mainWin.MainContentArea.Content = new MyClassesView(_dbManager);
        }

        // --- HELPER METHODS FOR COLOR RADIO BUTTONS ---

        private void SetSelectedColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor)) return;
            try
            {
                Color targetColor = (Color)ColorConverter.ConvertFromString(hexColor);
                var radioButtons = FindVisualChildren<RadioButton>(EditDrawer).Where(r => r.GroupName == "ThemeColors");
                foreach (var rb in radioButtons)
                {
                    if (rb.Background is SolidColorBrush brush && brush.Color == targetColor)
                    {
                        rb.IsChecked = true;
                        break;
                    }
                }
            }
            catch { /* Ignore invalid hex strings */ }
        }

        private string GetSelectedColor()
        {
            var radioButtons = FindVisualChildren<RadioButton>(EditDrawer).Where(r => r.GroupName == "ThemeColors");
            foreach (var rb in radioButtons)
            {
                if (rb.IsChecked == true && rb.Background is SolidColorBrush brush)
                {
                    // Convert the brush color back to a HEX string (format #RRGGBB)
                    return $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
                }
            }
            return "#3B82F6"; // Default Blue if nothing is checked
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}