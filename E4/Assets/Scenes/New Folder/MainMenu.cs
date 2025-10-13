using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer;

    private void Awake()
    {
        // Singleton pattern so this persists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVolume(float volume)
    {
        // Convert slider value (0–1) to mixer volume (-80dB to 0dB)
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
}
