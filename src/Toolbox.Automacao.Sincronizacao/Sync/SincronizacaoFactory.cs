// using System;
// using System.Collections.Generic;
// using System.Text;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Logging.Abstractions;
// using Microsoft.Extensions.Options;
// using Toolbox.Automacao.Sincronizacao.Configuration;
// using Toolbox.Automacao.Sincronizacao.Core.Abstractions;
// using Toolbox.Automacao.Sincronizacao.Infrastructure.Data;
// using Toolbox.Automacao.Sincronizacao.Infrastructure.Http;

// namespace Toolbox.Automacao.Sincronizacao.Services.Sync
// {
//     public class SincronizacaoFactory
//     {
//         private readonly IApiAutomacao _apiAutomacao;
//         private readonly SincronizacaoDbContext _context;

//         public SincronizacaoFactory()
//         {
//             var configuracao = new ApiConfiguracao
//             {
//                 BaseUrl = "https://api.exemplo.com",
//                 MediaType = "application/json",
//                 TimeoutSeconds = 30,
//             };

//             var httpCliente = new HttpClient();
//             httpCliente.BaseAddress = new Uri(configuracao.BaseUrl);
//             httpCliente.Timeout = TimeSpan.FromSeconds(configuracao.TimeoutSeconds);

//             var options = Options.Create(configuracao);

//             _apiAutomacao = new ApiAutomacao(httpCliente, options);

//             using var dbContextOptions =
//                 new DbContextOptionsBuilder<SincronizacaoDbContext>().Options;

//             _context = new SincronizacaoDbContext(dbContextOptions);
//         }

//         public SincronizacaoFactory(ApiConfiguracao configuracao, SincronizacaoDbContext context)
//         {
//             var httpCliente = new HttpClient();
//             httpCliente.BaseAddress = new Uri(configuracao.BaseUrl);
//             httpCliente.Timeout = TimeSpan.FromSeconds(configuracao.TimeoutSeconds);

//             var options = Options.Create(configuracao);

//             _apiAutomacao = new ApiAutomacao(httpCliente, options);
//             _context = context;
//         }

//         public SincronizarControladores CriarSincronizacaoControladores()
//         {
//             return new SincronizarControladores(
//                 _apiAutomacao,
//                 NullLogger<SincronizarControladores>.Instance,
//                 _context
//             );
//         }
//     }
// }
