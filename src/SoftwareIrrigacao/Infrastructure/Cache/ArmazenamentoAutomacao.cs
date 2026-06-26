using Toolbox.Automacao.Sincronizacao.Dtos;

namespace SoftwareIrrigacao.Infrastructure.Cache
{
    public class ArmazenamentoAutomacao
    {
        private readonly Lock _sync = new();

        private Controlador? _controlador;
        private IReadOnlyList<Modulo> _modulos = [];
        private IReadOnlyList<Dispositivo> _dispositivos = [];

        public Controlador? Controlador
        {
            get
            {
                lock (_sync)
                    return _controlador;
            }
        }

        public IReadOnlyList<Modulo> Modulos
        {
            get
            {
                lock (_sync)
                    return _modulos;
            }
        }

        public IReadOnlyList<Dispositivo> Dispositivos
        {
            get
            {
                lock (_sync)
                    return _dispositivos;
            }
        }

        public DateTimeOffset? AtualizadoEm { get; private set; }

        public bool Invalido
        {
            get
            {
                lock (_sync)
                    return _controlador is null;
            }
        }

        public void Atualizar(
            Controlador controlador,
            IEnumerable<Modulo> modulos,
            IEnumerable<Dispositivo> dispositivos
        )
        {
            lock (_sync)
            {
                _controlador = controlador;
                _modulos = modulos?.ToList() ?? [];
                _dispositivos = dispositivos?.ToList() ?? [];
                AtualizadoEm = DateTimeOffset.UtcNow;
            }
        }

        public void Limpar()
        {
            lock (_sync)
            {
                _controlador = null;
                _modulos = [];
                _dispositivos = [];
                AtualizadoEm = null;
            }
        }
    }
}
