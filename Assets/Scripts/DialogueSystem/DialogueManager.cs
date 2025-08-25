using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueDatabase dialogueDatabase;
    [SerializeField] private bool useChinese = true;

    
    [Header("Events")]
    public UnityEvent<DialogueEntry> OnDialogueStart;
    public UnityEvent<DialogueEntry> OnDialogueDisplay;
    public UnityEvent OnDialogueEnd;
    
    // Singleton instance
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DialogueManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DialogueManager");
                    _instance = go.AddComponent<DialogueManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // Current dialogue state
    private int currentDialogueNumber = -1;
    private bool isDialogueActive = false;
    
    // Properties
    public bool IsDialogueActive => isDialogueActive;
    public bool IsChinese => useChinese;
    public DialogueEntry CurrentDialogue => dialogueDatabase?.GetDialogueByNumber(currentDialogueNumber);
    
    private void Awake()
    {
        // Ensure singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Validate database
        if (dialogueDatabase == null)
        {
            Debug.LogWarning("DialogueManager: No DialogueDatabase assigned!");
        }
    }
    
    /// <summary>
    /// Set the dialogue database
    /// </summary>
    public void SetDialogueDatabase(DialogueDatabase database)
    {
        dialogueDatabase = database;
    }
    
    /// <summary>
    /// Start dialogue from specific dialog number
    /// </summary>
    public void StartDialogue(int dialogNumber)
    {
        if (dialogueDatabase == null)
        {
            Debug.LogError("DialogueManager: No dialogue database assigned!");
            return;
        }
        
        var dialogue = dialogueDatabase.GetDialogueByNumber(dialogNumber);
        if (dialogue == null)
        {
            Debug.LogError($"DialogueManager: Dialogue with number {dialogNumber} not found!");
            return;
        }
        
        currentDialogueNumber = dialogNumber;
        isDialogueActive = true;
        
        OnDialogueStart?.Invoke(dialogue);
        DisplayCurrentDialogue();
    }
    
    /// <summary>
    /// Display current dialogue
    /// </summary>
    public void DisplayCurrentDialogue()
    {
        if (!isDialogueActive || dialogueDatabase == null)
            return;
        
        var dialogue = dialogueDatabase.GetDialogueByNumber(currentDialogueNumber);
        if (dialogue != null)
        {
            OnDialogueDisplay?.Invoke(dialogue);
            Debug.Log($"Displaying dialogue {currentDialogueNumber}: {dialogue.GetDialogueContent(useChinese)}");
        }
    }
    
    /// <summary>
    /// Move to next dialogue
    /// </summary>
    public void NextDialogue()
    {
        if (!isDialogueActive || dialogueDatabase == null)
            return;
        
        int nextDialogueNumber = dialogueDatabase.GetNextDialogueNumber(currentDialogueNumber);
        
        if (nextDialogueNumber != -1)
        {
            currentDialogueNumber = nextDialogueNumber;
            DisplayCurrentDialogue();
        }
        else
        {
            EndDialogue();
        }
    }
    
    /// <summary>
    /// End current dialogue
    /// </summary>
    public void EndDialogue()
    {
        if (!isDialogueActive)
            return;
        
        isDialogueActive = false;
        currentDialogueNumber = -1;
        
        OnDialogueEnd?.Invoke();
        Debug.Log("Dialogue ended");
    }
    
    /// <summary>
    /// Toggle language between Chinese and English
    /// </summary>
    public void ToggleLanguage()
    {
        useChinese = !useChinese;
        
        // Refresh current dialogue if active
        if (isDialogueActive)
        {
            DisplayCurrentDialogue();
        }
        
        Debug.Log($"Language switched to: {(useChinese ? "Chinese" : "English")}");
    }
    
    /// <summary>
    /// Set language explicitly
    /// </summary>
    public void SetLanguage(bool chinese)
    {
        if (useChinese != chinese)
        {
            useChinese = chinese;
            
            // Refresh current dialogue if active
            if (isDialogueActive)
            {
                DisplayCurrentDialogue();
            }
        }
    }
    
    /// <summary>
    /// Skip to specific dialogue number
    /// </summary>
    public void GoToDialogue(int dialogNumber)
    {
        if (dialogueDatabase == null || !dialogueDatabase.HasDialogue(dialogNumber))
        {
            Debug.LogError($"DialogueManager: Cannot go to dialogue {dialogNumber} - not found!");
            return;
        }
        
        currentDialogueNumber = dialogNumber;
        
        if (!isDialogueActive)
        {
            isDialogueActive = true;
            OnDialogueStart?.Invoke(dialogueDatabase.GetDialogueByNumber(dialogNumber));
        }
        
        DisplayCurrentDialogue();
    }
    
    /// <summary>
    /// Get character image for current dialogue
    /// </summary>
    public Sprite GetCurrentCharacterImage()
    {
        if (dialogueDatabase == null || !isDialogueActive)
            return null;
        
        var dialogue = dialogueDatabase.GetDialogueByNumber(currentDialogueNumber);
        return dialogue != null ? dialogueDatabase.GetCharacterImage(dialogue.CharacterID) : null;
    }
    
    /// <summary>
    /// Get character image by character ID
    /// </summary>
    public Sprite GetCharacterImage(string characterID)
    {
        return dialogueDatabase?.GetCharacterImage(characterID);
    }
    
    #region Inspector Test Functions
    
    [Header("Testing & Debug")]
    [SerializeField] private int testDialogueNumber = 1;
    [SerializeField] private bool showDebugLogs = true;
    
    /// <summary>
    /// Test function: Start dialogue with test number
    /// </summary>
    [ContextMenu("Test Start Dialogue")]
    public void TestStartDialogue()
    {
        if (dialogueDatabase == null)
        {
            Debug.LogError("No DialogueDatabase assigned! Please assign one in the inspector.");
            return;
        }
        
        StartDialogue(testDialogueNumber);
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Started dialogue #{testDialogueNumber}");
    }
    
    /// <summary>
    /// Test function: Toggle language
    /// </summary>
    [ContextMenu("Test Toggle Language")]
    public void TestToggleLanguage()
    {
        ToggleLanguage();
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Language switched to: {(useChinese ? "Chinese" : "English")}");
    }
    
    /// <summary>
    /// Test function: Next dialogue
    /// </summary>
    [ContextMenu("Test Next Dialogue")]
    public void TestNextDialogue()
    {
        if (!isDialogueActive)
        {
            Debug.LogWarning("[TEST] No dialogue is currently active!");
            return;
        }
        
        NextDialogue();
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Advanced to next dialogue");
    }
    
    /// <summary>
    /// Test function: End dialogue
    /// </summary>
    [ContextMenu("Test End Dialogue")]
    public void TestEndDialogue()
    {
        if (!isDialogueActive)
        {
            Debug.LogWarning("[TEST] No dialogue is currently active!");
            return;
        }
        
        EndDialogue();
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Dialogue ended manually");
    }
    
    /// <summary>
    /// Test function: Print system status
    /// </summary>
    [ContextMenu("Test Print System Status")]
    public void TestPrintSystemStatus()
    {
        Debug.Log("=== DIALOGUE SYSTEM STATUS ===");
        Debug.Log($"DialogueManager Instance: {(Instance != null ? "✓ Active" : "✗ Missing")}");
        Debug.Log($"DialogueDatabase: {(dialogueDatabase != null ? "✓ Assigned" : "✗ Missing")}");
        Debug.Log($"Current Language: {(useChinese ? "Chinese (中文)" : "English")}");
        Debug.Log($"Dialogue Active: {(isDialogueActive ? "✓ Yes" : "✗ No")}");
        Debug.Log($"Current Dialogue Number: {(isDialogueActive ? currentDialogueNumber.ToString() : "None")}");
        
        if (dialogueDatabase != null)
        {
            Debug.Log($"Total Dialogue Entries: {dialogueDatabase.dialogueEntries.Count}");
            Debug.Log($"Character Images: {dialogueDatabase.characterImages.Count}");
        }
        Debug.Log("============================");
    }
    
    /// <summary>
    /// Test function: Print all dialogues
    /// </summary>
    [ContextMenu("Test Print All Dialogues")]
    public void TestPrintAllDialogues()
    {
        if (dialogueDatabase == null)
        {
            Debug.LogWarning("[TEST] No DialogueDatabase assigned!");
            return;
        }
        
        Debug.Log("=== ALL DIALOGUES ===");
        foreach (var entry in dialogueDatabase.dialogueEntries)
        {
            Debug.Log($"#{entry.dialogNumber} - {entry.CharacterID} ({entry.characterNameChinese}): " +
                      $"EN: \"{entry.contentEnglish}\" | CN: \"{entry.contentChinese}\"");
        }
        Debug.Log("====================");
    }
    
    /// <summary>
    /// Test function: Print character images
    /// </summary>
    [ContextMenu("Test Print Character Images")]
    public void TestPrintCharacterImages()
    {
        if (dialogueDatabase == null)
        {
            Debug.LogWarning("[TEST] No DialogueDatabase assigned!");
            return;
        }
        
        Debug.Log("=== CHARACTER IMAGES ===");
        if (dialogueDatabase.characterImages.Count == 0)
        {
            Debug.Log("No character images assigned!");
        }
        else
        {
            foreach (var charImg in dialogueDatabase.characterImages)
            {
                Debug.Log($"Character ID: \"{charImg.characterID}\" - " +
                          $"Image: {(charImg.characterSprite != null ? "✓ Assigned" : "✗ Missing")}");
            }
        }
        Debug.Log("========================");
    }
    
    /// <summary>
    /// Test function: Validate dialogue sequence
    /// </summary>
    [ContextMenu("Test Validate Dialogue Sequence")]
    public void TestValidateDialogueSequence()
    {
        if (dialogueDatabase == null)
        {
            Debug.LogWarning("[TEST] No DialogueDatabase assigned!");
            return;
        }
        
        Debug.Log("=== DIALOGUE SEQUENCE VALIDATION ===");
        var allNumbers = dialogueDatabase.GetAllDialogueNumbers();
        
        if (allNumbers.Count == 0)
        {
            Debug.LogWarning("No dialogues found in database!");
            return;
        }
        
        Debug.Log($"Total dialogues: {allNumbers.Count}");
        Debug.Log($"Dialogue range: {allNumbers[0]} to {allNumbers[allNumbers.Count - 1]}");
        
        // Check for gaps in sequence
        for (int i = 0; i < allNumbers.Count - 1; i++)
        {
            if (allNumbers[i + 1] - allNumbers[i] > 1)
            {
                Debug.LogWarning($"Gap in dialogue sequence: {allNumbers[i]} → {allNumbers[i + 1]}");
            }
        }
        
        // Check for duplicates
        var duplicates = allNumbers.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var duplicate in duplicates)
        {
            Debug.LogError($"Duplicate dialogue number found: {duplicate}");
        }
        
        Debug.Log("===================================");
    }
    
    /// <summary>
    /// Set test dialogue number from inspector
    /// </summary>
    public void SetTestDialogueNumber(int number)
    {
        testDialogueNumber = number;
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Test dialogue number set to: {number}");
    }
    
    #endregion
}
