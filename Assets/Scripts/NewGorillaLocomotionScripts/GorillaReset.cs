using System;
using UnityEngine;
using VInspector;

public class GorillaReset : MonoBehaviour
{
    public Transform resetPosition1;
    public Transform resetPosition2;
    public Transform resetPosition3;

    private bool hasTriggeredFirstReset = false;
    private Rigidbody rb;
    public static bool IsResetting { get; private set; } = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ResetToPosition(resetPosition1);
        GlobalEvent.OnWhiteFlashEndEvent.AddListener(ResetPosition1);
    }

    private void ResetPosition1()
    {
        ResetToPosition(resetPosition1);

        if (!hasTriggeredFirstReset)
        {
            OnFirstReset();
            hasTriggeredFirstReset = true;
        }
    }

    private void ResetToPosition(Transform targetPosition)
    {
        IsResetting = true;
        
        if (rb != null)
        {
            rb.MovePosition(targetPosition.position);
            rb.MoveRotation(targetPosition.rotation);
        }
        else
        {
            this.transform.position = targetPosition.position;
            this.transform.rotation = targetPosition.rotation;
        }
        
        // 延迟一帧后重置标志，确保位置更新完成
        StartCoroutine(ResetComplete());
    }
    
    private System.Collections.IEnumerator ResetComplete()
    {
        yield return null; // 等待一帧
        IsResetting = false;
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

    [Button("Test First Reset")]
    public void Reset()
    {
        ResetPosition1();
    }
}
