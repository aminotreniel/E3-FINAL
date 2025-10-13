using UnityEngine;

public class DeleteThisLater : MonoBehaviour
{
    [Range(0f, 1f)]
    public float globalVolume = 1f; // Slider in Inspector

    void Update()
    {
        AudioListener.volume = globalVolume;
    }
}
