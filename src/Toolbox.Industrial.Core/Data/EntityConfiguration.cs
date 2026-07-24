namespace Toolbox.Industrial.Core.Data;

public delegate void ApplyConfiguration(IEntityStore store);

public class EntityConfiguration
{
    public ApplyConfiguration? ApplyConfiguration { get; set; }
}
