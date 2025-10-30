using System;
using System.Collections;
using UnityEngine;

public class ToolUI : MonoBehaviour
{
    public Transform uiImage;
    public AudioSource audioSource;
    private bool hasShownOnce = false; // 标记是否已经显示过一次
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GlobalEvent.OnPickUpThingEvent.AddListener(OnPickUpThingEvent);
        GlobalEvent.OnCloseToolUIEvent.AddListener(OnCloseToolUIEvent);
    }

    private void OnCloseToolUIEvent()
    {
        uiImage.gameObject.SetActive(false);
        hasShownOnce = true; // 标记为已显示过
    }

    private void OnPickUpThingEvent()
    {
        // 只有在未显示过的情况下才执行
        if (!hasShownOnce)
        {
            StartCoroutine(ShowUIAfterDelay());
        }
    }

    private IEnumerator ShowUIAfterDelay()
    {
        yield return new WaitForSeconds(7f);
        uiImage.gameObject.SetActive(true);
        audioSource.Play();
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
