using UnityEngine;
using UnityEngine.Events;

public class TriggerVolume : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("What type of objects can trigger this volume")]
    public TriggerType triggerType = TriggerType.AnyObject;
    
    [Tooltip("Specific tag to detect (only used when TriggerType is SpecificTag)")]
    public string targetTag = "Player";
    
    [Tooltip("Specific layer to detect (only used when TriggerType is SpecificLayer)")]
    public LayerMask targetLayer = 1;
    
    [Header("Trigger Behavior")]
    [Tooltip("How the trigger should behave")]
    public TriggerBehavior behavior = TriggerBehavior.Repeatable;
    
    [Tooltip("Delay before trigger activates (in seconds)")]
    public float triggerDelay = 0f;
    
    [Header("Events")]
    [Tooltip("Called when an object enters the trigger")]
    public UnityEvent OnTriggerEntered;
    
    [Tooltip("Called when an object exits the trigger")]
    public UnityEvent OnTriggerExited;
    
    [Tooltip("Called when an object stays in the trigger")]
    public UnityEvent OnTriggerStayed;
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = true;
    
    [Tooltip("Show trigger volume in scene view")]
    public bool showGizmo = true;
    
    [Tooltip("Color of the trigger volume gizmo")]
    public Color gizmoColor = Color.green;
    
    // Private variables
    private BoxCollider triggerCollider;
    private bool hasTriggered = false;
    private Coroutine delayCoroutine;
    
    // Enums for dropdown options
    public enum TriggerType
    {
        AnyObject,
        Player,
        SpecificTag,
        SpecificLayer,
        PlayerAndObjects
    }
    
    public enum TriggerBehavior
    {
        Repeatable,
        OneTimeOnly,
        RequireStay
    }
    
    void Start()
    {
        SetupTriggerCollider();
    }
    
    void SetupTriggerCollider()
    {
        // Get or add BoxCollider
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            if (showDebugMessages)
                Debug.Log($"Added BoxCollider to {gameObject.name}");
        }
        
        // Ensure it's set as trigger
        triggerCollider.isTrigger = true;
        
        if (showDebugMessages)
            Debug.Log($"TriggerVolume '{gameObject.name}' initialized with {triggerType} detection");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!ShouldTrigger(other)) return;
        
        if (behavior == TriggerBehavior.OneTimeOnly && hasTriggered) return;
        
        if (showDebugMessages)
            Debug.Log($"Object '{other.gameObject.name}' entered trigger volume '{gameObject.name}'");
        
        if (triggerDelay > 0f)
        {
            if (delayCoroutine != null) StopCoroutine(delayCoroutine);
            delayCoroutine = StartCoroutine(DelayedTrigger(other, "Enter"));
        }
        else
        {
            ExecuteTriggerEnter(other);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!ShouldTrigger(other)) return;
        
        if (showDebugMessages)
            Debug.Log($"Object '{other.gameObject.name}' exited trigger volume '{gameObject.name}'");
        
        OnTriggerExited?.Invoke();
    }
    
    void OnTriggerStay(Collider other)
    {
        if (!ShouldTrigger(other)) return;
        
        if (behavior == TriggerBehavior.RequireStay)
        {
            OnTriggerStayed?.Invoke();
        }
    }
    
    bool ShouldTrigger(Collider other)
    {
        switch (triggerType)
        {
            case TriggerType.AnyObject:
                return true;
                
            case TriggerType.Player:
                return other.CompareTag("Player") || other.CompareTag("MainCamera");
                
            case TriggerType.SpecificTag:
                return other.CompareTag(targetTag);
                
            case TriggerType.SpecificLayer:
                return ((1 << other.gameObject.layer) & targetLayer) != 0;
                
            case TriggerType.PlayerAndObjects:
                return other.CompareTag("Player") || other.CompareTag("MainCamera") || 
                       other.GetComponent<Rigidbody>() != null;
                
            default:
                return false;
        }
    }
    
    void ExecuteTriggerEnter(Collider other)
    {
        OnTriggerEntered?.Invoke();
        hasTriggered = true;
    }
    
    System.Collections.IEnumerator DelayedTrigger(Collider other, string triggerAction)
    {
        yield return new WaitForSeconds(triggerDelay);
        
        if (triggerAction == "Enter")
        {
            ExecuteTriggerEnter(other);
        }
    }
    
    // Public methods for use in UnityEvents
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (showDebugMessages)
            Debug.Log($"Trigger volume '{gameObject.name}' has been reset");
    }
    
    public void ActivateGameObject(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(true);
            if (showDebugMessages)
                Debug.Log($"Activated GameObject: {target.name}");
        }
    }
    
    public void DeactivateGameObject(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(false);
            if (showDebugMessages)
                Debug.Log($"Deactivated GameObject: {target.name}");
        }
    }
    
    public void PlayAudioSource(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Play();
            if (showDebugMessages)
                Debug.Log($"Played AudioSource: {audioSource.name}");
        }
    }
    
    public void LogMessage(string message)
    {
        Debug.Log($"TriggerVolume Message: {message}");
    }
    
    // Gizmo for visual representation in Scene view
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.center, boxCol.size);
            
            // Draw wireframe
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        else
        {
            // Default size if no BoxCollider
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
} 