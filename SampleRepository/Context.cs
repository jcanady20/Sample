using Mongo.Context;

namespace SampleRepository
{
    internal class Context : Mongo.Context.MongoContext
    {
        public Context(string connectionString) : base(connectionString)
        { }

        public MongoSet<Models.User> Users { get; set; }
        public MongoSet<Models.Permission> Permissions { get; set; }

        protected override void OnRegisterClasses(MongoBuilder mongoBuilder)
        {
            base.OnRegisterClasses(mongoBuilder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
