using UnityEngine;
using VInspector;


public class DialogueManagerTest : MonoBehaviour
{
    public int dialogueNumberToStart = 1;
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
            Debug.Log("正在显示第二条对话内容");
        }
        else
        {
            Debug.LogWarning("DialogueManager实例未找到");
        }
    }
}
