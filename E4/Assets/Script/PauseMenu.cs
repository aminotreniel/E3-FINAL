using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using InfimaGames.LowPolyShooterPack;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    private GameObject pauseMenuUI;
    private Movement movementScript;

    private TextMeshProUGUI titleText;
    private TextMeshProUGUI controlsText;
    // removed status and warning texts per request
    private Image[] scanLines;
    private Image[] glitchOverlays;
    private RectTransform[] floatingParticles;

    [Header("Pause Key")]
    [SerializeField] private KeyCode pauseKey = KeyCode.P;

    [Header("Menu Control Keys")]
    [SerializeField] private KeyCode resumeKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode restartKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode exitKey = KeyCode.Alpha3;

    // Enhanced visual effects - TONED DOWN
    private float flickerTimer;
    private float flickerInterval = 0.3f; // Slower flicker
    private bool flickerVisible = true;
    private float scanLineTimer;
    private float glitchTimer;
    private float staticTimer;
    private AudioSource audioSource;
    
    private bool menuInitialized = false;

    void Awake()
    {
        // Force reset pause state before anything else happens
        GameIsPaused = false;
        Time.timeScale = 1f;
        Debug.Log("PauseMenu Awake - Reset pause state");
    }

    void Start()
    {
        Debug.Log("Enhanced PauseMenu started!");
        
        // Double-check pause state
        GameIsPaused = false;
        Time.timeScale = 1f;
        
        try
        {
            // Create UI but keep it hidden
            CreateEnhancedPauseMenuUI();
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(false);
                menuInitialized = true;
                Debug.Log("Pause menu UI created successfully and hidden");
            }
            else
            {
                Debug.LogError("Failed to create pause menu UI!");
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creating pause menu UI: " + e.Message);
            return;
        }
        
        // Ensure cursor is in game mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Find player Movement script
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            movementScript = player.GetComponent<Movement>();

        // Add audio source for UI sounds
        audioSource = gameObject.AddComponent<AudioSource>();
        
        Debug.Log("Pause menu initialized and HIDDEN - Press P to open");
    }

    void Update()
    {
        // Only listen for P key to toggle pause menu
        if (Input.GetKeyDown(pauseKey))
        {
            Debug.Log("P key pressed! Current pause state: " + GameIsPaused);
            if (GameIsPaused)
            {
                Debug.Log("Resuming game...");
                Resume();
            }
            else
            {
                Debug.Log("Pausing game...");
                Pause();
            }
        }

        // Only process menu controls when the game is actually paused
        if (GameIsPaused && pauseMenuUI.activeInHierarchy)
        {
            if (Input.GetKeyDown(resumeKey))
            {
                Debug.Log("Resume key (1) pressed");
                Resume();
            }
            else if (Input.GetKeyDown(restartKey))
            {
                Debug.Log("Restart key (2) pressed");
                Restart();
            }
            else if (Input.GetKeyDown(exitKey))
            {
                Debug.Log("Exit key (3) pressed");
                ExitGame();
            }

            // Run enhanced visual effects only when menu is visible
            RunEnhancedEffects();
        }
    }

    private void CreateEnhancedPauseMenuUI()
    {
        // === Main Canvas ===
        GameObject canvasGO = new GameObject("LaboratoryPauseCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // === Background - Multi-layered ===
        CreateBackgroundLayers(canvasGO);

        // === Main Content Panel ===
        GameObject mainPanel = CreateMainContentPanel(canvasGO);

        // === Terminal Header ===
        CreateTerminalHeader(mainPanel);


        // === Main Title with Advanced Effects ===
        CreateEnhancedTitle(mainPanel);


        // === Enhanced Controls Section ===
        CreateEnhancedControls(mainPanel);

        // === Visual Effects Overlays ===
        CreateVisualEffects(canvasGO);

        pauseMenuUI = canvasGO;
    }

    private void CreateBackgroundLayers(GameObject parent)
    {
        // Base dark overlay - warmer tone
        GameObject bgPanel = new GameObject("BackgroundOverlay");
        bgPanel.transform.SetParent(parent.transform, false);
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.1f, 0.15f, 0.95f); // Dark blue-gray
        SetFullScreen(bgPanel.GetComponent<RectTransform>());

        // Grid pattern overlay - subtle orange
        GameObject gridPanel = new GameObject("GridOverlay");
        gridPanel.transform.SetParent(parent.transform, false);
        Image gridImage = gridPanel.AddComponent<Image>();
        gridImage.color = new Color(0.3f, 0.2f, 0.1f, 0.08f); // Very subtle orange
        SetFullScreen(gridPanel.GetComponent<RectTransform>());

        // Vignette effect
        GameObject vignettePanel = new GameObject("VignetteOverlay");
        vignettePanel.transform.SetParent(parent.transform, false);
        Image vignetteImage = vignettePanel.AddComponent<Image>();
        vignetteImage.color = new Color(0f, 0f, 0f, 0.5f);
        SetFullScreen(vignettePanel.GetComponent<RectTransform>());
    }

    private GameObject CreateMainContentPanel(GameObject parent)
    {
        GameObject panelGO = new GameObject("MainTerminalPanel");
        panelGO.transform.SetParent(parent.transform, false);
        
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.12f, 0.15f, 0.18f, 0.92f); // Warmer dark gray
        
        // Add border effect - warm orange
        Outline outline = panelGO.AddComponent<Outline>();
        outline.effectColor = new Color(0.9f, 0.6f, 0.3f, 0.8f); // Warm orange
        outline.effectDistance = new Vector2(2, 2);

        RectTransform panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(800, 600);
        panelRT.anchoredPosition = Vector2.zero;

        return panelGO;
    }

    private void CreateTerminalHeader(GameObject parent)
    {
        GameObject headerGO = new GameObject("TerminalHeader");
        headerGO.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI headerText = headerGO.AddComponent<TextMeshProUGUI>();
        headerText.text = "◢ LABORATORY SECURITY TERMINAL ◣";
        headerText.fontSize = 24;
        headerText.color = new Color(0.9f, 0.7f, 0.4f, 0.9f); // Warm amber
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.fontStyle = FontStyles.Bold;

        RectTransform headerRT = headerGO.GetComponent<RectTransform>();
        headerRT.sizeDelta = new Vector2(750, 40);
        headerRT.anchoredPosition = new Vector2(0, 250);
    }


    private void CreateEnhancedTitle(GameObject parent)
    {
        GameObject titleGO = new GameObject("EnhancedTitle");
        titleGO.transform.SetParent(parent.transform, false);
        
        titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "SYSTEM PAUSED";
        titleText.fontSize = 64;
        titleText.color = new Color(0.9f, 0.7f, 0.4f); // Warm amber
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.UpperCase | FontStyles.Bold;

        // Subtle shadow for depth
        Shadow shadow1 = titleGO.AddComponent<Shadow>();
        shadow1.effectColor = new Color(0.3f, 0.2f, 0.1f, 0.5f);
        shadow1.effectDistance = new Vector2(3, -3);

        Outline titleOutline = titleGO.AddComponent<Outline>();
        titleOutline.effectColor = new Color(0f, 0f, 0f, 0.6f);
        titleOutline.effectDistance = new Vector2(2, 2);

        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(700, 100);
        titleRT.anchoredPosition = new Vector2(0, 80);
    }

    // Warning/banner removed per request

    private void CreateEnhancedControls(GameObject parent)
    {
        GameObject controlsGO = new GameObject("EnhancedControls");
        controlsGO.transform.SetParent(parent.transform, false);
        
        controlsText = controlsGO.AddComponent<TextMeshProUGUI>();
        controlsText.text =
            "EMERGENCY PROTOCOLS\n\n" +
            "[1] RESUME MISSION\n" +
            "[2] RESTART SYSTEM\n" +
            "[3] EVACUATE LAB";
        
        controlsText.fontSize = 28;
        controlsText.color = new Color(0.7f, 0.8f, 0.9f); // Soft blue-white
        controlsText.alignment = TextAlignmentOptions.Center;
        controlsText.fontStyle = FontStyles.Bold;

        Shadow controlsShadow = controlsGO.AddComponent<Shadow>();
        controlsShadow.effectColor = new Color(0.2f, 0.3f, 0.4f, 0.6f);
        controlsShadow.effectDistance = new Vector2(2, -2);

        RectTransform controlsRT = controlsGO.GetComponent<RectTransform>();
        controlsRT.sizeDelta = new Vector2(600, 200);
        controlsRT.anchoredPosition = new Vector2(0, -120);
    }

    private void CreateVisualEffects(GameObject parent)
    {
        // Create fewer, more subtle scan lines
        scanLines = new Image[2]; // Reduced from 3 to 2
        for (int i = 0; i < scanLines.Length; i++)
        {
            GameObject scanLineGO = new GameObject($"ScanLine_{i}");
            scanLineGO.transform.SetParent(parent.transform, false);
            scanLines[i] = scanLineGO.AddComponent<Image>();
            scanLines[i].color = new Color(0.9f, 0.6f, 0.3f, 0.15f); // Subtle warm glow
            
            RectTransform scanRT = scanLineGO.GetComponent<RectTransform>();
            scanRT.sizeDelta = new Vector2(1920, 1);
            scanRT.anchoredPosition = new Vector2(0, Random.Range(-540, 540));
            SetFullScreen(scanRT);
        }

        // Reduce glitch overlays and make them less intense
        glitchOverlays = new Image[1]; // Reduced from 2 to 1
        GameObject glitchGO = new GameObject("GlitchOverlay");
        glitchGO.transform.SetParent(parent.transform, false);
        glitchOverlays[0] = glitchGO.AddComponent<Image>();
        glitchOverlays[0].color = new Color(0.8f, 0.5f, 0.2f, 0f); // Warm tone when active
        SetFullScreen(glitchGO.GetComponent<RectTransform>());

        // Reduce floating particles for less distraction
        CreateFloatingParticles(parent);
    }

    private void CreateFloatingParticles(GameObject parent)
    {
        floatingParticles = new RectTransform[4]; // Reduced from 8 to 4
        for (int i = 0; i < floatingParticles.Length; i++)
        {
            GameObject particleGO = new GameObject($"FloatingParticle_{i}");
            particleGO.transform.SetParent(parent.transform, false);
            
            Image particleImg = particleGO.AddComponent<Image>();
            particleImg.color = new Color(0.8f, 0.6f, 0.4f, Random.Range(0.2f, 0.4f)); // Warm, subtle
            
            floatingParticles[i] = particleGO.GetComponent<RectTransform>();
            floatingParticles[i].sizeDelta = new Vector2(Random.Range(1, 3), Random.Range(1, 3)); // Smaller
            floatingParticles[i].anchoredPosition = new Vector2(
                Random.Range(-960, 960),
                Random.Range(-540, 540)
            );
        }
    }

    private void RunEnhancedEffects()
    {
        // Much gentler title flicker
        flickerTimer += Time.unscaledDeltaTime;
        if (flickerTimer >= flickerInterval)
        {
            flickerTimer = 0f;
            flickerVisible = !flickerVisible;

            if (titleText != null)
            {
                // Subtle fade instead of harsh flicker
                float alpha = flickerVisible ? 1f : 0.85f;
                titleText.color = new Color(0.9f, 0.7f, 0.4f, alpha);

                // Minimal scale change
                float scaleJitter = Random.Range(0.995f, 1.005f);
                titleText.rectTransform.localScale = new Vector3(scaleJitter, scaleJitter, 1f);
            }
        }

        // Slower, more subtle scan lines
        scanLineTimer += Time.unscaledDeltaTime;
        if (scanLineTimer >= 0.5f) // Much slower
        {
            scanLineTimer = 0f;
            foreach (var scanLine in scanLines)
            {
                if (scanLine != null)
                {
                    Vector2 pos = scanLine.rectTransform.anchoredPosition;
                    pos.y += Random.Range(-20, 20); // Less movement
                    if (pos.y > 600) pos.y = -600;
                    if (pos.y < -600) pos.y = 600;
                    scanLine.rectTransform.anchoredPosition = pos;
                    scanLine.color = new Color(0.9f, 0.6f, 0.3f, Random.Range(0.08f, 0.2f)); // Very subtle
                }
            }
        }

        // Much less frequent glitch effects
        glitchTimer += Time.unscaledDeltaTime;
        if (glitchTimer >= Random.Range(3f, 6f)) // Much longer intervals
        {
            glitchTimer = 0f;
            StartCoroutine(GentleGlitchEffect());
        }

        // Gentle floating particles
        foreach (var particle in floatingParticles)
        {
            if (particle != null)
            {
                Vector2 pos = particle.anchoredPosition;
                pos.y += Random.Range(-0.3f, 0.3f); // Much slower movement
                pos.x += Random.Range(-0.1f, 0.1f);
                particle.anchoredPosition = pos;
            }
        }

        // No system status or warning banner (removed)
    }

    private IEnumerator GentleGlitchEffect()
    {
        if (glitchOverlays != null && glitchOverlays.Length > 0)
        {
            var glitch = glitchOverlays[0];
            if (glitch != null)
            {
                // Very subtle glitch effect
                glitch.color = new Color(0.8f, 0.5f, 0.2f, Random.Range(0.05f, 0.15f));
                yield return new WaitForSecondsRealtime(Random.Range(0.1f, 0.2f));
                glitch.color = new Color(0.8f, 0.5f, 0.2f, 0f);
            }
        }
    }

    private void SetFullScreen(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // === MENU METHODS (Enhanced) ===
    public void Resume()
    {
        StartCoroutine(FadeOutMenu(() => {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsPaused = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (movementScript != null)
                movementScript.enabled = true;
        }));
    }

    public void Pause()
    {
        Debug.Log("Activating pause menu...");
        
        // Show the menu
        pauseMenuUI.SetActive(true);
        
        // Pause the game
        Time.timeScale = 0f;
        GameIsPaused = true;

        // Show cursor for menu interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player movement
        if (movementScript != null)
            movementScript.enabled = false;

        // Start fade in animation
        StartCoroutine(FadeInMenu());
        
        Debug.Log("Game paused successfully");
    }

    public void Restart()
    {
        StartCoroutine(FadeOutMenu(() => {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsPaused = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }));
    }

    public void ExitGame()
    {
        Debug.Log("Evacuating laboratory...");
        StartCoroutine(FadeOutMenu(() => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }));
    }

    private IEnumerator FadeInMenu()
    {
        CanvasGroup cg = pauseMenuUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = pauseMenuUI.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        while (cg.alpha < 1f)
        {
            cg.alpha += Time.unscaledDeltaTime * 4f;
            yield return null;
        }
    }

    private IEnumerator FadeOutMenu(System.Action onComplete)
    {
        CanvasGroup cg = pauseMenuUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = pauseMenuUI.AddComponent<CanvasGroup>();
        
        while (cg.alpha > 0f)
        {
            cg.alpha -= Time.unscaledDeltaTime * 6f;
            yield return null;
        }
        
        onComplete?.Invoke();
    }
}