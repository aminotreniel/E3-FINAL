

using UnityEngine;
using UnityEngine.SceneManagement; // Needed for SceneManager

public class play : MonoBehaviour
{

    void Start() { }
    void Update(){}

    public void PlayGame()
    {
        // Change "LevelSelection" to the name of your level selection scene
        Debug.Log("Play button clicked! Loading Level Selection...");
        SceneManager.LoadScene("Scene1");
    }
    public void Settings()
    {
        // Change "LevelSelection" to the name of your level selection scene
        Debug.Log("Play button clicked! Loading Level Selection...");
        SceneManager.LoadScene("Settings");
    }
}
