using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class TorchManagement : MonoBehaviour
{
    // Torch's actual light
    public GameObject torchLight;
    public GameObject playerAmbientLight;

    // Light settings: intensity and range
    Dictionary<int, float[]> lightSettings = new Dictionary<int, float[]>
    {
        { 0, new float[] { 0f, 0f } },      // Battery empty: light is off
        { 1, new float[] { 0.5f, 20f } },   // 1 bar
        { 2, new float[] { 0.9f, 42f } },   // 2 bars
        { 3, new float[] { 1.3f, 48f } },   // 3 bars
        { 4, new float[] { 2.0f, 53f } }    // Full battery: max intensity and range
    };
    Dictionary<int, float> ambientLightSettings = new Dictionary<int, float>
    {
        { 0, 0f },      // Battery empty: light is off
        { 1, 1.3f },   // 1 bar
        { 2, 1.45f },   // 2 bars
        { 3, 1.7f },   // 3 bars
        { 4, 2.0f }    // Full battery: max intensity
    };

    // Torch Logic
    public int maxBatteryBars = 4;
    public static int batteryBars;

    public float batteryBarDepleteTime = 25f;  // time for a single battery bar to deplete
    private float currentBarDepleteTime = 0;  // tracks how long since battery bar was full

    // If torch fully depletes
    public GameObject handleJumpScares;

    // Torch related audio
    private AudioSource audioSource;
    public AudioClip flickerSound;
    public AudioClip increaseSound;
    public AudioClip decreaseSound;
    public AudioClip torchOff;

    // Battery container on torch
    public GameObject batteryContainer;

    // Need to pause battery depletion if a scare is busy happening
    public GameObject notCaughtScares;
    public GameObject caughtScares;
    public GameObject torchDepleteScares;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Torch starts at full health
        batteryBars = maxBatteryBars;

        // Retrieve the intensity and range for the current battery bars
        float[] settings = lightSettings[batteryBars];

        // Set the light's intensity and range based on the battery level
        torchLight.GetComponent<Light>().intensity = settings[0];
        torchLight.GetComponent<Light>().range = settings[1];

        // Torch battery constantly depleting
        StartCoroutine(ConstantDepletion());
    }

    public void IncreaseBattery()
    {
        if (batteryBars < maxBatteryBars)
        {
            batteryBars++;
            // Add actual bars on torch
            batteryContainer.GetComponent<TorchBatteryManager>().IncreaseBattery(batteryBars);

            // Increase the actual light intensity
            ChangeLightIntensity();

            // Constant Depletion stops once the battery hits its final 20 seconds grace period, so need restart it
            if (batteryBars == 1)
            {
                currentBarDepleteTime = 0f;
                StartCoroutine(ConstantDepletion());
            }
        }

        // Don't need to play increase sound (battery pickup does that)

        // Reset the current deplete time (i.e. give the bar full health)
        currentBarDepleteTime = 0f;
    }


    private IEnumerator ConstantDepletion()
    {
        // Wait until bar has fully depleted
        while (currentBarDepleteTime < batteryBarDepleteTime)
        {
            // Need to pause battery depletion at certain times
            if (!PlayerMove.isColliding && !PlayerMove.isEnteringActionPoint && !PlayerMove.isLookingAround && !PlayerMove.isViewingActionPoint && !PlayerMove.stopPlayer && !PlayerMove.killPlayer && !notCaughtScares.GetComponent<NotCaughtScares>().enabled && !caughtScares.GetComponent<CaughtScares>().enabled && !torchDepleteScares.GetComponent<TorchDepleteScares>().enabled)
            {
                currentBarDepleteTime += Time.deltaTime;
            }

            yield return null; 
        }

        currentBarDepleteTime = 0; // next bar has full health
        StartCoroutine(DecreaseBattery());
    }

    private IEnumerator DecreaseBattery()
    {
        if (batteryBars > 1)
        {
            batteryBars--;

            StartCoroutine(Flicker()); // Flicker torch to visually show battery decrease
            yield return new WaitUntil(() => !audioSource.isPlaying);

            // Play torch decrease sound
            audioSource.clip = decreaseSound;
            audioSource.Play();

            // Remove actual health bars on torch
            StartCoroutine(batteryContainer.GetComponent<TorchBatteryManager>().DecreaseBattery(batteryBars + 1));

            StartCoroutine(ConstantDepletion()); // Next bar immediately begins depleting
        }

        // If battery completely runs out
        else
        {
            batteryBars = 0;

            // Remove actual bars on torch
            StartCoroutine(batteryContainer.GetComponent<TorchBatteryManager>().DecreaseBattery(batteryBars + 1));

            // 20 second grace period before final bar turns off
            yield return new WaitForSeconds(20f);
            //ChangeLightIntensity();
        }
    }

    public IEnumerator Flicker()
    {
        // Play flickering sound
        audioSource.clip = flickerSound;
        audioSource.Play();

        // Visually Flicker light
        StartCoroutine(FlickerLight());

        // Dim the light more
        yield return new WaitForSeconds(2.0f);
        ChangeLightIntensity();
    }

    private IEnumerator FlickerLight()
    {
        // Duration of the flicker effect
        float flickerDuration = 2.2f; // Total flicker duration
        float flickerEndTime = Time.time + flickerDuration;

        // Loop to create flickering effect
        while (Time.time < flickerEndTime)
        {
            // Toggle light on and off
            torchLight.SetActive(!torchLight.activeSelf);
            playerAmbientLight.SetActive(!playerAmbientLight.activeSelf);

            // Generate a random interval between flickerIntervalMin and flickerIntervalMax
            float flickerInterval = UnityEngine.Random.Range(0.1f, 0.25f); // Random interval between 0.1 and 0.8 seconds

            // Wait for the random flicker interval
            yield return new WaitForSeconds(flickerInterval);
        }

        // Ensure the light is on after flickering
        torchLight.SetActive(true);
        playerAmbientLight.SetActive(true);
    }

    private void ChangeLightIntensity()
    {
        if (lightSettings.ContainsKey(batteryBars))
        {
            // Retrieve the intensity and range for the current battery bars
            float[] settings = lightSettings[batteryBars];

            // Set the light's intensity and range based on the battery level
            torchLight.GetComponent<Light>().intensity = settings[0];
            torchLight.GetComponent<Light>().range = settings[1];

            // Also modify the player ambient light
            playerAmbientLight.GetComponent<Light>().intensity = ambientLightSettings[batteryBars];  // Scale down for ambient effect
        }
    }

    public void turnOff()
    {
        torchLight.GetComponent<Light>().intensity = 0;
        torchLight.GetComponent<Light>().range = 0;

        // Also modify the player ambient light
        playerAmbientLight.GetComponent<Light>().intensity = 0;  // Scale down for ambient effect
    }
    public void turnOn()
    {
        ChangeLightIntensity();
    }

}
