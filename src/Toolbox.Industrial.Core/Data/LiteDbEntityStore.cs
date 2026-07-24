using System.Linq.Expressions;
using LiteDB;

namespace Toolbox.Industrial.Core.Data;

internal class LiteDbEntityStore : IEntityStore
{
    private readonly ILiteDatabase _database;

    public LiteDbEntityStore(ILiteDatabase database, EntityConfiguration configuration)
    {
        _database = database;
        if (configuration.ApplyConfiguration != null)
        {
            configuration.ApplyConfiguration(this);
        }
    }

    public EntityBuilder<TEntity> Builder<TEntity>()
        where TEntity : Entity => BsonMapper.Global.Entity<TEntity>();

    public Task<bool> InsertAsync<TEntity>(TEntity entity)
        where TEntity : Entity =>
        Task.FromResult(
            _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Insert(entity) != null
        );

    public Task<bool> UpdateAsync<TEntity>(TEntity entity)
        where TEntity : Entity =>
        Task.FromResult(
            _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Update(entity)
        );

    public Task<bool> UpsertAsync<TEntity>(TEntity entity)
        where TEntity : Entity =>
        Task.FromResult(
            _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Upsert(entity)
        );

    public Task<bool> DeleteAsync<TEntity>(TEntity entity)
        where TEntity : Entity =>
        Task.FromResult(
            _database
                .GetCollection<TEntity>(Entity.GetCollection<TEntity>())
                .Delete(new BsonValue(entity.BsonId))
        );

    public Task<int> DeleteManyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity =>
        Task.FromResult(
            _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).DeleteMany(predicate)
        );

    public Task<int> DeleteAllAsync<TEntity>()
        where TEntity : Entity =>
        Task.FromResult(
            _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).DeleteAll()
        );

    public TEntity FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).FindOne(predicate);

    public Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity =>
        Task.FromResult(
            _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).FindOne(predicate)
        );

    public bool Any<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Exists(predicate);

    public Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity =>
        Task.FromResult(
            _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Exists(predicate)
        );
}
