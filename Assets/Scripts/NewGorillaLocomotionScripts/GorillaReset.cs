using System;
using UnityEngine;

public class GorillaReset : MonoBehaviour
{
    public Transform resetPosition1;
    public Transform resetPosition2;
    public Transform resetPosition3;
    
    private bool hasTriggeredFirstReset = false;
    
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
        
        if (!hasTriggeredFirstReset)
        {
            OnFirstReset();
            hasTriggeredFirstReset = true;
        }
    }
    
    private void OnFirstReset()
    {
        // 触发第七条对话
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.GoToDialogue(7);
            // 设置按钮为"关闭"模式（true表示关闭对话框模式）
            DialogueManager.Instance.SetNextButtonMode(true);
            Debug.Log("正在显示第七条对话内容，按钮设置为关闭模式");
        }
        else
        {
            Debug.LogWarning("DialogueManager实例未找到");
        }
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
