using System;
using System.Collections;
using UnityEngine;

public class ToolUI : MonoBehaviour
{
    public Transform uiImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GlobalEvent.OnPickUpThingEvent.AddListener(OnPickUpThingEvent);
        GlobalEvent.OnCloseToolUIEvent.AddListener(OnCloseToolUIEvent);
    }

    private void OnCloseToolUIEvent()
    {
        uiImage.gameObject.SetActive(false);
    }

    private void OnPickUpThingEvent()
    {
        StartCoroutine(ShowUIAfterDelay());
    }

    private IEnumerator ShowUIAfterDelay()
    {
        yield return new WaitForSeconds(7f);
        uiImage.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void OnDestroy()
    {
        GlobalEvent.OnPickUpThingEvent.RemoveListener(OnPickUpThingEvent);
    }
}
