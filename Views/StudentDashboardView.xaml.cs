using System;
using System.Windows;
using System.Windows.Controls;

namespace e_learning_app.Views
{
    public partial class StudentDashboardView : UserControl
    {
        public StudentDashboardView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var hour = DateTime.Now.Hour;
            string greeting = hour < 12 ? "Chào buổi sáng" : hour < 18 ? "Chào buổi chiều" : "Chào buổi tối";
            TxtGreeting.Text = $"{greeting}, Trần Minh Anh! 👋";
        }
    }
}
