using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Tray : MonoBehaviour
{
    public List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor> sockets;
    public List<Pickupable> pickupables;
    public GameObject GoalFinishedCanvas;
    public bool ToggleDebug = false;
    void Start()
    {
        GoalFinishedCanvas.SetActive(false);
        pickupables = new List<Pickupable>();
        sockets = new List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
        foreach (Transform child in transform)
        {
            if (child.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>() != null)
            {
                sockets.Add(child.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>());
            }
        }

        foreach (UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket in sockets)
        {
            socket.selectEntered.AddListener(UpdateTrayStatus);
            socket.selectExited.AddListener(OnItemRemoved);
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }



    public void UpdateTrayStatus(SelectEnterEventArgs args)
    {
        // Get the socketed object and check if it's a Pickupable
        var socketedObject = args.interactableObject.transform.gameObject;
        var pickupable = socketedObject.GetComponent<Pickupable>();
        
        if (pickupable != null && !pickupables.Contains(pickupable))
        {
            pickupables.Add(pickupable);
        }
        if (ToggleDebug)
        {
           Debug.Log($"Tray Status: {pickupables.Count}/4 items in tray");
        }
        CheckRequirement();
    }

    public void OnItemRemoved(SelectExitEventArgs args)
    {
        // Get the removed object and check if it's a Pickupable
        var removedObject = args.interactableObject.transform.gameObject;
        var pickupable = removedObject.GetComponent<Pickupable>();
        
        if (pickupable != null && pickupables.Contains(pickupable))
        {
            pickupables.Remove(pickupable);
        }
        
        if(ToggleDebug)
        {
            Debug.Log($"Tray Status: {pickupables.Count}/4 items in tray");
        }
        CheckRequirement();
    }

    private void CheckRequirement()
    {
        //if the tray is full, goal achieved
        if (pickupables.Count == 4)
        {
            GoalFinishedCanvas.SetActive(true);
        }
        else{
            GoalFinishedCanvas.SetActive(false);
        }
        if(ToggleDebug)
        {
            Debug.Log($"Tray Status: {pickupables.Count}/4 items in tray");
        }
       
    }

  
}
