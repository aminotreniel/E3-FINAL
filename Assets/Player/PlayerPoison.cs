using UnityEngine;
using UnityEngine.UI;

public class PlayerPoison : MonoBehaviour
{
    [Header("Poison Settings")]
    [Tooltip("Maximum poison value (start value)")]
    public float maxPoison = 100f;
    [Tooltip("Current poison value")]
    [Range(0f, 100f)]
    public float currentPoison = 100f;
    [Tooltip("How many poison points are lost per second")]
    public float decayPerSecond = 5f;
    [Tooltip("Fraction (0-1) at which blur effect should begin (e.g., 0.5 for 50%)")]
    [Range(0f, 1f)]
    public float blurThreshold = 0.5f;

    [Header("UI References")]
    [Tooltip("Optional UI Slider to display poison (set in Inspector)")]
    public Slider poisonSlider;
    [Tooltip("Optional UI Image used as blur overlay. Set to full-screen Image with a blur material or sprite; we'll fade its alpha.")]
    public UnityEngine.UI.Image blurOverlay;

    [Header("Blur Settings")]
    [Tooltip("Alpha when blur is fully on")]
    [Range(0f, 1f)]
    public float blurMaxAlpha = 0.85f;
    [Tooltip("How fast the blur fades in/out")]
    public float blurFadeSpeed = 2f;

    [Header("Radial Shader (optional)")]
    [Tooltip("Optional RawImage that uses the UI/RadialRevealBlur shader. If assigned, the script will drive its material _Progress property.")]
    public RawImage radialRawImage;
    [Tooltip("Material based on UI/RadialRevealBlur shader (assign a material asset here). A material instance will be created at runtime.")]
    public Material radialMaterial;
    

    // internal
    private float targetBlurAlpha = 0f;

    void Start()
    {
        // initialize values
        currentPoison = Mathf.Clamp(currentPoison, 0f, maxPoison);
        if (poisonSlider != null)
        {
            poisonSlider.minValue = 0f;
            poisonSlider.maxValue = maxPoison;
            poisonSlider.value = currentPoison;
        }

        if (blurOverlay != null)
        {
            var c = blurOverlay.color;
            c.a = 0f;
            blurOverlay.color = c;
            blurOverlay.gameObject.SetActive(true);
        }

        
    }

    void Update()
    {
        // decay over time
        if (currentPoison > 0f)
        {
            currentPoison -= decayPerSecond * Time.deltaTime;
            currentPoison = Mathf.Max(currentPoison, 0f);
        }

        // update slider if assigned
        if (poisonSlider != null)
        {
            poisonSlider.value = currentPoison;
        }

        // compute normalized fraction (0..1)
        float fraction = (maxPoison > 0f) ? (currentPoison / maxPoison) : 0f;

        // decide blur target for alpha overlay
        if (fraction <= blurThreshold)
        {
            // fraction maps from blurThreshold -> 0 to 0..1, then we lerp alpha
            float t = Mathf.InverseLerp(blurThreshold, 0f, fraction);
            targetBlurAlpha = Mathf.Lerp(0f, blurMaxAlpha, t);
        }
        else
        {
            targetBlurAlpha = 0f;
        }

        // apply blur fade for simple overlay
        if (blurOverlay != null)
        {
            var col = blurOverlay.color;
            col.a = Mathf.MoveTowards(col.a, targetBlurAlpha, blurFadeSpeed * Time.deltaTime);
            blurOverlay.color = col;
        }

        // (radial shader driver removed — using overlay Image fade only)
    }

    // public helper to apply additional poison (positive) or heal (negative)
    public void ApplyPoison(float amount)
    {
        currentPoison = Mathf.Clamp(currentPoison + amount, 0f, maxPoison);
        if (poisonSlider != null)
            poisonSlider.value = currentPoison;
    }
}
