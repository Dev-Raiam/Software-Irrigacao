import json

from models.request import Request
from telemetry_manager import TelemetryManager


class MessageManager:
    def __init__(self):
        pass

    def handle_message(self, msg: str):
        response = Request.from_dict(json.loads(msg))

        if response.comando == "telemetria":
            print("Telemetria recebida:")
            print(json.loads(msg))

            TelemetryManager().update_telemetry(response)
        else:
            print("Comando recebido:")
            print(json.loads(msg))
