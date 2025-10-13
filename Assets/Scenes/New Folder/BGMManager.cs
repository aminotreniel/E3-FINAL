using UnityEngine;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // survives scene changes
        }
        else
        {
            Destroy(gameObject); // kill duplicate
        }
    }
}
