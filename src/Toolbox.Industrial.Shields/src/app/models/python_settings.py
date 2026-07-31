class MqttConfiguration:
    def __init__(self,
        host:str,
        port:int,
        client_id:str,
        username:str,
        password:str,
        connection_timeout_seconds:int,
        default_qos:int,
        default_retain:bool,
        clean_session:bool
        ):
        self.host = host
        self.port = port
        self.client_id = client_id
        self.username = username
        self.password = password
        self.connection_timeout_seconds = connection_timeout_seconds
        self.default_qos = default_qos
        self.default_retain = default_retain
        self.clean_session = clean_session

    @classmethod
    def from_dict(cls, data: dict):
        return cls(
            host=data.get("host"),
            port=data.get("port"),
            client_id=data.get("clientId"),
            username=data.get("username"),
            password=data.get("password"),
            connection_timeout_seconds=data.get("connectionTimeoutSeconds"),
            default_qos=data.get("defaultQoS"),
            default_retain=data.get("defaultRetain"),
            clean_session=data.get("cleanSession")
        )

class PythonSettings:
    def __init__(self, version:int, generated_at: str, mqtt:MqttConfiguration):
        self.version = version
        self.generated_at = generated_at
        self.mqtt = mqtt

    @classmethod
    def from_dict(cls, data: dict):
        return cls(
            version=data.get("version"),
            generated_at=data.get("generatedAt"),
            mqtt=MqttConfiguration.from_dict(data.get("mqtt", {}))
        )