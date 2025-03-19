using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OlfactionObject : MonoBehaviour
{
    [Tooltip("The ParticleSystem for visual feedback when the trigger is activated")]
    public ParticleSystem perfumeParticles;

    [Tooltip("The DataSender component used to send the scent parameters to the dispenser")]
    public DataSender dataSender;

    [Tooltip("IP Address of the dispenser")]
    public string deviceIP;

    [Tooltip("Player's transform (e.g., the CenterEyeAnchor in your XR Rig)")]
    public Transform playerTransform;

    [Tooltip("Distance at which the trigger activates")]
    public float triggerDistance = 1.5f;

    [Tooltip("Time interval (in seconds) between each trigger request")]
    public float loopInterval = 7.0f;

    // Hold a reference to the coroutine so it can be stopped later
    private Coroutine triggerCoroutine;

    // This method calculates the scent parameters and sends the request using three parameters.
    void SendScentRequest(float distance)
    {
        // Calculate intensity based on the distance relative to triggerDistance.
        int intensity = Mathf.CeilToInt(3 - 2 * (distance / triggerDistance));
        Debug.Log("Calculated Intensity: " + intensity);
        float duration = 7; // Fixed duration for this example

        // Determine the intensity level string.
        string intensity_level = "";
        if (intensity == 1)
        {
            intensity_level = "low";
        }
        else if (intensity == 2)
        {
            intensity_level = "medium";
        }
        else if (intensity == 3)
        {
            intensity_level = "high";
        }

        // Construct the base URL for the dispenser endpoint.
        // (In this case, we're just using the deviceIP as the identifier.)
        string urlBase = deviceIP;
        Debug.Log("Sending scent request: URL=" + urlBase + "/diffuse?duration=" + duration + "&intensity=" + intensity_level);

        // Send the command using the SendCommand method instead of GetRequest.
        if (dataSender != null)
        {
            dataSender.SendCommand(urlBase, duration.ToString(), intensity_level);
        }
        else
        {
            Debug.LogWarning("DataSender is null! Please assign it in the Inspector.");
        }

        // Play the particle system for visual feedback.
        if (perfumeParticles != null)
        {
            perfumeParticles.Play();
        }
        else
        {
            Debug.LogWarning("perfumeParticles is null! Please assign it in the Inspector.");
        }
    }

    void Update()
    {
        // Calculate the current distance between the player and this object.
        float currentDistance = Vector3.Distance(playerTransform.position, transform.position);
        Debug.Log("Current Distance: " + currentDistance);

        // If the player is within range and the looping coroutine isn't already running, start it.
        if (currentDistance <= triggerDistance && triggerCoroutine == null)
        {
            Debug.Log("Player within trigger distance. Starting loop.");
            triggerCoroutine = StartCoroutine(TriggerLoop());
        }
        // If the player moves out of range and the coroutine is running, stop it.
        else if (currentDistance > triggerDistance && triggerCoroutine != null)
        {
            Debug.Log("Player out of trigger distance. Stopping loop.");
            StopCoroutine(triggerCoroutine);
            triggerCoroutine = null;
        }
    }

    IEnumerator TriggerLoop()
    {
        int count = 0;
        while (true)
        {
            count++;
            Debug.Log("TriggerLoop iteration: " + count);
            float currentDistance = Vector3.Distance(playerTransform.position, transform.position);
            SendScentRequest(currentDistance);
            yield return new WaitForSeconds(loopInterval);
        }
    }
}
