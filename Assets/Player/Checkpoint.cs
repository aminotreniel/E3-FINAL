using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    private static GameObject checkpointTextObj;   // keeps only one text on screen
    private static TextMeshProUGUI checkpointText;
    private static CanvasGroup canvasGroup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.SetCheckpoint(transform.position);
                ShowCheckpointMessage("✅ Checkpoint Saved");
            }
        }
    }

    private void ShowCheckpointMessage(string message)
    {
        if (checkpointTextObj == null)
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("CheckpointCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // Make sure it's on top

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // Create Text container
            checkpointTextObj = new GameObject("CheckpointText");
            checkpointTextObj.transform.SetParent(canvasObj.transform, false);
            
            // Add RectTransform to container
            RectTransform containerRect = checkpointTextObj.AddComponent<RectTransform>();
            
            // Position the container at bottom-left
            containerRect.sizeDelta = new Vector2(300f, 60f);
            containerRect.anchorMin = new Vector2(0f, 0f);
            containerRect.anchorMax = new Vector2(0f, 0f);
            containerRect.pivot = new Vector2(0f, 0f);
            containerRect.anchoredPosition = new Vector2(10f, 10f); // Positioned above health/poison bars
            
            // Add CanvasGroup for smooth fading
            canvasGroup = checkpointTextObj.AddComponent<CanvasGroup>();

            // Create background panel for better visibility
            GameObject bgObj = new GameObject("CheckpointBackground");
            bgObj.transform.SetParent(checkpointTextObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.12f, 0.8f); // Semi-transparent dark background
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = new Vector2(20f, 10f); // Padding around text
            
            // Add outline to background
            var bgOutline = bgObj.AddComponent<Outline>();
            bgOutline.effectColor = new Color(0.3f, 0.8f, 0.3f, 1f); // Green outline
            bgOutline.effectDistance = new Vector2(2, 2);
            
            // Add shadow to background
            var bgShadow = bgObj.AddComponent<Shadow>();
            bgShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            bgShadow.effectDistance = new Vector2(3, -3);

            // Create TextMeshPro text
            GameObject textObj = new GameObject("CheckpointTextLabel");
            textObj.transform.SetParent(checkpointTextObj.transform, false);
            checkpointText = textObj.AddComponent<TextMeshProUGUI>();
            checkpointText.fontSize = 28;
            checkpointText.fontStyle = FontStyles.Bold;
            checkpointText.color = new Color(0.3f, 1f, 0.3f, 1f); // Bright green
            checkpointText.alignment = TextAlignmentOptions.Left;

            // Proper RectTransform size & position
            RectTransform textRect = checkpointText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.sizeDelta = new Vector2(-20f, 0f); // Add padding
            textRect.anchoredPosition = new Vector2(10f, 0f); // Slight left offset
            
            // Add body outline
            var textOutline = textObj.AddComponent<Outline>();
            textOutline.effectColor = Color.black;
            textOutline.effectDistance = new Vector2(2, 2);
        }

        // Update message text
        checkpointText.text = "Checkpoint Saved"; // Remove emoji for font compatibility

        // Reset alpha & show
        checkpointTextObj.SetActive(true);
        canvasGroup.alpha = 1f;

        // Cancel any existing fade
        StopAllCoroutines();
        
        // Start fade out after 2 seconds
        StartCoroutine(FadeOut());
    }

    private System.Collections.IEnumerator FadeOut()
    {
        // Wait 2 seconds before fading
        yield return new WaitForSeconds(2f);
        
        // Fade out over 1 second
        float elapsed = 0f;
        float duration = 1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            }
            yield return null;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (checkpointTextObj != null)
        {
            checkpointTextObj.SetActive(false);
        }
    }
}