namespace Toolbox.Automacao.Core.Data;

public delegate void ApplyConfiguration(IRepository repository);

public class EntityConfiguration
{
    public ApplyConfiguration? ApplyConfiguration { get; set; }
}
