using Value = Toolbox.Automacao.Core.Models.Controlador;

namespace Toolbox.Automacao.Core.Data.Entities;

public class Controlador(Guid id, Value value) : Entity<Guid, Value>(id, value) { }
