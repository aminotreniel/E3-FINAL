using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class Elevator : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load (must be added to Build Settings)")]
    public string nextSceneName = "NextScene";
    
    [Header("Spawn Settings")]
    [Tooltip("Where the player should spawn in the next scene (optional - leave null to use default spawn)")]
    public Vector3 spawnPositionInNextScene = Vector3.zero;
    
    [Tooltip("Use custom spawn position?")]
    public bool useCustomSpawnPosition = false;
    
    [Header("Transition Settings")]
    [Tooltip("Duration of the fade to black transition")]
    public float fadeDuration = 1.5f;
    
    [Tooltip("How long to stay black before loading the scene")]
    public float blackoutDuration = 0.5f;
    
    private bool playerInside = false;
    private bool isTransitioning = false;
    private Canvas uiCanvas;
    private Image fadeImage;
    private TextMeshProUGUI promptText;
    
    void Start()
    {
        CreateUIElements();
        
        // Hide prompt text at start
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        // Check if player is inside and pressed E
        if (playerInside && !isTransitioning && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(TransitionToNextScene());
        }
    }
    
    private void CreateUIElements()
    {
        // Find existing canvas or create a temporary one (will be destroyed with scene)
        GameObject canvasObj = new GameObject("ElevatorCanvas");
        uiCanvas = canvasObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 200; // High sorting order to be on top
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // DO NOT persist this canvas - it should be destroyed with the scene
        // DontDestroyOnLoad(canvasObj); // REMOVED
        
        // Create prompt text (always create it automatically)
        GameObject textObj = new GameObject("ElevatorPromptText");
        textObj.transform.SetParent(uiCanvas.transform, false);
        
        promptText = textObj.AddComponent<TextMeshProUGUI>();
        promptText.text = "Press E to go down";
        promptText.fontSize = 36;
        promptText.color = Color.white;
        promptText.alignment = TextAlignmentOptions.Center;
        
        // Position at center of screen
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(600, 100);
        
        // Add outline for better visibility
        var outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, 3);
        
        // Hide it initially
        promptText.gameObject.SetActive(false);
        
        // Create fade image for blackout transition
        GameObject fadeObj = new GameObject("FadeImage");
        fadeObj.transform.SetParent(uiCanvas.transform, false);
        
        fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        fadeImage.raycastTarget = false;
        
        // Make it cover the entire screen
        RectTransform fadeRect = fadeObj.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.sizeDelta = Vector2.zero;
        fadeRect.SetAsLastSibling(); // Make sure it's on top when visible
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the elevator
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            if (promptText != null)
            {
                promptText.gameObject.SetActive(true);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if the player left the elevator
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
    
    private IEnumerator TransitionToNextScene()
    {
        isTransitioning = true;
        
        // Store original prompt text color
        Color originalTextColor = promptText != null ? promptText.color : Color.white;
        
        // Fade to black (both screen and prompt text)
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            
            // Fade the screen to black
            fadeImage.color = new Color(0, 0, 0, alpha);
            
            // Fade out the prompt text
            if (promptText != null)
            {
                promptText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 1 - alpha);
            }
            
            yield return null;
        }
        
        // Ensure fully black and text is invisible
        fadeImage.color = Color.black;
        if (promptText != null)
        {
            promptText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0);
        }
        
        // Wait while black
        yield return new WaitForSeconds(blackoutDuration);
        
        // Store spawn position if using custom position
        if (useCustomSpawnPosition)
        {
            PlayerPrefs.SetFloat("SpawnX", spawnPositionInNextScene.x);
            PlayerPrefs.SetFloat("SpawnY", spawnPositionInNextScene.y);
            PlayerPrefs.SetFloat("SpawnZ", spawnPositionInNextScene.z);
            PlayerPrefs.SetInt("UseCustomSpawn", 1);
        }
        else
        {
            PlayerPrefs.SetInt("UseCustomSpawn", 0);
        }
        
        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
