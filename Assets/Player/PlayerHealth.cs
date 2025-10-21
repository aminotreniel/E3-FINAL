
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Respawn Settings")]
    private Vector3 currentCheckpoint;
    public bool isDead = false;

    [Header("Damage Indicator Settings")]
    [Tooltip("Duration of the red flash when taking damage")]
    public float damageFlashDuration = 0.3f;

    // UI Elements (created automatically)
    private Canvas healthCanvas;
    private GameObject healthBarContainer;
    private Image healthBarBackground;
    private Image healthBarFill;
    private Image damageOverlay;

    void Start()
    {
        currentHealth = maxHealth;
        currentCheckpoint = transform.position; // First checkpoint = spawn position

        CreateHealthBarUI();
        UpdateHealthBar();
    }

    private void CreateHealthBarUI()
    {
        // Find or create canvas
        healthCanvas = FindFirstObjectByType<Canvas>();
        if (healthCanvas == null)
        {
            GameObject canvasObj = new GameObject("HealthCanvas");
            healthCanvas = canvasObj.AddComponent<Canvas>();
            healthCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            healthCanvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create main container with shadow
        healthBarContainer = new GameObject("HealthBarContainer");
        healthBarContainer.transform.SetParent(healthCanvas.transform, false);
        RectTransform containerRect = healthBarContainer.AddComponent<RectTransform>();
        
        // Position in top-left corner with better size
        containerRect.anchorMin = new Vector2(0f, 1f);
        containerRect.anchorMax = new Vector2(0f, 1f);
        containerRect.pivot = new Vector2(0f, 1f);
        containerRect.anchoredPosition = new Vector2(20f, -20f);
        containerRect.sizeDelta = new Vector2(280f, 40f);

        // Add shadow effect to container
        var containerShadow = healthBarContainer.AddComponent<Shadow>();
        containerShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        containerShadow.effectDistance = new Vector2(3, -3);

        // Create outer frame/border
        GameObject outerFrameObj = new GameObject("OuterFrame");
        outerFrameObj.transform.SetParent(healthBarContainer.transform, false);
        Image outerFrame = outerFrameObj.AddComponent<Image>();
        outerFrame.color = new Color(0.1f, 0.1f, 0.12f, 0.95f); // Dark blue-gray
        
        RectTransform outerFrameRect = outerFrameObj.GetComponent<RectTransform>();
        outerFrameRect.anchorMin = Vector2.zero;
        outerFrameRect.anchorMax = Vector2.one;
        outerFrameRect.sizeDelta = Vector2.zero;
        
        // Add thick outline to outer frame
        var outerOutline = outerFrameObj.AddComponent<Outline>();
        outerOutline.effectColor = new Color(0.4f, 0.4f, 0.45f, 1f);
        outerOutline.effectDistance = new Vector2(2, 2);

        // Create health bar container (inset)
        GameObject healthContainerObj = new GameObject("HealthBarInset");
        healthContainerObj.transform.SetParent(outerFrameObj.transform, false);
        Image healthInset = healthContainerObj.AddComponent<Image>();
        healthInset.color = new Color(0.05f, 0.05f, 0.08f, 1f); // Very dark inset
        
        RectTransform healthContainerRect = healthContainerObj.GetComponent<RectTransform>();
        healthContainerRect.anchorMin = Vector2.zero;
        healthContainerRect.anchorMax = Vector2.one;
        healthContainerRect.anchoredPosition = Vector2.zero;
        healthContainerRect.sizeDelta = new Vector2(-8f, -8f);
        
        // Add inner shadow for depth
        var insetShadow = healthContainerObj.AddComponent<Shadow>();
        insetShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        insetShadow.effectDistance = new Vector2(1, -1);

        // Create health bar background (darker area)
        GameObject healthBgObj = new GameObject("HealthBarBackground");
        healthBgObj.transform.SetParent(healthContainerObj.transform, false);
        healthBarBackground = healthBgObj.AddComponent<Image>();
        healthBarBackground.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        
        RectTransform healthBgRect = healthBgObj.GetComponent<RectTransform>();
        healthBgRect.anchorMin = Vector2.zero;
        healthBgRect.anchorMax = Vector2.one;
        healthBgRect.anchoredPosition = Vector2.zero;
        healthBgRect.sizeDelta = new Vector2(-6f, -6f);

        // Create health bar fill with gradient effect
        GameObject healthFillObj = new GameObject("HealthBarFill");
        healthFillObj.transform.SetParent(healthBgObj.transform, false);
        healthBarFill = healthFillObj.AddComponent<Image>();
        healthBarFill.color = new Color(0.2f, 1f, 0.3f, 1f); // Bright green
        
        RectTransform healthFillRect = healthFillObj.GetComponent<RectTransform>();
        healthFillRect.anchorMin = new Vector2(0f, 0f);
        healthFillRect.anchorMax = new Vector2(0f, 1f);
        healthFillRect.pivot = new Vector2(0f, 0.5f);
        healthFillRect.anchoredPosition = new Vector2(2f, 0f);
        healthFillRect.sizeDelta = new Vector2(0f, -4f);
        
        // Add glow to health bar
        var fillGlow = healthFillObj.AddComponent<Outline>();
        fillGlow.effectColor = new Color(0.5f, 1f, 0.5f, 0.6f);
        fillGlow.effectDistance = new Vector2(1, 1);
        
        // Create highlight overlay on health bar for shine effect
        GameObject highlightObj = new GameObject("HealthBarHighlight");
        highlightObj.transform.SetParent(healthFillObj.transform, false);
        Image highlight = highlightObj.AddComponent<Image>();
        highlight.color = new Color(1f, 1f, 1f, 0.25f); // White highlight
        
        RectTransform highlightRect = highlightObj.GetComponent<RectTransform>();
        highlightRect.anchorMin = new Vector2(0f, 0.6f);
        highlightRect.anchorMax = new Vector2(1f, 1f);
        highlightRect.sizeDelta = Vector2.zero;
        
        // Create damage overlay (red flash when taking damage)
        GameObject damageObj = new GameObject("DamageOverlay");
        damageObj.transform.SetParent(healthCanvas.transform, false);
        
        damageOverlay = damageObj.AddComponent<Image>();
        damageOverlay.color = new Color(1f, 0f, 0f, 0f); // Red, fully transparent
        damageOverlay.raycastTarget = false;
        
        RectTransform damageRect = damageObj.GetComponent<RectTransform>();
        damageRect.anchorMin = Vector2.zero;
        damageRect.anchorMax = Vector2.one;
        damageRect.sizeDelta = Vector2.zero;
    }

    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture for the character icon
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        float center = size / 2f;
        float radius = size / 2f - 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (distance <= radius)
                {
                    pixels[y * size + x] = Color.white;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null || healthBarBackground == null) return;

        // Clamp health to valid range
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        float healthPercentage = currentHealth / maxHealth;
        
        // Calculate the available width (background width minus padding)
        float bgWidth = healthBarBackground.rectTransform.rect.width - 4f; // 4f is total padding
        float targetWidth = bgWidth * healthPercentage;
        
        // Update the fill width
        healthBarFill.rectTransform.sizeDelta = new Vector2(targetWidth, healthBarFill.rectTransform.sizeDelta.y);

        // Change color based on health percentage with vibrant green colors
        if (healthPercentage > 0.6f)
        {
            // High health: Bright Green with glow
            healthBarFill.color = new Color(0.2f, 1f, 0.3f, 1f);
            
            // Update glow color
            var fillGlow = healthBarFill.GetComponent<Outline>();
            if (fillGlow != null)
                fillGlow.effectColor = new Color(0.5f, 1f, 0.5f, 0.6f);
        }
        else if (healthPercentage > 0.3f)
        {
            // Medium health: Yellow-Green with stronger glow
            healthBarFill.color = new Color(0.7f, 1f, 0.2f, 1f);
            
            var fillGlow = healthBarFill.GetComponent<Outline>();
            if (fillGlow != null)
                fillGlow.effectColor = new Color(0.8f, 1f, 0.4f, 0.7f);
        }
        else
        {
            // Low health: Yellow-Orange with intense glow
            healthBarFill.color = new Color(1f, 0.8f, 0.1f, 1f);
            
            var fillGlow = healthBarFill.GetComponent<Outline>();
            if (fillGlow != null)
                fillGlow.effectColor = new Color(1f, 0.9f, 0.3f, 0.8f);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();
        
        // Show damage flash
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private IEnumerator DamageFlash()
    {
        if (damageOverlay == null) yield break;
        
        // Flash red
        damageOverlay.color = new Color(1f, 0f, 0f, 0.3f);
        
        float elapsedTime = 0f;
        while (elapsedTime < damageFlashDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.3f, 0f, elapsedTime / damageFlashDuration);
            damageOverlay.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }
        
        // Ensure fully transparent
        damageOverlay.color = new Color(1f, 0f, 0f, 0f);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Died!");
        Respawn();
    }

    public void SetCheckpoint(Vector3 checkpointPos)
    {
        currentCheckpoint = checkpointPos;
        Debug.Log("Checkpoint updated: " + checkpointPos);
    }

    private void Respawn()
    {
        transform.position = currentCheckpoint; // move to last checkpoint

        currentHealth = maxHealth;
        UpdateHealthBar();

        isDead = false;
        Debug.Log("Respawned at checkpoint!");
    }
}
