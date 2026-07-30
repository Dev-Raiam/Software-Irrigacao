using Toolbox.Industrial.Core.Communication.RaspIO;

namespace SoftwareIrrigacao.Workes
{
    public class WorkerRaspIO : BackgroundService
    {
        private readonly IControllerIO _controller;
        private readonly ILogger<WorkerRaspIO> _logger;

        public WorkerRaspIO(IControllerIO controller, ILogger<WorkerRaspIO> logger)
        {
            _controller = controller;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                try
                {
                    var responseDigital = await _controller.ReadDigitalAsync(10, stoppingToken);
                    var responseAnalogico = await _controller.ReadAnalogAsync(10);
                    var responseEscritaDigital = await _controller.WriteDigitalAsync(10, false, stoppingToken);
                    var responseEscritaAnalogica = await _controller.WriteAnalogAsync(10, 1024, stoppingToken);

                    _logger.LogInformation("Comando de Excrita enviado com sucesso!!! {response}", responseDigital);
                    _logger.LogInformation("Comando de Excrita enviado com sucesso!!! {response}", responseAnalogico);
                    _logger.LogInformation("Comando de Excrita enviado com sucesso!!! {response}", responseEscritaDigital);
                    _logger.LogInformation("Comando de Excrita enviado com sucesso!!! {response}", responseEscritaAnalogica);
                }
                catch (Exception ex) 
                { 
                    _logger.LogError("Falha ao enviar comando {ex}",ex);
                }

                await Task.Delay(2000);
            }
        }
    }
}
