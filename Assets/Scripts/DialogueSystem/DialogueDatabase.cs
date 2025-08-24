using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "Dialogue/Database")]
public class DialogueDatabase : ScriptableObject
{
    [Header("Dialogue Entries")]
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();
    
    [Header("Character Images")]
    public List<CharacterImageData> characterImages = new List<CharacterImageData>();
    
    /// <summary>
    /// Get dialogue entry by dialog number
    /// </summary>
    public DialogueEntry GetDialogueByNumber(int dialogNumber)
    {
        return dialogueEntries.FirstOrDefault(entry => entry.dialogNumber == dialogNumber);
    }
    
    /// <summary>
    /// Get dialogue entry by character ID (English name)
    /// </summary>
    public List<DialogueEntry> GetDialoguesByCharacterID(string characterID)
    {
        return dialogueEntries.Where(entry => entry.CharacterID == characterID).ToList();
    }
    
    /// <summary>
    /// Get character image by character ID
    /// </summary>
    public Sprite GetCharacterImage(string characterID)
    {
        var imageData = characterImages.FirstOrDefault(img => img.characterID == characterID);
        return imageData?.characterSprite;
    }
    
    /// <summary>
    /// Get all dialogue numbers in order
    /// </summary>
    public List<int> GetAllDialogueNumbers()
    {
        return dialogueEntries.Select(entry => entry.dialogNumber).OrderBy(num => num).ToList();
    }
    
    /// <summary>
    /// Get next dialogue number
    /// </summary>
    public int GetNextDialogueNumber(int currentNumber)
    {
        var allNumbers = GetAllDialogueNumbers();
        var currentIndex = allNumbers.IndexOf(currentNumber);
        
        if (currentIndex >= 0 && currentIndex < allNumbers.Count - 1)
        {
            return allNumbers[currentIndex + 1];
        }
        
        return -1; // No next dialogue
    }
    
    /// <summary>
    /// Check if dialogue exists
    /// </summary>
    public bool HasDialogue(int dialogNumber)
    {
        return dialogueEntries.Any(entry => entry.dialogNumber == dialogNumber);
    }
}

[System.Serializable]
public class CharacterImageData
{
    public string characterID;
    public Sprite characterSprite;
}
