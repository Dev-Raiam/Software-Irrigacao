using LiteDB;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

public interface IProvedorDataSincronizacao
{
    Controlador? ObterControlador(CancellationToken cancellationToken);
    List<Modulo> ObterModulos(CancellationToken cancellationToken);
}

internal sealed class ProvedorDataSincronizacao : IProvedorDataSincronizacao
{
    private readonly ILiteDatabase _database;

    public ProvedorDataSincronizacao(ILiteDatabase database)
    {
        _database = database;
    }

    public Controlador? ObterControlador(CancellationToken cancellationToken)
    {
        var controlador = ObterControladorMaster(cancellationToken);
        return controlador;
    }

    public List<Dispositivo> ObterDispositivos(CancellationToken cancellationToken)
    {
        var controlador = ObterControladorMaster(cancellationToken);

        List<Dispositivo> dispositivos = new List<Dispositivo>();

        if (controlador == null)
            return dispositivos;

        foreach (var dispositivo in controlador.Dispositivos)
        {
            dispositivos.Add(dispositivo);
        }

        return dispositivos;
    }

    public List<Modulo> ObterModulos(CancellationToken cancellationToken)
    {
        var controlador = ObterControladorMaster(cancellationToken);

        List<Modulo> modulos = new List<Modulo>();

        if (controlador == null)
            return modulos;

        foreach (var modulo in controlador.Modulos)
        {
            modulos.Add(modulo);
        }

        return modulos;
    }

    private Controlador? ObterControladorMaster(CancellationToken cancellationToken)
    {
        var colecao = _database.GetCollection<ControladorConfiguracao>(Tabela.Controladores);

        var configuracao = colecao.FindOne(c => c.Controlador.Master);

        var controlador = configuracao == null ? null : configuracao.Controlador; 

        return controlador;
    }
}
