using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PromptDatabase", menuName = "Prompt/Database")]
public class PromptDatabaseSO : ScriptableObject
{
    [Header("Prompt Entries")]
    public List<PromptEntry> promptEntries = new List<PromptEntry>();
    
    /// <summary>
    /// Get prompt entry by index
    /// </summary>
    public PromptEntry GetPromptByIndex(int index)
    {
        return promptEntries.FirstOrDefault(entry => entry.index == index);
    }
    
    /// <summary>
    /// Get all prompt indices in order
    /// </summary>
    public List<int> GetAllPromptIndices()
    {
        return promptEntries.Select(entry => entry.index).OrderBy(num => num).ToList();
    }
    
    /// <summary>
    /// Check if prompt exists
    /// </summary>
    public bool HasPrompt(int index)
    {
        return promptEntries.Any(entry => entry.index == index);
    }
    
    /// <summary>
    /// Get prompts by progress pattern (e.g., all prompts with "1/3")
    /// </summary>
    public List<PromptEntry> GetPromptsByProgress(string progressPattern)
    {
        return promptEntries.Where(entry => entry.progress == progressPattern).ToList();
    }
    
    /// <summary>
    /// Get random prompt from available prompts
    /// </summary>
    public PromptEntry GetRandomPrompt()
    {
        if (promptEntries.Count == 0) return null;
        
        var validPrompts = promptEntries.Where(entry => entry.IsValid()).ToList();
        if (validPrompts.Count == 0) return null;
        
        int randomIndex = Random.Range(0, validPrompts.Count);
        return validPrompts[randomIndex];
    }
    
    /// <summary>
    /// Add prompt entry (for runtime or editor use)
    /// </summary>
    public void AddPrompt(PromptEntry prompt)
    {
        if (prompt != null && prompt.IsValid())
        {
            promptEntries.Add(prompt);
        }
    }
    
    /// <summary>
    /// Remove prompt by index
    /// </summary>
    public bool RemovePrompt(int index)
    {
        var prompt = GetPromptByIndex(index);
        if (prompt != null)
        {
            promptEntries.Remove(prompt);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Clear all prompts
    /// </summary>
    public void ClearPrompts()
    {
        promptEntries.Clear();
    }
    
    /// <summary>
    /// Get total count of valid prompts
    /// </summary>
    public int GetValidPromptCount()
    {
        return promptEntries.Count(entry => entry.IsValid());
    }
}
