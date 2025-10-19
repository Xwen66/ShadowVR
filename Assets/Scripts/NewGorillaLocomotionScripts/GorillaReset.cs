using System;
using UnityEngine;

public class GorillaReset : MonoBehaviour
{
    public Transform resetPosition1;
    public Transform resetPosition2;
    public Transform resetPosition3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.position = resetPosition1.position;   
        this.transform.rotation = resetPosition1.rotation;
        GlobalEvent.OnWhiteFlashEndEvent.AddListener(ResetPosition1);
    }

    private void ResetPosition1()
    {
        this.transform.position = resetPosition1.position;   
        this.transform.rotation = resetPosition1.rotation;
    }
    
    void OnDestroy()
    {
        GlobalEvent.OnWhiteFlashEndEvent.RemoveListener(ResetPosition1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
