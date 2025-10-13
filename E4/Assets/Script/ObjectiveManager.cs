using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

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
    [Header("UI")]
    [Tooltip("Title shown above the objective list.")]
    public TextMeshProUGUI titleText;

    [Tooltip("Body text where the checklist (checkbox + task lines) will be rendered.")]
    public TextMeshProUGUI bodyText;

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

    void Start()
    {
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

    // Render the title and checklist into the assigned TMP fields
    public void RenderObjectives()
    {
        if (titleText != null)
        {
            // Default title if empty
            if (string.IsNullOrEmpty(titleText.text))
                titleText.text = "Main Objectives";
        }

        if (bodyText == null)
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
            return;
        }

        var group = groups[currentGroupIndex];
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
}
