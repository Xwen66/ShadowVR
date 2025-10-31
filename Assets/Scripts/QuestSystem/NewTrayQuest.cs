using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public enum QuestStage
{
    Stage1,
    Stage2,
    Stage3
}

public class NewTrayQuest : MonoBehaviour
{
    #region 字段定义
    public AudioSource audioSource;
    public AudioClip UIChange;
    public AudioClip UICompletely;
    public AudioClip gameEnd;
    public AudioClip kitchenUIShow;
    public AudioClip floorOutSound;
    public List<GameObject> blocks;

    // 音效播放控制
    private float scriptStartTime;
    private const float AUDIO_DELAY_TIME = 3f; // 音效延迟时间（秒）

    // 定义提示管理器
    public PromptManager promptManager;
    // 定义XRSocketInteractor列表
    public List<XRSocketInteractor> sockets;
    public QuestStage currentStage = QuestStage.Stage1;
    // 定义字符串列表
    public List<string> stage1Items = new List<string>() { "Item1", "Item2", "Item3" };
    public List<string> stage2Items = new List<string>() { "Item3" };
    public List<string> stage3Items = new List<string>() { "Item4" };

    // 跟踪当前对话编号
    private int currentDialogueNumber = -1;

    // 跟踪任务进度变化
    private string lastProgressText = "";

    // shadow line的虚线图
    public GameObject shadowLine;

    // 厨房解锁UI
    public GameObject kitchenUnlockUI;


    #endregion

    #region Unity生命周期

    // 在MonoBehaviour创建后第一次执行Update之前调用一次
    void Start()
    {
        // 记录脚本开始运行的时间
        scriptStartTime = Time.time;

        // 订阅所有插槽的选择事件
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket != null)
                {
                    socket.selectEntered.AddListener(OnItemPlacedInSocket);
                    socket.selectExited.AddListener(OnItemRemovedFromSocket);
                }
            }
        }

        // 订阅对话事件
        GlobalEvent.nextDialogueEvent.AddListener(OnNextDialogue);

        // 订阅对话结束事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd.AddListener(OnDialogueEnd);
        }


    }

    void OnEnable()
    {
        // // 初始化提示显示
        UpdatePromptDisplay();
    }

    void OnDestroy()
    {
        // 取消订阅事件以防止内存泄漏
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket != null)
                {
                    socket.selectEntered.RemoveListener(OnItemPlacedInSocket);
                    socket.selectExited.RemoveListener(OnItemRemovedFromSocket);
                }
            }
        }

        // 取消订阅事件
        GlobalEvent.nextDialogueEvent.RemoveListener(OnNextDialogue);

        // 取消订阅对话结束事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd.RemoveListener(OnDialogueEnd);
        }
    }

    // 每帧调用一次
    void Update()
    {

    }

    #endregion

    #region 事件处理器

    // 物品放入插槽时的事件处理器
    private void OnItemPlacedInSocket(SelectEnterEventArgs args)
    {
        var socket = args.interactorObject as XRSocketInteractor;
        if (socket != null)
        {
            Debug.Log($"物品放入插槽: {GetInteractableName(args.interactableObject)}");
            LogAllSocketContents();

            // 物品放入时检查阶段条件
            CheckStageConditions();

            // 更新提示显示
            UpdatePromptDisplay();
        }
    }

    // 物品从插槽移除时的事件处理器
    private void OnItemRemovedFromSocket(SelectExitEventArgs args)
    {
        var socket = args.interactorObject as XRSocketInteractor;
        if (socket != null)
        {
            Debug.Log($"物品从插槽移除: {GetInteractableName(args.interactableObject)}");
            LogAllSocketContents();

            // 物品移除时检查阶段条件
            CheckStageConditions();

            // 更新提示显示
            UpdatePromptDisplay();
        }
    }

    #endregion

    #region 辅助方法

    // 获取插槽中所有物品名称的辅助方法
    private List<string> GetSocketItemNames(XRSocketInteractor socket)
    {
        var itemNames = new List<string>();
        if (socket != null && socket.hasSelection)
        {
            foreach (var interactable in socket.interactablesSelected)
            {
                itemNames.Add(GetInteractableName(interactable));
            }
        }
        return itemNames;
    }

    // 获取可交互对象名称的辅助方法
    private string GetInteractableName(IXRInteractable interactable)
    {
        if (interactable != null && interactable.transform != null)
        {
            return interactable.transform.gameObject.name;
        }
        return "未知物品";
    }

    // 记录所有插槽内容的方法
    private void LogAllSocketContents()
    {
        if (sockets == null || sockets.Count == 0)
        {
            Debug.Log("没有可用的插槽");
            return;
        }

        Debug.Log("=== 当前插槽内容 ===");

        int socketIndex = 0;
        foreach (var socket in sockets)
        {
            socketIndex++;
            if (socket == null)
            {
                Debug.Log($"插槽 {socketIndex}: [空插槽]");
                continue;
            }

            var currentItems = GetSocketItemNames(socket);
            if (currentItems.Count > 0)
            {
                Debug.Log($"插槽 {socketIndex}: {string.Join(", ", currentItems)}");
            }
            else
            {
                Debug.Log($"插槽 {socketIndex}: [空]");
            }
        }
        Debug.Log("==============================");
    }

    #endregion

    #region 阶段管理

    // 根据当前阶段检查阶段条件的方法
    private void CheckStageConditions()
    {
        switch (currentStage)
        {
            case QuestStage.Stage1:
                CheckStage1Conditions();
                break;
            case QuestStage.Stage2:
                CheckStage2Conditions();
                break;
            case QuestStage.Stage3:
                CheckStage3Conditions();
                break;
        }
    }

    // 检查阶段1条件的方法
    private void CheckStage1Conditions()
    {
        // 获取所有插槽中的当前物品名称
        var allCurrentItems = GetAllCurrentItemNames();

        // 检查是否所有必需物品都存在（包含检查，不是精确匹配）
        bool hasItem1 = false;
        bool hasItem2 = false;
        bool hasItem3 = false;

        foreach (string itemName in allCurrentItems)
        {
            if (itemName.Contains("Item1"))
                hasItem1 = true;
            if (itemName.Contains("Item2"))
                hasItem2 = true;
            if (itemName.Contains("Item3"))
                hasItem3 = true;
        }

        // 如果三个物品都存在，调用成功方法
        if (hasItem1 && hasItem2 && hasItem3)
        {
            Hahaha();
        }
    }

    // 检查阶段2条件的方法
    private void CheckStage2Conditions()
    {
        // 获取所有插槽中的当前物品名称
        var allCurrentItems = GetAllCurrentItemNames();

        // 检查Item3是否存在
        bool hasItem3 = false;

        foreach (string itemName in allCurrentItems)
        {
            if (itemName.Contains("Item3"))
                hasItem3 = true;
        }

        // 如果Item3不存在（已被移除），调用成功方法
        if (!hasItem3)
        {
            Debug.Log("阶段2完成: Item3已从插槽中移除!");
            HahahaStage2();
        }
    }

    // 检查阶段3条件的方法
    private void CheckStage3Conditions()
    {
        // 获取所有插槽中的当前物品名称
        var allCurrentItems = GetAllCurrentItemNames();

        // 检查Item4是否存在
        bool hasItem4 = false;

        foreach (string itemName in allCurrentItems)
        {
            if (itemName.Contains("Item4"))
                hasItem4 = true;
        }

        // 如果Item4存在，调用成功方法
        if (hasItem4)
        {
            Debug.Log("阶段3完成: Item4已放入插槽!");
            HahahaStage3();
        }
    }

    // 获取所有插槽中当前物品名称的辅助方法
    private List<string> GetAllCurrentItemNames()
    {
        var allItems = new List<string>();

        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket != null)
                {
                    var socketItems = GetSocketItemNames(socket);
                    allItems.AddRange(socketItems);
                }
            }
        }

        return allItems;
    }

    // 阶段1条件满足时调用的成功方法
    private void Hahaha()
    {
        Debug.Log("哈哈哈");

        // 播放阶段完成音效
        PlayUICompletelySound();

        // 禁用插槽中Item1和Item2的抓取交互
        DisableItemGrabInteractionInSockets("Item1");
        DisableItemGrabInteractionInSockets("Item2");

        StartCoroutine(SwitchToStage2AfterDelay());
    }

    // 阶段2条件满足时调用的成功方法
    private void HahahaStage2()
    {
        Debug.Log("哈哈哈");

        // 播放阶段完成音效
        PlayUICompletelySound();

        // 立即更新提示显示为1/1
        UpdatePromptDisplay();

        // 打开第八句对白，并设置按钮为下一步
        StartCoroutine(OpenDialogue8AfterDelay());

        StartCoroutine(SwitchToStage3AfterDelay());
    }

    // 阶段3条件满足时调用的成功方法
    private void HahahaStage3()
    {
        Debug.Log("哈哈哈");

        // 播放阶段完成音效
        PlayUICompletelySound();

        // 打开第16条对话，并设置按钮为关闭模式
        StartCoroutine(OpenDialogue16AfterDelay());

        StartCoroutine(SwitchToStageCompleteAfterDelay());
    }

    // 禁用插槽中特定名称物品抓取交互的辅助方法
    private void DisableItemGrabInteractionInSockets(string itemName)
    {
        if (sockets != null)
        {
            foreach (var socket in sockets)
            {
                if (socket != null && socket.hasSelection)
                {
                    foreach (var interactable in socket.interactablesSelected)
                    {
                        if (interactable != null && interactable.transform != null)
                        {
                            GameObject obj = interactable.transform.gameObject;
                            if (obj.name.Contains(itemName))
                            {
                                // 为此对象及其所有子对象设置Unity层级为4
                                SetLayersForObjectAndChildren(obj, 4);
                                Debug.Log($"通过设置Unity层级为4禁用了 {obj.name} 及其所有子对象的抓取交互");
                            }
                        }
                    }
                }
            }
        }
    }

    // 为对象及其所有子对象设置Unity层级的辅助方法
    private void SetLayersForObjectAndChildren(GameObject obj, int layerValue)
    {
        // 为主对象设置层级
        obj.layer = layerValue;

        // 为所有子对象设置层级
        Transform[] children = obj.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            child.gameObject.layer = layerValue;
        }
    }

    // 等待3秒后切换到阶段2的协程
    private IEnumerator SwitchToStage2AfterDelay()
    {
        yield return new WaitForSeconds(3f);
        currentStage = QuestStage.Stage2;
        Debug.Log("已切换到阶段2");

        // 切换阶段后更新提示显示
        UpdatePromptDisplay();
    }

    // 等待3秒后切换到阶段3的协程
    private IEnumerator SwitchToStage3AfterDelay()
    {
        yield return new WaitForSeconds(3f);
        currentStage = QuestStage.Stage3;
        Debug.Log("已切换到阶段3");

        // 切换阶段后更新提示显示
        UpdatePromptDisplay();
    }

    // 等待3秒后切换到阶段完成的协程
    private IEnumerator SwitchToStageCompleteAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("所有阶段完成!");

        // 显示完成消息
        if (promptManager != null)
        {
            promptManager.SetPromptText("所有已经完成");
            Debug.Log("显示完成消息: 都已经完成");
        }
    }

    #endregion

    #region 提示显示管理

    // 更新提示显示的方法
    private void UpdatePromptDisplay()
    {
        if (promptManager == null)
        {
            Debug.LogWarning("PromptManager未分配，无法更新提示显示");
            return;
        }

        string newProgressText = "";

        switch (currentStage)
        {
            case QuestStage.Stage1:
                newProgressText = UpdateStage1Prompt();
                break;
            case QuestStage.Stage2:
                newProgressText = UpdateStage2Prompt();
                break;
            case QuestStage.Stage3:
                newProgressText = UpdateStage3Prompt();
                break;
        }

        // 检查进度是否发生变化
        if (!string.IsNullOrEmpty(newProgressText) && newProgressText != lastProgressText)
        {
            PlayUIChangeSound();
            lastProgressText = newProgressText;
        }
    }

    // 更新阶段1的提示显示
    private string UpdateStage1Prompt()
    {
        var allCurrentItems = GetAllCurrentItemNames();

        // 计算当前收集的进度
        int collectedCount = 0;
        foreach (string itemName in allCurrentItems)
        {
            if (itemName.Contains("Item1") || itemName.Contains("Item2") || itemName.Contains("Item3"))
                collectedCount++;
        }

        string promptText = $"收集厨房提示卡  {collectedCount}/3";
        promptManager.SetPromptText(promptText);
        return promptText;
    }

    // 更新阶段2的提示显示
    private string UpdateStage2Prompt()
    {
        var allCurrentItems = GetAllCurrentItemNames();

        // 检查Item3是否还在插槽中
        bool hasItem3 = false;
        foreach (string itemName in allCurrentItems)
        {
            if (itemName.Contains("Item3"))
                hasItem3 = true;
        }

        // 如果Item3还在插槽中，显示0/1，如果已移除，显示1/1
        int removedCount = hasItem3 ? 0 : 1;
        string promptText = $"拿出不参加发布会的人的卡片  {removedCount}/1";
        promptManager.SetPromptText(promptText);
        return promptText;
    }

    // 更新阶段3的提示显示
    private string UpdateStage3Prompt()
    {
        var allCurrentItems = GetAllCurrentItemNames();

        // 检查Item4是否在插槽中
        bool hasItem4 = false;
        foreach (string itemName in allCurrentItems)
        {
            if (itemName.Contains("Item4"))
                hasItem4 = true;
        }

        // 如果Item4在插槽中，显示1/1，否则显示0/1
        int placedCount = hasItem4 ? 1 : 0;
        string promptText = $"放入1个奖章{placedCount}/1";
        promptManager.SetPromptText(promptText);
        return promptText;
    }

    #endregion

    #region Public Methods

    // 记录插槽中所有物品名称的方法
    [Button("记录插槽中的物品")]
    public void LogItemsInSockets()
    {
        if (sockets == null || sockets.Count == 0)
        {
            Debug.Log("没有可用的插槽或插槽列表为空");
            return;
        }

        foreach (var socket in sockets)
        {
            if (socket == null)
            {
                Debug.LogWarning("在插槽列表中发现空插槽");
                continue;
            }

            if (socket.hasSelection)
            {
                // 获取此插槽中所有选中的可交互对象
                foreach (var interactable in socket.interactablesSelected)
                {
                    if (interactable != null && interactable.transform != null)
                    {
                        string itemName = interactable.transform.gameObject.name;
                        Debug.Log($"插槽中有物品: {itemName}");
                    }
                    else
                    {
                        Debug.LogWarning("在选中物品中发现空的可交互对象或变换");
                    }
                }
            }
            else
            {
                Debug.Log("插槽为空");
            }
        }
    }

    /// <summary>
    /// 检查第三个阶段的任务是否完成
    /// </summary>
    [Button("检查第三阶段是否完成")]
    public void CheckStage3Completion()
    {
        // 获取所有插槽中的当前物品名称
        var allCurrentItems = GetAllCurrentItemNames();

        // 检查阶段3条件：Item4存在
        bool hasItem4 = false;
        foreach (string itemName in allCurrentItems)
        {
            if (itemName.Contains("Item4"))
                hasItem4 = true;
        }

        // 输出结果
        if (hasItem4)
        {
            Debug.Log("都完成");
            kitchenUnlockUI.SetActive(true);
            PlayGameEndSound();
            StartCoroutine(RestartGameAfterDelay());
        }
        else
        {
            Debug.Log("没有完成");
        }
    }

    #endregion

    #region 对话管理

    /// <summary>
    /// 处理下一句对话事件
    /// </summary>
    /// <param name="dialogueNumber">对话序号</param>
    private void OnNextDialogue(int dialogueNumber)
    {
        Debug.Log($"NewTrayQuest: 收到对话事件，对话序号: {dialogueNumber}");

        // 更新当前对话编号
        currentDialogueNumber = dialogueNumber;
    }

    /// <summary>
    /// 处理对话结束事件
    /// </summary>
    private void OnDialogueEnd()
    {
        // 检查当前对话是否是第八号对话
        if (currentDialogueNumber == 8)
        {
            Debug.Log("当前对话是第八号对话，对话结束");
            // 可以在这里添加对话结束后的逻辑
        }

        // 重置对话编号
        currentDialogueNumber = -1;
    }

    /// <summary>
    /// 延迟打开第八句对白的协程
    /// </summary>
    private IEnumerator OpenDialogue8AfterDelay()
    {
        yield return new WaitForSeconds(1f);

        // 触发第八个对话条目
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.GoToDialogue(8);
            // 设置按钮为"下一步"模式（false表示下一句对话模式）
            DialogueManager.Instance.SetNextButtonMode(false);
            shadowLine.SetActive(true);
            blocks.ForEach(block => block.SetActive(true));
            if (audioSource != null && floorOutSound != null)
            {
                audioSource.PlayOneShot(floorOutSound);
                Debug.Log("Playing floor out  sound for stage completion");
            }
            Debug.Log("正在显示第八句对话内容");
        }
        else
        {
            Debug.LogWarning("DialogueManager实例未找到");
        }
    }

    /// <summary>
    /// 延迟打开第16句对白的协程
    /// </summary>
    private IEnumerator OpenDialogue16AfterDelay()
    {
        yield return new WaitForSeconds(1f);

        // 触发第16个对话条目
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.GoToDialogue(16);
            // 设置按钮为"关闭"模式（true表示关闭模式）
            DialogueManager.Instance.SetNextButtonMode(true);
            Debug.Log("正在显示第16句对话内容，按钮模式设置为关闭");
        }
        else
        {
            Debug.LogWarning("DialogueManager实例未找到");
        }
    }

    #endregion

    #region 音效管理

    /// <summary>
    /// 播放任务进度变化音效
    /// </summary>
    private void PlayUIChangeSound()
    {
        // 检查是否已经过了延迟时间
        if (Time.time - scriptStartTime < AUDIO_DELAY_TIME)
        {
            Debug.Log($"音效播放被阻止：脚本运行时间不足 {AUDIO_DELAY_TIME} 秒");
            return;
        }

        if (audioSource != null && UIChange != null)
        {
            audioSource.PlayOneShot(UIChange);
            Debug.Log("Playing UI change sound for quest progress update");
        }
        else
        {
            Debug.LogWarning("Cannot play UI change sound: AudioSource or UIChange AudioClip is missing");
        }
    }

    /// <summary>
    /// 播放阶段完全完成音效
    /// </summary>
    private void PlayUICompletelySound()
    {
        // 检查是否已经过了延迟时间
        if (Time.time - scriptStartTime < AUDIO_DELAY_TIME)
        {
            Debug.Log($"音效播放被阻止：脚本运行时间不足 {AUDIO_DELAY_TIME} 秒");
            return;
        }

        if (audioSource != null && UICompletely != null)
        {
            audioSource.PlayOneShot(UICompletely);
            Debug.Log("Playing UI completely sound for stage completion");
        }
        else
        {
            Debug.LogWarning("Cannot play UI completely sound: AudioSource or UICompletely AudioClip is missing");
        }
    }

    /// <summary>
    /// 播放厨房UI显示音效
    /// </summary>
    private void PlayKitchenUIShowSound()
    {
        // 检查是否已经过了延迟时间
        if (Time.time - scriptStartTime < AUDIO_DELAY_TIME)
        {
            Debug.Log($"音效播放被阻止：脚本运行时间不足 {AUDIO_DELAY_TIME} 秒");
            return;
        }

        if (audioSource != null && kitchenUIShow != null)
        {
            audioSource.PlayOneShot(kitchenUIShow);
            Debug.Log("Playing kitchen UI show sound");
        }
        else
        {
            Debug.LogWarning("Cannot play kitchen UI show sound: AudioSource or kitchenUIShow AudioClip is missing");
        }
    }

    /// <summary>
    /// 播放游戏结束音效
    /// </summary>
    private void PlayGameEndSound()
    {
        // 检查是否已经过了延迟时间
        if (Time.time - scriptStartTime < AUDIO_DELAY_TIME)
        {
            Debug.Log($"音效播放被阻止：脚本运行时间不足 {AUDIO_DELAY_TIME} 秒");
            return;
        }

        if (audioSource != null && gameEnd != null)
        {
            audioSource.PlayOneShot(gameEnd);
            Debug.Log("Playing game end sound");
        }
        else
        {
            Debug.LogWarning("Cannot play game end sound: AudioSource or gameEnd AudioClip is missing");
        }
    }

    /// <summary>
    /// 延迟5秒后重新开始游戏的协程
    /// </summary>
    private IEnumerator RestartGameAfterDelay()
    {
        yield return new WaitForSeconds(10f);

        Debug.Log("10秒后重新开始游戏，回到第一个场景");

        // 使用Unity的SceneManager加载第一个场景（索引为0的场景）
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    [Button]
    public void ComploetelyStage3()
    {
        Debug.Log("强制完成第三阶段任务");

        // 显示厨房解锁UI
        if (kitchenUnlockUI != null)
        {
            kitchenUnlockUI.SetActive(true);
            Debug.Log("显示厨房解锁UI");
        }
        else
        {
            Debug.LogWarning("厨房解锁UI对象未分配");
        }

        // 播放游戏结束音效
        PlayGameEndSound();

        // 启动重新开始游戏的协程
        StartCoroutine(RestartGameAfterDelay());
    }

    #endregion
}
