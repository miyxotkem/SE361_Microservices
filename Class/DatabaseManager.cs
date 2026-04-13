using e_learning_app;
using Firebase.Auth;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;
using System.Windows;

namespace e_learning_app
{
    public class DatabaseManager
    {
        private FirestoreDb _db;
        private User _currentUser = null;

        public FirestoreDb GetDb => _db;

        public User GetCurrentUser() => _currentUser;
        public void SetCurrentUser(User user) => _currentUser = user;

        public void Initialize()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase", "firebase_json.json");
            if (File.Exists(path))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            }
            if (_db != null) return;

            string jsonPath = path;
            if (!File.Exists(jsonPath))
            {
                MessageBox.Show($"CẢNH BÁO: Không tìm thấy file JSON tại:\n{jsonPath}\n\nHãy kiểm tra lại đường dẫn!",
                                "Lỗi Đường Dẫn File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                _db = FirestoreDb.Create("e-learning-cd1b3");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo Firestore: " + ex.Message);
            }
        }

        // --- User Methods ---
        public async Task<string> AddUserAsync(User user)
        {
            CollectionReference usersRef = _db.Collection("Users");
            DocumentReference docRef = await usersRef.AddAsync(user);
            return docRef.Id;
        }

        public async Task<User> GetUserAsync(string documentId)
        {
            DocumentReference docRef = _db.Collection("Users").Document(documentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<User>() : null;
        }

        public async Task UpdateFullProfile(string userId, User updatedUser)
        {
            DocumentReference docRef = _db.Collection("Users").Document(userId);
            await docRef.SetAsync(updatedUser, SetOptions.Overwrite);
        }

        // --- Course Methods ---
        public async Task<bool> CreateCourseAsync(Course course)
        {
            try
            {
                if (_db == null) return false;
                await _db.Collection("Courses").Document(course.Id).SetAsync(course);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCourseAsync(Course course)
        {
            try
            {
                DocumentReference docRef = _db.Collection("Courses").Document(course.Id);

                await docRef.SetAsync(course, SetOptions.Overwrite);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCourseAsync(string courseId)
        {
            try
            {
                DocumentReference docRef = _db.Collection("Courses").Document(courseId);
                await docRef.DeleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete Error: {ex.Message}");
                return false;
            }
        }
    }
}