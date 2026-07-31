import json
import os
import time
from pathlib import Path

import paho.mqtt.client as mqtt
from message_manager import MessageManager
from models.python_settings import PythonSettings

FILE_PATH = Path(
    "D:/Desenvolvimento/Backend/SoftwareIrrigacao/"
    "src/SoftwareIrrigacao/bin/Debug/net10.0/py_settings.json"
)

mqtt_client = None
client_connected = False
message_manager = MessageManager()

try:
    while True:
        print("Buscando arquivo de configuração...")
        if os.path.exists(FILE_PATH):
            with open(FILE_PATH, "r", encoding="utf-8") as file:
                content = file.read()
                mqtt_settings = PythonSettings.from_dict(json.loads(content))
                print("Arquivo de configuração encontrado!")
            break
        time.sleep(5)

    def on_connect(_client, _userdata, _flags, reason_code, _properties):
        if reason_code == 0:
            print(f"Conectado com código de resultado {reason_code}")
            mqtt_client.subscribe("topico")
        else:
            print(f"Erro ao conectar: {reason_code}")

    def on_message(_client, _userdata, msg):
        message_manager.handle_message(msg.payload.decode())

    mqtt_client = mqtt.Client(
        client_id=mqtt_settings.mqtt.client_id,
        callback_api_version=mqtt.CallbackAPIVersion.VERSION2,
        clean_session=mqtt_settings.mqtt.clean_session
    )

    mqtt_client.on_connect = on_connect
    mqtt_client.on_message = on_message
    mqtt_client.reconnect_delay_set()

    while True:
        if not client_connected:
            try:
                mqtt_client.connect(
                    mqtt_settings.mqtt.host,
                    mqtt_settings.mqtt.port,
                    mqtt_settings.mqtt.connection_timeout_seconds
                )
                client_connected = True
            except Exception as ex:
                print(f"Erro ao conectar: {ex}")
                time.sleep(5)
        else:
            break
    mqtt_client.loop_forever()
except Exception as ex:
    print(f"Erro: {ex}")
except KeyboardInterrupt:
    print("Encerrando...")
    if mqtt_client is not None and mqtt_client.is_connected():
        print("Desconectando...")
        mqtt_client.disconnect()
        mqtt_client.loop_stop()
