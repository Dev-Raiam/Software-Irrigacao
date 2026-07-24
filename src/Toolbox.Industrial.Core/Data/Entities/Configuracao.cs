using LiteDB;

namespace Toolbox.Industrial.Core.Data;

public class Configuracao(Guid id, string value) : Entity<Guid, string>(id, value) { }
