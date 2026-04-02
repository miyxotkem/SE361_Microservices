using Google.Cloud.Firestore;

namespace FirebaseIntegration
{
    [FirestoreData]
    public class User
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Username { get; set; }

        [FirestoreProperty]
        public string Password { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string FullName { get; set; }

        [FirestoreProperty]
        public string PhoneNumber { get; set; }

        [FirestoreProperty]
        public string Role { get; set; }
    }
}