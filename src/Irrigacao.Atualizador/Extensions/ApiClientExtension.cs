using System.Net.Http.Json;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;

namespace Irrigacao.Atualizador.Extensions
{
    // Mudar para o Toolbox.Industrial.Core
    public static class ApiClientExtension
    {
        public static async Task<UpdateResponse?> CheckUpdate(
            this IApiClient client,
            IEntityStore store,
            ILogger logger,
            CancellationToken cancellationToken
        )
        {
            var credentials = await store.ObterCredenciais();

            if (credentials == null)
                return null;

            var message = new HttpRequestMessage(
                HttpMethod.Query,
                "/automacao/v1/integracoes/2eb57304-1df3-4883-8f81-29b3e9426f6c/atualizacao-disponivel"
            )
            {
                Content = JsonContent.Create(credentials),
            };

            var response = await client.SendAsync<UpdateResponse?>(message, cancellationToken);

            if (!response.Success)
            {
                logger.LogWarning(response.Exception, response.Error);
                return null;
            }

            if (response.Data == null)
                return null;

            logger.LogInformation(
                "Atualização Disponivel na Version {version} lançada em {lancamento}",
                response.Data.Versao,
                response.Data.Lancamento
            );

            return response.Data;
        }
        public static async Task UpdateConfirm(
            this IApiClient client,
            ILogger logger,
            Guid id,
            string url,
            CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(new UpdateConfirm(id)),
            };

            var response = await client.SendAsync<string?>(message, cancellationToken);

            if (!response.Success)
            {
                logger.LogError(response.Exception, response.Error);
            }

            logger.LogInformation("Confirmação de Atualização enviada com sucesso");
        }
    }
}
