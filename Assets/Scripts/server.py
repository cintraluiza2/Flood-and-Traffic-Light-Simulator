import socket
import time
import threading

HOST = '...'
PORT = ...

STATES = ["VERMELHO", "VERDE", "AMARELO"]
current_state_index = 0
current_state = STATES[current_state_index]

def change_light_state():
    """Thread function to change the traffic light state every 10 seconds"""
    global current_state_index, current_state
    
    while True:
        current_state_index = (current_state_index + 1) % len(STATES)
        current_state = STATES[current_state_index]
        print(f"Mudando estado do semáforo para: {current_state}")
        if current_state == "AMARELO":
            time.sleep(2) 
        else:
            time.sleep(10)
            

state_thread = threading.Thread(target=change_light_state, daemon=True)
state_thread.start()

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(1)
print(f'Servidor Python escutando em {HOST}:{PORT}')

try:
    while True:
        conn, addr = server_socket.accept()
        print(f'Conexão recebida de {addr}')
        
        try:
            data = conn.recv(1024)
            mensagem_recebida = data.decode('utf-8')
            print(f'Mensagem recebida: {mensagem_recebida}')
            
            resposta = current_state
            conn.sendall(resposta.encode('utf-8'))
            print(f"Resposta enviada: {resposta}")
        
        except Exception as e:
            print(f"Erro ao processar conexão: {e}")
        finally:
            conn.close()
            print("Conexão encerrada.\n")

except KeyboardInterrupt:
    print("Servidor encerrado pelo usuário")
    server_socket.close()
except Exception as e:
    print(f"Erro no servidor: {e}")
    server_socket.close()
