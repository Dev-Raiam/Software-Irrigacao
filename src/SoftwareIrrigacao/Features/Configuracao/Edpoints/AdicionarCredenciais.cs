//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Routing;
//using Microsoft.EntityFrameworkCore;
//using SoftwareIrrigacao.Infra.Cache;
//using System.ComponentModel.DataAnnotations;
//using System.Net;
//using Toolbox.Automacao.Core.Data;
//using Toolbox.Automacao.Core.Services;
//using Toolbox.Core.Mediator;
//using Toolbox.Core.Messages;

//namespace SoftwareIrrigacao.Features.Configuracao.Edpoints;

//public static class AdicionarCredenciais
//{
//    public class Command : Toolbox.Core.Messages.Command
//    {
//        [Required(ErrorMessage = "ContaId é obrigatório")]
//        public Guid ContaId { get; init; }

//        [Required(ErrorMessage = "PainelId é obrigatório")]
//        public Guid PainelId { get; init; }

//        [Required(ErrorMessage = "Integracao é obrigatório")]
//        public IntegracaoConfiguracao Integracao { get; init; } = null!;

//        public class IntegracaoConfiguracao
//        {
//            [Required(ErrorMessage = "Chave é obrigatório")]
//            public string Chave { get; init; } = null!;

//            [Required(ErrorMessage = "Segredo é obrigatório")]
//            public string Segredo { get; init; } = null!;

//            [Required(ErrorMessage = "ContextoId é obrigatório")]
//            public Guid ContextoId { get; init; }
//        };
//    }

//    public class Handler : ICommandHandler<Command>
//    {
//        private readonly CredenciaisAplicacao _credenciaisAplicacao;
//        private readonly ICriptografia _criptografia;
//        private readonly IMediator _mediator;

//        public Handler(
//            CredenciaisAplicacao credenciaisAplicacao,
//            ICriptografia criptografia,
//            IMediator mediator
//        )
//        {
//            _credenciaisAplicacao = credenciaisAplicacao;
//            _criptografia = criptografia;
//            _mediator = mediator;
//        }

//        private async Task<bool> ExisteCredenciais(CancellationToken cancellationToken)
//        {
//            var chaves = new[]
//            {
//                ChaveConfiguracao.Padrao.ContaId,
//                ChaveConfiguracao.Padrao.PainelId,
//                ChaveConfiguracao.Integracao.Chave,
//                ChaveConfiguracao.Integracao.Segredo,
//                ChaveConfiguracao.Integracao.ContextoId,
//            };
//            var chavesConfiguracoes = await _context
//                .Set<Toolbox.Automacao.Core.Models.Configuracao>()
//                .AsNoTracking()
//                .Where(c => chaves.Contains(c.Chave))
//                .Select(c => c.Chave)
//                .ToListAsync(cancellationToken);

//            bool existe =
//                chavesConfiguracoes.Contains(chaves[0])
//                && chavesConfiguracoes.Contains(chaves[1])
//                && chavesConfiguracoes.Contains(chaves[2])
//                && chavesConfiguracoes.Contains(chaves[3])
//                && chavesConfiguracoes.Contains(chaves[4]);

//            return existe;
//        }

//        private void AdicionarCredenciaisAplicacao(
//            Guid contaId,
//            Guid painelId,
//            Guid contextoId,
//            string chave,
//            string segredo
//        )
//        {
//            _credenciaisAplicacao.AdicionarConta(contaId);
//            _credenciaisAplicacao.AdicionarPainel(painelId);
//            _credenciaisAplicacao.AdicionarIntegracao(chave, segredo, contextoId);
//        }

//        public async Task<ResponseResult> Handle(
//            Command request,
//            CancellationToken cancellationToken
//        )
//        {
//            var credenciaisExistentes = await ExisteCredenciais(cancellationToken);

//            //if (credenciaisExistentes)
//            //{
//            //    // Retirar feito para Teste
//            //    AdicionarCredenciaisAplicacao(
//            //        request.ContaId,
//            //        request.PainelId,
//            //        request.Integracao.ContextoId,
//            //        request.Integracao.Chave,
//            //        request.Integracao.Segredo
//            //    );

//            //    return ResponseResult.Result(HttpStatusCode.Conflict);
//            //}

//            var painel = new Toolbox.Automacao.Core.Models.Configuracao(
//                ChaveConfiguracao.Padrao.PainelId,
//                request.PainelId!.ToString()
//            );

//            var conta = new Toolbox.Automacao.Core.Models.Configuracao(
//                ChaveConfiguracao.Padrao.ContaId,
//                request.ContaId!.ToString()
//            );

//            var chaveIntegracao = new Toolbox.Automacao.Core.Models.Configuracao(
//                ChaveConfiguracao.Integracao.Chave,
//                _criptografia.Criptografar(request.Integracao.Chave!)
//            );

//            var segredoIntegracao = new Toolbox.Automacao.Core.Models.Configuracao(
//                ChaveConfiguracao.Integracao.Segredo,
//                _criptografia.Criptografar(request.Integracao.Segredo!)
//            );

//            var contextoIdIntegracao = new Toolbox.Automacao.Core.Models.Configuracao(
//                ChaveConfiguracao.Integracao.ContextoId,
//                request.Integracao.ContextoId!.ToString()
//            );

//            var configuracoes = new List<Toolbox.Automacao.Core.Models.Configuracao>
//            {
//                painel,
//                conta,
//                chaveIntegracao,
//                segredoIntegracao,
//                contextoIdIntegracao,
//            };

//            await _context
//                .Set<Toolbox.Automacao.Core.Models.Configuracao>()
//                .AddRangeAsync(configuracoes, cancellationToken);
//            await _context.SaveChangesAsync(cancellationToken);

//            AdicionarCredenciaisAplicacao(
//                request.ContaId,
//                request.PainelId,
//                request.Integracao.ContextoId,
//                request.Integracao.Chave,
//                request.Integracao.Segredo
//            );

//            return ResponseResult.Result(HttpStatusCode.OK);
//        }
//    }

//    public static void Endpoint(IEndpointRouteBuilder app)
//    {
//        app.MapPost(
//                "/configuracao/credenciais",
//                async (
//                    [FromBody] Command command,
//                    [FromServices] IMediator mediator,
//                    CancellationToken cancellationToken
//                ) => await mediator.Execute(command, cancellationToken: cancellationToken)
//            )
//            .RequireAuthorization()
//            .RequireRateLimiting("limite-tentativas");
//    }
//}
