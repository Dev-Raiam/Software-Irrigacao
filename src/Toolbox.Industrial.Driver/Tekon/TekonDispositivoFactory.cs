using Toolbox.Industrial.Driver.Tekon.Dispositivos;
using Toolbox.Industrial.Driver.Tekon.Interfaces;

namespace Toolbox.Industrial.Driver.Tekon
{
    public class TekonDispositivoFactory : ITekonDispositivoFactory
    {
        private readonly IEnumerable<ITekonDispositivoPerfil> _perfils;

        public TekonDispositivoFactory()
        {
            _perfils =
            [
                new TWP_1AIPerfil(),
                new TWP_1DIPerfil(),
                new TWP_1UTPerfil(),
                new TWP_2AIPerfil(),
                new TWP_2DIPerfil(),
                new TWP_2UTPerfil(),
                new TWP_4AI4DI1UTPerfil(),
                new TWP4AIPerfil(),
                new TWPH_1UTPerfil(),
                new WGW420Perfil(),
            ];
        }

        public ITekonDispositivoPerfil CriarModelo(string modelo)
        {
            return _perfils.First(x => x.Modelo == modelo);
        }
    }
}
