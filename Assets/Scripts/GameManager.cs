using UnityEngine;

/// <summary>
/// 游戏管理器类，负责管理游戏的核心逻辑和状态
/// 使用单例模式确保全局只有一个实例
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("玩家角色设置")]
    [SerializeField] private GameObject _largePlayer;      // 大型玩家角色（人类形态）
    [SerializeField] public GameObject FoxPrefab;          // 狐狸预制体（可能用于特效或动画）
    [SerializeField] private GameObject _smallPlayer;      // 小型玩家角色（动物形态）

    [Header("冷却时间设置")]
    [SerializeField] private float _cooldown = 1f;         // 操作冷却时间（秒）
    private float _cooldownTimer = 0f;                     // 冷却计时器

    [Header("记忆碎片收集系统")]
    [SerializeField] private int _totalMemoryShards = 5;   // 总共需要收集的记忆碎片数量
    private int _currentMemoryShards = 0;                  // 当前已收集的记忆碎片数量

    [Header("单例实例")]
    public static GameManager Instance;                    // 单例实例，用于全局访问

    [Header("工具箱设置")]
    public GameObject ToolBox;                             // 工具箱游戏对象

    [Header("按键启用设置")]
    public bool canChangePerson = false;
    public bool canOpenToolBox = false;

    /// <summary>
    /// 游戏开始时调用，初始化单例实例
    /// </summary>
    void Start()
    {
        Instance = this;  // 设置单例实例为当前对象
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
            if (_largePlayer.activeSelf)
            {
                _largePlayer.SetActive(false);    // 禁用大型玩家
                FoxPrefab.SetActive(true);        // 启用狐狸预制体
                _smallPlayer.SetActive(true);     // 启用小型玩家
                _cooldownTimer = 0f;              // 重置冷却计时器
            }
            else
            {
                // 大型玩家模式（人类形态）
                _largePlayer.SetActive(true);     // 启用大型玩家
                FoxPrefab.SetActive(false);       // 禁用狐狸预制体
                _smallPlayer.SetActive(false);    // 禁用小型玩家
                _cooldownTimer = 0f;              // 重置冷却计时器
            }
        }

        // 检查右手控制器的扳机按钮是否按下（用于UI画布切换）
        if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand)
            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isPressedRight) && isPressedRight)
        {
            if (_largePlayer.activeSelf)
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

        // 检查左手控制器的Menu按钮是否按下（用于工具箱切换）
        if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand)
            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool isPressedLeftMenu) && isPressedLeftMenu
            && canOpenToolBox)  // 只有在允许打开工具箱时才执行
        {
            if (ToolBox.activeSelf)
            {
                // 如果工具箱已激活，则隐藏工具箱
                ToolBox.SetActive(false);
                _cooldownTimer = 0f;              // 重置冷却计时器
            }
            else
            {
                // 如果工具箱未激活，则显示工具箱
                ToolBox.SetActive(true);
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
