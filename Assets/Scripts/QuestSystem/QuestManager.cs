using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Management")]
    [SerializeField] private List<Quest> allQuests = new List<Quest>();
    [SerializeField] private List<Quest> activeQuests = new List<Quest>();
    [SerializeField] private List<Quest> completedQuests = new List<Quest>();
    
    [Header("Settings")]
    [SerializeField] private bool autoDiscoverQuests = true;
    [SerializeField] private int maxActiveQuests = 3;
    
    [Header("Events")]
    public UnityEvent<Quest> OnQuestStarted;
    public UnityEvent<Quest> OnQuestCompleted;
    public UnityEvent<Quest> OnQuestFailed;
    
    // Singleton pattern
    private static QuestManager _instance;
    public static QuestManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<QuestManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("QuestManager");
                    _instance = go.AddComponent<QuestManager>();
                }
            }
            return _instance;
        }
    }
    
    // Properties
    public List<Quest> ActiveQuests => new List<Quest>(activeQuests);
    public List<Quest> CompletedQuests => new List<Quest>(completedQuests);
    public List<Quest> AllQuests => new List<Quest>(allQuests);
    public int ActiveQuestCount => activeQuests.Count;
    public bool CanStartNewQuest => activeQuests.Count < maxActiveQuests;
    
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
        if (autoDiscoverQuests)
        {
            DiscoverAllQuests();
        }
        
        InitializeQuests();
    }
    
    private void DiscoverAllQuests()
    {
        // Find all quest objects in the scene
        Quest[] foundQuests = FindObjectsOfType<Quest>();
        
        foreach (var quest in foundQuests)
        {
            if (!allQuests.Contains(quest))
            {
                allQuests.Add(quest);
            }
        }
        
        Debug.Log($"QuestManager: Discovered {allQuests.Count} quests");
    }
    
    private void InitializeQuests()
    {
        foreach (var quest in allQuests)
        {
            // Subscribe to quest events
            quest.OnQuestStarted.AddListener(HandleQuestStarted);
            quest.OnQuestCompleted.AddListener(HandleQuestCompleted);
            quest.OnQuestFailed.AddListener(HandleQuestFailed);
            
            // Add to appropriate lists based on current state
            switch (quest.State)
            {
                case QuestState.Active:
                    if (!activeQuests.Contains(quest))
                        activeQuests.Add(quest);
                    break;
                case QuestState.Completed:
                    if (!completedQuests.Contains(quest))
                        completedQuests.Add(quest);
                    break;
            }
        }
    }
    
    public bool StartQuest(Quest quest)
    {
        if (quest == null) return false;
        if (!quest.CanStart()) return false;
        if (!CanStartNewQuest) return false;
        
        quest.StartQuest();
        return true;
    }
    
    public bool StartQuestByName(string questName)
    {
        var quest = allQuests.FirstOrDefault(q => q.QuestName == questName);
        return StartQuest(quest);
    }
    
    private void HandleQuestStarted(Quest quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
        }
        
        // Remove from completed if it was there (for repeatable quests)
        if (completedQuests.Contains(quest))
        {
            completedQuests.Remove(quest);
        }
        
        OnQuestStarted?.Invoke(quest);
        Debug.Log($"QuestManager: Quest started - {quest.QuestName}");
    }
    
    private void HandleQuestCompleted(Quest quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
        }
        
        if (!completedQuests.Contains(quest))
        {
            completedQuests.Add(quest);
        }
        
        OnQuestCompleted?.Invoke(quest);
        Debug.Log($"QuestManager: Quest completed - {quest.QuestName}");
        
        // Check for new quests that might be unlocked
        CheckForUnlockedQuests();
    }
    
    private void HandleQuestFailed(Quest quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
        }
        
        OnQuestFailed?.Invoke(quest);
        Debug.Log($"QuestManager: Quest failed - {quest.QuestName}");
    }
    
    private void CheckForUnlockedQuests()
    {
        // This could check for prerequisite quests and automatically start new ones
        // For now, just a placeholder
    }
    
    public Quest GetQuestByName(string questName)
    {
        return allQuests.FirstOrDefault(q => q.QuestName == questName);
    }
    
    public List<Quest> GetQuestsByState(QuestState state)
    {
        return allQuests.Where(q => q.State == state).ToList();
    }
    
    public float GetOverallProgress()
    {
        if (allQuests.Count == 0) return 0f;
        
        float totalProgress = 0f;
        foreach (var quest in allQuests)
        {
            totalProgress += quest.Progress;
        }
        
        return totalProgress / allQuests.Count;
    }
    
    public void ResetAllQuests()
    {
        foreach (var quest in allQuests)
        {
            quest.ResetQuest();
        }
        
        activeQuests.Clear();
        completedQuests.Clear();
    }
    
    // Debug methods
    [ContextMenu("List All Quests")]
    public void ListAllQuests()
    {
        Debug.Log($"=== Quest Manager Status ===");
        Debug.Log($"Total Quests: {allQuests.Count}");
        Debug.Log($"Active Quests: {activeQuests.Count}");
        Debug.Log($"Completed Quests: {completedQuests.Count}");
        
        foreach (var quest in allQuests)
        {
            Debug.Log($"- {quest.QuestName}: {quest.State} ({quest.Progress:P0})");
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        foreach (var quest in allQuests)
        {
            if (quest != null)
            {
                quest.OnQuestStarted.RemoveListener(HandleQuestStarted);
                quest.OnQuestCompleted.RemoveListener(HandleQuestCompleted);
                quest.OnQuestFailed.RemoveListener(HandleQuestFailed);
            }
        }
    }
}
