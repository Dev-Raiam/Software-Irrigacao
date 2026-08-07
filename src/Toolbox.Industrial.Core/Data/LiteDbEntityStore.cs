using System.Linq.Expressions;
using LiteDB;
using Toolbox.Industrial.Core.Security;

namespace Toolbox.Industrial.Core.Data;

internal class LiteDbEntityStore : IEntityStore
{
    private readonly ILiteDatabase _database;

    public LiteDbEntityStore(ILiteDatabase database, EntityConfiguration configuration)
    {
        _database = database;
        //Aplicar configurações para as entidades padrão
        //Configure<Configuracao>().Field(x => x.Value, "Configuracao");
        //Configure<Controlador>().Field(x => x.Value, "Controlador");
        if (configuration.ApplyConfiguration != null)
        {
            configuration.ApplyConfiguration(this);
        }
    }

    public EntityBuilder<TEntity> Configure<TEntity>()
        where TEntity : Entity => BsonMapper.Global.Entity<TEntity>();

    public ILiteQueryable<T> Query<T>(string collection) =>
        _database.GetCollection<T>(collection).Query();

    public ILiteQueryable<TEntity> Query<TEntity>()
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Query();

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

    public async Task<bool> DeleteAllDataCollectionsAsync()
    {
        var result = false;
        foreach (
            var collection in _database
                .GetCollection("$cols")
                .Query()
                .Where(doc => (doc["name"].AsString).StartsWith("$") == false)
                .ToList()
        )
        {
            var deleted = _database.GetCollection(collection["name"].AsString).DeleteAll();
            result = result || deleted > 0;
        }
        await Task.Delay(TimeSpan.FromSeconds(3));
        return result;
    }

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
