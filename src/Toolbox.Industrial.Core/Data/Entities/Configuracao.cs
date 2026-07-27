using LiteDB;

namespace Toolbox.Industrial.Core.Data;

public class Configuracao(Guid id, object value) : Entity<Guid, object>(id, value) { }
