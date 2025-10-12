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

    // 单例实例
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

    // 当前对话状态
    private int currentDialogueNumber = -1;
    private bool isDialogueActive = false;

    // 属性
    public bool IsDialogueActive => isDialogueActive;
    public bool IsChinese => useChinese;
    public DialogueEntry CurrentDialogue => dialogueDatabase?.GetDialogueByNumber(currentDialogueNumber);
    
    private void Awake()
    {
        // 确保单例
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
        // 验证数据库
        if (dialogueDatabase == null)
        {
            Debug.LogWarning("DialogueManager: No DialogueDatabase assigned!");
        }
    }

    /// <summary>
    /// 设置对话数据库
    /// </summary>
    public void SetDialogueDatabase(DialogueDatabase database)
    {
        dialogueDatabase = database;
    }

    /// <summary>
    /// 从指定对话编号开始对话
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
    /// 显示当前对话
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
    /// 移动到下一条对话
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
    /// 结束当前对话
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
    /// 在中文和英文之间切换语言
    /// </summary>
    public void ToggleLanguage()
    {
        useChinese = !useChinese;

        // 如果对话处于活动状态则刷新当前对话
        if (isDialogueActive)
        {
            DisplayCurrentDialogue();
        }

        Debug.Log($"Language switched to: {(useChinese ? "Chinese" : "English")}");
    }

    /// <summary>
    /// 显式设置语言
    /// </summary>
    public void SetLanguage(bool chinese)
    {
        if (useChinese != chinese)
        {
            useChinese = chinese;

            // 如果对话处于活动状态则刷新当前对话
            if (isDialogueActive)
            {
                DisplayCurrentDialogue();
            }
        }
    }

    /// <summary>
    /// 跳转到指定对话编号
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
    /// 获取当前对话的角色图像
    /// </summary>
    public Sprite GetCurrentCharacterImage()
    {
        if (dialogueDatabase == null || !isDialogueActive)
            return null;

        var dialogue = dialogueDatabase.GetDialogueByNumber(currentDialogueNumber);
        return dialogue != null ? dialogueDatabase.GetCharacterImage(dialogue.CharacterID) : null;
    }

    /// <summary>
    /// 根据角色ID获取角色图像
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
    /// 测试功能：使用测试编号开始对话
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
    /// 测试功能：切换语言
    /// </summary>
    [ContextMenu("Test Toggle Language")]
    public void TestToggleLanguage()
    {
        ToggleLanguage();

        if (showDebugLogs)
            Debug.Log($"[TEST] Language switched to: {(useChinese ? "Chinese" : "English")}");
    }

    /// <summary>
    /// 测试功能：下一条对话
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
    /// 测试功能：结束对话
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
    /// 测试功能：打印系统状态
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
    /// 测试功能：打印所有对话
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
    /// 测试功能：打印角色图像
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
    /// 测试功能：验证对话序列
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

        // 检查序列中的间隙
        for (int i = 0; i < allNumbers.Count - 1; i++)
        {
            if (allNumbers[i + 1] - allNumbers[i] > 1)
            {
                Debug.LogWarning($"Gap in dialogue sequence: {allNumbers[i]} → {allNumbers[i + 1]}");
            }
        }

        // 检查重复项
        var duplicates = allNumbers.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var duplicate in duplicates)
        {
            Debug.LogError($"Duplicate dialogue number found: {duplicate}");
        }

        Debug.Log("===================================");
    }

    /// <summary>
    /// 从检查器设置测试对话编号
    /// </summary>
    public void SetTestDialogueNumber(int number)
    {
        testDialogueNumber = number;

        if (showDebugLogs)
            Debug.Log($"[TEST] Test dialogue number set to: {number}");
    }

    #endregion
}
