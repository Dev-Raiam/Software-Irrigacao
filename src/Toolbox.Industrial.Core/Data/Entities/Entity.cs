using System.Text.Json;
using LiteDB;
using Toolbox.Core.Converters;
using Toolbox.Core.Extensions;

namespace Toolbox.Industrial.Core.Data.Entities
{
    public abstract class Entity
    {
        public static string GetCollection<T>()
            where T : Entity
        {
            return JsonNamingPolicy.SnakeCaseLower.ConvertName(
                PortuguesPluralizer.Pluralize(nameof(T))
            );
        }

        internal abstract object BsonId { get; }

        public class Keys
        {
            public static Guid ContaId = "Padrao.ContaId".GetId();
            public static Guid PainelId = "Padrao.PainelId".GetId();

            public static class Auth
            {
                public static Guid Chave = "Autenticacao.Chave".GetId();
                public static Guid Segredo = "Autenticacao.Segredo".GetId();
                public static Guid ContextoId = "Autenticacao.ContextoId".GetId();
            }

            public static class Topic
            {
                public static Guid Configuracao = "Topico.Configuracao".GetId();
            }
        }
    }

    public abstract class Entity<TKey, TValue> : Entity
    {
        protected static string CollectionName = string.Empty;

        protected Entity() { }

        public Entity(TKey id, TValue value)
        {
            Id = id;
            Value = value;
            LastUpdateAt = DateTime.UtcNow;
        }

        [BsonId]
        public TKey Id { get; init; } = default!;
#pragma warning disable CS8603 // Possible null reference return.
        internal override object BsonId => Id;
#pragma warning restore CS8603 // Possible null reference return.
        [BsonField("Valor")]
        public TValue Value { get; private set; } = default!;

        [BsonField("UltimaAtualizacao")]
        public DateTime LastUpdateAt { get; private set; }

        public void Update(TValue value)
        {
            Value = value;
            LastUpdateAt = DateTime.UtcNow;
        }
    }
}
