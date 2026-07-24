using Value = Toolbox.Industrial.Core.Models.Controlador;

namespace Toolbox.Industrial.Core.Data.Entities;

public class Controlador(Guid id, Value value) : Entity<Guid, Value>(id, value) { }
