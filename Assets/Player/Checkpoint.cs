using UnityEngine;
using UnityEngine.UI;

public class Checkpoint : MonoBehaviour
{
    private static GameObject checkpointTextObj;   // keeps only one text on screen
    private static UnityEngine.UI.Text checkpointText;

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

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // Create Text
            checkpointTextObj = new GameObject("CheckpointText");
            checkpointTextObj.transform.SetParent(canvasObj.transform);

            checkpointText = checkpointTextObj.AddComponent<UnityEngine.UI.Text>();
            checkpointText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            checkpointText.fontSize = 36;
            checkpointText.color = Color.white;  // white text
            checkpointText.alignment = TextAnchor.LowerLeft;

            // Proper RectTransform size & position
            RectTransform rt = checkpointText.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 100);
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(20, 20);
        }

        // Update message text
        checkpointText.text = message;

        // Reset alpha & show
        checkpointTextObj.SetActive(true);
        checkpointText.canvasRenderer.SetAlpha(1f);

        // Fade out after 2 seconds
        checkpointText.CrossFadeAlpha(0f, 2f, false);
    }
}