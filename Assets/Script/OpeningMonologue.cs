using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpeningMonologue : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI captionText;
    private Camera mainCam;

    [Header("Settings")]
    public string[] captions = { "My head… what happened? Where am I…?" };
    public float effectDuration = 5f;        // how long dizziness lasts
    public float swayAmount = 2f;            // rotation sway in degrees
    public float swaySpeed = 2f;             // speed of sway
    public float captionDelay = 1f;          // wait before showing text
    public float captionVisibleTime = 3f;    // how long text stays on screen

    [Header("Blur Settings")]
    public int blurDownscale = 6; // larger = more blur (lower res)
    public float blurMaxAlpha = 0.9f; // how visible the blurred layer becomes
    public float blurFadeInTime = 0.6f; // fade into blur
    public float blurFadeOutTime = 0.8f; // fade out at end

    private Quaternion originalRotation;
    private Vector3 originalPosition;

    private void Start()
    {
        Debug.Log("OpeningMonologue: Start()");
        if (captionText != null)
        {
            captionText.text = "";
            Debug.Log("OpeningMonologue: captionText assigned: " + captionText.name);
        }
        else
        {
            Debug.Log("OpeningMonologue: captionText not assigned in inspector");
        }
        mainCam = Camera.main;
        if (mainCam != null)
            originalRotation = mainCam.transform.localRotation;

        if (mainCam != null)
            originalPosition = mainCam.transform.localPosition;

        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayDizzyBlurEffect()
    {
        if (mainCam == null)
        {
            Debug.LogWarning("OpeningMonologue: mainCam is null, trying Camera.main");
            mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("OpeningMonologue: No camera found for dizzy effect");
                yield break;
            }
        }

        Debug.Log("OpeningMonologue: creating dizzy RT (downscale=" + blurDownscale + ")");
        // create downsampled RT
        int rtW = Mathf.Max(1, Screen.width / blurDownscale);
        int rtH = Mathf.Max(1, Screen.height / blurDownscale);
        RenderTexture rt = new RenderTexture(rtW, rtH, 0, RenderTextureFormat.Default);
        rt.filterMode = FilterMode.Bilinear;

        // create helper camera
        GameObject camGO = new GameObject("_DizzyCam");
        camGO.transform.position = mainCam.transform.position;
        camGO.transform.rotation = mainCam.transform.rotation;
        Camera dizzyCam = camGO.AddComponent<Camera>();
        dizzyCam.CopyFrom(mainCam);
        dizzyCam.targetTexture = rt;
        Debug.Log("OpeningMonologue: dizzy camera created, RT size=" + rtW + "x" + rtH);

        // create fullscreen RawImage to show RT
        GameObject canvasGO = new GameObject("_DizzyCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        // place dizzy canvas below the monologue canvas but high enough to render above other UI
        canvas.sortingOrder = 2000;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject rawGO = new GameObject("_DizzyRaw");
        rawGO.transform.SetParent(canvasGO.transform, false);
        RawImage raw = rawGO.AddComponent<RawImage>();
        RectTransform rtr = raw.GetComponent<RectTransform>();
        rtr.anchorMin = Vector2.zero; rtr.anchorMax = Vector2.one; rtr.offsetMin = Vector2.zero; rtr.offsetMax = Vector2.zero;
        raw.texture = rt;
        raw.color = new Color(1f, 1f, 1f, 0f);
        raw.raycastTarget = false;
        Debug.Log("OpeningMonologue: RawImage created and assigned RT");

        // fade in blur
        float t = 0f;
        while (t < blurFadeInTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(0f, blurMaxAlpha, t / blurFadeInTime);
            raw.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }

        // sustain dizziness with camera sway while decreasing intensity
        float elapsed = 0f;
        while (elapsed < effectDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float intensity = 1f - Mathf.Clamp01(elapsed / effectDuration);

            // sway camera rotation
            float swayX = Mathf.Sin(Time.unscaledTime * swaySpeed) * swayAmount * intensity;
            float swayY = Mathf.Cos(Time.unscaledTime * swaySpeed) * swayAmount * intensity;
            mainCam.transform.localRotation = originalRotation * Quaternion.Euler(swayX, swayY, 0);
            mainCam.transform.localPosition = originalPosition + mainCam.transform.right * (0.02f * intensity);

            yield return null;
        }

        // fade out blur
        t = 0f;
        Color startCol = raw.color;
        while (t < blurFadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(startCol.a, 0f, t / blurFadeOutTime);
            raw.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }

        // cleanup
        mainCam.transform.localRotation = originalRotation;
        mainCam.transform.localPosition = originalPosition;
        dizzyCam.targetTexture = null;
        Destroy(rt);
        Destroy(camGO);
        Destroy(canvasGO);
    }

    private IEnumerator FadeExistingBlackScreenOut(float duration)
    {
        // Look for CanvasGroup named BlackScreen
        var go = GameObject.Find("BlackScreen");
        CanvasGroup cg = null;
        UnityEngine.UI.Image img = null;
        if (go != null)
        {
            cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
                img = go.GetComponent<UnityEngine.UI.Image>();
        }

        // also check for BlackScreenImage
        if (cg == null && img == null)
        {
            var go2 = GameObject.Find("BlackScreenImage");
            if (go2 != null)
            {
                img = go2.GetComponent<UnityEngine.UI.Image>();
            }
        }

        if (cg == null && img == null)
            yield break;

        float t = 0f;
        if (cg != null)
        {
            float start = cg.alpha;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(start, 0f, t / duration);
                yield return null;
            }
            cg.alpha = 0f;
        }
        else if (img != null)
        {
            Color c = img.color;
            float start = c.a;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(start, 0f, t / duration);
                img.color = c;
                yield return null;
            }
            c.a = 0f;
            img.color = c;
        }
    }

    private IEnumerator PlayIntro()
    {
        Debug.Log("OpeningMonologue: PlayIntro started");
        // If there's an existing BlackScreen UI, fade it out so captions are visible
        yield return StartCoroutine(FadeExistingBlackScreenOut(0.25f));

        // Ensure captionText exists; if not, create a fallback TextMeshProUGUI on a canvas
        // Ensure we have a guaranteed top-most canvas for captions
        GameObject monCanvasGO = GameObject.Find("_OpeningMonologue_Canvas");
        Canvas monCanvas;
        if (monCanvasGO == null)
        {
            monCanvasGO = new GameObject("_OpeningMonologue_Canvas");
            monCanvas = monCanvasGO.AddComponent<Canvas>();
            monCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            monCanvas.overrideSorting = true;
            monCanvas.sortingOrder = 32767;
            monCanvasGO.AddComponent<CanvasScaler>();
            monCanvasGO.AddComponent<GraphicRaycaster>();
            Debug.Log("OpeningMonologue: created top-most canvas '_OpeningMonologue_Canvas'.");
        }
        else
        {
            monCanvas = monCanvasGO.GetComponent<Canvas>();
            if (monCanvas == null) monCanvas = monCanvasGO.AddComponent<Canvas>();
            monCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            monCanvas.overrideSorting = true;
            monCanvas.sortingOrder = 32767;
        }

        // Ensure captionText exists; if not, try to find one in children
        if (captionText == null)
            captionText = GetComponentInChildren<TextMeshProUGUI>();

        if (captionText == null)
        {
            Debug.LogWarning("OpeningMonologue: captionText not assigned — creating fallback UI text on top canvas.");

            GameObject textGO = new GameObject("OpeningCaptionTMP");
            textGO.transform.SetParent(monCanvasGO.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 36;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            RectTransform tr = tmp.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.5f, 0.5f);
            tr.anchorMax = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = new Vector2(0f, 0f);
            tr.sizeDelta = new Vector2(900f, 200f);

            captionText = tmp;
            Debug.Log("OpeningMonologue: created fallback caption TextMeshProUGUI.");
        }
        else
        {
            // If a captionText exists in the scene, reparent it to our top-most canvas so it's guaranteed visible
            if (captionText.transform.root.gameObject != monCanvasGO)
            {
                captionText.transform.SetParent(monCanvasGO.transform, false);
                Debug.Log("OpeningMonologue: reparented existing captionText to top-most canvas.");
            }
        }

        // Play dizziness + blur first (waking up), then show captions
        Debug.Log("OpeningMonologue: starting dizzy blur effect (before captions)");
        yield return StartCoroutine(PlayDizzyBlurEffect());

        // Wait before showing captions (use realtime so it runs when timeScale == 0)
        yield return new WaitForSecondsRealtime(captionDelay);

        // Show captions after the waking/dizzy effect
        foreach (string line in captions)
        {
            if (captionText != null)
                captionText.text = line;
            else
                Debug.Log("OpeningMonologue: captionText is null, skipping text set.");

            yield return new WaitForSecondsRealtime(captionVisibleTime);

            if (captionText != null)
                captionText.text = "";

            yield return null;
        }
    }
}
