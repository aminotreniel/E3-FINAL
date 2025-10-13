using System.Collections;
using UnityEngine;
using TMPro;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorPivot;
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public Vector3 closedRotation = new Vector3(0, 0, 0);
    public float openSpeed = 2f;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRange = 3f;
    public Transform interactionPoint;

    private bool isOpen = false;
    private bool isMoving = false;
    private bool playerInRange = false;
    private Transform player;
    private AudioSource audioSource;

    [Header("UI Settings")]
    public Canvas interactionCanvas;
    public TextMeshProUGUI interactionText;
    private CanvasGroup interactionGroup;
    public float fadeSpeed = 5f;

    [Header("Key Lock Settings")]
    public bool requiresKey = false;
    public string keyName = "GoldenKey";
    private bool playerHasKey = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (interactionPoint == null)
            interactionPoint = transform;

        if (interactionCanvas != null)
        {
            interactionGroup = interactionCanvas.GetComponent<CanvasGroup>();
            if (interactionGroup == null)
                interactionGroup = interactionCanvas.gameObject.AddComponent<CanvasGroup>();
            interactionGroup.alpha = 0f;
        }
    }

    void Update()
    {
        CheckPlayerDistance();

        if (playerInRange && Input.GetKeyDown(interactionKey) && !isMoving)
        {
            if (isOpen)
                CloseDoor();
            else
            {
                if (!requiresKey || playerHasKey)
                    OpenDoor();
                else
                    Debug.Log("You need the " + keyName + " to open this door!");
            }
        }

        // Smooth fade
        if (interactionGroup != null)
        {
            float targetAlpha = playerInRange ? 1f : 0f;
            interactionGroup.alpha = Mathf.Lerp(interactionGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }

    void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(interactionPoint.position, player.position);
        playerInRange = distance <= interactionRange;
    }

    public void OpenDoor()
    {
        if (!isMoving && !isOpen)
        {
            StartCoroutine(RotateDoor(openRotation));
            isOpen = true;
            PlaySound(doorOpenSound);
        }
    }

    public void CloseDoor()
    {
        if (!isMoving && isOpen)
        {
            StartCoroutine(RotateDoor(closedRotation));
            isOpen = false;
            PlaySound(doorCloseSound);
        }
    }

    IEnumerator RotateDoor(Vector3 targetRotation)
    {
        isMoving = true;
        Quaternion startRotation = doorPivot.rotation;
        Quaternion endRotation = Quaternion.Euler(targetRotation);
        float timeElapsed = 0;

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime * openSpeed;
            doorPivot.rotation = Quaternion.Lerp(startRotation, endRotation, timeElapsed);
            yield return null;
        }

        doorPivot.rotation = endRotation;
        isMoving = false;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void AcquireKey(string key)
    {
        if (requiresKey && key == keyName)
        {
            playerHasKey = true;
            Debug.Log("Player acquired " + keyName + " for this door.");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Transform point = (interactionPoint != null) ? interactionPoint : transform;
        Gizmos.DrawWireSphere(point.position, interactionRange);
    }
}
