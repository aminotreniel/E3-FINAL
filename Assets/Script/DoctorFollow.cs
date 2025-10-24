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
    
    [Header("Animation Settings")]
    public string sadIdleAnimation = "SadIdle";
    public string talkingAnimation = "Talking";
    public string idleAnimation = "Idle";
    public string walkingAnimation = "Walking";
    
    [Header("Ground Alignment")]
    public bool alignToGround = true;
    public float groundCheckDistance = 2f;
    public LayerMask groundLayer = -1; // Default to all layers
    
    private Transform player;
    private PlayerHealth playerHealth;
    private bool isRecruited = false;
    private bool playerInDetectionRange = false;
    private bool playerNearby = false;
    private bool hasShownInitialDialogue = false;
    private bool isShowingDialogue = false;
    private float nextHealTime = 0f;
    private Animator animator;
    private bool isWalking = false;
    private Vector3 lastPosition;
    
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
        
        // Get or create Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }
        
        // Disable root motion - animation plays in place, script controls movement
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
        
        // Initialize position tracking
        lastPosition = transform.position;
        
        // Start with SadIdle animation
        PlayAnimation(sadIdleAnimation, true);
        
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
            
            // Align to ground
            if (alignToGround)
            {
                AlignToGround();
            }
            
            // Check if doctor is moving and update animation
            float moveDistance = Vector3.Distance(transform.position, lastPosition);
            bool currentlyWalking = moveDistance > 0.01f;
            
            if (currentlyWalking && !isWalking)
            {
                // Started walking
                PlayAnimation(walkingAnimation, true);
                isWalking = true;
            }
            else if (!currentlyWalking && isWalking)
            {
                // Stopped walking
                PlayAnimation(idleAnimation, true);
                isWalking = false;
            }
            
            lastPosition = transform.position;
        }
    }
    
    private IEnumerator ShowInitialDialogue()
    {
        isShowingDialogue = true;
        currentDialogueState = DialogueState.InitialPlea;
        
        // Play Talking animation
        PlayAnimation(talkingAnimation, false);
        
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
        
        // Return to SadIdle after talking
        PlayAnimation(sadIdleAnimation, true);
        
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
        
        // Play Talking animation
        PlayAnimation(talkingAnimation, false);
        
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
        
        // Switch to Idle after recruitment
        PlayAnimation(idleAnimation, true);
        
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
            // Calculate direction to player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            
            // Only move on X and Z axes, preserve Y for ground alignment
            Vector3 horizontalDirection = new Vector3(directionToPlayer.x, 0f, directionToPlayer.z).normalized;
            
            // Move toward player using script (not animation root motion)
            Vector3 newPosition = transform.position + horizontalDirection * moveSpeed * Time.deltaTime;
            newPosition.y = transform.position.y; // Keep Y position stable
            transform.position = newPosition;
            
            // Rotate to face player
            if (horizontalDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
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
    
    private void AlignToGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f; // Start slightly above
        
        // Cast ray downward to find ground
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            // Smoothly move to ground position
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            
            // Align rotation to ground normal (optional, for slopes)
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void PlayAnimation(string animationName, bool loop)
    {
        if (animator == null) return;
        
        // If animator has an AnimatorController, use triggers/parameters
        if (animator.runtimeAnimatorController != null)
        {
            // Reset all boolean parameters first to avoid conflicts
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(param.name, false);
                }
            }
            
            // Try to set trigger or boolean parameter
            foreach (var param in animator.parameters)
            {
                if (param.name == animationName && param.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(animationName);
                    return;
                }
                else if (param.name == animationName && param.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(animationName, true);
                    return;
                }
            }
            
            // Force play by state name at layer 0, with immediate transition
            animator.Play(animationName, 0, 0f);
        }
        else
        {
            // No controller, play animation clip directly
            AnimationClip clip = FindAnimationClip(animationName);
            if (clip != null)
            {
                Animation legacyAnim = GetComponent<Animation>();
                if (legacyAnim == null)
                {
                    legacyAnim = gameObject.AddComponent<Animation>();
                }
                
                legacyAnim.AddClip(clip, animationName);
                legacyAnim.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
                legacyAnim.Play(animationName);
            }
        }
    }
    
    private AnimationClip FindAnimationClip(string clipName)
    {
        // Try to find animation clip in animator
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == clipName)
                    return clip;
            }
        }
        return null;
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
