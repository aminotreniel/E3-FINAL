using UnityEngine;
using TMPro;  // Import TextMeshPro namespace

/// <summary>
/// DEPRECATED: This functionality has been merged into ObjectiveManager.
/// Use ObjectiveManager.Instance.AddScore(points) instead of GameManager.Instance.AddScore(points)
/// 
/// ObjectiveManager now handles both objectives and score tracking in one unified system.
/// This script is kept for backward compatibility but can be safely removed if no other scripts reference it.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton for global access

    [Header("UI")]
    public TMP_Text scoreText;  // drag your TMP text object here

    private int score = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        Debug.LogWarning("GameManager is deprecated. Use ObjectiveManager instead for score tracking.");
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
        
        // Forward to ObjectiveManager if it exists
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.AddScore(points);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
}
