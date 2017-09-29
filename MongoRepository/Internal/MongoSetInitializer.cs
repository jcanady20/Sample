using Mongo.Context.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mongo.Context.Internal
{
    public class MongoSetInitializer : IMongoSetInitializer
    {
        private readonly IMongoSetFinder _setFinder;
        private readonly IMongoSetSource _setSource;
        public MongoSetInitializer(IMongoSetFinder setFinder, IMongoSetSource setSource)
        {
            _setFinder = setFinder;
            _setSource = setSource;
        }

        public MongoSet<TEntity> CreateSet<TEntity>(MongoContext context, string collectionName) where TEntity : class
        {
            return (MongoSet<TEntity>)_setSource.Create(context, collectionName, typeof(TEntity));
        }

        public IDictionary<Type, object> InitializeSets(MongoContext context, IDictionary<Type, MongoClassMap> classMaps)
        {
            var map = new Dictionary<Type, object>();
            foreach (var setInfo in _setFinder.FindSets(context).Where(x => x.Setter != null))
            {
                var type = setInfo.EntityType;
                var cm = classMaps[type];
                var set = _setSource.Create(context, cm?.CollectionName ?? setInfo.CollectionName, setInfo.EntityType);
                setInfo.Setter.SetValue(context, set);
                if (!map.ContainsKey(type))
                {
                    map.Add(type, set);
                }
            }
            return map;
        }
    }
}
