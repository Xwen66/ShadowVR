using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class TrayQuestRequirement
{
    public string itemName;
    public string itemTag;
    public int requiredCount = 1;
    [HideInInspector] public int currentCount = 0;
}

public class TrayQuest : Quest
{
    [Header("Tray Quest Settings")]
    [SerializeField] private List<TrayQuestRequirement> itemRequirements = new List<TrayQuestRequirement>();
    [SerializeField] private bool requireAllItems = false; // If false, any X items will do
    [SerializeField] private int totalItemsRequired = 4;
    
    [Header("Tray Components")]
    public List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor> sockets;
    public List<Pickupable> pickupables;
    public GameObject GoalFinishedCanvas;
    
    [Header("Debug")]
    public bool ToggleDebug = false;
    
    protected override void Awake()
    {
        base.Awake(); // Call base class Awake
        
        // Initialize quest info if not set
        if (string.IsNullOrEmpty(QuestName))
            QuestName = $"Fill the Tray ({totalItemsRequired} items)";
            
        if (string.IsNullOrEmpty(QuestDescription))
            QuestDescription = $"Place {totalItemsRequired} items in the tray to complete this task.";
    }
    
    void Start()
    {
        InitializeTrayQuest();
    }
    
    private void InitializeTrayQuest()
    {
        if (GoalFinishedCanvas != null)
            GoalFinishedCanvas.SetActive(false);
            
        pickupables = new List<Pickupable>();
        sockets = new List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        
        // Auto-discover socket interactors in children
        foreach (Transform child in transform)
        {
            var socket = child.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            if (socket != null)
            {
                sockets.Add(socket);
            }
        }

        // Set up event listeners for all sockets
        foreach (var socket in sockets)
        {
            socket.selectEntered.AddListener(UpdateTrayStatus);
            socket.selectExited.AddListener(OnItemRemoved);
        }
        
        if (ToggleDebug)
            Debug.Log($"TrayQuest initialized with {sockets.Count} sockets");
    }

 



    public void UpdateTrayStatus(SelectEnterEventArgs args)
    {
        if (!IsActive) return; // Only process if quest is active
        
        // Get the socketed object and check if it's a Pickupable
        var socketedObject = args.interactableObject.transform.gameObject;
        var pickupable = socketedObject.GetComponent<Pickupable>();
        
        if (pickupable != null && !pickupables.Contains(pickupable))
        {
            pickupables.Add(pickupable);
            pickupable.isInTray = true; // Utilize the Pickupable property
            
            // Update requirement counts if using specific requirements
            UpdateRequirementCounts(pickupable, 1);
            
            if (ToggleDebug)
            {
                Debug.Log($"Item placed: {socketedObject.name}. Progress: {pickupables.Count}/{totalItemsRequired}");
            }
        }
        
        UpdateQuestProgress();
        CheckCompletion();
    }

    public void OnItemRemoved(SelectExitEventArgs args)
    {
        if (!IsActive) return; // Only process if quest is active
        
        // Get the removed object and check if it's a Pickupable
        var removedObject = args.interactableObject.transform.gameObject;
        var pickupable = removedObject.GetComponent<Pickupable>();
        
        if (pickupable != null && pickupables.Contains(pickupable))
        {
            pickupables.Remove(pickupable);
            pickupable.isInTray = false; // Utilize the Pickupable property
            
            // Update requirement counts if using specific requirements
            UpdateRequirementCounts(pickupable, -1);
            
            if (ToggleDebug)
            {
                Debug.Log($"Item removed: {removedObject.name}. Progress: {pickupables.Count}/{totalItemsRequired}");
            }
        }
        
        UpdateQuestProgress();
        CheckCompletion();
    }
    
    private void UpdateRequirementCounts(Pickupable pickupable, int delta)
    {
        if (itemRequirements.Count == 0) return;
        
        foreach (var requirement in itemRequirements)
        {
            // Check by name or tag
            if ((!string.IsNullOrEmpty(requirement.itemName) && pickupable.name.Contains(requirement.itemName)) ||
                (!string.IsNullOrEmpty(requirement.itemTag) && pickupable.CompareTag(requirement.itemTag)))
            {
                requirement.currentCount = Mathf.Max(0, requirement.currentCount + delta);
                break;
            }
        }
    }
    
    private void UpdateQuestProgress()
    {
        float progress = GetQuestProgress();
        UpdateProgress(progress); // Use base class method
    }
    
    private float GetQuestProgress()
    {
        if (requireAllItems && itemRequirements.Count > 0)
        {
            // Progress based on specific requirements
            float totalProgress = 0f;
            foreach (var requirement in itemRequirements)
            {
                totalProgress += Mathf.Clamp01((float)requirement.currentCount / requirement.requiredCount);
            }
            return totalProgress / itemRequirements.Count;
        }
        else
        {
            // Simple count-based progress
            return Mathf.Clamp01((float)pickupables.Count / totalItemsRequired);
        }
    }

    public override void CheckCompletion()
    {
        if (!IsActive) return;
        
        bool isComplete = false;
        
        if (requireAllItems && itemRequirements.Count > 0)
        {
            // Check if all specific requirements are met
            isComplete = true;
            foreach (var requirement in itemRequirements)
            {
                if (requirement.currentCount < requirement.requiredCount)
                {
                    isComplete = false;
                    break;
                }
            }
        }
        else
        {
            // Simple count check
            isComplete = pickupables.Count >= totalItemsRequired;
        }
        
        // Update UI based on completion status
        if (GoalFinishedCanvas != null)
        {
            GoalFinishedCanvas.SetActive(isComplete);
        }
        
        // Complete quest if all conditions are met
        if (isComplete && !IsCompleted)
        {
            CompleteQuest(); // Use base class method
        }
    }
    
    public override string GetHintText()
    {
        if (!IsActive)
            return "Quest not started";
            
        string hint = "";
        
        if (requireAllItems && itemRequirements.Count > 0)
        {
            hint = "Required items:\n";
            foreach (var requirement in itemRequirements)
            {
                hint += $"• {requirement.itemName}: {requirement.currentCount}/{requirement.requiredCount}\n";
            }
        }
        else
        {
            hint = $"Place {pickupables.Count}/{totalItemsRequired} items in the tray";
        }
        
        return hint;
    }
    
    public override void PromptHintUI()
    {
        base.PromptHintUI(); // Call base implementation
        
        // Could show specific tray quest UI here
        if (ToggleDebug)
        {
            Debug.Log(GetHintText());
        }
    }
    
    // Public methods for external control
    public void SetRequiredItemCount(int count)
    {
        totalItemsRequired = count;
        UpdateQuestProgress();
        CheckCompletion();
    }
    
    public int GetCurrentItemCount()
    {
        return pickupables.Count;
    }
    
    public int GetRequiredItemCount()
    {
        return totalItemsRequired;
    }
    
    void OnDestroy()
    {
        // Clean up event listeners
        foreach (var socket in sockets)
        {
            if (socket != null)
            {
                socket.selectEntered.RemoveListener(UpdateTrayStatus);
                socket.selectExited.RemoveListener(OnItemRemoved);
            }
        }
    }
}
