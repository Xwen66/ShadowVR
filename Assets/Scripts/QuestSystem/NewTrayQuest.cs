using System.Collections.Generic;
using UnityEngine;
using VInspector;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class NewTrayQuest : MonoBehaviour
{
    // define a list of XRSocketInteractor
    public List<XRSocketInteractor> sockets;
    
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
