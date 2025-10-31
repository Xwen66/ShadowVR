using UnityEngine;
using VInspector;

public class MemoryCollision : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip pickUpSound;
    public GameObject effectObject;
    public GameObject memoryModelObject;
    private GameObject instantiatedEffectObject;
    public string ItemType;
    public string ItemName;
    public string ItemDescription;
    public string ItemDescription2;
    public Sprite ItemImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 保存实例化后的对象引用
        instantiatedEffectObject = Instantiate(effectObject, transform.position, transform.rotation);
        instantiatedEffectObject.GetComponent<ItemEffectObject>().moveTarget = transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision other)
    {
        Debug.LogError("collider碰到东西了");
        // TransferItemToUIManager();

        //检测layer 是不是"GorillaRig"
        if (other.gameObject.layer == LayerMask.NameToLayer("GorillaRig"))
        {
            Debug.LogError("collider玩家碰撞到卡片");
            TransferItemToUIManager();
        }
    }


    void OnTriggerEnter(Collider other)
    {
        Debug.LogError("trigger碰到东西了");
        //检测layer 是不是"GorillaRig"
        if (other.gameObject.layer == LayerMask.NameToLayer("GorillaRig"))
        {
            Debug.LogError("trigger玩家碰撞到卡片");
            TransferItemToUIManager();
        }
    }

    void TransferItemToUIManager()
    {
        GlobalEvent.OnPickUpThingEvent.Invoke();
        
        // 播放捡起音效
        PlayPickUpSound();
        
        Debug.LogError("正在传递物品信息到UI管理器");
        GetUIManager uiManager = GetUIManager.Instance;

        // 将当前物品的信息赋值给UI管理器
        uiManager.ItemType = this.ItemType;
        uiManager.ItemName = this.ItemName;
        uiManager.ItemDescription = this.ItemDescription;
        uiManager.ItemDescription2 = this.ItemDescription2;
        uiManager.ItemImage = this.ItemImage;

        // 刷新UI显示
        uiManager.UpdateUI();

        // 显示UI
        uiManager.ShowUIForFiveSeconds();

        // 触发特效对象
        if (instantiatedEffectObject != null)
        {
            instantiatedEffectObject.GetComponent<ItemEffectObject>().OnGet();
        }
        
        // 处理记忆模型对象
        HandleMemoryModelObject();
        
        // 将当前物品插入到插槽中
        SetSelfToSock();

        Debug.LogError("物品信息传递完成，UI已更新");
    }


    [ContextMenu("set self to sock")]
    public void SetSelfToSock()
    {
        // 获取ToolBoxManager单例实例
        ToolBoxManager toolBoxManager = ToolBoxManager.Instance;

        if (toolBoxManager == null)
        {
            Debug.LogError("无法获取ToolBoxManager实例");
            return;
        }

        // 确保插槽列表已初始化
        if (toolBoxManager.memorySocketInteractorList == null || toolBoxManager.memorySocketInteractorList.Count == 0)
        {
            Debug.LogWarning("memory插槽列表为空，正在搜索所有插槽...");
            toolBoxManager.SearchAllSocketInteractors();
        }

        // 使用ToolBoxManager的强制插入方法，将当前游戏对象插入到可用memory插槽中
        bool success = toolBoxManager.ForceInsertToAvailableMemorySocket(this.gameObject);

        if (success)
        {
            Debug.Log($"成功将 {this.gameObject.name} 插入到memory插槽中");
        }
        else
        {
            Debug.LogWarning($"无法将 {this.gameObject.name} 插入到memory插槽中，可能没有可用插槽");
        }
    }

    [Button("test")]
    public void Test()
    {
        Debug.LogError("collider碰到东西了");
        TransferItemToUIManager();

    }

    /// <summary>
    /// 播放捡起物品音效
    /// </summary>
    private void PlayPickUpSound()
    {
        if (audioSource != null && pickUpSound != null)
        {
            audioSource.PlayOneShot(pickUpSound);
            Debug.Log("Playing pick up sound for memory item");
        }
        else
        {
            Debug.LogWarning("Cannot play pick up sound: AudioSource or pickUpSound AudioClip is missing");
        }
    }
    
    /// <summary>
    /// 处理记忆模型对象，将其从父物体中移出并激活
    /// </summary>
    private void HandleMemoryModelObject()
    {
        if (memoryModelObject != null)
        {
            // 将模型对象从父物体中移出
            memoryModelObject.transform.SetParent(null);
            
            // 激活模型对象
            memoryModelObject.SetActive(true);
            
            Debug.Log($"记忆模型对象 {memoryModelObject.name} 已从父物体中移出并激活");
        }
        else
        {
            Debug.LogWarning("memoryModelObject 为空，无法处理记忆模型对象");
        }
    }
}
