using LiteDB;

namespace Toolbox.Industrial.Core.Data.Entities;

public class Configuracao(Guid id, string value) : Entity<Guid, string>(id, value) { }
