using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Light Settings")]
    public Light targetLight;             // Assign in Inspector (or auto-detected)
    public Color flickerColor = Color.red;
    public float minIntensity = 0.3f;
    public float maxIntensity = 1.5f;

    [Header("Flicker Behavior")]
    public bool useHardBlink = false;     // Toggle between smooth flicker & hard blink
    public float flickerSpeed = 0.1f;     // Lower = faster flicker
    public float blinkInterval = 0.1f;    // Time between blinks (if using Hard Blink)

    [Header("Random Settings")]
    public bool randomize = true;
    public float randomRange = 0.2f;

    private float timer;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        if (targetLight != null)
            targetLight.color = flickerColor;
    }

    void Update()
    {
        if (targetLight == null) return;

        if (useHardBlink)
        {
            // Hard blinking mode
            timer += Time.deltaTime;
            if (timer >= blinkInterval)
            {
                targetLight.enabled = !targetLight.enabled; // toggle light on/off
                timer = 0f;
            }
        }
        else
        {
            // Smooth flicker mode using Perlin noise
            float noise = Mathf.PerlinNoise(Time.time * (1f / flickerSpeed), 0.0f);
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

            if (randomize)
                intensity += Random.Range(-randomRange, randomRange);

            targetLight.intensity = Mathf.Clamp(intensity, minIntensity, maxIntensity);
        }
    }
}
