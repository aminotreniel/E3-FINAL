using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("Red color for the flickering light")]
    public Color flickerColor = new Color(1f, 0f, 0f, 1f); // Pure bright red
    
    [Tooltip("Minimum light intensity")]
    [Range(0f, 10f)]
    public float minIntensity = 2f;
    
    [Tooltip("Maximum light intensity")]
    [Range(0f, 20f)]
    public float maxIntensity = 8f;
    
    [Tooltip("Light range in units")]
    [Range(1f, 100f)]
    public float lightRange = 15f;
    
    [Tooltip("Enable shadows for this light")]
    public bool enableShadows = true;

    [Header("Flicker Behavior")]
    [Tooltip("Type of flicker effect")]
    public FlickerMode flickerMode = FlickerMode.SmoothFlicker;
    
    [Tooltip("Speed of flicker (lower = faster)")]
    [Range(0.01f, 1f)]
    public float flickerSpeed = 0.15f;
    
    [Tooltip("Time between hard blinks (if using Hard Blink mode)")]
    [Range(0.05f, 1f)]
    public float blinkInterval = 0.2f;
    
    [Tooltip("Add random variation to flicker")]
    public bool addRandomVariation = true;
    
    [Tooltip("Amount of random variation")]
    [Range(0f, 1f)]
    public float randomVariation = 0.3f;

    [Header("Emergency/Alarm Mode (Optional)")]
    [Tooltip("Enable alarm mode for more dramatic effect")]
    public bool alarmMode = false;
    
    [Tooltip("Alarm pulse speed")]
    [Range(0.5f, 5f)]
    public float alarmSpeed = 2f;

    private Light lightComponent;
    private float timer;
    private float noiseOffset;

    public enum FlickerMode
    {
        SmoothFlicker,      // Continuous smooth flickering
        HardBlink,          // On/off blinking
        Unstable,           // Chaotic flickering like damaged light
        AlarmPulse          // Rhythmic alarm-like pulse
    }

    void Awake()
    {
        // Check if Light component already exists
        lightComponent = GetComponent<Light>();
        
        // If no Light component exists, create one automatically
        if (lightComponent == null)
        {
            lightComponent = gameObject.AddComponent<Light>();
            Debug.Log("LightFlicker: Light component automatically added to " + gameObject.name);
        }
        
        // Configure the light automatically
        SetupLight();
    }

    void SetupLight()
    {
        if (lightComponent == null) return;

        // Set light type to Spot (pointing downwards)
        lightComponent.type = LightType.Spot;
        
        // Set pure red color - FORCE it to be visible
        lightComponent.color = flickerColor;
        
        // Set initial intensity - MUCH BRIGHTER
        lightComponent.intensity = maxIntensity;
        
        // Set range
        lightComponent.range = lightRange;
        
        // Set spot angle (cone angle) - wider for more visibility
        lightComponent.spotAngle = 60f;
        
        // Inner spot angle for better cone definition
        lightComponent.innerSpotAngle = 30f;
        
        // Configure shadows - disable for better visibility and performance
        lightComponent.shadows = LightShadows.None;
        
        // Point the light downwards (negative Y axis)
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        
        // Set render mode for better visibility in URP
        lightComponent.renderMode = LightRenderMode.ForcePixel;
        
        // Ensure light is enabled
        lightComponent.enabled = true;
        
        // Set culling mask to Everything to ensure it lights all objects
        lightComponent.cullingMask = -1;
        
        // Random offset for Perlin noise to make each light unique
        noiseOffset = Random.Range(0f, 100f);
        
        Debug.Log("LightFlicker: RED flickering light configured on " + gameObject.name + " with intensity: " + maxIntensity);
    }

    void Update()
    {
        if (lightComponent == null) return;

        // Update color in case it was changed in inspector
        lightComponent.color = flickerColor;

        // Choose flicker behavior based on mode
        switch (flickerMode)
        {
            case FlickerMode.SmoothFlicker:
                SmoothFlicker();
                break;
            case FlickerMode.HardBlink:
                HardBlink();
                break;
            case FlickerMode.Unstable:
                UnstableFlicker();
                break;
            case FlickerMode.AlarmPulse:
                AlarmPulse();
                break;
        }

        // Apply alarm mode override if enabled
        if (alarmMode)
        {
            AlarmPulse();
        }
    }

    void SmoothFlicker()
    {
        // Use Perlin noise for smooth, organic flickering
        float noise = Mathf.PerlinNoise((Time.time + noiseOffset) * (1f / flickerSpeed), 0.0f);
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        // Add random variation for more realism
        if (addRandomVariation)
        {
            intensity += Random.Range(-randomVariation, randomVariation);
        }

        lightComponent.intensity = Mathf.Clamp(intensity, minIntensity, maxIntensity);
    }

    void HardBlink()
    {
        // Hard on/off blinking
        timer += Time.deltaTime;
        if (timer >= blinkInterval)
        {
            lightComponent.enabled = !lightComponent.enabled;
            timer = 0f;
        }
    }

    void UnstableFlicker()
    {
        // Chaotic flickering like a damaged or unstable light
        float noise1 = Mathf.PerlinNoise((Time.time + noiseOffset) * 10f, 0.0f);
        float noise2 = Mathf.PerlinNoise((Time.time + noiseOffset) * 5f, 100.0f);
        
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, noise1 * noise2);
        
        // Add sharp random drops to simulate power issues
        if (Random.value < 0.05f) // 5% chance per frame
        {
            intensity = minIntensity * 0.3f;
        }

        lightComponent.intensity = Mathf.Clamp(intensity, 0f, maxIntensity);
    }

    void AlarmPulse()
    {
        // Rhythmic alarm-style pulsing
        float pulse = Mathf.Sin(Time.time * alarmSpeed * Mathf.PI) * 0.5f + 0.5f;
        lightComponent.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
    }

    // Allow changes in inspector during play mode
    void OnValidate()
    {
        if (Application.isPlaying && lightComponent != null)
        {
            lightComponent.color = flickerColor;
            lightComponent.range = lightRange;
            
            if (enableShadows)
                lightComponent.shadows = LightShadows.Soft;
            else
                lightComponent.shadows = LightShadows.None;
        }
    }

    // Debug visualization in Scene view
    void OnDrawGizmos()
    {
        // Draw a red wire sphere to show light position and range
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        
        // Draw direction arrow pointing down
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 2f);
    }
}
