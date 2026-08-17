using System.Text.Json;
using LiteDB;
using Toolbox.Core.Converters;
using Toolbox.Core.Extensions;

namespace Toolbox.Industrial.Core.Data
{
    public abstract class Entity
    {
        protected Entity() { }

        public Entity(Guid id)
        {
            Id = id;
        }

        [BsonId]
        public Guid Id { get; init; }

        //#pragma warning disable CS8603 // Possible null reference return.
        //        internal override object BsonId => Id;
        //#pragma warning restore CS8603 // Possible null reference return.
        //        internal abstract object BsonId { get; }

        public static string GetCollection<TEntity>()
            where TEntity : Entity
        {
            return JsonNamingPolicy.SnakeCaseLower.ConvertName(
                PortuguesPluralizer.Pluralize(typeof(TEntity).Name)
            );
        }

        public class Keys
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

            /// <summary>
            /// 
            /// </summary>
            public static Guid AtualizacaoId = "Padrao.AtualizacaoId".GetId();  

            /// <summary>
            /// d640e94a-0d64-4c80-b0b2-0e0bd5421316
            /// </summary>
            public static Guid VersaoAtual = "Padrao.VersaoAtual".GetId();  
           
            /// <summary>
            /// 
            /// </summary>
            public static Guid DataVersaoAtual = "Padrao.DataVersaoAtual".GetId();  

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
                /// 
                /// </summary>
                public static Guid Interno = "Mqtt.Interno".GetId();

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
                public static Guid CertificateMqttInterno = "Security.Certificate.Mqtt.Interno".GetId();
                public static Guid CertificateMqttRemoto =
                    "Security.Certificate.Mqtt.Remoto".GetId();
                public static Guid CertificateHttpsLocal =
                    "Security.Certificate.Https.Local".GetId();
            }

            public static class Topic
            {
                public static Guid Configuracao = "Topico.Configuracao".GetId();
            }
        }
    }

    public abstract class Entity<TValue> : Entity
    {
        protected Entity() { }

        public Entity(Guid id, TValue valor)
            : base(id)
        {
            Valor = valor;
            UltimaAtualizacao = DateTime.UtcNow;
        }

        public virtual TValue Valor { get; protected set; } = default!;

        public virtual DateTime UltimaAtualizacao { get; protected set; }

        public virtual void Atualizar(TValue valor)
        {
            Valor = valor;
            UltimaAtualizacao = DateTime.UtcNow;
        }
    }
}
