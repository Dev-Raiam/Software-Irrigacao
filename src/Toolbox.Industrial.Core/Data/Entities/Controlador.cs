using Value = Toolbox.Industrial.Core.Communication.Api.Contracts.Controlador;

namespace Toolbox.Industrial.Core.Data;

public class Controlador(Guid id, Value value) : Entity<Guid, Value>(id, value) { }
