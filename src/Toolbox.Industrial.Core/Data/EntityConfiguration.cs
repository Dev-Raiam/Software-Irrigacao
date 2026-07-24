namespace Toolbox.Industrial.Core.Data;

public delegate void ApplyConfiguration(IRepository repository);

public class EntityConfiguration
{
    public ApplyConfiguration? ApplyConfiguration { get; set; }
}
