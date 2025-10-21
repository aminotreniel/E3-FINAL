using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerPoison : MonoBehaviour
{
    public static PlayerPoison Instance; // Singleton to prevent duplicates
    
    [Header("Poison Settings")]
    [Tooltip("Maximum poison value (start value)")]
    public float maxPoison = 100f;
    
    [Tooltip("Current poison value")]
    public float currentPoison = 100f;
    
    [Tooltip("How many poison points are lost per second")]
    public float decayPerSecond = 1f;
    
    [Tooltip("Fraction (0-1) at which blackout effect should begin (e.g., 0.5 for 50%)")]
    [Range(0f, 1f)]
    public float blackoutThreshold = 0.5f;

    [Header("Blackout Settings")]
    [Tooltip("Maximum blackout intensity when poison is at 0 (0-1, where 1 is completely black)")]
    [Range(0f, 1f)]
    public float maxBlackoutAlpha = 0.9f;
    
    [Tooltip("How fast the blackout fades in/out")]
    public float blackoutFadeSpeed = 2f;

    // UI Elements (created automatically)
    private Canvas poisonCanvas;
    private GameObject poisonBarContainer;
    private Image poisonBarBackground;
    private Image poisonBarFill;
    private Image blackoutOverlay;
    
    private float targetBlackoutAlpha = 0f;

    void Awake()
    {
        // Singleton pattern to prevent duplicate poison systems across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make poison system persist across scenes
        }
        else
        {
            // Destroy duplicate
            Destroy(gameObject);
            return;
        }
    }

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

        // Calculate blackout intensity based on poison level
        if (fraction <= blackoutThreshold)
        {
            // Map from blackoutThreshold -> 0 to 0..1 for blackout intensity
            float t = Mathf.InverseLerp(blackoutThreshold, 0f, fraction);
            targetBlackoutAlpha = Mathf.Lerp(0f, maxBlackoutAlpha, t);
        }
        else
        {
            targetBlackoutAlpha = 0f;
        }

        // Apply blackout fade
        if (blackoutOverlay != null)
        {
            var col = blackoutOverlay.color;
            col.a = Mathf.MoveTowards(col.a, targetBlackoutAlpha, blackoutFadeSpeed * Time.deltaTime);
            blackoutOverlay.color = col;
        }
    }

    private void CreatePoisonBarUI()
    {
        // Check if we already have our UI
        if (poisonBarContainer != null)
        {
            return; // UI already exists, don't recreate
        }

        // Create our own dedicated canvas for poison
        GameObject canvasObj = new GameObject("PoisonCanvas");
        poisonCanvas = canvasObj.AddComponent<Canvas>();
        poisonCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        poisonCanvas.sortingOrder = 101; // Slightly higher than health
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Make canvas persist across scenes too
        DontDestroyOnLoad(canvasObj);

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
        
        // Create blackout overlay (screen goes black when poison is low)
        GameObject blackoutObj = new GameObject("PoisonBlackoutOverlay");
        blackoutObj.transform.SetParent(poisonCanvas.transform, false);
        
        blackoutOverlay = blackoutObj.AddComponent<Image>();
        blackoutOverlay.color = new Color(0f, 0f, 0f, 0f); // Pure black, fully transparent at start
        blackoutOverlay.raycastTarget = false;
        
        RectTransform blackoutRect = blackoutObj.GetComponent<RectTransform>();
        blackoutRect.anchorMin = Vector2.zero;
        blackoutRect.anchorMax = Vector2.one;
        blackoutRect.sizeDelta = Vector2.zero;
        blackoutRect.SetAsFirstSibling(); // Behind other UI elements but in front of game
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
