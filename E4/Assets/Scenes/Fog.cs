using UnityEngine;

public class Fog : MonoBehaviour
{
    [Header("Fog Settings")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;
    [SerializeField] private Color fogColor = new Color(0.2f, 0.3f, 0.25f, 1f); // Eerie greenish-grey for sci-fi lab
    [SerializeField] private float fogDensity = 0.02f; // For Exponential modes
    [SerializeField] private float fogStartDistance = 10f; // For Linear mode
    [SerializeField] private float fogEndDistance = 50f; // For Linear mode

    void Start()
    {
        // Enable fog
        RenderSettings.fog = enableFog;
        
        // Set fog mode (ExponentialSquared gives nice atmospheric effect)
        RenderSettings.fogMode = fogMode;
        
        // Set fog color (dark greenish tint for creepy lab atmosphere)
        RenderSettings.fogColor = fogColor;
        
        // Set fog density for exponential modes
        RenderSettings.fogDensity = fogDensity;
        
        // Set fog distances for linear mode
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
        
        Debug.Log("Fog system initialized for sci-fi laboratory environment");
    }

    void OnDestroy()
    {
        // Optional: Disable fog when this script is destroyed
        RenderSettings.fog = false;
    }

    // Optional: Allow runtime adjustments through code
    public void SetFogDensity(float density)
    {
        fogDensity = Mathf.Clamp(density, 0f, 1f);
        RenderSettings.fogDensity = fogDensity;
    }

    public void SetFogColor(Color color)
    {
        fogColor = color;
        RenderSettings.fogColor = fogColor;
    }

    public void ToggleFog(bool enable)
    {
        enableFog = enable;
        RenderSettings.fog = enableFog;
    }
}
