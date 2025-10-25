using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public enum QuestStage
{
    Stage1,
    Stage2,
    Stage3
}

public class NewTrayQuest : MonoBehaviour
{
    // define a list of XRSocketInteractor
    public List<XRSocketInteractor> sockets;
    public QuestStage currentStage = QuestStage.Stage1;
    // define list of string
    public List<string> stage1Items = new List<string>() {"Item1", "Item2", "Item3"};
    public List<string> stage2Items = new List<string>() {"Item2"};
    public List<string> stage3Items = new List<string>() {"Item7", "Item8", "Item9"};
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Subscribe to selection events for all sockets
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket != null)
                {
                    socket.selectEntered.AddListener(OnItemPlacedInSocket);
                    socket.selectExited.AddListener(OnItemRemovedFromSocket);
                }
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket != null)
                {
                    socket.selectEntered.RemoveListener(OnItemPlacedInSocket);
                    socket.selectExited.RemoveListener(OnItemRemovedFromSocket);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    // Event handler for when an item is placed in a socket
    private void OnItemPlacedInSocket(SelectEnterEventArgs args)
    {
        var socket = args.interactorObject as XRSocketInteractor;
        if (socket != null)
        {
            Debug.Log($"Item placed in socket: {GetInteractableName(args.interactableObject)}");
            LogAllSocketContents();
            
            // Check stage conditions when item is placed
            CheckStageConditions();
        }
    }
    
    // Event handler for when an item is removed from a socket
    private void OnItemRemovedFromSocket(SelectExitEventArgs args)
    {
        var socket = args.interactorObject as XRSocketInteractor;
        if (socket != null)
        {
            Debug.Log($"Item removed from socket: {GetInteractableName(args.interactableObject)}");
            LogAllSocketContents();
            
            // Check stage conditions when item is removed
            CheckStageConditions();
        }
    }
    
    // Helper method to get all item names from a socket
    private List<string> GetSocketItemNames(XRSocketInteractor socket)
    {
        var itemNames = new List<string>();
        if (socket != null && socket.hasSelection)
        {
            foreach (var interactable in socket.interactablesSelected)
            {
                itemNames.Add(GetInteractableName(interactable));
            }
        }
        return itemNames;
    }
    
    // Helper method to get the name of an interactable
    private string GetInteractableName(IXRInteractable interactable)
    {
        if (interactable != null && interactable.transform != null)
        {
            return interactable.transform.gameObject.name;
        }
        return "Unknown Item";
    }
    
    // Method to log all socket contents
    private void LogAllSocketContents()
    {
        if (sockets == null || sockets.Count == 0)
        {
            Debug.Log("No sockets available");
            return;
        }
        
        Debug.Log("=== Current Socket Contents ===");
        
        int socketIndex = 0;
        foreach (var socket in sockets)
        {
            socketIndex++;
            if (socket == null)
            {
                Debug.Log($"Socket {socketIndex}: [NULL SOCKET]");
                continue;
            }
            
            var currentItems = GetSocketItemNames(socket);
            if (currentItems.Count > 0)
            {
                Debug.Log($"Socket {socketIndex}: {string.Join(", ", currentItems)}");
            }
            else
            {
                Debug.Log($"Socket {socketIndex}: [EMPTY]");
            }
        }
        Debug.Log("==============================");
    }
    
    // Method to check stage conditions based on current stage
    private void CheckStageConditions()
    {
        switch (currentStage)
        {
            case QuestStage.Stage1:
                CheckStage1Conditions();
                break;
            case QuestStage.Stage2:
                // TODO: Implement Stage2 conditions
                break;
            case QuestStage.Stage3:
                // TODO: Implement Stage3 conditions
                break;
        }
    }
    
    // Method to check Stage1 conditions
    private void CheckStage1Conditions()
    {
        // Get all current item names from all sockets
        var allCurrentItems = GetAllCurrentItemNames();
        
        // Check if all required items are present (contains check, not exact match)
        bool hasItem1 = false;
        bool hasItem2 = false;
        bool hasItem3 = false;
        
        foreach (string itemName in allCurrentItems)
        {
            if (itemName.Contains("Item1"))
                hasItem1 = true;
            if (itemName.Contains("Item2"))
                hasItem2 = true;
            if (itemName.Contains("Item3"))
                hasItem3 = true;
        }
        
        // If all three items are present, call the success method
        if (hasItem1 && hasItem2 && hasItem3)
        {
            Hahaha();
        }
    }
    
    // Helper method to get all current item names from all sockets
    private List<string> GetAllCurrentItemNames()
    {
        var allItems = new List<string>();
        
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket != null)
                {
                    var socketItems = GetSocketItemNames(socket);
                    allItems.AddRange(socketItems);
                }
            }
        }
        
        return allItems;
    }
    
    // Success method called when Stage1 conditions are met
    private void Hahaha()
    {
        Debug.Log("哈哈哈");
        
        // Disable Item1 and Item2 grab interaction from sockets
        DisableItemGrabInteractionInSockets("Item1");
        DisableItemGrabInteractionInSockets("Item2");

        StartCoroutine(SwitchToStage2AfterDelay());
    }
    
    // Helper method to disable grab interaction of items with specific name from sockets
    private void DisableItemGrabInteractionInSockets(string itemName)
    {
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket != null && socket.hasSelection)
                {
                    foreach (var interactable in socket.interactablesSelected)
                    {
                        if (interactable != null && interactable.transform != null)
                        {
                            GameObject obj = interactable.transform.gameObject;
                            if (obj.name.Contains(itemName))
                            {
                                // Set Unity layer to 4 for this object and all its children
                                SetLayersForObjectAndChildren(obj, 4);
                                Debug.Log($"Disabled grab interaction on {obj.name} and all its children by setting Unity layer to 4");
                            }
                        }
                    }
                }
            }
        }
    }
    
    // Helper method to set Unity layers for an object and all its children
    private void SetLayersForObjectAndChildren(GameObject obj, int layerValue)
    {
        // Set layer for the main object
        obj.layer = layerValue;
        
        // Set layer for all child objects
        Transform[] children = obj.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            child.gameObject.layer = layerValue;
        }
    }
    
    // Coroutine to wait 3 seconds then switch to Stage2
    private IEnumerator SwitchToStage2AfterDelay()
    {
        yield return new WaitForSeconds(3f);
        currentStage = QuestStage.Stage2;
        Debug.Log("Switched to Stage2");
    }

    // Method to log names of all items in sockets
    [Button("Log Items In Sockets")]
    public void LogItemsInSockets()
    {
        if (sockets == null || sockets.Count == 0)
        {
            Debug.Log("No sockets available or sockets list is empty");
            return;
        }

        foreach (var socket in sockets)
        {
            if (socket == null)
            {
                Debug.LogWarning("Found null socket in sockets list");
                continue;
            }

            if (socket.hasSelection)
            {
                // Get all selected interactables in this socket
                foreach (var interactable in socket.interactablesSelected)
                {
                    if (interactable != null && interactable.transform != null)
                    {
                        string itemName = interactable.transform.gameObject.name;
                        Debug.Log($"Socket has item: {itemName}");
                    }
                    else
                    {
                        Debug.LogWarning("Found null interactable or transform in selected items");
                    }
                }
            }
            else
            {
                Debug.Log("Socket is empty");
            }
        }
    }
}
