using UnityEngine;

public class FogSCript : MonoBehaviour
{
    [Header("Fog Settings")]
    [Tooltip("Enable or disable fog")]
    public bool enableFog = true;
    
    [Tooltip("Fog color - greenish/grayish for sci-fi laboratory atmosphere")]
    public Color fogColor = new Color(0.5f, 0.6f, 0.55f, 1f);
    
    [Tooltip("Fog mode: Linear, Exponential, or ExponentialSquared")]
    public FogMode fogMode = FogMode.Exponential;
    
    [Header("Linear Fog Settings (if Linear mode is selected)")]
    [Tooltip("Distance where fog starts (Linear mode only)")]
    public float fogStartDistance = 1f;
    
    [Tooltip("Distance where fog completely obscures (Linear mode only)")]
    public float fogEndDistance = 25f;
    
    [Header("Exponential Fog Settings (if Exponential mode is selected)")]
    [Tooltip("Fog density for Exponential/ExponentialSquared mode")]
    [Range(0f, 0.3f)]
    public float fogDensity = 0.15f;
    
    [Header("Dynamic Fog (Optional)")]
    [Tooltip("Enable fog density pulsing for eerie effect")]
    public bool enableFogPulsing = false;
    
    [Tooltip("Speed of fog pulsing")]
    public float pulseSpeed = 0.5f;
    
    [Tooltip("Amount of density variation")]
    [Range(0f, 0.1f)]
    public float pulseAmount = 0.02f;
    
    private float baseFogDensity;
    
    void Start()
    {
        // Store the base fog density for pulsing effect
        baseFogDensity = fogDensity;
        
        // Apply fog settings immediately
        ApplyFogSettings();
    }
    
    void Update()
    {
        // Optional: Create pulsing fog effect for more atmosphere
        if (enableFog && enableFogPulsing && (fogMode == FogMode.Exponential || fogMode == FogMode.ExponentialSquared))
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            RenderSettings.fogDensity = baseFogDensity + pulse;
        }
    }
    
    void ApplyFogSettings()
    {
        // Enable or disable fog
        RenderSettings.fog = enableFog;
        
        if (enableFog)
        {
            // Set fog color
            RenderSettings.fogColor = fogColor;
            
            // Set fog mode
            RenderSettings.fogMode = fogMode;
            
            // Set fog parameters based on mode
            if (fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
            }
            else
            {
                RenderSettings.fogDensity = fogDensity;
            }
        }
    }
    
    // Allow runtime changes in the Inspector to take effect
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            baseFogDensity = fogDensity;
            ApplyFogSettings();
        }
    }
    
    // Restore previous fog settings when this script is disabled or destroyed
    void OnDisable()
    {
        // Optional: You can restore default fog settings here if needed
        // RenderSettings.fog = false;
    }
}
