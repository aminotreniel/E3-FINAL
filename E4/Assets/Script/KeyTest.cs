using UnityEngine;

public class KeyTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P key pressed!");
        }
    }
}
