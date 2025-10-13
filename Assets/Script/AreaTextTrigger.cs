using UnityEngine;
using TMPro;
using System.Collections;

public class AreaTextTrigger : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI messageText;   // Assign in Inspector

    [Header("Message Settings")]
    [TextArea] public string[] messages;  // Multiple messages in Inspector
    public float displayTime = 3f;        // How long each message stays
    public float fadeDuration = 1f;       // Time for fade in/out
    public float delayBetween = 0.5f;     // Delay between messages

    private bool hasTriggered = false;    // Prevent multiple triggers

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
        foreach (string msg in messages)
        {
            // Set message text and reset alpha
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

            // Small delay before next message
            yield return new WaitForSeconds(delayBetween);
        }
    }
}
