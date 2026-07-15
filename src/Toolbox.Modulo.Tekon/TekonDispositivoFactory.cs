using Toolbox.Modulo.Tekon.Abstractions;
using Toolbox.Modulo.Tekon.Dispositivos;
using Toolbox.Modulo.Tekon.Dispositivos.WGW420;

namespace Toolbox.Modulo.Tekon
{

    internal class TekonDispositivoFactory
    {
        private readonly IEnumerable<ITekonDispositivoPerfil> _perfils;

        public TekonDispositivoFactory()
        {
            _perfils =
            [
                new TWP_1AIPerfil(),
                new WGW420Perfil(),
                new TWP_4AI4DI1UTPerfil()
            ];
        }

        public ITekonDispositivoPerfil CriarModelo(
            string modelo)
        {
            return _perfils
                .First(x =>
                    x.Modelo == modelo);
        }
    }
}
