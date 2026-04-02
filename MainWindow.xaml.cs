using e_learning_app.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace e_learning_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            btnDashBoard.Focus();
        }
        private void NavDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new TeacherDashboardView();
        }

        private void NavClasses_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new MyClassesView();
        }
        private void NavQuestions_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new QuestionBankView();
        }
        private void NavReports_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new ReportsView();
        }
        private void NavNotifications_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new NotificationsView();
        }
        private void NavSemestersettings_Click(object sender, RoutedEventArgs e)
        {
            MainContentArea.Content = new SemesterSettingsView();
        }


        public void NavigateTo(UserControl view) => MainContentArea.Content = view;
    }
}