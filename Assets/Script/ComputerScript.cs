using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ComputerScript : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Distance at which the prompt appears")]
    public float detectionRange = 3f;

    [Header("Computer Message Content - Multi-Stage")]
    [TextArea(2, 4)]
    public string messagePage1 = "<b>ACCESS TERMINAL\nSystem Status: [CRITICAL]</b>";
    
    [TextArea(2, 4)]
    public string messagePage2 = "<b>WARNING: Containment breach detected in Sector 7...</b>";
    
    [TextArea(2, 4)]
    public string messagePage3 = "<b>All personnel evacuate immediately. This is not a drill.</b>";
    
    [TextArea(2, 4)]
    public string messagePage4 = "<b>Emergency lockdown initiated.\n\n[SYSTEM OFFLINE]</b>";

    // Internals
    private Transform player;
    private bool playerNearby;
    private bool isShowing;

    // UI - Same as Notebook.cs
    private Canvas promptCanvas;
    private TextMeshProUGUI promptText;
    private GameObject promptContainer;

    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Create UI using the same method as Notebook
        CreatePromptUI();
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerNearby = distanceToPlayer <= detectionRange;

        // Show/hide prompt when near
        if (!isShowing)
        {
            if (promptContainer != null)
            {
                promptContainer.SetActive(playerNearby);
            }

            // Press E to access computer
            if (playerNearby && Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(ShowComputerText());
            }
        }
    }

    private IEnumerator ShowComputerText()
    {
        isShowing = true;

        // Hide the prompt first
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }

        yield return new WaitForSeconds(0.3f);

        // Page 1
        if (promptText != null)
        {
            promptText.text = messagePage1;
            promptText.fontSize = 22;
            promptText.alignment = TextAlignmentOptions.Center;
        }

        if (promptContainer != null)
        {
            promptContainer.SetActive(true);
        }

        yield return new WaitForSeconds(3.5f);

        // Page 2
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        if (promptText != null)
        {
            promptText.text = messagePage2;
            promptText.fontSize = 22;
        }

        if (promptContainer != null)
        {
            promptContainer.SetActive(true);
        }

        yield return new WaitForSeconds(3.5f);

        // Page 3
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        if (promptText != null)
        {
            promptText.text = messagePage3;
            promptText.fontSize = 22;
        }

        if (promptContainer != null)
        {
            promptContainer.SetActive(true);
        }

        yield return new WaitForSeconds(4f);

        // Page 4 - Final message
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        if (promptText != null)
        {
            promptText.text = messagePage4;
            promptText.fontSize = 22;
        }

        if (promptContainer != null)
        {
            promptContainer.SetActive(true);
        }

        yield return new WaitForSeconds(4.5f);

        // Hide the text
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }

        yield return new WaitForSeconds(0.3f);

        // Restore the prompt
        if (promptText != null)
        {
            promptText.text = "Press <color=#FFD700>[E]</color> to Access Computer";
            promptText.fontSize = 32;
            promptText.alignment = TextAlignmentOptions.Center;
        }

        isShowing = false;
    }

    private void CreatePromptUI()
    {
        // EXACT SAME as Notebook.cs
        GameObject canvasObj = new GameObject("ComputerPromptCanvas");
        promptCanvas = canvasObj.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        promptCanvas.sortingOrder = 200; // Higher than notebook (150)

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject containerObj = new GameObject("PromptContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);

        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0f);
        containerRect.anchorMax = new Vector2(0.5f, 0f);
        containerRect.pivot = new Vector2(0.5f, 0f);
        containerRect.anchoredPosition = new Vector2(0, 180f);
        containerRect.sizeDelta = new Vector2(900f, 180f);

        GameObject bgObj = new GameObject("PromptBackground");
        bgObj.transform.SetParent(containerObj.transform, false);

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        var bgOutline = bgObj.AddComponent<Outline>();
        bgOutline.effectColor = new Color(0.3f, 1f, 0.3f, 1f);
        bgOutline.effectDistance = new Vector2(3, 3);

        var bgShadow = bgObj.AddComponent<Shadow>();
        bgShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        bgShadow.effectDistance = new Vector2(4, -4);

        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(bgObj.transform, false);

        promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "Press <color=#FFD700>[E]</color> to Access Computer";
        promptText.fontSize = 32;
        promptText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.textWrappingMode = TextWrappingModes.Normal;
        promptText.overflowMode = TextOverflowModes.Truncate;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-40f, -30f);

        var textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(2, 2);

        promptContainer = containerObj;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
