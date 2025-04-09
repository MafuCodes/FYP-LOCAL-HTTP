using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;


public class DataSender : M2MqttUnity.M2MqttUnityClient
{
    [Header("Adafruit IO Credentials")]
    public string adafruitIOKey = "aio_kLZe40TLR6etGrMlLRABSicq2tz";
    public string adafruitIOUsername = "Mahfuj";
    public string feedName = "exhalia-mqtt";

    private bool isPublishing = false; // Flag to prevent multiple messages from being sent at the same time

    private string topic;

    protected override void Awake()
    {
        base.Awake();
        mqttUserName = adafruitIOUsername;
        mqttPassword = adafruitIOKey;
        topic = $"{adafruitIOUsername}/feeds/{feedName}";
    }

    protected override void Start()
    {
        brokerAddress = "io.adafruit.com";
        brokerPort = 1883; // Default MQTT port
        autoConnect = true; // Automatically connect on startup

        base.Start();
    }

    public void SendCommand(string deviceIP, string duration, string intensity)
    {
        string message = $"{{\"deviceIP\":\"{deviceIP}\",\"duration\":\"{duration}\",\"intensity\":\"{intensity}\"}}";
        if (!isPublishing)
        {
            StartCoroutine(PublishRequest(message));
        }
    }

    private IEnumerator PublishRequest(string message)
    {
        isPublishing = true;

        if (client != null && client.IsConnected)
        {
            client.Publish(topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            Debug.Log("Message sent: " + message);
        }
        else
        {
            Debug.LogWarning("Client not connected. Cannot send message.");
            yield return null;
        }
        yield return new WaitForSeconds(7); // Wait for 2 seconds before sending the next message
        isPublishing = false;

    }


    protected override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("Successfully connected to Adafruit IO.");
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        base.OnConnectionFailed(errorMessage);
        Debug.LogError("Connection to Adafruit IO failed: " + errorMessage);
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        Debug.Log("Disconnected from Adafruit IO.");
    }

}

