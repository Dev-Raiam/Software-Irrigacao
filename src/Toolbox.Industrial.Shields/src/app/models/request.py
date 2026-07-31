class Porta:
    def __init__(self, device_id: str, porta: str, sinal: str, tipo: str):
        self.device_id = device_id
        self.porta = porta
        self.sinal = sinal
        self.tipo = tipo

    @classmethod
    def from_dict(cls, data: dict):
        return cls(
            device_id=data.get("device_id"),
            porta=data.get("porta"),
            sinal=data.get("sinal"),
            tipo=data.get("tipo")
        )


class Request:
    def __init__(self,
        comando: str | None = None,
        device_id: str | None = None,
        porta: str | None = None,
        sinal: str | None = None,
        tipo: str | None = None,
        valor: int | None = None,
        portas: list[Porta] | None = None
        ):
        self.comando = comando
        self.device_id = device_id
        self.porta = porta
        self.sinal = sinal
        self.tipo = tipo
        self.valor = valor
        self.portas = portas or []

    @staticmethod
    def from_dict(data: dict):
        portas_data = data.get("portas")
        portas = [Porta.from_dict(p) for p in portas_data] if portas_data else []

        return Request(
            comando=data.get("comando"),
            device_id=data.get("device_id"),
            porta=data.get("porta"),
            sinal=data.get("sinal"),
            tipo=data.get("tipo"),
            valor=data.get("valor"),
            portas=portas
        )