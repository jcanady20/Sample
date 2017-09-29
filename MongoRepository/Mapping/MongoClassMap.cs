﻿using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mongo.Context.Mapping
{
    public class MongoClassMap : BsonClassMap
    {
        protected ICollection<MongoIndex> _indexes = new List<MongoIndex>();
        protected string _collectionName;
        public MongoClassMap(Type classType) : base(classType)
        { }

        public string CollectionName
        {
            get
            {
                return _collectionName;
            }
            set
            {
                _collectionName = value;
            }
        }

        public IEnumerable<MongoIndex> Indexes
        {
            get
            {
                return _indexes;
            }
            set
            {
                _indexes = value.ToList();
            }
        }

        public void SetCollectionName(string collectionName)
        {
            _collectionName = collectionName;
        }

        public MongoClassMap AddIndex(MongoIndex index)
        {
            _indexes.Add(index);
            return this;
        }
    }
}
