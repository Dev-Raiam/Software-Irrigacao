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

        public static async Task<TResult?> ObterConfiguracao<TResult>(
            this IEntityStore store,
            Guid id
        )
        {
            var config = await store.GetAsync<Configuracao>(id);
            try
            {
                if (typeof(TResult) == typeof(Configuracao))
                {
                    if (config == null)
                        return default;

                    return (TResult)(object)config;
                }

                if (config?.Valor == null)
                    return default;

                if (typeof(TResult) == typeof(Guid))
                    return (TResult)(object)Guid.Parse(config.Valor.ToString()!);

                return (TResult)Convert.ChangeType(config.Valor, typeof(TResult));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task<Controlador?> ObterControlador(this IEntityStore store, Guid id)
        {
            return (await store.FirstOrDefaultAsync<Data.Controlador>(x => x.Id == id))?.Valor;
        }

        public static async Task<Controlador?> ObterControladorMaster(this IEntityStore store)
        {
            return (await store.FirstOrDefaultAsync<Data.Controlador>(x => x.Valor.Master))?.Valor;
        }

        #region Uso interno

        internal static TResult? GetCertificate<TResult>(
            this IEntityStore store,
            Guid id,
            string? subject = null
        )
        {
            if (!string.IsNullOrWhiteSpace(subject))
            {
                id = $"{id}{subject}".GetId();
            }
            var config = store.FirstOrDefault<Configuracao>(x => x.Id == id);

            if (config == null)
                return default;

            if (typeof(TResult) == typeof(Configuracao))
            {
                return (TResult)(object)config;
            }
            if (config.Valor == null)
                return default;

            return (TResult)(object)config.Valor;
        }

        #endregion Uso interno
    }
}
