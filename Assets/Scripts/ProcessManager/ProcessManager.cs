using UnityEngine;
using System.Collections;
using VInspector;


public class ProcessManager : MonoBehaviour
{


    //各种引用
    public GameObject hedgehogModel;
    public GameObject hedgehogCreateVFXPrefab;
    public GameObject fox;
    public PromptManager promptManager;
    public PromptMove promptMove;
    // public Transform hedgehogCreatePosition;
    public GameManager gameManager;
    // public GameObject pressAUI;

    // 跟踪当前对话编号
    private int currentDialogueNumber = -1;

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
        
        // 订阅对话结束事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd.AddListener(OnDialogueEnd);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        // 取消订阅事件
        GlobalEvent.nextDialogueEvent.RemoveListener(OnNextDialogue);
        
        // 取消订阅对话结束事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd.RemoveListener(OnDialogueEnd);
        }
    }

    /// <summary>
    /// 处理下一句对话事件
    /// </summary>
    /// <param name="dialogueNumber">对话序号</param>
    private void OnNextDialogue(int dialogueNumber)
    {
        Debug.Log($"ProcessManager: 收到对话事件，对话序号: {dialogueNumber}");
        
        // 更新当前对话编号
        currentDialogueNumber = dialogueNumber;

        if (dialogueNumber == 1)
        {
            // 延迟2秒后生成物体
            StartCoroutine(DelayedSpawn(2f));
        }

        if (dialogueNumber == 4)
        {
            //第二步  小刺猬说完"你睡着前，好像要带上托盘去厨房找狮子妈妈。"，后生成托盘上的任务UI
            DialogueManager.Instance.SetNextButtonMode(false);
            QuestUIManager.Instance.SetItemQuest1Text();
            promptManager.ShowPrompt(1);
            promptMove.EnterFollowingState();

        }

        if (dialogueNumber == 5)// 说完第五句时（切换到第六句）
        {
            //第二步  小刺猬开始说"我体型小。。。"出现"按A切换视角"
            Debug.LogError("按A切换视角");
            gameManager.canChangePerson = true;
            // pressAUI.SetActive(true);
            GlobalEvent.OnPressAUIEvent.Invoke();

        }

    }
    
    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    private void OnDialogueEnd()
    {
        // 检查当前对话是否是第七号对话
        if (currentDialogueNumber == 7)
        {
            Debug.Log("哈哈哈");
        }
        
        // 重置对话编号
        currentDialogueNumber = -1;
    }








    /// <summary>
    /// 延迟生成物体的协程
    /// </summary>
    /// <param name="delay">延迟时间（秒）</param>
    private IEnumerator DelayedSpawn(float delay)
    {
        yield return new WaitForSeconds(delay);

        //active hedgehog model
        hedgehogModel.SetActive(true);
        GameObject hedgehogCreateVFX = Instantiate(hedgehogCreateVFXPrefab, hedgehogModel.transform.position, hedgehogModel.transform.rotation);

        // 生成后再等待2秒
        yield return new WaitForSeconds(2f);


        // 触发第四个对话条目
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.GoToDialogue(2);
            // 设置按钮为"下一条"模式（false表示下一句对话模式）
            DialogueManager.Instance.SetNextButtonMode(false);
            Debug.Log("正在显示第二条对话内容");
        }
        else
        {
            Debug.LogWarning("DialogueManager实例未找到");
        }
    }



    //test
    [Button("Test")]
    public void Test()
    {
        QuestUIManager.Instance.SetItemQuest1Text();

    }

}
