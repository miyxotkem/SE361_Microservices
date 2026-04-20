using e_learning_app.Class;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace e_learning_app.Views
{
    public class DeadlineColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime deadline)
            {
                if (deadline < DateTime.Now)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                else
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class CourseDetailView : UserControl
    {
        private readonly DatabaseManager _dbManager;
        private readonly Course _course;
        private string CurrentUserId => _dbManager.GetCurrentUser()?.Id;
        private ObservableCollection<CourseContent> _courseContents;
        private ObservableCollection<Assignment> _assignments;

        private CourseContent _editingContent = null;
        private CourseContent _contentToDelete = null;

        private Assignment _currentViewedAssignment = null;
        private string _selectedSubmissionFilePath = "";

        private Assignment _editingAssignment = null;
        private Assignment _assignmentToDelete = null;

        // Lưu trữ đường dẫn tệp đính kèm của Giảng viên để upload
        private string _selectedAssignFilePath = "";

        private readonly Cloudinary _cloudinary;

        public Brush CourseAccentBrush
        {
            get { return (Brush)GetValue(CourseAccentBrushProperty); }
            set { SetValue(CourseAccentBrushProperty, value); }
        }
        public static readonly DependencyProperty CourseAccentBrushProperty =
            DependencyProperty.Register("CourseAccentBrush", typeof(Brush), typeof(CourseDetailView), new PropertyMetadata(Brushes.SlateBlue));

        public CourseDetailView(DatabaseManager dbManager, Course course)
        {
            InitializeComponent();
            _dbManager = dbManager;
            _course = course;

            Account account = new Account(
                "drg8swbxp",
                "167723827683986",
                "3aclNKhg3htYds76wcUrxjTdnRU");

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;

            InitializeYearComboBox();
            ApplyRolePermissions();
            LoadCourseContent();
            LoadAssignments();
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

        private void ApplyRolePermissions()
        {
            bool isInstructor = _course.InstructorId == CurrentUserId;
            BtnMoreActions.Visibility = isInstructor ? Visibility.Visible : Visibility.Collapsed;
            BtnAddContent.Visibility = isInstructor ? Visibility.Visible : Visibility.Collapsed;
            BtnShowAddAssignment.Visibility = isInstructor ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void LoadCourseContent()
        {
            try
            {
                var contents = await _dbManager.GetCourseContentsAsync(_course.Id);
                if (contents != null)
                {
                    _courseContents = new ObservableCollection<CourseContent>(contents.OrderBy(c => c.OrderIndex));
                    DocumentsList.ItemsSource = _courseContents;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải nội dung: " + ex.Message);
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
            Brush accentBrush = Brushes.SlateBlue;
            try { accentBrush = (SolidColorBrush)converter.ConvertFromString(_course.AccentColor); }
            catch { }

            CoverPhoto.Background = accentBrush;
            CourseAccentBrush = accentBrush;

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

        private void DocumentsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DocumentsList.SelectedItem is CourseContent selectedContent)
            {
                try
                {
                    if (selectedContent.Type == "Link")
                    {
                        Process.Start(new ProcessStartInfo { FileName = selectedContent.Data, UseShellExecute = true });
                    }
                    else if (selectedContent.Type == "Document")
                    {
                        if (Uri.IsWellFormedUriString(selectedContent.Data, UriKind.Absolute))
                        {
                            string previewUrl = selectedContent.Data;

                            if (previewUrl.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                                previewUrl.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                                previewUrl.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase))
                            {
                                previewUrl = $"https://docs.google.com/viewer?url={Uri.EscapeDataString(previewUrl)}";
                            }

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = previewUrl,
                                UseShellExecute = true
                            });
                        }
                        else MessageBox.Show("Đường dẫn tệp không hợp lệ.", "Lỗi Mạng", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else if (selectedContent.Type == "Note")
                    {
                        TxtNoteTitle.Text = selectedContent.Title;
                        TxtNoteContent.Text = selectedContent.Data;
                        MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
                        NoteReaderDrawer.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể mở tệp: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseNoteReader_Click(object sender, RoutedEventArgs e)
        {
            NoteReaderDrawer.Visibility = Visibility.Collapsed;
            MainScrollViewer.Effect = null;
        }

        private void DragHandle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_course.InstructorId != CurrentUserId) return;
            var dragHandle = sender as FrameworkElement;
            var item = dragHandle?.DataContext as CourseContent;
            if (item != null) DragDrop.DoDragDrop(dragHandle, item, DragDropEffects.Move);
        }

        private async void DocumentsList_Drop(object sender, DragEventArgs e)
        {
            if (_course.InstructorId != CurrentUserId) return;

            var droppedData = e.Data.GetData(typeof(CourseContent)) as CourseContent;
            var targetElement = e.OriginalSource as FrameworkElement;
            var target = targetElement?.DataContext as CourseContent;

            if (droppedData != null && target != null && droppedData != target)
            {
                int oldIndex = _courseContents.IndexOf(droppedData);
                int newIndex = _courseContents.IndexOf(target);

                _courseContents.RemoveAt(oldIndex);
                _courseContents.Insert(newIndex, droppedData);
                for (int i = 0; i < _courseContents.Count; i++) _courseContents[i].OrderIndex = i;

                try { await _dbManager.UpdateCourseContentOrderAsync(_course.Id, _courseContents.ToList()); }
                catch (Exception ex) { MessageBox.Show("Lỗi khi lưu thứ tự mới: " + ex.Message); LoadCourseContent(); }
            }
        }

        private void BtnEditContent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string contentId)
            {
                _editingContent = _courseContents.FirstOrDefault(c => c.Id == contentId);
                if (_editingContent == null) return;

                TxtContentFormTitle.Text = "Chỉnh sửa nội dung";
                BtnSubmitContent.Content = "Lưu thay đổi";
                AddTitleInput.Text = _editingContent.Title;
                SetComboBoxByContent(AddTypeInput, _editingContent.Type);

                AddLinkInput.Text = ""; AddNoteInput.Text = "";

                if (_editingContent.Type == "Link") AddLinkInput.Text = _editingContent.Data;
                else if (_editingContent.Type == "Note") AddNoteInput.Text = _editingContent.Data;
                else if (_editingContent.Type == "Document")
                {
                    // Lấy lại tên file gọn gàng
                    string displayTitle = GetDisplayNameFromUrl(_editingContent.Data, "document");
                    AddDocPathInput.Text = displayTitle;
                    AddDocPathInput.Tag = _editingContent.Data; // Lưu URL cũ vào Tag
                }

                MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
                AddContentDrawer.Visibility = Visibility.Visible;
            }
        }

        private void BtnDeleteContent_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string contentId)
            {
                _contentToDelete = _courseContents.FirstOrDefault(c => c.Id == contentId);
                if (_contentToDelete != null)
                {
                    MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
                    DeleteContentOverlay.Visibility = Visibility.Visible;
                }
            }
        }

        private void CloseDeleteContentModal_Click(object sender, RoutedEventArgs e)
        {
            DeleteContentOverlay.Visibility = Visibility.Collapsed;
            MainScrollViewer.Effect = null;
            _contentToDelete = null;
        }

        private async void ConfirmDeleteContent_Click(object sender, RoutedEventArgs e)
        {
            if (_contentToDelete != null)
            {
                if (await _dbManager.DeleteCourseContentAsync(_course.Id, _contentToDelete.Id))
                {
                    if (_contentToDelete.Type == "Document" && _contentToDelete.Data.StartsWith("http"))
                    {
                        try
                        {
                            string publicId = GetPublicIdFromUrl(_contentToDelete.Data);
                            if (!string.IsNullOrEmpty(publicId))
                            {
                                await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Raw });
                            }
                        }
                        catch (Exception ex) { Debug.WriteLine("Lỗi xóa Cloudinary: " + ex.Message); }
                    }
                    _courseContents.Remove(_contentToDelete);
                }
            }
            CloseDeleteContentModal_Click(null, null);
        }

        // ==========================================
        // CÁC HÀM XỬ LÝ URL CLOUDINARY
        // ==========================================
        private string GetPublicIdFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                string path = Uri.UnescapeDataString(uri.AbsolutePath);

                int uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex >= 0)
                {
                    string afterUpload = path.Substring(uploadIndex + 8);
                    int slashIndex = afterUpload.IndexOf("/");

                    if (slashIndex >= 0 && afterUpload.StartsWith("v") && char.IsDigit(afterUpload[1]))
                    {
                        return Path.GetFileName(afterUpload.Substring(slashIndex + 1));
                    }
                    return Path.GetFileName(afterUpload);
                }
                return Path.GetFileName(path);
            }
            catch { return string.Empty; }
        }

        private string GetDisplayNameFromUrl(string url, string prefixType)
        {
            string rawFileName = GetPublicIdFromUrl(url);
            string[] parts = rawFileName.Split('_');

            if (prefixType == "submission" && parts.Length >= 3)
            {
                return string.Join("_", parts.Skip(2));
            }
            else if (prefixType == "assignment" && parts.Length >= 2)
            {
                return string.Join("_", parts.Skip(1));
            }
            return rawFileName;
        }

        private void BtnAddContent_Click(object sender, RoutedEventArgs e)
        {
            _editingContent = null;
            TxtContentFormTitle.Text = "Thêm nội dung mới";
            BtnSubmitContent.Content = "Thêm vào lớp";
            AddTitleInput.Text = ""; AddLinkInput.Text = ""; AddNoteInput.Text = "";
            AddDocPathInput.Text = ""; AddDocPathInput.Tag = null;
            AddTypeInput.SelectedIndex = 0;
            MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
            AddContentDrawer.Visibility = Visibility.Visible;
        }

        private void CloseAddDrawer_Click(object sender, RoutedEventArgs e)
        {
            AddContentDrawer.Visibility = Visibility.Collapsed;
            MainScrollViewer.Effect = null;
        }

        private void AddTypeInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InputAreaDocument != null) InputAreaDocument.Visibility = Visibility.Collapsed;
            if (InputAreaLink != null) InputAreaLink.Visibility = Visibility.Collapsed;
            if (InputAreaNote != null) InputAreaNote.Visibility = Visibility.Collapsed;

            var selectedItem = AddTypeInput.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string type = selectedItem.Content.ToString();
                if (type == "Document" && InputAreaDocument != null) InputAreaDocument.Visibility = Visibility.Visible;
                else if (type == "Link" && InputAreaLink != null) InputAreaLink.Visibility = Visibility.Visible;
                else if (type == "Note" && InputAreaNote != null) InputAreaNote.Visibility = Visibility.Visible;
            }
        }

        private void BtnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "All Files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() == true)
            {
                AddDocPathInput.Text = openFileDialog.FileName;
                AddDocPathInput.Tag = openFileDialog.FileName; // Lưu đường dẫn mới

                if (string.IsNullOrWhiteSpace(AddTitleInput.Text))
                {
                    AddTitleInput.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
            }
        }

        private async void ConfirmAddContent_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AddTitleInput.Text))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề!"); return;
            }

            string type = (AddTypeInput.SelectedItem as ComboBoxItem)?.Content.ToString();
            string data = "";

            if (type == "Link")
            {
                data = AddLinkInput.Text;
                if (string.IsNullOrWhiteSpace(data)) { MessageBox.Show("Vui lòng nhập link!"); return; }
            }
            else if (type == "Note")
            {
                data = AddNoteInput.Text;
                if (string.IsNullOrWhiteSpace(data)) { MessageBox.Show("Vui lòng nhập ghi chú!"); return; }
            }
            else if (type == "Document")
            {
                // Tag đang chứa đường dẫn thật nếu chọn file mới, hoặc chứa URL cũ nếu đang Edit
                string sourcePath = AddDocPathInput.Tag?.ToString();

                if (string.IsNullOrWhiteSpace(sourcePath)) { MessageBox.Show("Vui lòng chọn tệp hợp lệ!"); return; }

                if (!sourcePath.StartsWith("http") && File.Exists(sourcePath))
                {
                    try
                    {
                        BtnSubmitContent.Content = "Đang tải lên...";
                        BtnSubmitContent.IsEnabled = false;

                        var uploadParams = new RawUploadParams()
                        {
                            File = new FileDescription(sourcePath),
                            UseFilename = true,
                            UniqueFilename = true
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        if (uploadResult.Error != null) throw new Exception(uploadResult.Error.Message);
                        data = uploadResult.SecureUrl.ToString();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi tải tệp lên đám mây: " + ex.Message);
                        BtnSubmitContent.Content = _editingContent != null ? "Lưu thay đổi" : "Thêm vào lớp";
                        BtnSubmitContent.IsEnabled = true; return;
                    }
                    finally
                    {
                        BtnSubmitContent.Content = _editingContent != null ? "Lưu thay đổi" : "Thêm vào lớp";
                        BtnSubmitContent.IsEnabled = true;
                    }
                }
                else { data = sourcePath; }
            }

            if (_editingContent != null)
            {
                if (_editingContent.Type == "Document" && _editingContent.Data != data && _editingContent.Data.StartsWith("http"))
                {
                    try
                    {
                        string exactPublicId = GetPublicIdFromUrl(_editingContent.Data);
                        if (!string.IsNullOrEmpty(exactPublicId))
                        {
                            await _cloudinary.DestroyAsync(new DeletionParams(exactPublicId) { ResourceType = ResourceType.Raw });
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine("Lỗi xóa Cloudinary: " + ex.Message); }
                }

                _editingContent.Title = AddTitleInput.Text;
                _editingContent.Type = type;
                _editingContent.Data = data;

                if (await _dbManager.UpdateCourseContentAsync(_course.Id, _editingContent))
                {
                    int index = _courseContents.IndexOf(_editingContent);
                    _courseContents.RemoveAt(index);
                    _courseContents.Insert(index, _editingContent);
                    DocumentsList.SelectedItem = null;
                }
            }
            else
            {
                int nextOrderIndex = _courseContents != null && _courseContents.Any() ? _courseContents.Max(c => c.OrderIndex) + 1 : 0;
                var newContent = new CourseContent
                {
                    CourseId = _course.Id,
                    Title = AddTitleInput.Text,
                    Type = type,
                    Data = data,
                    OrderIndex = nextOrderIndex
                };
                var collectionRef = _dbManager.GetDb.Collection("Courses").Document(_course.Id).Collection("Contents");
                var docRef = await collectionRef.AddAsync(newContent);
                newContent.Id = docRef.Id;
                if (_courseContents == null) _courseContents = new ObservableCollection<CourseContent>();
                _courseContents.Add(newContent);
            }
            CloseAddDrawer_Click(null, null);
        }

        private async void LoadAssignments()
        {
            try
            {
                var query = _dbManager.GetDb.Collection("Courses").Document(_course.Id).Collection("Assignments");
                var snapshot = await query.GetSnapshotAsync();

                var list = new List<Assignment>();
                foreach (var doc in snapshot.Documents)
                {
                    var assign = doc.ConvertTo<Assignment>();
                    assign.Id = doc.Id;
                    assign.Deadline = assign.Deadline.ToLocalTime();
                    list.Add(assign);
                }

                _assignments = new ObservableCollection<Assignment>(list.OrderBy(a => a.Deadline));
                AssignmentsList.ItemsSource = _assignments;
                TxtAssignmentCount.Text = _assignments.Count.ToString();
            }
            catch (Exception ex) { Debug.WriteLine("Lỗi tải bài tập: " + ex.Message); }
        }

        private void BtnShowAddAssignment_Click(object sender, RoutedEventArgs e)
        {
            _editingAssignment = null;
            TxtAssignmentFormTitle.Text = "Tạo bài tập mới";
            BtnConfirmAddAssignment.Content = "Tạo bài tập";

            AssignTitleInput.Text = ""; AssignDescInput.Text = "";
            _selectedAssignFilePath = ""; TxtAssignSelectedFile.Text = "Kéo thả tệp vào đây hoặc nhấn để chọn";

            AssignDatePicker.SelectedDate = DateTime.Today.AddDays(7);
            AssignTimeInput.Text = "23:59";

            MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
            AddAssignmentDrawer.Visibility = Visibility.Visible;
        }

        private void BtnEditAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string assignId)
            {
                _editingAssignment = _assignments.FirstOrDefault(a => a.Id == assignId);
                if (_editingAssignment == null) return;

                TxtAssignmentFormTitle.Text = "Chỉnh sửa bài tập";
                BtnConfirmAddAssignment.Content = "Lưu thay đổi";

                AssignTitleInput.Text = _editingAssignment.Title;
                AssignDescInput.Text = _editingAssignment.Description;
                AssignDatePicker.SelectedDate = _editingAssignment.Deadline.Date;
                AssignTimeInput.Text = _editingAssignment.Deadline.ToString("HH:mm");

                if (!string.IsNullOrEmpty(_editingAssignment.AttachedFileUrl))
                {
                    _selectedAssignFilePath = "";
                    string displayName = GetDisplayNameFromUrl(_editingAssignment.AttachedFileUrl, "assignment");
                    TxtAssignSelectedFile.Text = "Đã đính kèm: " + displayName;
                }
                else
                {
                    _selectedAssignFilePath = "";
                    TxtAssignSelectedFile.Text = "Kéo thả tệp vào đây hoặc nhấn để chọn";
                }

                MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
                AddAssignmentDrawer.Visibility = Visibility.Visible;
            }
        }

        private void BtnDeleteAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string assignId)
            {
                _assignmentToDelete = _assignments.FirstOrDefault(a => a.Id == assignId);
                if (_assignmentToDelete != null)
                {
                    MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
                    DeleteAssignmentOverlay.Visibility = Visibility.Visible;
                }
            }
        }

        private void CloseDeleteAssignmentModal_Click(object sender, RoutedEventArgs e)
        {
            DeleteAssignmentOverlay.Visibility = Visibility.Collapsed;
            MainScrollViewer.Effect = null;
            _assignmentToDelete = null;
        }

        private async void ConfirmDeleteAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (_assignmentToDelete != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_assignmentToDelete.AttachedFileUrl))
                    {
                        string oldPublicId = GetPublicIdFromUrl(_assignmentToDelete.AttachedFileUrl);
                        if (!string.IsNullOrEmpty(oldPublicId))
                        {
                            await _cloudinary.DestroyAsync(new DeletionParams($"assignments/{_course.Id}/{oldPublicId}") { ResourceType = ResourceType.Raw });
                        }
                    }

                    await _dbManager.GetDb.Collection("Courses").Document(_course.Id).Collection("Assignments").Document(_assignmentToDelete.Id).DeleteAsync();

                    _assignments.Remove(_assignmentToDelete);
                    TxtAssignmentCount.Text = _assignments.Count.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa bài tập: " + ex.Message);
                }
            }
            CloseDeleteAssignmentModal_Click(null, null);
        }

        private void CloseAddAssignmentDrawer_Click(object sender, RoutedEventArgs e)
        {
            AddAssignmentDrawer.Visibility = Visibility.Collapsed;
            MainScrollViewer.Effect = null;
            _editingAssignment = null;
        }

        private void AssignDropZone_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "All Files|*.*" };
            if (dialog.ShowDialog() == true)
            {
                _selectedAssignFilePath = dialog.FileName;
                TxtAssignSelectedFile.Text = "Đã chọn: " + Path.GetFileName(_selectedAssignFilePath);
            }
        }

        private void AssignDropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    _selectedAssignFilePath = files[0];
                    TxtAssignSelectedFile.Text = "Đã chọn: " + Path.GetFileName(_selectedAssignFilePath);
                }
            }
        }

        private async void ConfirmAddAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AssignTitleInput.Text) || !AssignDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng nhập Tên bài tập và Chọn ngày Hạn nộp!"); return;
            }

            DateTime deadlineDate = AssignDatePicker.SelectedDate.Value;
            if (!TimeSpan.TryParse(AssignTimeInput.Text, out TimeSpan time)) time = new TimeSpan(23, 59, 0);
            DateTime finalDeadline = deadlineDate.Add(time).ToUniversalTime();

            string fileUrl = "";

            BtnConfirmAddAssignment.Content = "Đang xử lý...";
            BtnConfirmAddAssignment.IsEnabled = false;

            if (!string.IsNullOrWhiteSpace(_selectedAssignFilePath) && File.Exists(_selectedAssignFilePath))
            {
                try
                {
                    string originalFileName = Path.GetFileNameWithoutExtension(_selectedAssignFilePath);
                    originalFileName = string.Join("_", originalFileName.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
                    string extension = Path.GetExtension(_selectedAssignFilePath);

                    string customPublicId = $"assignments/{_course.Id}/{DateTime.Now.Ticks}_{originalFileName}{extension}";

                    var uploadParams = new RawUploadParams()
                    {
                        File = new FileDescription(_selectedAssignFilePath),
                        PublicId = customPublicId,
                        UseFilename = true,
                        UniqueFilename = false
                    };
                    var res = await _cloudinary.UploadAsync(uploadParams);
                    fileUrl = res.SecureUrl.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải file đính kèm: " + ex.Message);
                    BtnConfirmAddAssignment.Content = _editingAssignment == null ? "Tạo bài tập" : "Lưu thay đổi";
                    BtnConfirmAddAssignment.IsEnabled = true; return;
                }
            }
            else
            {
                if (_editingAssignment != null) fileUrl = _editingAssignment.AttachedFileUrl;
            }

            try
            {
                var collectionRef = _dbManager.GetDb.Collection("Courses").Document(_course.Id).Collection("Assignments");

                if (_editingAssignment != null)
                {
                    if (!string.IsNullOrEmpty(_editingAssignment.AttachedFileUrl) && _editingAssignment.AttachedFileUrl != fileUrl)
                    {
                        string oldPublicId = GetPublicIdFromUrl(_editingAssignment.AttachedFileUrl);
                        if (!string.IsNullOrEmpty(oldPublicId))
                            await _cloudinary.DestroyAsync(new DeletionParams($"assignments/{_course.Id}/{oldPublicId}") { ResourceType = ResourceType.Raw });
                    }

                    _editingAssignment.Title = AssignTitleInput.Text;
                    _editingAssignment.Description = AssignDescInput.Text;
                    _editingAssignment.Deadline = finalDeadline;
                    _editingAssignment.AttachedFileUrl = fileUrl;

                    await collectionRef.Document(_editingAssignment.Id).SetAsync(_editingAssignment, Google.Cloud.Firestore.SetOptions.MergeAll);
                }
                else
                {
                    var newAssignment = new Assignment
                    {
                        CourseId = _course.Id,
                        Title = AssignTitleInput.Text,
                        Description = AssignDescInput.Text,
                        Deadline = finalDeadline,
                        AttachedFileUrl = fileUrl,
                        CreatedAt = DateTime.UtcNow
                    };
                    await collectionRef.AddAsync(newAssignment);
                }

                LoadAssignments();
                CloseAddAssignmentDrawer_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi lưu Database: " + ex.Message); }
            finally
            {
                BtnConfirmAddAssignment.Content = _editingAssignment == null ? "Tạo bài tập" : "Lưu thay đổi";
                BtnConfirmAddAssignment.IsEnabled = true;
            }
        }

        private async void AssignmentItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as FrameworkElement;
            _currentViewedAssignment = border?.DataContext as Assignment;

            if (_currentViewedAssignment != null)
            {
                DetailAssignTitle.Text = _currentViewedAssignment.Title;
                DetailAssignDeadline.Text = _currentViewedAssignment.Deadline.ToString("dd/MM/yyyy HH:mm");
                DetailAssignDesc.Text = string.IsNullOrWhiteSpace(_currentViewedAssignment.Description) ? "Không có mô tả thêm." : _currentViewedAssignment.Description;

                bool isLate = DateTime.Now > _currentViewedAssignment.Deadline;
                if (isLate)
                {
                    DetailAssignDeadline.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    DetailAssignDeadlineBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2"));
                    DetailAssignDeadlineBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FECACA"));
                }
                else
                {
                    DetailAssignDeadline.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    DetailAssignDeadlineBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECFDF5"));
                    DetailAssignDeadlineBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A7F3D0"));
                }

                if (string.IsNullOrEmpty(_currentViewedAssignment.AttachedFileUrl))
                {
                    BtnDetailDownloadAttach.Visibility = Visibility.Collapsed;
                }
                else
                {
                    BtnDetailDownloadAttach.Visibility = Visibility.Visible;
                    BtnDetailDownloadAttach.Tag = _currentViewedAssignment.AttachedFileUrl;
                }

                var converter = new BrushConverter();
                Brush accentBrush = Brushes.SlateBlue;
                try { accentBrush = (SolidColorBrush)converter.ConvertFromString(_course.AccentColor); } catch { }
                BtnDetailSubmit.Background = accentBrush;
                DropZoneBorder.Stroke = accentBrush;
                BtnDownloadAll.Background = accentBrush;

                _selectedSubmissionFilePath = "";
                TxtSelectedFile.Text = "Kéo thả tệp vào đây hoặc nhấn để chọn";

                bool isInstructor = _course.InstructorId == CurrentUserId;

                StudentSubmitArea.Visibility = isInstructor ? Visibility.Collapsed : Visibility.Visible;
                InstructorManageArea.Visibility = isInstructor ? Visibility.Visible : Visibility.Collapsed;

                if (isInstructor)
                {
                    BtnDownloadAll.IsEnabled = false;
                    TxtSubmissionCount.Text = "Đang kiểm tra số lượng bài nộp...";
                    try
                    {
                        var subsRef = _dbManager.GetDb.Collection("Courses").Document(_course.Id)
                                             .Collection("Assignments").Document(_currentViewedAssignment.Id)
                                             .Collection("Submissions");
                        var snap = await subsRef.GetSnapshotAsync();
                        int count = snap.Count;
                        TxtSubmissionCount.Text = $"Đã có {count} sinh viên nộp bài.";
                        BtnDownloadAll.IsEnabled = count > 0;
                    }
                    catch { TxtSubmissionCount.Text = "Lỗi khi kiểm tra dữ liệu."; }
                }
                else
                {
                    DetailSubmissionStatus.Text = "Đang kiểm tra...";
                    DetailSubmissionStatus.Foreground = Brushes.Gray;
                    BtnDetailSubmit.Content = "Đang kiểm tra...";
                    BtnDetailSubmit.IsEnabled = false;
                    SubmittedFileDetailsPanel.Visibility = Visibility.Collapsed;

                    try
                    {
                        var subRef = _dbManager.GetDb.Collection("Courses").Document(_course.Id)
                                             .Collection("Assignments").Document(_currentViewedAssignment.Id)
                                             .Collection("Submissions").Document(CurrentUserId);
                        var snapshot = await subRef.GetSnapshotAsync();

                        if (snapshot.Exists)
                        {
                            var sub = snapshot.ConvertTo<Submission>();

                            DetailSubmissionStatus.Text = "Đã nộp bài";
                            DetailSubmissionStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                            BtnDetailSubmit.Content = "Nộp lại tệp khác";

                            string displayFileName = GetDisplayNameFromUrl(sub.FileUrl, "submission");

                            TxtSubmittedFileName.Text = "Tệp: " + displayFileName;
                            TxtSubmittedTime.Text = "Thời gian nộp: " + sub.SubmittedAt.ToLocalTime().ToString("HH:mm - dd/MM/yyyy");

                            SubmittedFileDetailsPanel.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            DetailSubmissionStatus.Text = "Chưa nộp";
                            DetailSubmissionStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                            BtnDetailSubmit.Content = "Tải lên tệp bài làm";
                        }
                    }
                    catch
                    {
                        DetailSubmissionStatus.Text = "Không rõ";
                    }
                    BtnDetailSubmit.IsEnabled = true;
                }

                MainScrollViewer.Effect = new BlurEffect { Radius = 10 };
                AssignmentDetailDrawer.Visibility = Visibility.Visible;
            }
        }

        private void CloseAssignmentDetail_Click(object sender, RoutedEventArgs e)
        {
            AssignmentDetailDrawer.Visibility = Visibility.Collapsed;
            MainScrollViewer.Effect = null;
            _currentViewedAssignment = null;
        }

        private async void BtnDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            if (_currentViewedAssignment == null) return;

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Lưu tệp nén chứa bài nộp",
                Filter = "ZIP Archive|*.zip",
                FileName = $"Submissions_{_currentViewedAssignment.Title.Replace(" ", "_")}.zip"
            };

            if (dialog.ShowDialog() == true)
            {
                BtnDownloadAll.Content = "Đang chuẩn bị dữ liệu...";
                BtnDownloadAll.IsEnabled = false;

                try
                {
                    var subsRef = _dbManager.GetDb.Collection("Courses").Document(_course.Id)
                                         .Collection("Assignments").Document(_currentViewedAssignment.Id)
                                         .Collection("Submissions");
                    var snapshot = await subsRef.GetSnapshotAsync();

                    if (snapshot.Count == 0)
                    {
                        MessageBox.Show("Chưa có sinh viên nào nộp bài.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    using (FileStream zipToOpen = new FileStream(dialog.FileName, FileMode.Create))
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    using (HttpClient client = new HttpClient())
                    {
                        int currentIndex = 0;
                        foreach (var doc in snapshot.Documents)
                        {
                            currentIndex++;
                            BtnDownloadAll.Content = $"Đang tải ({currentIndex}/{snapshot.Count})...";

                            var sub = doc.ConvertTo<Submission>();
                            string displayFileName = GetDisplayNameFromUrl(sub.FileUrl, "submission");
                            string finalZipName = sub.IsLate ? $"late_{sub.StudentId}_{displayFileName}" : $"{sub.StudentId}_{displayFileName}";

                            var fileBytes = await client.GetByteArrayAsync(sub.FileUrl);

                            ZipArchiveEntry fileEntry = archive.CreateEntry(finalZipName);
                            using (Stream writer = fileEntry.Open())
                            {
                                await writer.WriteAsync(fileBytes, 0, fileBytes.Length);
                            }
                        }
                    }

                    MessageBox.Show("Đã tải xuống thành công tất cả bài nộp!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    BtnDownloadAll.Content = "Tải về tất cả bài nộp (.zip)";
                    BtnDownloadAll.IsEnabled = true;
                }
            }
        }

        private void DownloadAttachedFile_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string url = btn?.Tag?.ToString();
            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
        }

        private void DropZone_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Title = "Chọn tệp bài làm của bạn", Filter = "All Files|*.*" };
            if (dialog.ShowDialog() == true)
            {
                _selectedSubmissionFilePath = dialog.FileName;
                TxtSelectedFile.Text = "Đã chọn: " + Path.GetFileName(_selectedSubmissionFilePath);
            }
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    _selectedSubmissionFilePath = files[0];
                    TxtSelectedFile.Text = "Đã chọn: " + Path.GetFileName(_selectedSubmissionFilePath);
                }
            }
        }

        private async void BtnSubmitAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (_currentViewedAssignment == null) return;

            if (string.IsNullOrEmpty(_selectedSubmissionFilePath) || !File.Exists(_selectedSubmissionFilePath))
            {
                MessageBox.Show("Vui lòng chọn hoặc kéo thả tệp bài làm của bạn vào ô nét đứt trước khi nộp!", "Chưa chọn tệp", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            BtnDetailSubmit.Content = "Đang xử lý...";
            BtnDetailSubmit.IsEnabled = false;

            try
            {
                var subRef = _dbManager.GetDb.Collection("Courses").Document(_course.Id)
                                     .Collection("Assignments").Document(_currentViewedAssignment.Id)
                                     .Collection("Submissions").Document(CurrentUserId);

                var snapshot = await subRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var oldSubmission = snapshot.ConvertTo<Submission>();
                    if (!string.IsNullOrEmpty(oldSubmission.FileUrl))
                    {
                        try
                        {
                            string exactPublicId = GetPublicIdFromUrl(oldSubmission.FileUrl);
                            if (!string.IsNullOrEmpty(exactPublicId))
                            {
                                string fullOldPublicId = $"submissions/{_course.Id}/{_currentViewedAssignment.Id}/{exactPublicId}";
                                await _cloudinary.DestroyAsync(new DeletionParams(fullOldPublicId) { ResourceType = ResourceType.Raw });
                            }
                        }
                        catch { }
                    }
                }

                bool isLate = DateTime.Now > _currentViewedAssignment.Deadline;

                string originalFileName = Path.GetFileNameWithoutExtension(_selectedSubmissionFilePath);
                originalFileName = string.Join("_", originalFileName.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
                string extension = Path.GetExtension(_selectedSubmissionFilePath);

                string customPublicId = $"submissions/{_course.Id}/{_currentViewedAssignment.Id}/{CurrentUserId}_{DateTime.Now.Ticks}_{originalFileName}{extension}";

                var uploadParams = new RawUploadParams()
                {
                    File = new FileDescription(_selectedSubmissionFilePath),
                    PublicId = customPublicId,
                    UseFilename = true,
                    UniqueFilename = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null) throw new Exception(uploadResult.Error.Message);

                var submission = new Submission
                {
                    AssignmentId = _currentViewedAssignment.Id,
                    StudentId = CurrentUserId,
                    FileUrl = uploadResult.SecureUrl.ToString(),
                    SubmittedAt = DateTime.UtcNow,
                    IsLate = isLate
                };

                await subRef.SetAsync(submission);

                DetailSubmissionStatus.Text = "Đã nộp bài";
                DetailSubmissionStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                BtnDetailSubmit.Content = "Nộp lại tệp khác";

                SubmittedFileDetailsPanel.Visibility = Visibility.Visible;
                TxtSubmittedFileName.Text = "Tệp: " + Path.GetFileName(_selectedSubmissionFilePath);
                TxtSubmittedTime.Text = "Thời gian nộp: " + DateTime.Now.ToString("HH:mm - dd/MM/yyyy");

                _selectedSubmissionFilePath = "";
                TxtSelectedFile.Text = "Kéo thả tệp vào đây hoặc nhấn để chọn";

                string statusMsg = isLate ? "Đã cập nhật bài nộp (TRỄ HẠN) thành công!" : "Đã cập nhật bài nộp (ĐÚNG HẠN) thành công!";
                MessageBox.Show(statusMsg, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong quá trình nộp bài: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnDetailSubmit.Content = DetailSubmissionStatus.Text == "Đã nộp bài" ? "Nộp lại tệp khác" : "Tải lên tệp bài làm";
            }
            finally
            {
                BtnDetailSubmit.IsEnabled = true;
            }
        }

        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            EditTitleInput.Text = _course.Title; EditDescInput.Text = _course.Description;
            EditClassInput.Text = _course.ClassName; EditEmojiInput.Text = _course.Emoji;
            EditCategoryInput.Text = _course.Category;
            SetSelectedColor(_course.AccentColor);
            SetComboBoxByContent(EditTypeInput, _course.CourseType);

            if (!string.IsNullOrEmpty(_course.Semester) && _course.Semester.Contains(" - "))
            {
                string[] parts = _course.Semester.Split(new[] { " - " }, StringSplitOptions.None);
                SetComboBoxByContent(EditSemesterInput, parts[0].Trim());
                if (parts.Length > 1)
                {
                    string yearValue = parts[1].Trim();
                    foreach (var item in EditYearInput.Items)
                        if (item.ToString() == yearValue) { EditYearInput.SelectedItem = item; break; }
                }
            }
            MainScrollViewer.Effect = new BlurEffect { Radius = 10 }; EditDrawer.Visibility = Visibility.Visible;
        }

        private void SetComboBoxByContent(ComboBox cb, string value)
        {
            foreach (ComboBoxItem item in cb.Items) if (item.Content.ToString() == value) { cb.SelectedItem = item; return; }
        }

        private async void ConfirmEdit_Click(object sender, RoutedEventArgs e)
        {
            _course.Title = EditTitleInput.Text; _course.Description = EditDescInput.Text;
            _course.ClassName = EditClassInput.Text; _course.Emoji = EditEmojiInput.Text;
            _course.Category = EditCategoryInput.Text; _course.AccentColor = GetSelectedColor();
            _course.CourseType = (EditTypeInput.SelectedItem as ComboBoxItem)?.Content.ToString();
            string sem = (EditSemesterInput.SelectedItem as ComboBoxItem)?.Content.ToString();
            string year = EditYearInput.SelectedItem?.ToString();
            _course.Semester = $"{sem} - {year}";

            if (await _dbManager.UpdateCourseAsync(_course))
            {
                UpdateUI(); CloseEditDrawer_Click(null, null);
            }
        }

        private void CloseEditDrawer_Click(object sender, RoutedEventArgs e)
        {
            EditDrawer.Visibility = Visibility.Collapsed; MainScrollViewer.Effect = null;
        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            MainScrollViewer.Effect = new BlurEffect { Radius = 10 }; DeleteOverlay.Visibility = Visibility.Visible;
        }

        private async void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (await _dbManager.DeleteCourseAsync(_course.Id)) NavigateBack();
        }

        private void CloseDeleteModal_Click(object sender, RoutedEventArgs e)
        {
            DeleteOverlay.Visibility = Visibility.Collapsed; MainScrollViewer.Effect = null;
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
            if (mainWin != null) mainWin.MainContentArea.Content = new MyClassesView(_dbManager, CurrentUserId);
        }

        private void SetSelectedColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor)) return;
            try
            {
                Color targetColor = (Color)ColorConverter.ConvertFromString(hexColor);
                var radioButtons = FindVisualChildren<RadioButton>(EditDrawer).Where(r => r.GroupName == "ThemeColors");
                foreach (var rb in radioButtons)
                    if (rb.Background is SolidColorBrush brush && brush.Color == targetColor) { rb.IsChecked = true; break; }
            }
            catch { }
        }

        private string GetSelectedColor()
        {
            var radioButtons = FindVisualChildren<RadioButton>(EditDrawer).Where(r => r.GroupName == "ThemeColors");
            foreach (var rb in radioButtons)
                if (rb.IsChecked == true && rb.Background is SolidColorBrush brush)
                    return $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
            return "#3B82F6";
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T) yield return (T)child;
                    foreach (T childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
                }
            }
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = e.OriginalSource as DependencyObject;
            var clickedItem = FindVisualParent<ListBoxItem>(clickedElement);
            if (clickedItem == null) DocumentsList.SelectedItem = null;
        }

        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = child;
            while (parentObject != null)
            {
                if (parentObject is T parent) return parent;
                if (parentObject is FrameworkContentElement contentElement) parentObject = contentElement.Parent;
                else parentObject = VisualTreeHelper.GetParent(parentObject);
            }
            return null;
        }
    }
}