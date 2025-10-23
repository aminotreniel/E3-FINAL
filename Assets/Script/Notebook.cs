using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Notebook : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Distance at which the prompt appears")]
    public float detectionRange = 3f;

    [Header("Note Content - Multi-Stage")]
    [TextArea(2, 4)]
    public string notePage1 = "<b>Research Log - Dr. Maria Santos\nDate: [REDACTED]\n\n\"Project Sampaguita was meant to be our salvation...\"</b>";
    
    [TextArea(2, 4)]
    public string notePage2 = "<b>\"We extracted compounds from the national flower, believing its purity could cure any disease...\"</b>";
    
    [TextArea(2, 4)]
    public string notePage3 = "<b>\"But something went wrong. The serum mutated. It didn't heal... it transformed people into those... creatures.\"</b>";
    
    [TextArea(2, 4)]
    public string notePage4 = "<b>\"God forgive us. We created the very monsters we swore to protect humanity from.\"\n\n[The rest is torn and illegible]</b>";

    [Header("Highlight Motion")]
    public bool idleAnimate = true;
    public float bobAmplitude = 0.05f;
    public float bobSpeed = 1.2f;
    public float rotateSpeed = 15f;

    // Internals
    private Transform player;
    private Vector3 basePos;
    private bool playerNearby;
    private bool isShowing;
    private int currentPage = 0; // Track which page we're on

    // UI - Same as DoctorScript
    private Canvas promptCanvas;
    private TextMeshProUGUI promptText;
    private GameObject promptContainer;

    void Start()
    {
        basePos = transform.position;
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Create UI using the same method as DoctorScript
        CreatePromptUI();
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }
    }

    private void Update()
    {
        // Idle highlight motion
        if (idleAnimate)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = basePos + new Vector3(0f, bob, 0f);
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
        }

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

            // Press Y to read
            if (playerNearby && Input.GetKeyDown(KeyCode.Y))
            {
                StartCoroutine(ShowNoteText());
            }
        }
    }

    private IEnumerator ShowNoteText()
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
            promptText.text = notePage1;
            promptText.fontSize = 22; // Slightly smaller for better fit
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
            promptText.text = notePage2;
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
            promptText.text = notePage3;
            promptText.fontSize = 22;
        }

        if (promptContainer != null)
        {
            promptContainer.SetActive(true);
        }

        yield return new WaitForSeconds(4f);

        // Page 4 - Final revelation
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        if (promptText != null)
        {
            promptText.text = notePage4;
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
            promptText.text = "Press <color=#FFD700>[Y]</color> to Read";
            promptText.fontSize = 32;
            promptText.alignment = TextAlignmentOptions.Center;
        }

        isShowing = false;
    }

    private void CreatePromptUI()
    {
        // EXACT SAME as DoctorScript
        GameObject canvasObj = new GameObject("NotebookPromptCanvas");
        promptCanvas = canvasObj.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        promptCanvas.sortingOrder = 150;

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
        containerRect.sizeDelta = new Vector2(900f, 180f); // Increased width and height for multi-line text

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
        promptText.text = "Press <color=#FFD700>[Y]</color> to Read";
        promptText.fontSize = 32;
        promptText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.textWrappingMode = TextWrappingModes.Normal; // Enable text wrapping
        promptText.overflowMode = TextOverflowModes.Truncate; // Prevent overflow

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-40f, -30f); // Increased padding

        var textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(2, 2);

        promptContainer = containerObj;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
