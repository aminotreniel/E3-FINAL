using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SceneExitTrigger : MonoBehaviour
{
    private Canvas canvas;
    private TextMeshProUGUI messageText;
    private Image blackOverlay;

    [Header("Message Settings")]
    [TextArea] public string[] messages;   // You can set these in the Inspector
    public float displayTime = 3f;        // Time each message stays
    public float fadeDuration = 1f;       // Fade in/out speed
    public float delayBetween = 0.5f;     // Delay between messages

    [Header("Exit Settings")]
    public float exitGlitchDuration = 2f; // ⏳ Total time before quitting

    private bool hasTriggered = false;

    void Start()
    {
        // Create Canvas dynamically
        GameObject canvasObj = new GameObject("ExitCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Create message text
        GameObject textObj = new GameObject("ExitMessage");
        textObj.transform.SetParent(canvas.transform);
        messageText = textObj.AddComponent<TextMeshProUGUI>();
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontSize = 40;
        messageText.color = Color.red;
        messageText.rectTransform.anchorMin = new Vector2(0, 0);
        messageText.rectTransform.anchorMax = new Vector2(1, 1);
        messageText.rectTransform.offsetMin = Vector2.zero;
        messageText.rectTransform.offsetMax = Vector2.zero;
        messageText.text = "";
        messageText.gameObject.SetActive(false);

        // Create black overlay for fade
        GameObject overlayObj = new GameObject("BlackOverlay");
        overlayObj.transform.SetParent(canvas.transform);
        blackOverlay = overlayObj.AddComponent<Image>();
        blackOverlay.color = new Color(0, 0, 0, 0);
        blackOverlay.rectTransform.anchorMin = new Vector2(0, 0);
        blackOverlay.rectTransform.anchorMax = new Vector2(1, 1);
        blackOverlay.rectTransform.offsetMin = Vector2.zero;
        blackOverlay.rectTransform.offsetMax = Vector2.zero;
        blackOverlay.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(PlayExitSequence());
        }
    }

    private IEnumerator PlayExitSequence()
    {
        foreach (string msg in messages)
        {
            // Show text
            messageText.text = msg;
            Color color = messageText.color;
            color.a = 0;
            messageText.color = color;
            messageText.gameObject.SetActive(true);

            // Fade in
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                color.a = Mathf.Lerp(0, 1, t / fadeDuration);
                messageText.color = color;
                yield return null;
            }

            // Stay visible
            yield return new WaitForSeconds(displayTime);

            // Fade out
            t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                color.a = Mathf.Lerp(1, 0, t / fadeDuration);
                messageText.color = color;
                yield return null;
            }

            messageText.gameObject.SetActive(false);
            yield return new WaitForSeconds(delayBetween);
        }

        // After last message, start glitchy fade-out
        yield return StartCoroutine(GlitchFadeOut(exitGlitchDuration));

        // Exit game after fade-out
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    private IEnumerator GlitchFadeOut(float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;

            // Random flicker alpha
            float alpha = Random.Range(0.3f, 1f);
            blackOverlay.color = new Color(0, 0, 0, alpha);

            // Random flicker speed
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        // Final solid black
        blackOverlay.color = new Color(0, 0, 0, 1);
    }
}
