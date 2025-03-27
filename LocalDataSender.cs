using System;
using System.Collections;
using System.Collections.Generic;
using System.IO; // For file operations
using UnityEngine;
using UnityEngine.Networking;

public class DataSender : MonoBehaviour
{
    private bool isSendingRequest = false;
    private string filePath;

    void Start()
    {
        // Set the file path where the CSV will be saved
        filePath = Path.Combine(Application.persistentDataPath, "timestamps.csv");
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

    public void GetRequest(string url)
    {
        if (!isSendingRequest)
        {
            StartCoroutine(GetRequestCoroutine(url));
        }
    }

    IEnumerator GetRequestCoroutine(string url)
    {
        isSendingRequest = true;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            //send_timestamp
            string sendTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.Log("Send Timestamp: " + sendTimestamp);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
                //receive response timestamp
                string responseTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                Debug.Log("Response Timestamp: " + responseTimestamp);

                // Write to CSV file
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"{sendTimestamp},{responseTimestamp}");
                }
            }
        }

        yield return new WaitForSeconds(7f); // Wait for 0.7 seconds before allowing another request

        isSendingRequest = false;
    }
}
