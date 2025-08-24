using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private int dialogueNumberToStart = 1;
    [SerializeField] private TriggerType triggerType = TriggerType.OnTriggerEnter;
    [SerializeField] private KeyCode triggerKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Trigger State")]
    [SerializeField] private bool canTriggerMultipleTimes = false;
    [SerializeField] private bool isTriggered = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private string promptText = "Press E to talk";
    
    [Header("Events")]
    public UnityEvent OnDialogueTrigger;
    
    private bool playerInRange = false;
    
    public enum TriggerType
    {
        OnTriggerEnter,
        OnKeyPress,
        OnClick,
        Manual
    }
    
    private void Start()
    {
        // Hide interaction prompt initially
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    private void Update()
    {
        // Handle key press trigger
        if (triggerType == TriggerType.OnKeyPress && playerInRange && !isTriggered)
        {
            if (Input.GetKeyDown(triggerKey))
            {
                TriggerDialogue();
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            
            // Show interaction prompt
            if (interactionPrompt != null && triggerType == TriggerType.OnKeyPress)
                interactionPrompt.SetActive(true);
            
            // Auto trigger if set to OnTriggerEnter
            if (triggerType == TriggerType.OnTriggerEnter && !isTriggered)
            {
                TriggerDialogue();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            
            // Hide interaction prompt
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
    
    private void OnMouseDown()
    {
        if (triggerType == TriggerType.OnClick && !isTriggered)
        {
            TriggerDialogue();
        }
    }
    
    /// <summary>
    /// Trigger the dialogue
    /// </summary>
    public void TriggerDialogue()
    {
        // Check if already triggered and not repeatable
        if (isTriggered && !canTriggerMultipleTimes)
            return;
        
        // Check if DialogueManager exists
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueTrigger: DialogueManager not found!");
            return;
        }
        
        // Mark as triggered
        isTriggered = true;
        
        // Hide interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        // Start dialogue
        DialogueManager.Instance.StartDialogue(dialogueNumberToStart);
        
        // Invoke custom events
        OnDialogueTrigger?.Invoke();
        
        Debug.Log($"Dialogue triggered: Starting dialogue number {dialogueNumberToStart}");
    }
    
    /// <summary>
    /// Reset trigger state (useful for repeatable dialogues)
    /// </summary>
    public void ResetTrigger()
    {
        isTriggered = false;
    }
    
    /// <summary>
    /// Set dialogue number to trigger
    /// </summary>
    public void SetDialogueNumber(int dialogueNumber)
    {
        dialogueNumberToStart = dialogueNumber;
    }
    
    /// <summary>
    /// Enable/disable trigger
    /// </summary>
    public void SetTriggerEnabled(bool enabled)
    {
        this.enabled = enabled;
        
        if (!enabled && interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
}
