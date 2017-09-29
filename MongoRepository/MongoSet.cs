using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Bson;
using Mongo.Context.Extensions;
using System.Threading.Tasks;

namespace Mongo.Context
{
    /// <summary>
    /// MongoCollection<typeparamref name="T"/> wrapper to allow Linq and expression based access to
    /// the collection without the need to switch between MongoQuery and Linq query operators.
    /// </summary>
    /// <typeparam name="T">Class implementing IMongoEntity</typeparam>
    public class MongoSet<TEntity> : IMongoSet<TEntity> where TEntity : class
    {
        private string _collectionName;
        private MongoContext _context;

        public MongoSet(MongoContext context)
        {
            _collectionName = Internal.NamePluralization.GetCollectionName(ElementType);
            _context = context;
        }

        public MongoSet(MongoContext context, string collectionName)
        {
            _collectionName = collectionName;
            _context = context;
        }

        /// <summary>
        /// Underlying MongoCollection for this MongoSet
        /// </summary>
        protected IMongoCollection<TEntity> Collection =>  _context.GetDatabase().GetCollection<TEntity>(_collectionName);

        public string CollectionName => _collectionName;

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Collection.AsQueryable<TEntity>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type ElementType =>  typeof(TEntity);

        public Expression Expression => Collection.AsQueryable().Expression;

        public IQueryProvider Provider => Collection.AsQueryable().Provider;

        /// <summary>
        /// Inserts a new item
        /// </summary>
        /// <param name="item">The item to insert</param>
        /// <exception cref="MongoDB.Drive.MongoException" />
        public void Insert(TEntity item)
        {
            this.Collection.InsertOne(item);
        }

        /// <summary>
        /// Inserts an IEnumerable of items in a batch
        /// </summary>
        /// <param name="items">The items to insert</param>
        public void InsertBatch(IEnumerable<TEntity> items)
        {
            this.Collection.InsertMany(items);
        }

        /// <summary>
        /// Saves the item
        /// </summary>
        /// <param name="item">The item to save</param>
        [Obsolete("This method is no longer supported at the driver level, will be removed in a future update", false)]
        public void Save(TEntity item)
        {
            var expression = CreateExpression(item);
            var result = this.Collection.ReplaceOne(expression, item, new UpdateOptions() { IsUpsert = true });
        }

        /// <summary>
        /// Remove the item
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>The number of records affected. If WriteConcern is unacknowledged -1 is returned</returns>
        public long Remove(TEntity item)
        {
            var classMap = _context.Builder.LookupClassMap(typeof(TEntity));
            if (classMap == null)
            {
                throw new ArgumentNullException(nameof(classMap));
            }
            var value = classMap.IdMemberMap.Getter(item);
            var elName = classMap.IdMemberMap.ElementName;

            var filter = new BsonDocument(elName, BsonValue.Create(value));
            var result = Collection.DeleteOne(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// Remove the item/s matching the criteria
        /// </summary>
        /// <param name="criteria">criteria expression</param>
        /// <returns>The number of records affected. If WriteConcern is unacknowledged -1 is returned</returns>
        public long Remove(Expression<Func<TEntity, bool>> criteria)
        {
            var result = this.Collection.DeleteOne(criteria);
            return result.DeletedCount;
        }

        /// <summary>
        /// Removes all items
        /// </summary>
        /// <returns>The number of records affected. If WriteConcern is unacknowledged -1 is returned</returns>
        /// <remarks>Careful this deletes everything in the MongoSet/Collection!</remarks>
        [Obsolete("Extremely Dangerous Code Path", false)]
        public long RemoveAll()
        {
            var filter = new BsonDocument();
            var result = Collection.DeleteMany(filter);
            return result.DeletedCount;
        }

        /// <summary>
        /// Update one property of an object.
        /// </summary>
        /// <typeparam name="TMember">The type of the property to be updated</typeparam>
        /// <param name="propertySelector">The property selector expression</param>
        /// <param name="value">New value of the property</param>
        /// <param name="criteria">Criteria to update documents based on</param>
        /// <returns>The number of records affected. If WriteConcern is unacknowledged -1 is returned</returns>
        public long Update<TMember>(Expression<Func<TEntity, TMember>> propertySelector, TMember value, Expression<Func<TEntity, bool>> criteria)
        {
            var updateBuilder = new UpdateDefinitionBuilder<TEntity>();
            var updateDefinition = updateBuilder.Set<TMember>(propertySelector, value);
            return this.Update(updateDefinition, criteria);
        }

        /// <summary>
        /// Update with UpdateBuilder. Use MongoSet<typeparamref name"T"/>.Set().Set()... to build update
        /// </summary>
        /// <param name="update">The update object</param>
        /// <param name="criteria">Criteria to update documents based on</param>
        /// <returns></returns>
        private long Update(UpdateDefinition<TEntity> update, Expression<Func<TEntity, bool>> criteria)
        {
            var updateOptions = new UpdateOptions();
            var result = this.Collection.UpdateMany(criteria, update, updateOptions);
            return result.ModifiedCount;
        }

        public string GetElementName(Expression<Func<TEntity, object>> propertyExpression)
        {
            var result = String.Empty;
            var classMap = _context.Builder.LookupClassMap(typeof(TEntity));
            if (classMap == null)
            {
                throw new ArgumentException("No ClassMap reference was found.", nameof(propertyExpression));
            }
            var propertyName = PropertyHelpers.GetPropertyName<TEntity>(propertyExpression);
            var memberMap = classMap.GetMemberMap(propertyName);
            if (memberMap == null)
            {
                throw new ArgumentException("No MemberMap reference was found.", nameof(memberMap));
            }
            return memberMap.ElementName;
        }

        public bool Contains(TEntity item)
        {
            var classMap = _context.Builder.LookupClassMap(typeof(TEntity));
            if (classMap == null)
            {
                throw new ArgumentNullException(nameof(classMap));
            }
            var idName = classMap.IdMemberMap.ElementName;
            var value = classMap.IdMemberMap.Getter(item);
            var filter = new BsonDocument(idName, BsonValue.Create(value));
            var result = Collection.Find<TEntity>(filter);
            return result != null;
        }

        public async Task InsertAsync(TEntity item)
        {
            await this.Collection.InsertOneAsync(item);
        }

        public async Task InsertBatchAsync(IEnumerable<TEntity> items)
        {
            await this.Collection.InsertManyAsync(items);
        }

        public async Task<long> RemoveAsync(TEntity item)
        {
            var classMap = _context.Builder.LookupClassMap(typeof(TEntity));
            if (classMap == null)
            {
                throw new ArgumentNullException(nameof(classMap));
            }
            var value = classMap.IdMemberMap.Getter(item);
            var elName = classMap.IdMemberMap.ElementName;

            var filter = new BsonDocument(elName, BsonValue.Create(value));
            var result = await Collection.DeleteOneAsync(filter);
            return result.DeletedCount;
        }

        public async Task<long> RemoveAsync(Expression<Func<TEntity, bool>> criteria)
        {
            var result = await this.Collection.DeleteOneAsync(criteria);
            return result.DeletedCount;
        }

        public async Task<long> UpdateAsync<TMember>(Expression<Func<TEntity, TMember>> propertySelector, TMember value, Expression<Func<TEntity, bool>> criteria)
        {
            var updateBuilder = new UpdateDefinitionBuilder<TEntity>();
            var updateDefinition = updateBuilder.Set<TMember>(propertySelector, value);
            return await this.UpdateAsync(updateDefinition, criteria);
        }

        private async Task<long> UpdateAsync(UpdateDefinition<TEntity> update, Expression<Func<TEntity, bool>> criteria)
        {
            var updateOptions = new UpdateOptions();
            var result = await this.Collection.UpdateManyAsync(criteria, update, updateOptions);
            return result.ModifiedCount;
        }

        [Obsolete("This method is no longer supported at the driver level, will be removed in a future update", false)]
        public async Task SaveAsync(TEntity item)
        {
            var expression = CreateExpression(item);
            await this.Collection.ReplaceOneAsync<TEntity>(expression, item, new UpdateOptions() { IsUpsert = true });
        }

        public async Task<bool> ContainsAsync(TEntity item)
        {
            var classMap = _context.Builder.LookupClassMap(typeof(TEntity));
            if (classMap == null)
            {
                throw new ArgumentNullException(nameof(classMap));
            }
            var idName = classMap.IdMemberMap.ElementName;
            var value = classMap.IdMemberMap.Getter(item);
            var filter = new BsonDocument(idName, BsonValue.Create(value));
            var result = await Collection.FindAsync<TEntity>(filter);
            return result != null;
        }

        private Expression<Func<TEntity, bool>> CreateExpression(TEntity item)
        {
            var classMap = _context.Builder.LookupClassMap(typeof(TEntity));
            if (classMap.IdMemberMap == null)
            {
                throw new ArgumentNullException($"This operation requires an IdMemberMap for type: {typeof(TEntity).Name}. Check mongo mapping for this object type.");
            }
            //  Dynamically build the filter expression tree for this request
            //  x => x.Id == value
            var param = Expression.Parameter(typeof(TEntity), "x");
            var oId = Expression.PropertyOrField(param, classMap.IdMemberMap.MemberName);
            var val = classMap.IdMemberMap.Getter(item);
            var body = Expression.Equal(oId, Expression.Constant(val));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(body, param);
            return lambda;
        }
    }
}
