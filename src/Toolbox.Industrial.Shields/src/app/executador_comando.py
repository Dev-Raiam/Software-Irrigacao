# class Comando:
#     """Representa um comando para dispositivo de irrigação.
    
#     Attributes:
#         dispositivo_id: Identificador único do dispositivo
#         sinal: Tipo de sinal (Analogico/Digital)
#         tipo: Tipo de operação (Input/Output)
#         porta: Identificação da porta (ex: Q.0)
#         valor: Valor do comando (bool para digital, int para analógico)
#     """
#     def __init__(self, dispositivo_id: str, sinal: str, tipo: str, porta: str, valor: bool | int):
#         self.dispositivo_id = dispositivo_id
#         self.sinal = sinal
#         self.tipo = tipo
#         self.porta = porta
#         self.valor = valor


# class ExecutadorComando:
#     def __init__(self):
#         pass
    
#     def executar(self, comando):
#         if(comando["tipo"] == ""):
#             self._abrirPorta()
#         elif(comando["tipo"] == "fechar"):
#             self._fecharPorta()
#         pass
#     def _abrirPorta(self):
#         pass
#     def _fecharPorta(self):
#         pass
#     def _publicar(self):
#         pass
