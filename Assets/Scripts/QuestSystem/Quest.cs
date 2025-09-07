using UnityEngine;
using UnityEngine.Events;
using System;

public enum QuestState
{
    Inactive,
    Active,
    Completed,
    Failed
}


public class Quest : MonoBehaviour
{
    [Header("Quest Information")]
    public string QuestName = "New Quest";
    [TextArea(3, 5)]
    public string QuestDescription = "Complete this quest to progress.";
    [TextArea(2, 3)]
    protected string _questHintTextEng = "Look for clues to complete this quest.";
    protected string _questHintTextCn = "任务提示";
    
    [Header("Quest Settings")]
    public bool startActiveOnAwake = false;
    public bool canBeRepeated = false;
    
    [Header("Quest State")]
    [SerializeField] protected QuestState _currentState = QuestState.Inactive;
    [SerializeField] protected float _questProgress = 0f; // 0.0 to 1.0
    
    [Header("Events")]
    public UnityEvent<Quest> OnQuestStarted;
    public UnityEvent<Quest> OnQuestCompleted;
    public UnityEvent<Quest> OnQuestFailed;
    public UnityEvent<Quest, float> OnProgressChanged;
    
    // Properties
    public QuestState State => _currentState;
    public float Progress => _questProgress;
    public bool IsActive 
    { 
        get { return _currentState == QuestState.Active; }
        set 
        { 
            if (value && _currentState != QuestState.Active)
                StartQuest();
            else if (!value && _currentState == QuestState.Active)
                _currentState = QuestState.Inactive;
        }
    }
    public bool IsCompleted => _currentState == QuestState.Completed;
    public bool IsFailed => _currentState == QuestState.Failed;
    
    protected virtual void Awake()
    {
        if (startActiveOnAwake)
        {
            StartQuest();
        }
    }
    
    public virtual void StartQuest()
    {
        if (_currentState == QuestState.Active) return;
        if (_currentState == QuestState.Completed && !canBeRepeated) return;
        
        _currentState = QuestState.Active;
        _questProgress = 0f;
        
        OnQuestStarted?.Invoke(this);
        OnProgressChanged?.Invoke(this, _questProgress);
        
        Debug.Log($"Quest Started: {QuestName}");
    }
    
    protected virtual void CompleteQuest()
    {
        if (_currentState != QuestState.Active) return;
        
        _currentState = QuestState.Completed;
        _questProgress = 1f;
        
        OnQuestCompleted?.Invoke(this);
        OnProgressChanged?.Invoke(this, _questProgress);
        
        Debug.Log($"Quest Completed: {QuestName}");
    }
    
    protected virtual void FailQuest()
    {
        if (_currentState != QuestState.Active) return;
        
        _currentState = QuestState.Failed;
        
        OnQuestFailed?.Invoke(this);
        
        Debug.Log($"Quest Failed: {QuestName}");
    }
    
    protected virtual void UpdateProgress(float newProgress)
    {
        if (_currentState != QuestState.Active) return;
        
        _questProgress = Mathf.Clamp01(newProgress);
        OnProgressChanged?.Invoke(this, _questProgress);
        
        // Auto-complete if progress reaches 1.0
        if (_questProgress >= 1f)
        {
            CompleteQuest();
        }
    }
    
    
    public virtual void ResetQuest()
    {
        _currentState = QuestState.Inactive;
        _questProgress = 0f;
    }
    
    public virtual string GetHintText()
    {
        return _questHintTextEng;
    }
    
    public virtual void CheckCompletion()
    {
        // Override in derived classes
    }
    

    //
    public virtual void PromptHintUI()
    {
        // Could integrate with PromptSystem here
        var promptManager = FindFirstObjectByType<PromptManager>();
        if (promptManager != null)
        {
           promptManager.ShowPrompt(new PromptEntry(_questHintTextEng,_questHintTextCn));
        }
    }
    
    public virtual void PromptCompletionUI()
    {
        var promptManager = FindFirstObjectByType<PromptManager>();
        if (promptManager != null)
        {
           promptManager.ShowPrompt(new PromptEntry($"Quest Completed: {QuestName}","任务完成"));
        }
    }
    public virtual string GetProgressText()
    {
        return $"{QuestName}: {Mathf.RoundToInt(_questProgress * 100)}% Complete";
    }
    
    public virtual bool CanStart()
    {
        return _currentState == QuestState.Inactive || (_currentState == QuestState.Completed && canBeRepeated);
    }
    
    // Utility method for setting hint text
    protected void SetHintText(string hint)
    {
        _questHintTextEng = hint;
    }
}
