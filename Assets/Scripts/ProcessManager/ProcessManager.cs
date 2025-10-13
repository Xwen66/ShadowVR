using UnityEngine;

public class ProcessManager : MonoBehaviour
{
    // 单例实例
    private static ProcessManager _instance;
    
    // 公共访问属性
    public static ProcessManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ProcessManager>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("ProcessManager");
                    _instance = singletonObject.AddComponent<ProcessManager>();
                }
            }
            return _instance;
        }
    }
    
    // 确保单例在场景切换时不被销毁
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 订阅对话事件
        GlobalEvent.nextDialogueEvent.AddListener(OnNextDialogue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        GlobalEvent.nextDialogueEvent.RemoveListener(OnNextDialogue);
    }

    /// <summary>
    /// 处理下一句对话事件
    /// </summary>
    /// <param name="dialogueNumber">对话序号</param>
    private void OnNextDialogue(int dialogueNumber)
    {
        Debug.Log($"ProcessManager: 收到对话事件，对话序号: {dialogueNumber}");
    }
}
