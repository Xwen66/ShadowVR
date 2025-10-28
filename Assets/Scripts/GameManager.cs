using UnityEngine;


/// <summary>
/// 游戏管理器类，负责管理游戏的核心逻辑和状态
/// 使用单例模式确保全局只有一个实例
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("玩家角色设置")]
    [SerializeField] private GameObject _largePlayerModel;      // 大型玩家角色（人类形态）
    [SerializeField] private GameObject _smallPlayerModel;      // 小型玩家角色（刺猬形态）
    [SerializeField] public GameObject FoxPrefab;          // 狐狸预制体（可能用于特效或动画）
    public Transform FoxLeftHand;                         // 狐狸左手位置
    public Transform FoxRightHand;                        // 狐狸右手位置
    [SerializeField] private GameObject _smallPlayer;      // 小型玩家角色（动物形态）

    [Header("冷却时间设置")]
    [SerializeField] private float _cooldown = 1f;         // 操作冷却时间（秒）
    private float _cooldownTimer = 0f;                     // 冷却计时器

    [Header("记忆碎片收集系统")]
    [SerializeField] private int _totalMemoryShards = 5;   // 总共需要收集的记忆碎片数量
    private int _currentMemoryShards = 0;                  // 当前已收集的记忆碎片数量

    [Header("单例实例")]
    public static GameManager Instance;                    // 单例实例，用于全局访问

    [Header("按键启用设置")]
    public bool canChangePerson = false;

    [Header("玩家状态")]
    public bool isLargePlayer = true;  // 记录当前是否是大型玩家状态（人类形态），true为人类形态，false为动物形态

    [Header("临时方法")]
    public DialogueMove dialogueMove;

    /// <summary>
    /// 游戏开始时调用，初始化单例实例
    /// </summary>
    void Start()
    {
        Instance = this;  // 设置单例实例为当前对象

        GlobalEvent.OnPressAUIEvent.AddListener(OnPressAUIEvent);
    }

    private void OnPressAUIEvent()
    {
        canChangePerson = true;
    }

    private void OnDestroy()
    {
        GlobalEvent.OnPressAUIEvent.RemoveListener(OnPressAUIEvent);
    }

    /// <summary>
    /// 固定时间间隔更新，用于处理物理相关的逻辑
    /// 每帧调用一次，处理玩家输入和状态切换
    /// </summary>
    void FixedUpdate()
    {
        // 更新冷却计时器
        _cooldownTimer += Time.deltaTime;

        // 如果还在冷却中，直接返回
        if (_cooldownTimer < _cooldown) return;

        // 检查右手控制器的A按钮是否按下（XR输入）或者空格键是否按下（键盘输入）
        if ((UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand)
            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool isPressed) && isPressed || Input.GetKeyDown(KeyCode.Space))
            && canChangePerson)  // 只有在允许切换形态时才执行
        {
            // 小型玩家模式（动物形态）
            if (_largePlayerModel.activeSelf)
            {
                _largePlayerModel.SetActive(false);    // 禁用大型玩家
                FoxPrefab.SetActive(true);        // 启用狐狸预制体
                _smallPlayer.SetActive(true);     // 启用小型玩家
                _smallPlayerModel.SetActive(false);     // 启用小型玩家
                isLargePlayer = false;            // 更新状态为非大型玩家（动物形态）
                _cooldownTimer = 0f;              // 重置冷却计时器
                Debug.LogError("GameManager: 触发角色切换事件，参数为true，代表刺猬模式");
                GlobalEvent.OnChangePersonEvent.Invoke(true);  // 触发角色切换事件，参数为true，代表刺猬模式

                // 主动设置DialogueMove的模式为狐狸模式（Mode2）
                if (dialogueMove != null)
                {
                    dialogueMove.SwitchToMode1();
                }
            }
            else
            {
                // 大型玩家模式（人类形态）
                _largePlayerModel.SetActive(true);     // 启用大型玩家
                FoxPrefab.SetActive(false);       // 禁用狐狸预制体
                _smallPlayer.SetActive(false);    // 禁用小型玩家
                _smallPlayerModel.SetActive(true);    // 禁用小型玩家
                isLargePlayer = true;             // 更新状态为大型玩家（人类形态）
                _cooldownTimer = 0f;              // 重置冷却计时器
                Debug.LogError("GameManager: 触发角色切换事件，参数为false，代表狐狸模式");
                GlobalEvent.OnChangePersonEvent.Invoke(false);  // 触发角色切换事件，参数为false，代表狐狸模式

                // 主动设置DialogueMove的模式为刺猬模式（Mode1）
                if (dialogueMove != null)
                {
                    dialogueMove.SwitchToMode2();
                }
            }
        }

        // 检查右手控制器的扳机按钮是否按下（用于UI画布切换）
        if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand)
            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isPressedRight) && isPressedRight)
        {
            if (_largePlayerModel.activeSelf)
            {
                // 大型玩家模式下切换UI画布到模式0
                UIManager.Instance.ToggleUICanvas(0);
                _cooldownTimer = 0f;              // 重置冷却计时器
            }
            else
            {
                // 小型玩家模式下切换UI画布到模式1
                UIManager.Instance.ToggleUICanvas(1);
                _cooldownTimer = 0f;              // 重置冷却计时器
            }
        }

    }

    /// <summary>
    /// 添加记忆碎片，更新收集进度
    /// </summary>
    public void AddMemoryShard()
    {
        _currentMemoryShards++;  // 增加当前记忆碎片数量
        UIManager.Instance.UpdateCurrentMemoryShards(_currentMemoryShards);  // 更新UI显示

        // 检查是否收集了所有记忆碎片
        if (_currentMemoryShards >= _totalMemoryShards)
        {
            // 游戏胜利逻辑（待实现）
            // 可以在这里添加胜利条件触发的代码
        }
    }

    /// <summary>
    /// 获取总共需要的记忆碎片数量
    /// </summary>
    public int TotalMemoryShards
    {
        get { return _totalMemoryShards; }
    }

    /// <summary>
    /// 获取当前已收集的记忆碎片数量
    /// </summary>
    public int CurrentMemoryShards
    {
        get { return _currentMemoryShards; }
    }
}
