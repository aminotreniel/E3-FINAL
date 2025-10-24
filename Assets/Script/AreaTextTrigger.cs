using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class AreaTextTrigger : MonoBehaviour
{
    [Header("Message Settings")]
    [TextArea] public string[] messages;  // Multiple messages in Inspector
    public float displayTime = 3f;        // How long each message stays
    public float fadeDuration = 1f;       // Time for fade in/out
    public float delayBetween = 0.5f;     // Delay between messages

    private bool hasTriggered = false;    // Prevent multiple triggers
    
    // UI Elements
    private Canvas uiCanvas;
    private GameObject messageContainer;
    private TextMeshProUGUI messageText;
    
    void Start()
    {
        CreateMessageUI();
    }
    
    private void CreateMessageUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("AreaMessageCanvas");
        uiCanvas = canvasObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Container
        messageContainer = new GameObject("MessageContainer");
        messageContainer.transform.SetParent(canvasObj.transform, false);
        
        RectTransform containerRect = messageContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.15f);
        containerRect.anchorMax = new Vector2(0.5f, 0.15f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(1000f, 150f);
        
        // Create Text (no background)
        GameObject textObj = new GameObject("MessageText");
        textObj.transform.SetParent(messageContainer.transform, false);
        
        messageText = textObj.AddComponent<TextMeshProUGUI>();
        messageText.text = "";
        messageText.fontSize = 42;
        messageText.color = new Color(1f, 1f, 1f, 0f); // Start transparent
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.textWrappingMode = TextWrappingModes.Normal;
        messageText.fontStyle = FontStyles.Bold;
        
        // Try to load font
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
        {
            font = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
        }
        if (font != null)
        {
            messageText.font = font;
        }
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10f, 10f);
        textRect.offsetMax = new Vector2(-10f, -10f);
        
        // Strong black outline for clarity
        var textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = new Color(0f, 0f, 0f, 0f); // Start transparent
        textOutline.effectDistance = new Vector2(4, 4);
        
        // Add shadow for extra readability
        var textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0f, 0f, 0f, 0f); // Start transparent
        textShadow.effectDistance = new Vector2(3, -3);
        
        // Hide container initially
        messageContainer.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(ShowMessages());
        }
    }

    private IEnumerator ShowMessages()
    {
        messageContainer.SetActive(true);
        
        // Get text effects for fading
        Outline textOutline = messageText.GetComponent<Outline>();
        Shadow textShadow = messageText.GetComponent<Shadow>();
        
        foreach (string msg in messages)
        {
            // Set message text
            messageText.text = msg;
            
            // Fade in text with outline and shadow
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, t / fadeDuration);
                
                // Fade text
                Color textColor = messageText.color;
                textColor.a = alpha;
                messageText.color = textColor;
                
                // Fade outline
                if (textOutline != null)
                {
                    Color outlineColor = textOutline.effectColor;
                    outlineColor.a = alpha;
                    textOutline.effectColor = outlineColor;
                }
                
                // Fade shadow
                if (textShadow != null)
                {
                    Color shadowColor = textShadow.effectColor;
                    shadowColor.a = alpha * 0.8f;
                    textShadow.effectColor = shadowColor;
                }
                
                yield return null;
            }

            // Stay visible
            yield return new WaitForSeconds(displayTime);

            // Fade out text with outline and shadow
            t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, t / fadeDuration);
                
                // Fade text
                Color textColor = messageText.color;
                textColor.a = alpha;
                messageText.color = textColor;
                
                // Fade outline
                if (textOutline != null)
                {
                    Color outlineColor = textOutline.effectColor;
                    outlineColor.a = alpha;
                    textOutline.effectColor = outlineColor;
                }
                
                // Fade shadow
                if (textShadow != null)
                {
                    Color shadowColor = textShadow.effectColor;
                    shadowColor.a = alpha * 0.8f;
                    textShadow.effectColor = shadowColor;
                }
                
                yield return null;
            }

            // Small delay before next message
            yield return new WaitForSeconds(delayBetween);
        }
        
        messageContainer.SetActive(false);
    }
}
