using UnityEngine;
using TMPro;  // Import TextMeshPro namespace

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
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }
}
