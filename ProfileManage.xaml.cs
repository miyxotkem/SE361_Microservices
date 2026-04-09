using System;
using System.Windows;
using System.Windows.Controls;
using FirebaseIntegration;
using Google.Cloud.Firestore;
using User = FirebaseIntegration.User;

namespace e_learning_app
{
    public partial class ProfileManage : UserControl
    {
        private DatabaseManager _dbManager;

        public ProfileManage(DatabaseManager dbManager)
        {
            InitializeComponent();
            _dbManager = dbManager;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dbManager.GetCurrentUser() != null)
                {
                    User user = await _dbManager.GetUserAsync(_dbManager.GetCurrentUser().Id);
                    if (user != null)
                    {
                        txtFullName.Text = user.FullName;
                        txtEmail.Text = user.Email;
                        txtPhone.Text = user.PhoneNumber;
                        txtUsername.Text = user.Username;
                        txtRole.Text = user.Role;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user: " + ex.Message);
            }
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            MainProfileUI.Visibility = Visibility.Collapsed;
            FullScreenOverlay.Visibility = Visibility.Visible;
            FullScreenOverlay.Content = new CurrentPassword(this, _dbManager);
        }

        public void ShowNewPasswordView()
        {
            FullScreenOverlay.Content = new NewPassword(this, _dbManager);
        }

        public void ClosePasswordView()
        {
            FullScreenOverlay.Visibility = Visibility.Collapsed;
            MainProfileUI.Visibility = Visibility.Visible;
            FullScreenOverlay.Content = null;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = _dbManager.GetCurrentUser();

                user.FullName = txtFullName.Text;
                user.Email = txtEmail.Text;
                user.PhoneNumber = txtPhone.Text;
                user.Username = txtUsername.Text;
                user.Role = txtRole.Text;

                await _dbManager.UpdateFullProfile(user.Id, user);
                MessageBox.Show("Profile updated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating profile: " + ex.Message);
            }
        }
    }
}