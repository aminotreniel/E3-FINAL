


using UnityEngine;
using UnityEngine.SceneManagement; // Needed for SceneManager

public class levelselect : MonoBehaviour
{

    void Start() { }
    void Update(){}

    public void Loadlvl1()
    {
        // Change "LevelSelection" to the name of your level selection scene
        Debug.Log("Play button clicked! Loading Level Selection...");
        SceneManager.LoadScene("Scene1");
    }
    /**public void Loadlvl2()
    {
        // Change "LevelSelection" to the name of your level selection scene
        Debug.Log("Play button clicked! Loading Level Selection...");
        SceneManager.LoadScene("Settings");
    }*/
    public void MainMenu()
    {
        // Change "LevelSelection" to the name of your level selection scene
        Debug.Log("Play button clicked! Loading Level Selection...");
        SceneManager.LoadScene("Main_menu");
    }
}
