using UnityEngine;
using VInspector;


public class DialogueManagerTest : MonoBehaviour
{
    public int dialogueNumberToStart = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [Button("Test Start Dialogue")]
    void TestStartDialogue()
    {
        // 调用GoToDialogue方法显示第二条对话内容
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.GoToDialogue(dialogueNumberToStart);
            Debug.Log("正在显示第四条对话内容");
        }
        else
        {
            Debug.LogWarning("DialogueManager实例未找到");
        }
    }

    [Button("change button to next")]
    void ChangeButtonToNext()
    {
        // 将按钮功能设置为"下一步"
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetNextButtonMode(false);
            Debug.Log("按钮已设置为下一步模式");
        }
        else
        {
            Debug.LogWarning("DialogueManager实例未找到");
        }
    }

    [Button("change button to close")]
    void ChangeButtonToClose()
    {
        // 将按钮功能设置为"关闭"
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetNextButtonMode(true);
            Debug.Log("按钮已设置为关闭模式");
        }
        else
        {
            Debug.LogWarning("DialogueManager实例未找到");
        }
    }
}
