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

    // Calculates the scent parameters URL based on the distance
    string ScentParametersUrl(float distance)
    {
        int intensity = Mathf.CeilToInt(3 - 2 * (distance / triggerDistance));
        Debug.Log("Calculated Intensity: " + intensity);
        float duration = 3; // Fixed duration for this example

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

        string deviceURL = "http://" + deviceIP + "/diffuse?duration=" + duration + "&intensity=" + intensity_level;
        return deviceURL;
    }

    void Update()
    {
        float currentDistance = Vector3.Distance(playerTransform.position, transform.position);
        // If player is within range and our looping coroutine isn't running, start it
        if (currentDistance <= triggerDistance && triggerCoroutine == null)
        {
            Debug.Log("Player within trigger distance. Starting loop.");
            triggerCoroutine = StartCoroutine(TriggerLoop());
        }
        // If player moves out of range and our coroutine is running, stop it
        else if (currentDistance > triggerDistance && triggerCoroutine != null)
        {
            Debug.Log("Player out of trigger distance. Stopping loop.");
            StopCoroutine(triggerCoroutine);
            triggerCoroutine = null;
        }
    }

    IEnumerator TriggerLoop()
    {
        while (true)
        {
            // Get current distance in case it affects intensity
            float currentDistance = Vector3.Distance(playerTransform.position, transform.position);
            string url = ScentParametersUrl(currentDistance);
            Debug.Log("Looping - Sending request to: " + url);

            if (dataSender != null)
            {
                dataSender.GetRequest(url);
            }
            else
            {
                Debug.LogWarning("DataSender is null! Please assign it in the Inspector.");
            }

            if (perfumeParticles != null)
            {
                perfumeParticles.Play();
            }
            else
            {
                Debug.LogWarning("perfumeParticles is null! Please assign it in the Inspector.");
            }

            yield return new WaitForSeconds(loopInterval);
        }
    }
}
