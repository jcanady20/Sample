using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using System.Reflection;
using Mongo.Context.Mapping;

namespace Mongo.Context
{
    public class MongoBuilder : IMongoBuilder
    {
        private Type _contextType;
        private MongoContext _context;
        private static readonly Object _objLock = new object();
        private readonly IDictionary<Type, MongoClassMap> _typeClassMaps;
        public MongoBuilder(MongoContext context)
        {
            _typeClassMaps = new Dictionary<Type, MongoClassMap>();
            _contextType = context.GetType();
            _context = context;
        }

        public bool IsFrozen
        {
            get
            {
                return _typeClassMaps.Values.Any(x => x.IsFrozen);
            }
        }

        public IDictionary<Type, object> InitializeSets()
        {
            var setFinder = new Internal.MongoSetFinder();
            var setSource = new Internal.MongoSetSource();
            var setInitializer = new Internal.MongoSetInitializer(setFinder, setSource);
            return setInitializer.InitializeSets(_context, _typeClassMaps);
        }

        public void InitializeIndexes()
        {
            var setFinder = new Internal.MongoSetFinder();
            foreach (var setinfo in setFinder.FindSets(_context))
            {
                var mcm = _typeClassMaps[setinfo.EntityType];
                if (mcm == null)
                {
                    continue;
                }
                _context.EnsureIndexes(mcm.CollectionName, mcm.Indexes);
            }
        }

        public MongoClassMap<T> Entry<T>()
        {
            return (MongoClassMap<T>)_typeClassMaps[typeof(T)];
        }

        public void FromAssembly(Assembly assembly)
        {
            lock (_objLock)
            {
                var classMaps = assembly.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(MongoClassMap)));
                foreach (var cm in classMaps)
                {
                    var baseType = cm.BaseType;
                    //  Check to see if this type has already been registered
                    var instance = (MongoClassMap)Activator.CreateInstance(cm);
                    if(_typeClassMaps.ContainsKey(instance.ClassType))
                    {
                        throw new ArgumentException($"ClassMaps already contains a reference to the type '{instance.ClassType}'");
                    }
                    _typeClassMaps.Add(instance.ClassType, instance);
                    if (baseType.IsGenericType)
                    {
                        var AssignableType = baseType.GetGenericArguments().First();
                        if (MongoClassMap.IsClassMapRegistered(AssignableType) == false)
                        {
                            MongoClassMap.RegisterClassMap(instance);
                        }
                    }
                }
            }
        }

        public MongoClassMap LookupClassMap(Type type)
        {
            return _typeClassMaps[type];
        }
    }
}
