using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DoctorScript : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float detectionRange = 5f;
    public float interactionDistance = 3f;
    
    [Header("Follow Settings")]
    public float followDistance = 2f;
    public float maxFollowDistance = 15f;
    public float stoppingDistance = 1.5f;
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 8f;
    
    [Header("Healing Settings")]
    public float healAmount = 5f;
    public float healInterval = 3f;
    [Range(0f, 1f)]
    public float healThreshold = 0.8f;
    
    private Transform player;
    private PlayerHealth playerHealth;
    private bool isRecruited = false;
    private bool playerInDetectionRange = false;
    private bool playerNearby = false;
    private bool hasShownInitialDialogue = false;
    private bool isShowingDialogue = false;
    private float nextHealTime = 0f;
    
    private enum DialogueState
    {
        None,
        InitialPlea,
        ShowInteractPrompt,
        ThankYou,
        PlayerResponse,
        Following
    }
    private DialogueState currentDialogueState = DialogueState.None;
    
    private Canvas promptCanvas;
    private TextMeshProUGUI promptText;
    private GameObject promptContainer;
    
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        
        CreatePromptUI();
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (!isRecruited)
        {
            playerInDetectionRange = distanceToPlayer <= detectionRange;
            playerNearby = distanceToPlayer <= interactionDistance;
            
            if (playerInDetectionRange && !hasShownInitialDialogue && !isShowingDialogue)
            {
                StartCoroutine(ShowInitialDialogue());
            }
            else if (hasShownInitialDialogue && playerNearby && !isShowingDialogue)
            {
                if (currentDialogueState == DialogueState.ShowInteractPrompt)
                {
                    if (promptContainer != null)
                    {
                        promptContainer.SetActive(true);
                    }
                    
                    if (Input.GetKeyDown(KeyCode.O))
                    {
                        StartCoroutine(RecruitDialogue());
                    }
                }
            }
            else if (!playerNearby && currentDialogueState == DialogueState.ShowInteractPrompt)
            {
                if (promptContainer != null)
                {
                    promptContainer.SetActive(false);
                }
            }
        }
        else
        {
            FollowPlayer(distanceToPlayer);
            HealPlayerOverTime();
        }
    }
    
    private IEnumerator ShowInitialDialogue()
    {
        isShowingDialogue = true;
        currentDialogueState = DialogueState.InitialPlea;
        
        if (promptText != null)
        {
            promptText.text = "<b>Doctor:</b> Please help me! I'm a doctor, I can help you!";
            promptText.fontSize = 28;
            promptText.alignment = TextAlignmentOptions.Center;
        }
        
        if (promptContainer != null)
        {
            promptContainer.SetActive(true);
        }
        
        yield return new WaitForSeconds(4f);
        
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }
        
        hasShownInitialDialogue = true;
        currentDialogueState = DialogueState.ShowInteractPrompt;
        
        if (promptText != null)
        {
            promptText.text = "Press <color=#FFD700>[O]</color> to Help the Doctor";
            promptText.fontSize = 32;
        }
        
        isShowingDialogue = false;
    }
    
    private IEnumerator RecruitDialogue()
    {
        isShowingDialogue = true;
        
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        currentDialogueState = DialogueState.ThankYou;
        if (promptText != null)
        {
            promptText.text = "<b>Doctor:</b> Thank you for helping me! I'll help you in return.";
            promptText.fontSize = 28;
        }
        
        if (promptContainer != null)
        {
            promptContainer.SetActive(true);
        }
        
        yield return new WaitForSeconds(3f);
        
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        currentDialogueState = DialogueState.PlayerResponse;
        if (promptText != null)
        {
            promptText.text = "<b>You:</b> No worries, just stick with me.";
            promptText.fontSize = 28;
        }
        
        if (promptContainer != null)
        {
            promptContainer.SetActive(true);
        }
        
        yield return new WaitForSeconds(3f);
        
        if (promptContainer != null)
        {
            promptContainer.SetActive(false);
        }
        
        isRecruited = true;
        currentDialogueState = DialogueState.Following;
        isShowingDialogue = false;
    }
    
    private void CreatePromptUI()
    {
        GameObject canvasObj = new GameObject("DoctorPromptCanvas");
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
        containerRect.sizeDelta = new Vector2(700f, 100f);
        
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
        promptText.text = "Press <color=#FFD700>[O]</color> to Help the Doctor";
        promptText.fontSize = 32;
        promptText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        promptText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-30f, -15f);
        
        var textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(2, 2);
        
        promptContainer = containerObj;
    }
    
    private void FollowPlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > maxFollowDistance) return;
        
        if (distanceToPlayer > stoppingDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
            
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    private void HealPlayerOverTime()
    {
        if (playerHealth == null) return;
        
        if (Time.time >= nextHealTime)
        {
            float healthPercentage = playerHealth.currentHealth / playerHealth.maxHealth;
            
            if (healthPercentage < healThreshold && playerHealth.currentHealth < playerHealth.maxHealth)
            {
                playerHealth.currentHealth += healAmount;
                playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth, playerHealth.maxHealth);
                
                var updateMethod = playerHealth.GetType().GetMethod("UpdateHealthBar", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (updateMethod != null)
                {
                    updateMethod.Invoke(playerHealth, null);
                }
            }
            
            nextHealTime = Time.time + healInterval;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxFollowDistance);
    }
}
