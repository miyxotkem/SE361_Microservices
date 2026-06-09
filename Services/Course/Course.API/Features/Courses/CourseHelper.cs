using Google.Cloud.Firestore;

namespace Course.API.Features.Courses
{
    public static class CourseHelper
    {
        public static object ConvertFirestoreTypes(object value)
        {
            if (value is Timestamp timestamp)
            {
                return timestamp.ToDateTime().ToUniversalTime();
            }
            if (value is Dictionary<string, object> dict)
            {
                var newDict = new Dictionary<string, object>();
                foreach (var kvp in dict)
                {
                    newDict[kvp.Key] = ConvertFirestoreTypes(kvp.Value);
                }
                return newDict;
            }
            if (value is List<object> list)
            {
                var newList = new List<object>();
                foreach (var item in list)
                {
                    newList.Add(ConvertFirestoreTypes(item));
                }
                return newList;
            }
            return value;
        }

        public static Dictionary<string, object> ConvertFirestoreTypes(Dictionary<string, object> dict)
        {
            var newDict = new Dictionary<string, object>();
            foreach (var kvp in dict)
            {
                newDict[kvp.Key] = ConvertFirestoreTypes(kvp.Value);
            }
            return newDict;
        }
    }
}
