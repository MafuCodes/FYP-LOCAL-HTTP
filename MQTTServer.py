import json
import requests
import paho.mqtt.client as mqtt

ADAFRUIT_IO_USERNAME = 'Mahfuj'
ADAFRUIT_IO_KEY = 'aio_kLZe40TLR6etGrMlLRABSicq2tz'
MQTT_FEED_NAME = 'exhalia-mqtt'

MQTT_BROKER = 'io.adafruit.com'
MQTT_PORT = 1883
MQTT_TOPIC = f'{ADAFRUIT_IO_USERNAME}/feeds/{MQTT_FEED_NAME}'


def on_connect(client, userdata, flags, rc):
    print(f"Connected with result code {rc}")
    client.subscribe(MQTT_TOPIC)


def on_message(client, userdata, msg):
    print(f"Received message: {msg.payload.decode()}")
    command = json.loads(msg.payload.decode())

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


client = mqtt.Client()
client.username_pw_set(ADAFRUIT_IO_USERNAME, ADAFRUIT_IO_KEY)
client.on_connect = on_connect
client.on_message = on_message

client.connect(MQTT_BROKER, MQTT_PORT, 60)

client.loop_forever()
