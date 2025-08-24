using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    [Header("Dialogue Information")]
    public int dialogNumber;
    public string characterNameChinese;
    public string characterNameEnglish; // This serves as character ID
    
    [Header("Dialogue Content")]
    [TextArea(3, 6)]
    public string contentChinese;
    
    [TextArea(3, 6)]
    public string contentEnglish;
    
    // Character ID is the English name
    public string CharacterID => characterNameEnglish;
    
    // Helper method to get character name based on language
    public string GetCharacterName(bool isChinese = true)
    {
        return isChinese ? characterNameChinese : characterNameEnglish;
    }
    
    // Helper method to get dialogue content based on language
    public string GetDialogueContent(bool isChinese = true)
    {
        return isChinese ? contentChinese : contentEnglish;
    }
    
    // Legacy method for backward compatibility
    public string GetDialogueText(bool isChinese = true)
    {
        return GetDialogueContent(isChinese);
    }
}
