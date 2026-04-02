using Firebase.Auth;
using Google.Cloud.Firestore;
using System;
using System.Threading.Tasks;

namespace FirebaseIntegration
{
    public class DatabaseManager
    {
        private FirestoreDb db;
        public FirestoreDb GetDb => db;

        private User currentUser;
        public User GetCurrentUser() => currentUser;
        public void SetCurrentUser(User user)
        {
            this.currentUser = user;
        }

        public void Initialize()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\Users\\Thinh Phat\\Documents\\UIT\\SE104_E-learningSystem\\firebase\\google_json.json");
            db = FirestoreDb.Create("e-learning-cd1b3");
        }

        public async Task<string> AddUserAsync(User user)
        {
            CollectionReference usersRef = db.Collection("Users");
            DocumentReference docRef = await usersRef.AddAsync(user);
            return docRef.Id;
        }

        public async Task<User> GetUserAsync(string documentId)
        {
            DocumentReference docRef = db.Collection("Users").Document(documentId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<User>();
            }
            return null;
        }

        public async Task UpdateFullProfile(string userId, User updatedUser)
        {
            DocumentReference docRef = db.Collection("Users").Document(userId);
            await docRef.SetAsync(updatedUser, SetOptions.Overwrite);
        }

        public async Task UpdateContactInfo(string userId, string newName, string newPhone)
        {
            DocumentReference docRef = db.Collection("Users").Document(userId);
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "FullName", newName },
                { "PhoneNumber", newPhone }
            };
            await docRef.UpdateAsync(updates);
        }
    }
}