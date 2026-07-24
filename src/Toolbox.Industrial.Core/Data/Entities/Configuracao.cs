using LiteDB;

namespace Toolbox.Automacao.Core.Data.Entities;

public class Configuracao(Guid id, string value) : Entity<Guid, string>(id, value) { }
