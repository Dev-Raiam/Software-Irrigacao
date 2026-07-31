import threading

from models.request import Request


class TelemetryManager:
    _instance = None
    _telemetria: Request
    _thread_started = False

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance

    def _process_telemetry(self):
        while True:
            if self._telemetria:
                for porta in self._telemetria.portas:
                    print(f"Processando telemetria: {porta.device_id}")
            threading.Event().wait(15)

    def update_telemetry(self, telemetria: Request):
        self._telemetria = telemetria

        if not self._thread_started:
            thread = threading.Thread(target=self._process_telemetry, daemon=True)
            thread.start()
            self._thread_started = True

