from flask import Flask, request
import requests
import json
from Adafruit_IO import Client, Feed, Data
import time

app = Flask(__name__)

ADAFRUIT_IO_KEY = 'aio_kLZe40TLR6etGrMlLRABSicq2tz'
ADAFRUIT_IO_USERNAME = 'Mahfuj'
FEED_NAME = 'exhalia-http'

aio = Client(ADAFRUIT_IO_USERNAME, ADAFRUIT_IO_KEY)


last_timestamp = None  # Variable to store the timestamp of the last processed data

@app.route('/command', methods=['POST'])
def handle_command():
    data = request.json
    device_ip = data['deviceIP']
    duration = data['duration']
    intensity = data['intensity']
    
    url = f'http://{device_ip}/diffuse?duration={duration}&intensity={intensity}'
    
    try:
        response = requests.get(url)
        response.raise_for_status()
        return {'status': 'success', 'response': response.text}, 200
    except requests.RequestException as e:
        return {'status': 'error', 'error': str(e)}, 500

def check_feed_updates():
    global last_timestamp
    while True:
        try:
            data = aio.receive(FEED_NAME)
            if data is not None:
                current_timestamp = data.created_at
                if current_timestamp != last_timestamp and last_timestamp != None:
                    last_timestamp = current_timestamp
                    print(f'Received data at {current_timestamp}: {data.value}')
                    
                    command = json.loads(data.value)
                    device_ip = command['deviceIP']
                    duration = command['duration']
                    intensity = command['intensity']
                    
                    url = f'http://{device_ip}/diffuse?duration={duration}&intensity={intensity}'
                    try:
                        response = requests.get(url)
                        response.raise_for_status()
                        print(f'Success: {response.text}')
                    except requests.RequestException as e:
                        print(f'Error: {e}')
                elif last_timestamp == None:
                    last_timestamp = current_timestamp
                    
                    
        except Exception as e:
            print(f'Error receiving data: {e}')
        
        time.sleep(0.2)  # Poll every 0.2 seconds for updates

if __name__ == '__main__':
    # Start a separate thread to continuously check for feed updates
    import threading
    update_thread = threading.Thread(target=check_feed_updates, daemon=True)
    update_thread.start()
    
    # Run the Flask web server
    app.run(port=5000)
