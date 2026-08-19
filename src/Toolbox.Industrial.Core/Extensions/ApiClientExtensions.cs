using System.Net.Http.Headers;
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

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Industrial.V1));

            var response = await apiClient.SendAsync<List<Controlador>>(
                request,
                cancellationToken
            );

            return response;
        }
    }
}
