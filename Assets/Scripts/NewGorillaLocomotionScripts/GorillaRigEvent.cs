using System;
using UnityEngine;

public class GorillaRigEvent : MonoBehaviour
{
    public Vector3 startPosition;
    public bool isCheckingDistance = false;
    public float distancetest;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GlobalEvent.OnCompleteMoveTeachingEvent.AddListener(LogTest);
        Invoke("RecordPosition", 0.5f);

    }

    void OnDestroy()
    {
        GlobalEvent.OnCompleteMoveTeachingEvent.RemoveListener(LogTest);
        // GlobalEvent.OnChangePersonEvent.RemoveListener(OnChangePersonEvent);
    }

    // Update is called once per frame
    void Update()
    {
        distancetest = Vector3.Distance(transform.position, startPosition);
        if (isCheckingDistance)
        {
            if (Vector3.Distance(transform.position, startPosition) > 0.2f)
            {
                GlobalEvent.OnCompleteMoveTeachingEvent.Invoke();
                isCheckingDistance = false;
            }
        }
    }

    public void RecordPosition()
    {
        startPosition = transform.position;
        isCheckingDistance = true;
        Debug.LogError("记录位置");
    }

    private void LogTest()
    {
        Debug.LogError("玩家完成移动教学");
    }

    private void OnChangePersonEvent(bool arg0)
    {
        Debug.LogError("OnChangePersonEvent");
        // RecordPosition(); 
    }
}
