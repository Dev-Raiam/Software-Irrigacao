# import json
# import win32file
# import win32pipe
# from shields_io import ShieldsIO



# shilds_Io = ShieldsIO()

# commands = {

#     "ReadDigital": shilds_Io.read_digital,

#     "WriteDigital": shilds_Io.write_digital,

#     "ReadAnalog": shilds_Io.read_analog,

#     "WriteAnalog": shilds_Io.write_analog
# }


# def process(request):

#     handler = commands[request["Command"]]

#     value = handler(request)

#     return {
#         "Id": request["Id"],
#         "Success": True,
#         "Value": value
#     }


# PIPE_NAME = r"\\.\pipe\meu_pipe_comunicacao"

# try:
#     while True:

#         pipe = win32pipe.CreateNamedPipe(
#             PIPE_NAME,
#             win32pipe.PIPE_ACCESS_DUPLEX,
#             win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
#             1,  # max instances
#             4096,  # output buffer size
#             4096,  # input buffer size
#             0,  # default timeout
#             None  # security attributes
#         )
        
#         print("Aguardando conexão...")
        
#         win32pipe.ConnectNamedPipe(pipe,None)

#         print("Cliente conectado!")

#         try:
#             while True:

#                 data = win32file.ReadFile(pipe, 4096)[1]

#                 print("Recebido:", data)

#                 if not data:
#                     break

#                 request = json.loads(data.decode())

#                 response = process(request)

#                 response_data = (json.dumps(response) + "\n").encode()
                
#                 win32file.WriteFile(pipe, response_data)

#         except (json.JSONDecodeError, ValueError) as e:
#             print("Erro ao decodificar JSON:", e)
#         except OSError as e:
#             print("Erro de I/O:", e)
#         except Exception as e:
#             print("Erro inesperado:", e)
#         finally:
#             win32file.CloseHandle(pipe)
        
# except KeyboardInterrupt:
#     print("\nServidor encerrado pelo usuário")

# finally:
#     win32file.CloseHandle(pipe)
