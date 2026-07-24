using System.Linq.Expressions;
using LiteDB;
using Toolbox.Automacao.Core.Data.Entities;

namespace Toolbox.Automacao.Core.Data;

public interface IRepository
{
    EntityBuilder<TEntity> EntityBuilder<TEntity>()
        where TEntity : Entity;

    bool Insert<TEntity>(TEntity entity)
        where TEntity : Entity;

    bool Update<TEntity>(TEntity entity)
        where TEntity : Entity;

    bool Upsert<TEntity>(TEntity entity)
        where TEntity : Entity;

    bool Delete<TEntity>(TEntity entity)
        where TEntity : Entity;

    int DeleteMany<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity;

    int DeleteAll<TEntity>()
        where TEntity : Entity;

    TEntity FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity;

    bool Any<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity;
}

internal class Repository : IRepository
{
    private readonly ILiteDatabase _database;

    public Repository(ILiteDatabase database, EntityConfiguration configuration)
    {
        _database = database;
        if (configuration.ApplyConfiguration != null)
        {
            configuration.ApplyConfiguration(this);
        }
    }

    public EntityBuilder<TEntity> EntityBuilder<TEntity>()
        where TEntity : Entity => BsonMapper.Global.Entity<TEntity>();

    public bool Insert<TEntity>(TEntity entity)
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Insert(entity);

    public bool Update<TEntity>(TEntity entity)
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Update(entity);

    public bool Upsert<TEntity>(TEntity entity)
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Upsert(entity);

    public bool Delete<TEntity>(TEntity entity)
        where TEntity : Entity =>
        _database
            .GetCollection<TEntity>(Entity.GetCollection<TEntity>())
            .Delete(new BsonValue(entity.BsonId));

    public int DeleteMany<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).DeleteMany(predicate);

    public int DeleteAll<TEntity>()
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).DeleteAll();

    public TEntity FirstOrDefault<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).FindOne(predicate);

    public bool Any<TEntity>(Expression<Func<TEntity, bool>> predicate)
        where TEntity : Entity =>
        _database.GetCollection<TEntity>(Entity.GetCollection<TEntity>()).Exists(predicate);
}
