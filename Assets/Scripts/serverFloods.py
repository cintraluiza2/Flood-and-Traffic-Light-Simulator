import socket
import json
import time

# Configuração dos valores de escoamento
ESCOAMENTOS = {
    "area_a": 12.5,
    "area_b": 8.2,
    "area_c": 15.7,
    "area_d": 6.4,
    "unidade": "m³/s"
}

# Configuração do servidor
HOST = "..."
PORT = ...
INTERVALO_ENVIO = 1.0  # Envia dados a cada 1 segundo

def iniciar_servidor():
    """Inicia o servidor e envia dados periodicamente"""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((HOST, PORT))
        s.listen()
        print(f"Servidor iniciado em {HOST}:{PORT}. Aguardando conexão da Unity...")
        
        conn, addr = s.accept()
        with conn:
            print(f"Conectado com {addr}")
            try:
                while True:
                    # Envia os dados diretamente
                    conn.sendall(json.dumps(ESCOAMENTOS).encode('utf-8'))
                    print(f"Dados enviados: {ESCOAMENTOS}")
                    
                    # Aguarda antes do próximo envio
                    time.sleep(INTERVALO_ENVIO)
                    
            except (ConnectionResetError, BrokenPipeError):
                print("Conexão com a Unity foi perdida")
            except KeyboardInterrupt:
                print("Servidor encerrado pelo usuário")

if __name__ == "__main__":
    iniciar_servidor()
