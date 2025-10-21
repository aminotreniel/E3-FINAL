using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerPoison : MonoBehaviour
{
    [Header("Poison Settings")]
    [Tooltip("Maximum poison value (start value)")]
    public float maxPoison = 100f;
    
    [Tooltip("Current poison value")]
    public float currentPoison = 100f;
    
    [Tooltip("How many poison points are lost per second")]
    public float decayPerSecond = 1f;
    
    [Tooltip("Fraction (0-1) at which blur effect should begin (e.g., 0.5 for 50%)")]
    [Range(0f, 1f)]
    public float blurThreshold = 0.5f;

    [Header("Blur Settings")]
    [Tooltip("Maximum blur intensity when poison is at 0")]
    [Range(0f, 1f)]
    public float maxBlurAlpha = 0.7f;
    
    [Tooltip("How fast the blur fades in/out")]
    public float blurFadeSpeed = 2f;

    // UI Elements (created automatically)
    private Canvas poisonCanvas;
    private GameObject poisonBarContainer;
    private Image poisonBarBackground;
    private Image poisonBarFill;
    private Image blurOverlay;
    
    private float targetBlurAlpha = 0f;

    void Start()
    {
        currentPoison = Mathf.Clamp(currentPoison, 0f, maxPoison);
        
        CreatePoisonBarUI();
        UpdatePoisonBar();
    }

    void Update()
    {
        // Decay poison over time
        if (currentPoison > 0f)
        {
            currentPoison -= decayPerSecond * Time.deltaTime;
            currentPoison = Mathf.Max(currentPoison, 0f);
            UpdatePoisonBar();
        }

        // Compute normalized fraction (0..1)
        float fraction = (maxPoison > 0f) ? (currentPoison / maxPoison) : 0f;

        // Calculate blur intensity based on poison level
        if (fraction <= blurThreshold)
        {
            // Map from blurThreshold -> 0 to 0..1 for blur intensity
            float t = Mathf.InverseLerp(blurThreshold, 0f, fraction);
            targetBlurAlpha = Mathf.Lerp(0f, maxBlurAlpha, t);
        }
        else
        {
            targetBlurAlpha = 0f;
        }

        // Apply blur fade
        if (blurOverlay != null)
        {
            var col = blurOverlay.color;
            col.a = Mathf.MoveTowards(col.a, targetBlurAlpha, blurFadeSpeed * Time.deltaTime);
            blurOverlay.color = col;
        }
    }

    private void CreatePoisonBarUI()
    {
        // Find or create canvas
        poisonCanvas = FindFirstObjectByType<Canvas>();
        if (poisonCanvas == null)
        {
            GameObject canvasObj = new GameObject("PoisonCanvas");
            poisonCanvas = canvasObj.AddComponent<Canvas>();
            poisonCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            poisonCanvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create main container with shadow
        poisonBarContainer = new GameObject("PoisonBarContainer");
        poisonBarContainer.transform.SetParent(poisonCanvas.transform, false);
        RectTransform containerRect = poisonBarContainer.AddComponent<RectTransform>();
        
        // Position in top-left corner, below health bar
        containerRect.anchorMin = new Vector2(0f, 1f);
        containerRect.anchorMax = new Vector2(0f, 1f);
        containerRect.pivot = new Vector2(0f, 1f);
        containerRect.anchoredPosition = new Vector2(20f, -70f); // Below health bar
        containerRect.sizeDelta = new Vector2(280f, 40f);

        // Add shadow effect to container
        var containerShadow = poisonBarContainer.AddComponent<Shadow>();
        containerShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        containerShadow.effectDistance = new Vector2(3, -3);

        // Create outer frame/border
        GameObject outerFrameObj = new GameObject("OuterFrame");
        outerFrameObj.transform.SetParent(poisonBarContainer.transform, false);
        Image outerFrame = outerFrameObj.AddComponent<Image>();
        outerFrame.color = new Color(0.1f, 0.12f, 0.1f, 0.95f); // Dark green-gray
        
        RectTransform outerFrameRect = outerFrameObj.GetComponent<RectTransform>();
        outerFrameRect.anchorMin = Vector2.zero;
        outerFrameRect.anchorMax = Vector2.one;
        outerFrameRect.sizeDelta = Vector2.zero;
        
        // Add thick outline to outer frame
        var outerOutline = outerFrameObj.AddComponent<Outline>();
        outerOutline.effectColor = new Color(0.3f, 0.5f, 0.3f, 1f); // Green tint
        outerOutline.effectDistance = new Vector2(2, 2);

        // Create poison bar container (inset)
        GameObject poisonContainerObj = new GameObject("PoisonBarInset");
        poisonContainerObj.transform.SetParent(outerFrameObj.transform, false);
        Image poisonInset = poisonContainerObj.AddComponent<Image>();
        poisonInset.color = new Color(0.05f, 0.08f, 0.05f, 1f); // Very dark green inset
        
        RectTransform poisonContainerRect = poisonContainerObj.GetComponent<RectTransform>();
        poisonContainerRect.anchorMin = Vector2.zero;
        poisonContainerRect.anchorMax = Vector2.one;
        poisonContainerRect.anchoredPosition = Vector2.zero;
        poisonContainerRect.sizeDelta = new Vector2(-8f, -8f);
        
        // Add inner shadow for depth
        var insetShadow = poisonContainerObj.AddComponent<Shadow>();
        insetShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        insetShadow.effectDistance = new Vector2(1, -1);

        // Create poison bar background (darker area)
        GameObject poisonBgObj = new GameObject("PoisonBarBackground");
        poisonBgObj.transform.SetParent(poisonContainerObj.transform, false);
        poisonBarBackground = poisonBgObj.AddComponent<Image>();
        poisonBarBackground.color = new Color(0.1f, 0.15f, 0.1f, 1f);
        
        RectTransform poisonBgRect = poisonBgObj.GetComponent<RectTransform>();
        poisonBgRect.anchorMin = Vector2.zero;
        poisonBgRect.anchorMax = Vector2.one;
        poisonBgRect.anchoredPosition = Vector2.zero;
        poisonBgRect.sizeDelta = new Vector2(-6f, -6f);

        // Create poison bar fill
        GameObject poisonFillObj = new GameObject("PoisonBarFill");
        poisonFillObj.transform.SetParent(poisonBgObj.transform, false);
        poisonBarFill = poisonFillObj.AddComponent<Image>();
        poisonBarFill.color = new Color(1f, 0.85f, 0.2f, 1f); // Bright yellow-gold
        
        RectTransform poisonFillRect = poisonFillObj.GetComponent<RectTransform>();
        poisonFillRect.anchorMin = new Vector2(0f, 0f);
        poisonFillRect.anchorMax = new Vector2(0f, 1f);
        poisonFillRect.pivot = new Vector2(0f, 0.5f);
        poisonFillRect.anchoredPosition = new Vector2(2f, 0f);
        poisonFillRect.sizeDelta = new Vector2(0f, -4f);
        
        // Add glow to poison bar
        var fillGlow = poisonFillObj.AddComponent<Outline>();
        fillGlow.effectColor = new Color(1f, 1f, 0.5f, 0.6f);
        fillGlow.effectDistance = new Vector2(1, 1);
        
        // Create highlight overlay on poison bar for shine effect
        GameObject highlightObj = new GameObject("PoisonBarHighlight");
        highlightObj.transform.SetParent(poisonFillObj.transform, false);
        Image highlight = highlightObj.AddComponent<Image>();
        highlight.color = new Color(1f, 1f, 1f, 0.25f); // White highlight
        
        RectTransform highlightRect = highlightObj.GetComponent<RectTransform>();
        highlightRect.anchorMin = new Vector2(0f, 0.6f);
        highlightRect.anchorMax = new Vector2(1f, 1f);
        highlightRect.sizeDelta = Vector2.zero;
        
        // Create blur overlay (green tinted blur when poison is low)
        GameObject blurObj = new GameObject("PoisonBlurOverlay");
        blurObj.transform.SetParent(poisonCanvas.transform, false);
        
        blurOverlay = blurObj.AddComponent<Image>();
        blurOverlay.color = new Color(0.2f, 0.5f, 0.2f, 0f); // Green tint, fully transparent at start
        blurOverlay.raycastTarget = false;
        
        RectTransform blurRect = blurObj.GetComponent<RectTransform>();
        blurRect.anchorMin = Vector2.zero;
        blurRect.anchorMax = Vector2.one;
        blurRect.sizeDelta = Vector2.zero;
        blurRect.SetAsFirstSibling(); // Behind other UI elements but in front of game
    }

    private void UpdatePoisonBar()
    {
        if (poisonBarFill == null || poisonBarBackground == null) return;

        // Clamp poison to valid range
        currentPoison = Mathf.Clamp(currentPoison, 0, maxPoison);
        
        float poisonPercentage = currentPoison / maxPoison;
        
        // Calculate the available width (background width minus padding)
        float bgWidth = poisonBarBackground.rectTransform.rect.width - 4f;
        float targetWidth = bgWidth * poisonPercentage;
        
        // Update the fill width
        poisonBarFill.rectTransform.sizeDelta = new Vector2(targetWidth, poisonBarFill.rectTransform.sizeDelta.y);

        // Change color based on poison percentage
        if (poisonPercentage > 0.6f)
        {
            // High poison: Bright Yellow-Gold
            poisonBarFill.color = new Color(1f, 0.85f, 0.2f, 1f);
            
            var fillGlow = poisonBarFill.GetComponent<Outline>();
            if (fillGlow != null)
                fillGlow.effectColor = new Color(1f, 1f, 0.5f, 0.6f);
        }
        else if (poisonPercentage > 0.3f)
        {
            // Medium poison: Orange
            poisonBarFill.color = new Color(1f, 0.5f, 0.1f, 1f);
            
            var fillGlow = poisonBarFill.GetComponent<Outline>();
            if (fillGlow != null)
                fillGlow.effectColor = new Color(1f, 0.6f, 0.2f, 0.7f);
        }
        else
        {
            // Low poison: Red (critical warning)
            poisonBarFill.color = new Color(1f, 0.15f, 0.1f, 1f);
            
            var fillGlow = poisonBarFill.GetComponent<Outline>();
            if (fillGlow != null)
                fillGlow.effectColor = new Color(1f, 0.3f, 0.3f, 0.8f);
        }
    }

    // Public helper to apply additional poison (positive) or heal (negative)
    public void ApplyPoison(float amount)
    {
        currentPoison = Mathf.Clamp(currentPoison + amount, 0f, maxPoison);
        UpdatePoisonBar();
    }
}
