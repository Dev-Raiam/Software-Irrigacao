using System.Linq.Expressions;
using LiteDB;

namespace Toolbox.Industrial.Core.Data;

public interface IEntityStore
{
    EntityBuilder<TEntity> Configure<TEntity>()
        where TEntity : Entity;

    ILiteQueryable<BsonDocument> Query(string collection);

    ILiteQueryable<TEntity> Query<TEntity>()
        where TEntity : Entity;

    Task<bool> InsertAsync<TEntity>(TEntity entity)
        where TEntity : Entity;

    Task<bool> UpdateAsync<TEntity>(TEntity entity)
        where TEntity : Entity;

    Task<bool> UpsertAsync<TEntity>(TEntity entity)
        where TEntity : Entity;

    Task<int> DeleteAllAsync<TEntity>()
        where TEntity : Entity;

    Task<bool> DeleteAsync<TEntity>(TEntity entity)
        where TEntity : Entity;

    Task<int> DeleteManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity;

    Task<bool> DeleteAllDataCollectionsAsync();

    TEntity FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity;

    Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity;

    Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity;

    bool Any<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity;
}
