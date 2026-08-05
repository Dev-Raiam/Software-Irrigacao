using System.Text.Json;
using System.Text.RegularExpressions;
using LiteDB;
using Toolbox.Core.Extensions;

namespace Toolbox.Industrial.Core.Data
{
    public abstract class Entity
    {
        private static readonly Dictionary<string, string> Irregulares = new(
            StringComparer.OrdinalIgnoreCase
        )
        {
            { "cao", "caes" },
            { "pao", "paes" },
            { "mao", "maos" },
            { "alemao", "alemaes" },
            { "cidadao", "cidadaos" },
            { "cristao", "cristaos" },
            { "licao", "licoes" },
            { "mal", "males" },
            { "consul", "consules" },
            { "luz", "luzes" },
            { "flor", "flores" },
            { "lavoura", "lavouras" },
            { "setor", "setores" },
        };

        public static string Pluralize(string singular)
        {
            if (string.IsNullOrWhiteSpace(singular))
                return singular;

            var parts = SplitCamelCase(singular);

            if (parts.Count > 1)
            {
                return string.Join("", parts.Select(p => PluralizeSingle(p)));
            }

            return PluralizeSingle(singular);

            //if (string.IsNullOrWhiteSpace(singular))
            //    return singular;

            //singular = singular.Trim();

            //// Se estiver nos irregulares, retorna direto
            //if (Irregulares.TryGetValue(singular, out var plural))
            //    return plural;

            //// 🔹 Regras gramaticais comuns:

            //// Termina em "r" ou "z" → + "es"
            //if (singular.EndsWith("r", StringComparison.OrdinalIgnoreCase) ||
            //    singular.EndsWith("z", StringComparison.OrdinalIgnoreCase))
            //    return singular + "es";

            //// Termina em "m" → troca "m" por "ns"
            //if (singular.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            //    return singular.Substring(0, singular.Length - 1) + "ns";

            //// Termina em "l"
            //if (singular.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            //{
            //    // Se antes do "l" tiver vogal, vira "is" (ex: "animal" → "animais")
            //    if ("aeiou".Contains(char.ToLower(singular[singular.Length - 2])))
            //        return singular.Substring(0, singular.Length - 1) + "is";

            //    // Senão, vira "es" (ex: "álcool" → "álcoois")
            //    return singular + "es";
            //}

            //// Termina em "ão" → três casos possíveis
            //if (singular.EndsWith("ao", StringComparison.OrdinalIgnoreCase))
            //{
            //    // Padrão mais comum: "ão" → "ões" (ex: "leão" → "leões")
            //    return singular.Substring(0, singular.Length - 2) + "oes";
            //}

            //// Termina em "s" → fica igual se for oxítona (ex: "lápis" → "lápis")
            //if (singular.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            //    return singular;

            //// Default → só adiciona "s"
            //return singular + "s";
        }

        private static string PluralizeSingle(string word)
        {
            if (Irregulares.TryGetValue(word, out var plural))
                return CapitalizeLike(word, plural);

            // Termina em "r" ou "z"
            if (
                word.EndsWith("r", StringComparison.OrdinalIgnoreCase)
                || word.EndsWith("z", StringComparison.OrdinalIgnoreCase)
            )
                return word + "es";

            // Termina em "m"
            if (word.EndsWith("m", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 1) + "ns";

            // Termina em "l"
            if (word.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            {
                if (word.Length > 1 && "aeiou".Contains(char.ToLower(word[word.Length - 2])))
                    return word.Substring(0, word.Length - 1) + "is"; // Animal → Animais
                return word + "es"; // Fóssil → Fósseis
            }

            // Termina em "ão"
            if (word.EndsWith("ão", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 2) + "ões";

            // Termina em "ao"
            if (word.EndsWith("ao", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 2) + "oes";

            // Termina em "s" → fica igual
            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                return word;

            // Default → + s
            return word + "s";
        }

        private static List<string> SplitCamelCase(string input)
        {
            return Regex
                .Matches(input, @"([A-Z][a-z]*)")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();
        }

        private static string CapitalizeLike(string original, string word)
        {
            if (string.IsNullOrEmpty(original))
                return word;
            if (char.IsUpper(original[0]))
                return char.ToUpper(word[0]) + word.Substring(1);
            return char.ToLower(word[0]) + word.Substring(1);
        }

        public static string GetCollection<TEntity>()
            where TEntity : Entity
        {
            return JsonNamingPolicy.SnakeCaseLower.ConvertName(
                Pluralize(typeof(TEntity).Name) //PortuguesPluralizer
            );
        }

        internal abstract object BsonId { get; }

        internal class Keys
        {
            /// <summary>
            /// c8327b74-9333-45fc-f5b0-1235a2c1fd1a
            /// </summary>
            public static Guid ContaId = "Padrao.ContaId".GetId();

            /// <summary>
            /// 4984ed4d-a658-4586-1c45-ad5e036a19ae
            /// </summary>
            public static Guid PainelId = "Padrao.PainelId".GetId();

            /// <summary>
            ///
            /// </summary>
            public static Guid ControladorId = "Padrao.ControladorId".GetId();

            public static class Serilog
            {
                /// <summary>
                /// 7f2dfe01-7141-4ce8-643d-07a86cfafef5
                /// </summary>
                public static Guid Config = "SeriLog.Config".GetId();
            }

            public static class Auth
            {
                /// <summary>
                /// ff8b0b1e-d2eb-4f97-0ef4-ef2c18913466
                /// </summary>
                public static Guid Chave = "Autenticacao.Chave".GetId();

                /// <summary>
                /// cc2074d6-6960-43c6-cb69-82665f6342e9
                /// </summary>
                public static Guid Segredo = "Autenticacao.Segredo".GetId();

                /// <summary>
                /// 6e08e891-62ff-4b64-bff3-ab7931ef5c50
                /// </summary>
                public static Guid ContextoId = "Autenticacao.ContextoId".GetId();
            }

            public static class Mqtt
            {
                /// <summary>
                /// d7166437-954a-4065-051c-95c6d3a06b70
                /// </summary>
                public static Guid Local = "Mqtt.Local".GetId();

                /// <summary>
                /// aac72483-9d76-4e3f-b562-fe50ff6966a6
                /// </summary>
                public static Guid Remoto = "Mqtt.Remoto".GetId();
            }

            public static class Api
            {
                /// <summary>
                /// 5b8c9baa-00d3-4ab5-9ca8-2913660cf776
                /// </summary>
                public static Guid BaseAddress = "Api.BaseAddress".GetId();

                public static class Jwt
                {
                    /// <summary>
                    /// 0da49e09-17a7-47e8-8b2a-2e27654188c4
                    /// </summary>
                    public static Guid SecKeys = "Authentication.Jwt.SecurityKeys".GetId();

                    /// <summary>
                    /// 508646db-6b3d-487d-9dfa-1be85042af1d
                    /// </summary>
                    public static Guid ValidIssuers = "Authentication.Jwt.ValidIssuers".GetId();

                    /// <summary>
                    /// adf8f36c-4f38-4b79-7081-dbcdd7438150
                    /// </summary>
                    public static Guid JwksUrl = "Authentication.Jwt.JwksUrl".GetId();
                }
            }

            public static class Security
            {
                public static Guid CertificateAuthority = "Security.Certificate.Authority".GetId();
                public static Guid CertificateMqttLocal = "Security.Certificate.Mqtt.Local".GetId();
                public static Guid CertificateMqttRemoto = "Security.Certificate.Mqtt.Remoto".GetId();
                public static Guid CertificateHttpsLocal = "Security.Certificate.Https.Local".GetId();
            }

            public static class Topic
            {
                public static Guid Configuracao = "Topico.Configuracao".GetId();
            }
        }
    }

    public abstract class Entity<TKey, TValue> : Entity
    {
        protected Entity() { }

        public Entity(TKey id, TValue valor)
        {
            Id = id;
            Valor = valor;
            UltimaAtualizacao = DateTime.UtcNow;
        }

        [BsonId]
        public TKey Id { get; init; } = default!;

#pragma warning disable CS8603 // Possible null reference return.
        internal override object BsonId => Id;
#pragma warning restore CS8603 // Possible null reference return.

        public virtual DateTime UltimaAtualizacao { get; protected set; }

        public virtual TValue Valor { get; protected set; } = default!;

        public virtual void Atualizar(TValue valor)
        {
            Valor = valor;
            UltimaAtualizacao = DateTime.UtcNow;
        }
    }
}
