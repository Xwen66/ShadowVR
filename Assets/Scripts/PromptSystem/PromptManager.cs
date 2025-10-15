using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PromptManager : MonoBehaviour
{
    [Header("Prompt Settings")]
    [SerializeField] private PromptDatabaseSO promptDatabase;
    [SerializeField] private bool useChinese = true;
    
    [Header("UI Components")]
    public TextMeshProUGUI TextContent;
    public Image ButtonA;
    public Image ButtonX;
    public Canvas PromptCanvas;
    public Button CloseButton;
    
    [Header("Display Settings")]
    [SerializeField] private bool includeProgress = true;
    [SerializeField] private float defaultDisplayDuration = 5f;
    [SerializeField] private bool autoHide = false;
    
    [Header("Events")]
    public UnityEvent<PromptEntry> OnPromptShow;
    public UnityEvent OnPromptHide;
    
    // Singleton instance
    private static PromptManager _instance;
    public static PromptManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PromptManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PromptManager");
                    _instance = go.AddComponent<PromptManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    // Current prompt state
    private PromptEntry currentPrompt;
    private bool isPromptActive = false;
    private float hideTimer = 0f;
    
    // Properties
    public bool IsPromptActive => isPromptActive;
    public bool IsChinese => useChinese;
    public PromptEntry CurrentPrompt => currentPrompt;
    
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
        // Validate components
        if (promptDatabase == null)
        {
            Debug.LogWarning("PromptManager: No PromptDatabase assigned!");
        }
        
        if (PromptCanvas == null)
        {
            Debug.LogWarning("PromptManager: No PromptCanvas assigned!");
        }
        
        if (CloseButton != null)
        {
            CloseButton.onClick.AddListener(HidePrompt);
        }
        
        // Hide prompt canvas initially
        if (PromptCanvas != null)
        {
            PromptCanvas.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Handle auto-hide timer
        if (isPromptActive && autoHide && hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                HidePrompt();
            }
        }
    }
    
    /// <summary>
    /// Set the prompt database
    /// </summary>
    public void SetPromptDatabase(PromptDatabaseSO database)
    {
        promptDatabase = database;
    }
    
    /// <summary>
    /// Show prompt by index (called by QuestManager or events)
    /// </summary>
    public void ShowPrompt(int promptIndex)
    {
        if (promptDatabase == null)
        {
            Debug.LogError("PromptManager: No prompt database assigned!");
            return;
        }
        
        var prompt = promptDatabase.GetPromptByIndex(promptIndex);
        if (prompt == null)
        {
            Debug.LogError($"PromptManager: Prompt with index {promptIndex} not found!");
            return;
        }
        
        ShowPrompt(prompt);
    }
    
    /// <summary>
    /// Show specific prompt entry
    /// </summary>
    public void ShowPrompt(PromptEntry prompt)
    {
        if (prompt == null || !prompt.IsValid())
        {
            Debug.LogError("PromptManager: Invalid prompt entry!");
            return;
        }
        
        currentPrompt = prompt;
        isPromptActive = true;
        
        // Update UI
        UpdatePromptDisplay();
        
        // Show canvas
        if (PromptCanvas != null)
        {
            PromptCanvas.gameObject.SetActive(true);
        }
        
        // Set auto-hide timer
        if (autoHide)
        {
            hideTimer = defaultDisplayDuration;
        }
        
        // Trigger event
        OnPromptShow?.Invoke(prompt);
        
        Debug.Log($"Showing prompt {prompt.index}: {prompt.GetPromptContent(useChinese)}");
    }
    
    /// <summary>
    /// Hide current prompt
    /// </summary>
    public void HidePrompt()
    {
        if (!isPromptActive)
            return;
        
        isPromptActive = false;
        currentPrompt = null;
        hideTimer = 0f;
        
        // Hide canvas
        if (PromptCanvas != null)
        {
            PromptCanvas.gameObject.SetActive(false);
        }
        
        // Trigger event
        OnPromptHide?.Invoke();
        
        Debug.Log("Prompt hidden");
    }
    
    /// <summary>
    /// Update the prompt display UI
    /// </summary>
    private void UpdatePromptDisplay()
    {
        if (currentPrompt == null || TextContent == null)
            return;
        
        string displayText = currentPrompt.GetDisplayText(useChinese, includeProgress);
        TextContent.text = displayText;
    }
    
    /// <summary>
    /// Toggle language between Chinese and English
    /// </summary>
    public void ToggleLanguage()
    {
        useChinese = !useChinese;
        
        // Refresh current prompt if active
        if (isPromptActive)
        {
            UpdatePromptDisplay();
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
            
            // Refresh current prompt if active
            if (isPromptActive)
            {
                UpdatePromptDisplay();
            }
        }
    }
    
    /// <summary>
    /// Show random prompt (for testing or fallback)
    /// </summary>
    public void ShowRandomPrompt()
    {
        if (promptDatabase == null)
        {
            Debug.LogError("PromptManager: No prompt database assigned!");
            return;
        }
        
        var randomPrompt = promptDatabase.GetRandomPrompt();
        if (randomPrompt != null)
        {
            ShowPrompt(randomPrompt);
        }
        else
        {
            Debug.LogWarning("PromptManager: No valid prompts available!");
        }
    }
    
    /// <summary>
    /// Show prompts by progress pattern
    /// </summary>
    public void ShowPromptByProgress(string progressPattern)
    {
        if (promptDatabase == null)
        {
            Debug.LogError("PromptManager: No prompt database assigned!");
            return;
        }
        
        var prompts = promptDatabase.GetPromptsByProgress(progressPattern);
        if (prompts.Count > 0)
        {
            // Show first matching prompt
            ShowPrompt(prompts[0]);
        }
        else
        {
            Debug.LogWarning($"PromptManager: No prompts found with progress pattern '{progressPattern}'!");
        }
    }
    
    /// <summary>
    /// Enable/disable auto-hide functionality
    /// </summary>
    public void SetAutoHide(bool enable, float duration = 5f)
    {
        autoHide = enable;
        defaultDisplayDuration = duration;
    }
    
    /// <summary>
    /// Force hide prompt after delay
    /// </summary>
    public void HidePromptAfterDelay(float delay)
    {
        if (isPromptActive)
        {
            hideTimer = delay;
            autoHide = true;
        }
    }
    
    #region Inspector Test Functions
    
    [Header("Testing & Debug")]
    [SerializeField] private int testPromptIndex = 2;
    [SerializeField] private bool showDebugLogs = true;
    
    /// <summary>
    /// Test function: Show prompt with test index
    /// </summary>
    [ContextMenu("Test Show Prompt")]
    public void TestShowPrompt()
    {
        if (promptDatabase == null)
        {
            Debug.LogError("No PromptDatabase assigned! Please assign one in the inspector.");
            return;
        }
        
        ShowPrompt(testPromptIndex);
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Showed prompt #{testPromptIndex}");
    }
    
    /// <summary>
    /// Test function: Hide prompt
    /// </summary>
    [ContextMenu("Test Hide Prompt")]
    public void TestHidePrompt()
    {
        HidePrompt();
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Prompt hidden");
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
    /// Test function: Show random prompt
    /// </summary>
    [ContextMenu("Test Show Random Prompt")]
    public void TestShowRandomPrompt()
    {
        ShowRandomPrompt();
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Showed random prompt");
    }

      [ContextMenu("Test Show Next Prompt")]
    public void TestShowNextPrompt()
    {
        if (currentPrompt != null)
        {
            ShowPrompt(currentPrompt.index + 1);
        }
        else
        {
            Debug.LogError("No current prompt found!");
        }
        
        if (showDebugLogs)
            Debug.Log($"[TEST] Showed next prompt");
    }
    
    /// <summary>
    /// Test function: Print system status
    /// </summary>
    [ContextMenu("Test Print System Status")]
    public void TestPrintSystemStatus()
    {
        Debug.Log("=== PROMPT SYSTEM STATUS ===");
        Debug.Log($"PromptManager Instance: {(Instance != null ? "✓ Active" : "✗ Missing")}");
        Debug.Log($"PromptDatabase: {(promptDatabase != null ? "✓ Assigned" : "✗ Missing")}");
        Debug.Log($"Current Language: {(useChinese ? "Chinese (中文)" : "English")}");
        Debug.Log($"Prompt Active: {(isPromptActive ? "✓ Yes" : "✗ No")}");
        Debug.Log($"Auto Hide: {(autoHide ? "✓ Enabled" : "✗ Disabled")}");
        Debug.Log($"Current Prompt Index: {(isPromptActive && currentPrompt != null ? currentPrompt.index.ToString() : "None")}");
        
        if (promptDatabase != null)
        {
            Debug.Log($"Total Prompt Entries: {promptDatabase.promptEntries.Count}");
            Debug.Log($"Valid Prompts: {promptDatabase.GetValidPromptCount()}");
        }
        Debug.Log("===========================");
    }
    
    #endregion
}
