using UnityEngine;
using System.Collections;

public class ProcessManager : MonoBehaviour
{


    //各种引用
    public GameObject hedgehogModel;
    public GameObject hedgehogCreateVFXPrefab;
    public GameObject fox;



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

        if (dialogueNumber == 1)
        {
            // 延迟2秒后生成物体
            StartCoroutine(DelayedSpawn(2f));
        }

        if (dialogueNumber == 3)
        {
            DialogueManager.Instance.SetNextButtonMode(true);
        }

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

        // 输出哈哈哈
        Debug.Log("哈哈哈");

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
}
