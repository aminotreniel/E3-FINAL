using UnityEngine;

public class SoundEffects : MonoBehaviour
{
    [Header("Alarm Settings")]
    [Tooltip("The alarm sound to play")]
    public AudioClip alarmSound;
    
    [Tooltip("Trigger zone that starts the alarm (must have 'is Trigger' checked)")]
    public BoxCollider alarmTriggerZone;
    
    [Header("Stop Alarm Settings")]
    [Tooltip("Enable this to stop the alarm when reaching Point B")]
    public bool stopAlarmAtPointB = true;
    
    [Tooltip("Trigger zone that stops the alarm (must have 'is Trigger' checked)")]
    public BoxCollider stopTriggerZone;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume of the alarm sound")]
    public float alarmVolume = 0.8f;
    
    [Tooltip("Should the alarm loop?")]
    public bool loopAlarm = true;
    
    // Private variables
    private AudioSource audioSource;
    private bool alarmTriggered = false;
    private bool alarmStopped = false;
    
    void Start()
    {
        // Create and setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = alarmVolume;
        audioSource.loop = loopAlarm;
        
        // Setup trigger zones
        if (alarmTriggerZone != null)
        {
            // Ensure it's a trigger
            alarmTriggerZone.isTrigger = true;
            
            // Add AlarmTrigger component to handle collision
            AlarmTrigger triggerScript = alarmTriggerZone.gameObject.GetComponent<AlarmTrigger>();
            if (triggerScript == null)
            {
                triggerScript = alarmTriggerZone.gameObject.AddComponent<AlarmTrigger>();
            }
            triggerScript.Initialize(this, true);
        }
        else
        {
            Debug.LogWarning("SoundEffects: Alarm Trigger Zone not assigned!");
        }
        
        // Setup stop zone if enabled
        if (stopAlarmAtPointB && stopTriggerZone != null)
        {
            // Ensure it's a trigger
            stopTriggerZone.isTrigger = true;
            
            // Add AlarmTrigger component to handle collision
            AlarmTrigger stopScript = stopTriggerZone.gameObject.GetComponent<AlarmTrigger>();
            if (stopScript == null)
            {
                stopScript = stopTriggerZone.gameObject.AddComponent<AlarmTrigger>();
            }
            stopScript.Initialize(this, false);
        }
    }
    
    public void TriggerAlarm()
    {
        if (alarmTriggered || alarmSound == null || alarmStopped)
            return;
        
        alarmTriggered = true;
        audioSource.clip = alarmSound;
        audioSource.Play();
        
        Debug.Log("Alarm triggered!");
    }
    
    public void StopAlarm()
    {
        if (!alarmTriggered || alarmStopped)
            return;
        
        alarmStopped = true;
        audioSource.Stop();
        
        Debug.Log("Alarm stopped!");
    }
    
    public bool IsAlarmPlaying()
    {
        return alarmTriggered && !alarmStopped;
    }
}

// Helper component to detect player collision
public class AlarmTrigger : MonoBehaviour
{
    private SoundEffects soundEffects;
    private bool isStartTrigger;
    
    public void Initialize(SoundEffects effects, bool startTrigger)
    {
        soundEffects = effects;
        isStartTrigger = startTrigger;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered
        if (other.CompareTag("Player"))
        {
            if (isStartTrigger)
            {
                soundEffects.TriggerAlarm();
            }
            else
            {
                soundEffects.StopAlarm();
            }
        }
    }
}
