using System;

namespace Mongo.Context
{
    public static class MongoId
    {
        public static string GenerateId()
        {
            return GenerateId(DateTime.UtcNow);
        }
        public static string GenerateId(DateTime datetime)
        {
            return MongoDB.Bson.ObjectId.GenerateNewId(datetime).ToString();
        }
        public static string Empty()
        {
            return MongoDB.Bson.ObjectId.Empty.ToString();
        }
    }
}
