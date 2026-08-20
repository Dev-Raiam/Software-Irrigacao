using System.Net.Http.Headers;
using System.Net.Mime;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;

namespace Toolbox.Industrial.Core.Extensions
{
    public static class ApiClientExtensions
    {
        public static async Task<Result<List<Controlador>>> ObterControladores(
            this IApiClient apiClient,
            Guid painelId,
            CancellationToken cancellationToken
        )
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"automacao/v1/paineis/{painelId}/controladores?status=todos"
            );

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypes.Industrial.V1)
            );

            var response = await apiClient.SendAsync<List<Controlador>>(request, cancellationToken);

            return response;
        }

        internal static async Task<Result<Token>> Authenticate(
            this IApiClient apiClient,
            Credentials credentials,
            CancellationToken cancellationToken
        )
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "autenticacao/v1/autenticar-cliente"
            );

            request.Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(credentials),
                System.Text.Encoding.UTF8,
                MediaTypeNames.Application.Json
            );

            var response = await apiClient.SendAsync<Token>(request, cancellationToken);

            return response;
        }
    }
}
