using UnityEngine;

/// <summary>
/// 狐狸角色控制脚本
/// 负责处理狐狸形态下的输入控制和工具箱操作
/// </summary>
public class Fox : MonoBehaviour
{
    [Header("工具箱设置")]
    [SerializeField] private ToolBoxMove _toolBoxMove;        // 工具箱移动脚本引用
    
    [Header("按键启用设置")]
    [SerializeField] private bool _canOpenToolBox = false;   // 是否允许打开工具箱
    
    [Header("冷却时间设置")]
    [SerializeField] private float _cooldown = 1f;          // 操作冷却时间（秒）
    private float _cooldownTimer = 0f;                      // 冷却计时器

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 固定时间间隔更新，用于处理物理相关的逻辑
    /// 每帧调用一次，处理玩家输入和工具箱操作
    /// </summary>
    void FixedUpdate()
    {
        // 更新冷却计时器
        _cooldownTimer += Time.deltaTime;

        // 如果还在冷却中，直接返回
        if (_cooldownTimer < _cooldown) return;

        // 检查左手控制器的Menu按钮是否按下（用于工具箱切换）
        if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand)
            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool isPressedLeftMenu) && isPressedLeftMenu
            && _canOpenToolBox)  // 只有在允许打开工具箱时才执行
        {
            // 使用ToolBoxMove脚本切换工具箱状态
            if (_toolBoxMove != null)
            {
                _toolBoxMove.ToggleState();
                _cooldownTimer = 0f;              // 重置冷却计时器
                Debug.Log("Fox: 工具箱状态已切换");
            }
            else
            {
                Debug.LogWarning("Fox: ToolBoxMove脚本未设置，无法切换工具箱状态");
            }
        }
    }

    /// <summary>
    /// 设置是否允许打开工具箱
    /// </summary>
    /// <param name="canOpen">是否允许打开工具箱</param>
    public void SetCanOpenToolBox(bool canOpen)
    {
        _canOpenToolBox = canOpen;
    }

    /// <summary>
    /// 获取当前是否允许打开工具箱
    /// </summary>
    /// <returns>是否允许打开工具箱</returns>
    public bool GetCanOpenToolBox()
    {
        return _canOpenToolBox;
    }

    /// <summary>
    /// 设置工具箱移动脚本引用
    /// </summary>
    /// <param name="toolBoxMove">工具箱移动脚本</param>
    public void SetToolBoxMove(ToolBoxMove toolBoxMove)
    {
        _toolBoxMove = toolBoxMove;
    }
}
