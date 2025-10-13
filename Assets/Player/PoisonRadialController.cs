using UnityEngine;

[RequireComponent(typeof(UnityEngine.UI.RawImage))]
public class PoisonRadialController : MonoBehaviour
{
    [Tooltip("Reference to the PlayerPoison script to sample poison fraction from")]
    public PlayerPoison playerPoison;

    [Tooltip("Material using the UI/RadialRevealBlur shader")]
    public Material radialMaterial;

    [Tooltip("When true, the radial progress will be driven by normalized poison (0..1).")]
    public bool invert = false;

    [Header("Threshold Mapping")]
    [Tooltip("Fraction (0..1) at which the radial blur begins. Example: 0.5 means blur starts when poison <= 50%.")]
    [Range(0f, 1f)]
    public float startThreshold = 0.5f;

    private UnityEngine.UI.RawImage rawImage;
    private int progressID;

    void Awake()
    {
        rawImage = GetComponent<UnityEngine.UI.RawImage>();
        progressID = Shader.PropertyToID("_Progress");
        if (radialMaterial != null)
        {
            // assign material instance to avoid modifying shared asset at runtime
            rawImage.material = new Material(radialMaterial);
        }
    }

    void Update()
    {
        if (playerPoison == null || rawImage == null || rawImage.material == null)
            return;

        float fraction = (playerPoison.maxPoison > 0f) ? (playerPoison.currentPoison / playerPoison.maxPoison) : 0f;
        // map fraction to progress with threshold: when fraction > startThreshold => progress 0
        float progress = 0f;
        if (fraction <= startThreshold)
        {
            // map from startThreshold -> 0 to 0..1
            float t = Mathf.InverseLerp(startThreshold, 0f, fraction);
            progress = invert ? 1f - t : t;
        }
        else
        {
            progress = 0f;
        }

        progress = Mathf.Clamp01(progress);
        rawImage.material.SetFloat(progressID, progress);
    }
}
