using UnityEngine;
using UnityEngine.Events;

public class CollectibleSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Automatically setup this GameObject as a collectible")]
    public bool autoSetup = true;
    
    [Header("Collectible Settings")]
    [Tooltip("Reference to GameManager for adding memory shards")]
    public GameManager gameManager;

    void Start()
    {
        if (autoSetup)
        {
            SetupAsCollectible();
        }
    }

    public void SetupAsCollectible()
    {
        // Add BoxCollider if doesn't exist
        BoxCollider triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        // Configure as trigger
        triggerCollider.isTrigger = true;
        
        // Add TriggerVolume if doesn't exist
        TriggerVolume triggerVolume = GetComponent<TriggerVolume>();
        if (triggerVolume == null)
        {
            triggerVolume = gameObject.AddComponent<TriggerVolume>();
        }
        
        // Configure TriggerVolume
        triggerVolume.triggerType = TriggerVolume.TriggerType.Player;
        triggerVolume.behavior = TriggerVolume.TriggerBehavior.OneTimeOnly;
        
        // Setup UnityEvents
        SetupCollectionEvents(triggerVolume);
        
        Debug.Log($"Collectible setup complete for: {gameObject.name}");
    }
    
    void SetupCollectionEvents(TriggerVolume triggerVolume)
    {
        // Clear existing events
        triggerVolume.OnTriggerEntered.RemoveAllListeners();
        
        // Add disable GameObject event
        triggerVolume.OnTriggerEntered.AddListener(() => {
            gameObject.SetActive(false);
            Debug.Log($"Collected: {gameObject.name}");
        });
        
        // Add memory shard if GameManager exists
        if (gameManager != null)
        {
            triggerVolume.OnTriggerEntered.AddListener(() => {
                gameManager.AddMemoryShard();
            });
        }
        else
        {
            // Try to find GameManager automatically
            GameManager foundManager = FindObjectOfType<GameManager>();
            if (foundManager != null)
            {
                triggerVolume.OnTriggerEntered.AddListener(() => {
                    foundManager.AddMemoryShard();
                });
            }
        }
    }
    
    // Public method to manually trigger collection
    public void CollectItem()
    {
        gameObject.SetActive(false);
        if (gameManager != null)
        {
            gameManager.AddMemoryShard();
        }
    }
} 