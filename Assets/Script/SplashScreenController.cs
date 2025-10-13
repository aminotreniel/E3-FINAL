using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // ← This line is required for IEnumerator

public class SplashScreenController : MonoBehaviour
{
    public float splashDuration = 3f; // Duration in seconds
    public string nextSceneName = "EnemyScene"; // Replace with your actual scene name

    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(splashDuration);
        SceneManager.LoadScene(nextSceneName);
    }
}
