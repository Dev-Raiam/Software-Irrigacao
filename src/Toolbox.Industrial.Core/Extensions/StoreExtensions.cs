using System.Linq.Expressions;
using Toolbox.Core.Extensions;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Security;
using Controlador = Toolbox.Industrial.Core.Communication.Api.Contracts.Controlador;

namespace Toolbox.Industrial.Core.Extensions
{
    public static class StoreExtensions
    {
        public static TEntity? Get<TEntity>(this IEntityStore store, Guid id)
            where TEntity : Entity
        {
            return store.FirstOrDefault<TEntity>(x => x.Id == id);
        }

        public static async Task<TEntity?> GetAsync<TEntity>(this IEntityStore store, Guid id)
            where TEntity : Entity
        {
            return await store.FirstOrDefaultAsync<TEntity>(x => x.Id == id);
        }

        public static TEntity? Get<TEntity>(
            this IEntityStore store,
            Expression<Func<TEntity, bool>> predicate
        )
            where TEntity : Entity
        {
            return store.FirstOrDefault(predicate);
        }

        public static async Task<TEntity?> GetAsync<TEntity>(
            this IEntityStore store,
            Expression<Func<TEntity, bool>> predicate
        )
            where TEntity : Entity
        {
            return await store.FirstOrDefaultAsync(predicate);
        }

        public static async Task<T?> ObterConfiguracao<T>(this IEntityStore store, Guid id)
        {
            var config = await store.GetAsync<Configuracao>(id);
            if (config?.Valor == null)
                return default;

            if (typeof(T) == typeof(Guid))
                return (T)(object)Guid.Parse(config.Valor.ToString()!);

            return (T)Convert.ChangeType(config.Valor, typeof(T));
        }

        public static async Task<Controlador?> ObterControladorMaster(this IEntityStore store)
        {
            return (await store.FirstOrDefaultAsync<Data.Controlador>(x => x.Valor.Master))?.Valor;
        }

        #region Uso interno

        internal static Certificate? GetCertificate(
            this IEntityStore store,
            Guid id,
            string? subject = null
        )
        {
            if (!string.IsNullOrWhiteSpace(subject))
            {
                id = $"{id}{subject}".GetId();
            }
            return store.FirstOrDefault<Configuracao>(x => x.Id == id)?.Valor as Certificate;
        }

        internal static Configuracao? ObterCertificado(
            this IEntityStore store,
            Guid id,
            string? subject = null
        )
        {
            if (!string.IsNullOrWhiteSpace(subject))
            {
                id = $"{id}{subject}".GetId();
            }
            return store.FirstOrDefault<Configuracao>(x => x.Id == id);
        }

        #endregion Uso interno
    }
}
