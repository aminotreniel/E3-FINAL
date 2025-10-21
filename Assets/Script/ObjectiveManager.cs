using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class ObjectiveTask
{
    [Tooltip("The text that will be displayed for this task (e.g. 'Defeat the lieutenant')")] 
    [TextArea]
    public string text;

    [Tooltip("Optional ID used for matching when collecting items via code.")]
    public string id;

    [HideInInspector]
    public bool completed = false;
}

[System.Serializable]
public class ObjectiveGroup
{
    [Tooltip("Group title shown as the header when this batch is active.")]
    public string groupTitle = "";

    [Tooltip("Tasks that belong to this group/batch.")]
    public List<ObjectiveTask> tasks = new List<ObjectiveTask>();
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance; // Singleton for global access

    [Header("Objectives")]
    [Tooltip("Editable list of tasks. The order determines display and progression.")]
    // Legacy single-list kept for backward-compatibility. If `groups` is empty on Start
    // we will migrate these into a single group so existing inspector data keeps working.
    public List<ObjectiveTask> tasks = new List<ObjectiveTask>();

    [Tooltip("Grouped objective batches. The manager will show one group's tasks at a time and advance to the next group when all tasks in the current group are complete.")]
    public List<ObjectiveGroup> groups = new List<ObjectiveGroup>();

    [Tooltip("Text shown when all tasks are complete.")]
    public string allCompleteText = "All objectives complete.";

    [Tooltip("Use ASCII checkboxes ([ ] / [x]) instead of Unicode symbols. Enable if your font doesn't show the Unicode boxes.")]
    public bool useAsciiCheckboxes = true;

    [Header("Score Settings")]
    [Tooltip("Enable score tracking and display")]
    public bool enableScore = true;

    // UI Elements (created automatically)
    private Canvas objectiveCanvas;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI bodyText;
    private TextMeshProUGUI scoreText;
    
    // Score tracking
    private int score = 0;

    void Awake()
    {
        // Singleton pattern with scene persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Create UI first
        CreateObjectiveUI();
        
        // Migrate legacy flat `tasks` into groups if no groups were set up.
        if ((groups == null || groups.Count == 0) && tasks != null && tasks.Count > 0)
        {
            var g = new ObjectiveGroup();
            g.groupTitle = "Main Objectives";
            g.tasks = new List<ObjectiveTask>(tasks);
            groups = new List<ObjectiveGroup> { g };
        }

        // Ensure we have a valid current group index
        if (groups == null)
            groups = new List<ObjectiveGroup>();

        currentGroupIndex = GetNextActiveGroupIndex();
        RenderObjectives();
    }

    // Index of the currently visible group/batch
    private int currentGroupIndex = -1;

    private void CreateObjectiveUI()
    {
        // Check if we already have our UI
        if (titleText != null)
        {
            return; // UI already exists, don't recreate
        }

        // Create our own dedicated canvas for objectives
        GameObject canvasObj = new GameObject("ObjectiveCanvas");
        objectiveCanvas = canvasObj.AddComponent<Canvas>();
        objectiveCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        objectiveCanvas.sortingOrder = 102; // Higher than health and poison
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Make canvas persist across scenes too
        DontDestroyOnLoad(canvasObj);

        // Create main container in upper right corner
        GameObject containerObj = new GameObject("ObjectiveContainer");
        containerObj.transform.SetParent(objectiveCanvas.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        
        // Position in top-right corner
        containerRect.anchorMin = new Vector2(1f, 1f);
        containerRect.anchorMax = new Vector2(1f, 1f);
        containerRect.pivot = new Vector2(1f, 1f);
        containerRect.anchoredPosition = new Vector2(-20f, -20f);
        containerRect.sizeDelta = new Vector2(350f, 200f);

        // Add background panel
        Image bgPanel = containerObj.AddComponent<Image>();
        bgPanel.color = new Color(0.1f, 0.1f, 0.12f, 0.85f); // Semi-transparent dark background
        
        // Add outline
        var outline = containerObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.4f, 0.4f, 0.45f, 1f);
        outline.effectDistance = new Vector2(2, 2);
        
        // Add shadow
        var shadow = containerObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        shadow.effectDistance = new Vector2(3, -3);

        // Create title text
        GameObject titleObj = new GameObject("ObjectiveTitle");
        titleObj.transform.SetParent(containerObj.transform, false);
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Main Objectives";
        titleText.fontSize = 20;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(1f, 0.9f, 0.5f, 1f); // Gold color
        titleText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -10f);
        titleRect.sizeDelta = new Vector2(-20f, 30f);
        
        // Add title outline
        var titleOutline = titleObj.AddComponent<Outline>();
        titleOutline.effectColor = Color.black;
        titleOutline.effectDistance = new Vector2(1, 1);

        // Create body text (objectives list)
        GameObject bodyObj = new GameObject("ObjectiveBody");
        bodyObj.transform.SetParent(containerObj.transform, false);
        bodyText = bodyObj.AddComponent<TextMeshProUGUI>();
        bodyText.text = "";
        bodyText.fontSize = 16;
        bodyText.color = Color.white;
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        
        RectTransform bodyRect = bodyObj.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = new Vector2(10f, -45f);
        bodyRect.sizeDelta = new Vector2(-20f, -55f);
        
        // Add body outline
        var bodyOutline = bodyObj.AddComponent<Outline>();
        bodyOutline.effectColor = Color.black;
        bodyOutline.effectDistance = new Vector2(1, 1);
        
        // Create score text (below objectives) if enabled
        if (enableScore)
        {
            GameObject scoreObj = new GameObject("ScoreText");
            scoreObj.transform.SetParent(objectiveCanvas.transform, false);
            scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = "Score: 0";
            scoreText.fontSize = 18;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.color = new Color(0.5f, 0.9f, 1f, 1f); // Cyan color
            scoreText.alignment = TextAlignmentOptions.TopRight;
            
            RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(1f, 1f);
            scoreRect.anchorMax = new Vector2(1f, 1f);
            scoreRect.pivot = new Vector2(1f, 1f);
            scoreRect.anchoredPosition = new Vector2(-20f, -240f); // Below objectives
            scoreRect.sizeDelta = new Vector2(200f, 30f);
            
            // Add score outline
            var scoreOutline = scoreObj.AddComponent<Outline>();
            scoreOutline.effectColor = Color.black;
            scoreOutline.effectDistance = new Vector2(1, 1);
            
            // Add score shadow
            var scoreShadow = scoreObj.AddComponent<Shadow>();
            scoreShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            scoreShadow.effectDistance = new Vector2(2, -2);
        }
    }

    // Render the title and checklist into the assigned TMP fields
    public void RenderObjectives()
    {
        if (titleText == null || bodyText == null)
            return;

        // Build the checklist lines: an empty box for incomplete, or a checked box for complete.
        // Default to ASCII boxes for broad font compatibility; you can toggle via `useAsciiCheckboxes`.
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // If there are no groups, show the all-complete text
        if (groups == null || groups.Count == 0)
        {
            sb.AppendLine(allCompleteText);
            bodyText.text = sb.ToString();
            return;
        }

        // Ensure currentGroupIndex is valid
        if (currentGroupIndex < 0 || currentGroupIndex >= groups.Count)
        {
            sb.AppendLine(allCompleteText);
            bodyText.text = sb.ToString();
            titleText.text = "Objectives Complete!";
            return;
        }

        var group = groups[currentGroupIndex];
        
        // Update title with current group name
        if (!string.IsNullOrEmpty(group.groupTitle))
            titleText.text = group.groupTitle;
        else
            titleText.text = "Main Objectives";
        
        for (int i = 0; i < group.tasks.Count; i++)
        {
            var t = group.tasks[i];
            string box;
            if (useAsciiCheckboxes)
                box = t.completed ? "[x]" : "[ ]";
            else
                box = t.completed ? "\u2611" : "\u25A1";
            sb.AppendLine(box + " " + t.text);
        }

        bodyText.text = sb.ToString();
    }

    // Mark a task complete by index (0-based)
    public void CompleteTaskByIndex(int index)
    {
        if (groups == null || currentGroupIndex < 0 || currentGroupIndex >= groups.Count)
            return;

        var group = groups[currentGroupIndex];
        if (index < 0 || index >= group.tasks.Count)
            return;

        group.tasks[index].completed = true;
        Debug.Log("ObjectiveManager: completed task index=" + index + " in group=" + currentGroupIndex);
        // Advance group if needed
        AdvanceGroupIfNeeded();
        RenderObjectives();
    }

    // Mark tasks complete by ID (useful when an item pickup calls by id)
    public void CompleteTaskById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;
        if (groups == null || currentGroupIndex < 0 || currentGroupIndex >= groups.Count)
            return;

        var group = groups[currentGroupIndex];
        for (int i = 0; i < group.tasks.Count; i++)
        {
            var task = group.tasks[i];
            if (!task.completed && !string.IsNullOrEmpty(task.id) && string.Equals(task.id.Trim(), id.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                task.completed = true;
                Debug.Log("ObjectiveManager: completed task id='" + id + "' (task index=" + i + ", group=" + currentGroupIndex + ")");
                AdvanceGroupIfNeeded();
                RenderObjectives();
                return;
            }
        }
    }

    // Backwards-compatible method called by old pickup scripts
    public void OnKeyPickedUp()
    {
        // Try to match id "key" exactly, otherwise complete first incomplete
        // Try to complete in current group
        if (groups != null && currentGroupIndex >= 0 && currentGroupIndex < groups.Count)
        {
            var group = groups[currentGroupIndex];
            for (int i = 0; i < group.tasks.Count; i++)
            {
                var t = group.tasks[i];
                if (!t.completed && !string.IsNullOrEmpty(t.id) && t.id.ToLowerInvariant().Contains("key"))
                {
                    t.completed = true;
                    AdvanceGroupIfNeeded();
                    RenderObjectives();
                    return;
                }
            }
        }

        CompleteFirstIncomplete();
    }

    public void OnDoorOpened()
    {
        if (groups != null && currentGroupIndex >= 0 && currentGroupIndex < groups.Count)
        {
            var group = groups[currentGroupIndex];
            for (int i = 0; i < group.tasks.Count; i++)
            {
                var t = group.tasks[i];
                if (!t.completed && !string.IsNullOrEmpty(t.id) && t.id.ToLowerInvariant().Contains("door"))
                {
                    t.completed = true;
                    AdvanceGroupIfNeeded();
                    RenderObjectives();
                    return;
                }
            }
        }

        CompleteFirstIncomplete();
    }

    private void CompleteFirstIncomplete()
    {
        if (groups == null || currentGroupIndex < 0 || currentGroupIndex >= groups.Count)
            return;

        var group = groups[currentGroupIndex];
        for (int i = 0; i < group.tasks.Count; i++)
        {
            if (!group.tasks[i].completed)
            {
                group.tasks[i].completed = true;
                AdvanceGroupIfNeeded();
                RenderObjectives();
                return;
            }
        }
    }

    // Allow setting the title text at runtime
    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    // Allow replacing the tasks at runtime
    public void SetTasks(List<ObjectiveTask> newTasks)
    {
        tasks = newTasks ?? new List<ObjectiveTask>();
        // migrate into a single group
        groups = new List<ObjectiveGroup>() { new ObjectiveGroup { groupTitle = "Main Objectives", tasks = new List<ObjectiveTask>(tasks) } };
        currentGroupIndex = GetNextActiveGroupIndex();
        RenderObjectives();
    }

    // Replace all groups at runtime
    public void SetGroups(List<ObjectiveGroup> newGroups)
    {
        groups = newGroups ?? new List<ObjectiveGroup>();
        currentGroupIndex = GetNextActiveGroupIndex();
        RenderObjectives();
    }

    // Returns the index of the next group that has incomplete tasks or -1 if none
    private int GetNextActiveGroupIndex()
    {
        if (groups == null)
            return -1;

        for (int g = 0; g < groups.Count; g++)
        {
            var group = groups[g];
            if (group.tasks == null || group.tasks.Count == 0)
                continue;

            bool allComplete = true;
            foreach (var t in group.tasks)
            {
                if (!t.completed)
                {
                    allComplete = false;
                    break;
                }
            }

            if (!allComplete)
                return g;
        }

        return -1;
    }

    // Advance to the next group if the current group is fully completed
    private void AdvanceGroupIfNeeded()
    {
        if (groups == null || currentGroupIndex < 0 || currentGroupIndex >= groups.Count)
            return;

        var group = groups[currentGroupIndex];
        bool allComplete = true;
        foreach (var t in group.tasks)
        {
            if (!t.completed)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            Debug.Log("ObjectiveManager: completed group '" + (string.IsNullOrEmpty(group.groupTitle) ? currentGroupIndex.ToString() : group.groupTitle) + "'");
            // Move to next group that has incomplete tasks
            currentGroupIndex = GetNextActiveGroupIndex();
        }
    }

    // ===== SCORE MANAGEMENT =====
    
    /// <summary>
    /// Add points to the score and update the UI
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
    }
    
    /// <summary>
    /// Set the score to a specific value
    /// </summary>
    public void SetScore(int newScore)
    {
        score = newScore;
        UpdateScoreUI();
    }
    
    /// <summary>
    /// Get the current score value
    /// </summary>
    public int GetScore()
    {
        return score;
    }
    
    /// <summary>
    /// Reset the score to zero
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }
    
    private void UpdateScoreUI()
    {
        if (enableScore && scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
}
