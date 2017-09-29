using System;
using MongoDB.Driver;
using Mongo.Context.Extensions;
using Mongo.Context.Mapping;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace Mongo.Context
{
    /// <summary>
    /// Mongo Context class similar to EntityFrameworks DbContext.
    /// </summary>
    public class MongoContext : IDisposable
    {
        private IMongoDatabase _database;
        private string _databaseName;
        private MongoUrlBuilder _connectionStringBuilder;
        private MongoClient _client;
        private IDictionary<Type, object> _sets;

        public MongoContext(string connectionString, string databaseName = null)
        {
            _connectionStringBuilder = new MongoUrlBuilder(connectionString);
            _connectionStringBuilder.DatabaseName = DatabaseName(databaseName);
            _client = new MongoClient(_connectionStringBuilder.ToMongoUrl());
            RegisterClasses();
        }

        public IMongoDatabase GetDatabase()
        {
            if (_database == null)
            {
                _database = _client.GetDatabase(_databaseName); ;
            }
            return _database;
        }

        public IMongoSet<TEntity> Set<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            IMongoSet<TEntity> set = null;
            if(_sets.ContainsKey(type))
            {
                set = (IMongoSet<TEntity>)_sets[type];
            }
            return set;
        }

        internal MongoBuilder Builder { get; private set; }

        protected virtual void OnRegisterClasses(MongoBuilder mongoBuilder)
        { }

        private void RegisterClasses()
        {
            Builder = Builder ?? new MongoBuilder(this);
            if (!Builder.IsFrozen)
            {
                OnRegisterClasses(Builder);
            }
            _sets = Builder.InitializeSets();
            Builder.InitializeIndexes();
        }

        private string DatabaseName(string databaseName = null)
        {
            if (String.IsNullOrEmpty(databaseName) == false)
            {
                return _databaseName = databaseName;
            }

            if (String.IsNullOrEmpty(_connectionStringBuilder.DatabaseName) == false)
            {
                return _databaseName = _connectionStringBuilder.DatabaseName;
            }

            if (string.IsNullOrEmpty(_databaseName))
            {
                throw new ArgumentException("DatabaseName cannot be null or empty");
            }
            return _databaseName;
        }

        internal void EnsureIndexes(string collectionName, IEnumerable<MongoIndex> indexes)
        {
            if (!indexes.HasItems())
            {
                return;
            }
            var db = GetDatabase();
            if (db == null)
            {
                throw new Exception("Unable to obtain requested Mongo database instance");
            }
            var collection = db.GetCollection<BsonDocument>(collectionName);
            foreach (var idx in indexes)
            {
                var indexKeysBuilder = BuildIndexKeys(idx);
                var indexOptions = BuildIndexOptions(idx);
                collection.Indexes.CreateOne(indexKeysBuilder, indexOptions);
            }
        }

        private IndexKeysDefinition<BsonDocument> BuildIndexKeys(MongoIndex idx)
        {
            var indexKeysBuilder = Builders<BsonDocument>.IndexKeys;
            var indexedKeys = new List<IndexKeysDefinition<BsonDocument>>();
            foreach (var key in idx.Keys)
            {
                if (idx.Desending)
                {
                    indexedKeys.Add(indexKeysBuilder.Descending(key));
                }
                else
                {
                    indexedKeys.Add(indexKeysBuilder.Ascending(key));
                }
            }
            return indexKeysBuilder.Combine(indexedKeys);
        }

        private CreateIndexOptions BuildIndexOptions(MongoIndex idx)
        {
            var indexOptions = new CreateIndexOptions();
            if (idx.Unique)
            {
                indexOptions.Unique = idx.Unique;
            }
            if (idx.TimeToLive > -1)
            {
                indexOptions.ExpireAfter = TimeSpan.FromSeconds(idx.TimeToLive);
            }
            return indexOptions;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connectionStringBuilder = null;
                _databaseName = null;
            }
        }
    }
}