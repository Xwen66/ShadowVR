using UnityEngine;

[System.Serializable]
public class PromptEntry
{
    [Header("Prompt Information")]
    public int index;
    public string progress; // Format: "n/3" etc.
    
    [Header("Prompt Content")]
    [TextArea(2, 4)]
    public string contentChinese;
    
    [TextArea(2, 4)]
    public string contentEnglish;
    
    // Helper method to get prompt content based on language
    public string GetPromptContent(bool isChinese = true)
    {
        return isChinese ? contentChinese : contentEnglish;
    }
    
    // Helper method to check if this is a valid prompt entry
    public bool IsValid()
    {
        return index >= 0 && (!string.IsNullOrEmpty(contentChinese) || !string.IsNullOrEmpty(contentEnglish));
    }
    
    // Helper method to get formatted display text with progress
    public string GetDisplayText(bool isChinese = true, bool includeProgress = true)
    {
        string content = GetPromptContent(isChinese);
        
        if (includeProgress && !string.IsNullOrEmpty(progress))
        {
            return $"{content}\n{progress}";
        }
        
        return content;
    }
}
