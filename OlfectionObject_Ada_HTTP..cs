using System;
using System.IO; // For file operations
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DataSender : MonoBehaviour
{
    private bool isSendingRequest = false;

    // Replace these with your Adafruit IO credentials
    public string adafruitIOKey = "aio_bIPa7615TKpIhUSlORmQmG2PHhMn";
    public string adafruitIOUsername = "Mahfuj";
    public string feedName = "exhalia-http";

    private string filePath;

    void Start()
    {
        // Set the file path where the CSV will be saved
        filePath = Application.persistentDataPath + "/TIMESTAMPS-VR-TO-ADAFRUIT.csv";
        Debug.Log("File Path: " + filePath);

        // Write headers to the CSV file if it doesn't exist
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("Send Timestamp,Response Timestamp");
            }
        }
    }

    public void GetRequest(string deviceIP, string duration, string intensity)
    {
        if (!isSendingRequest)
        {
            StartCoroutine(SendToAdafruitIO(deviceIP, duration, intensity));
        }
    }

    IEnumerator SendToAdafruitIO(string deviceIP, string duration, string intensity)
    {
        isSendingRequest = true;

        string url = $"https://io.adafruit.com/api/v2/{adafruitIOUsername}/feeds/{feedName}/data";
        string innerJson = $"{{\"deviceIP\":\"{deviceIP}\",\"duration\":\"{duration}\",\"intensity\":\"{intensity}\"}}";
        string json = $"{{\"value\":\"{innerJson.Replace("\"", "\\\"")}\"}}";
        Debug.Log("URL: " + url);
        Debug.Log("JSON: " + json);

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-AIO-Key", adafruitIOKey);

            string sendToAdafruitTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Send Timestamp: " + sendToAdafruitTimestamp);


            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
                string ResponseFromAdafruitTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log("Response Timestamp: " + ResponseFromAdafruitTimestamp);

                // Write to CSV file
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"{sendToAdafruitTimestamp},{ResponseFromAdafruitTimestamp}");
                }
            }
        }

        yield return new WaitForSeconds(7f);
        isSendingRequest = false;
    }
}
