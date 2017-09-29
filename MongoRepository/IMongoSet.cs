using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mongo.Context
{
    public interface IMongoSet<TEntity> : IQueryable<TEntity>, IQueryable, IEnumerable<TEntity> where TEntity : class
    {
        string CollectionName { get; }
        void Insert(TEntity item);
        Task InsertAsync(TEntity item);
        void InsertBatch(IEnumerable<TEntity> items);
        Task InsertBatchAsync(IEnumerable<TEntity> items);
        long Remove(Expression<Func<TEntity, bool>> criteria);
        Task<long> RemoveAsync(Expression<Func<TEntity, bool>> criteria);
        long RemoveAll();
        long Update<TMember>(Expression<Func<TEntity, TMember>> propertySelector, TMember value, Expression<Func<TEntity, bool>> criteria);
        Task<long> UpdateAsync<TMember>(Expression<Func<TEntity, TMember>> propertySelector, TMember value, Expression<Func<TEntity, bool>> criteria);
        void Save(TEntity item);
        Task SaveAsync(TEntity item);
        bool Contains(TEntity item);
        Task<bool> ContainsAsync(TEntity item);
    }
}
