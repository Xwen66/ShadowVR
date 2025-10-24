using UnityEngine;

public class ItemCollision : MonoBehaviour
{
    public GameObject effectObject;
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
        TransferItemToUIManager();

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
        if (toolBoxManager.socketInteractorList == null || toolBoxManager.socketInteractorList.Count == 0)
        {
            Debug.LogWarning("插槽列表为空，正在搜索所有插槽...");
            toolBoxManager.SearchAllSocketInteractors();
        }

        // 使用ToolBoxManager的强制插入方法，将当前游戏对象插入到可用插槽中
        bool success = toolBoxManager.ForceInsertToAvailableSocket(this.gameObject);

        if (success)
        {
            Debug.Log($"成功将 {this.gameObject.name} 插入到工具箱插槽中");
        }
        else
        {
            Debug.LogWarning($"无法将 {this.gameObject.name} 插入到工具箱插槽中，可能没有可用插槽");
        }
    }
}
