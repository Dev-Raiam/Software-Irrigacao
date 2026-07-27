using Toolbox.Industrial.Core.Data;
using static Toolbox.Industrial.Core.Data.Entity;
using MqttConfiguration = Toolbox.Industrial.Core.Communication.Mqtt.Configuration;

namespace SoftwareIrrigacao.Data
{
    internal static class SeedData
    {
        public static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            var entityConfig = scope.ServiceProvider.GetRequiredService<EntityConfiguration>();
            entityConfig.ApplyConfiguration = (IEntityStore store) =>
            {
                //store.Configure<Configuracao>().Field(x => x.Value, "Dados");
                
            };


            var store = scope.ServiceProvider.GetRequiredService<IEntityStore>();

            await AdicionarConfiguracoes(serviceProvider, store);
        }

        private static async Task AdicionarConfiguracoes(
            IServiceProvider serviceProvider,
            IEntityStore store
        )
        {
            await store.InsertAsync(new Log(LogType.Info, "Dados de seed inicializados.", new Exception("Teste"), new {Nome = "teste", Aprovado = true }));
            await store.InsertAsync(new Log(LogType.Info, "Dados de seed inicializados.", new Exception("Teste2")));

            var id = Entity.Keys.Api.BaseAddress;
            if ((await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id))?.Value == null)
            {
                await store.UpsertAsync(
                    new Configuracao(id: id, value: "https://api.toolbox.app.br")
                );
            }

            id = Entity.Keys.Mqtt.Local;
            if ((await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id))?.Value == null)
            {
                var config = new MqttConfiguration { Username = "master", Password = "broker@MQ" };

                await store.UpsertAsync(
                    new Configuracao(
                        id: id,
                        value: config//System.Text.Json.JsonSerializer.Serialize(config)
                    )
                );
            }

            id = Entity.Keys.Mqtt.Remoto;
            if ((await store.FirstOrDefaultAsync<Configuracao>(x => x.Id == id))?.Value == null)
            {
                var config = new MqttConfiguration
                {
                    Host = "broker.freemqtt.com",
                    Username = "freemqtt",
                    Password = "public",
                };

                await store.UpsertAsync(
                    new Configuracao(
                        id: id,
                        value: config//System.Text.Json.JsonSerializer.Serialize(config)
                    )
                );
            }
        }
    }
}
