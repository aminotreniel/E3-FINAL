#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CreateRadialPoisonDemo
{
    [MenuItem("Tools/Info - Radial Poison Demo (Deprecated)")]
    public static void ShowInfo()
    {
        EditorUtility.DisplayDialog(
            "Radial Poison Demo - Deprecated", 
            "This tool is no longer needed!\n\n" +
            "The PlayerPoison script now automatically creates all UI elements including:\n" +
            "• Poison bar with custom styling\n" +
            "• Blur overlay effect\n" +
            "• All visual effects in code\n\n" +
            "Simply attach the PlayerPoison script to your player and everything will be created automatically at runtime.\n\n" +
            "No manual setup required!", 
            "OK"
        );
        
        Debug.Log("PlayerPoison now handles all UI creation automatically. No manual setup needed!");
    }
}
#endif
