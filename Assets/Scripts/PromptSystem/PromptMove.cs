using UnityEngine;

public class PromptMove : MonoBehaviour
{
    [Header("位置设置")]
    [Tooltip("位置目标Transform")]
    public Transform positionTarget;
    
    [Tooltip("相对于目标位置的偏移量")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Header("朝向设置")]
    [Tooltip("朝向目标Transform")]
    public Transform rotationTarget;

    [Header("平滑追踪设置")]
    [Tooltip("平滑追踪的速度")]
    public float smoothSpeed = 5.0f;
    
    [Tooltip("是否启用平滑追踪模式")]
    public bool useSmoothTracking = false;
    
    // 跟随状态枚举
    public enum FollowState
    {
        Following,    // 跟随状态：使用插值平滑追踪
        NotFollowing  // 不跟随状态：直接设置位置，不使用偏移
    }
    
    // 当前跟随状态（默认为不跟随状态）
    private FollowState currentState = FollowState.NotFollowing;
    
    // 平滑追踪相关变量
    private bool shouldResetPosition = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 确保有必要的组件引用
        if (positionTarget == null)
        {
            Debug.LogWarning("PositionTarget 未设置！");
        }
        
        if (rotationTarget == null)
        {
            Debug.LogWarning("RotationTarget 未设置！");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 处理位置重置（在方法调用后的第一帧执行）
        if (shouldResetPosition && positionTarget != null)
        {
            // 立即将位置设置到目标位置（无偏移）
            transform.position = positionTarget.position;
            
            // 重置标志
            shouldResetPosition = false;
        }
        
        // 根据当前状态处理位置更新
        if (positionTarget != null)
        {
            if (currentState == FollowState.Following)
            {
                // 跟随状态：使用插值平滑追踪
                Vector3 targetPos = positionTarget.position + positionOffset;
                
                if (useSmoothTracking)
                {
                    // 持续使用Lerp平滑追踪目标位置
                    transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
                }
                else
                {
                    // 直接设置位置：等于positionTarget的位置 + 偏移量
                    transform.position = targetPos;
                }
            }
            else if (currentState == FollowState.NotFollowing)
            {
                // 不跟随状态：直接设置为目标位置（不使用偏移）
                transform.position = positionTarget.position;
            }
        }
        
        // 设置朝向：一直朝向rotationTarget
        if (rotationTarget != null)
        {
            transform.LookAt(rotationTarget.position);
        }
    }
    
    /// <summary>
    /// 重置位置到目标位置（无偏移）
    /// 在平滑追踪模式下，此方法会将物体立即设置到目标位置，然后继续平滑追踪
    /// </summary>
    [ContextMenu("重置位置到目标位置")]
    public void ResetToTargetPosition()
    {
        if (positionTarget == null)
        {
            Debug.LogWarning("PositionTarget 未设置，无法重置位置！");
            return;
        }
        
        // 设置标志，在下一帧Update中执行位置重置
        shouldResetPosition = true;
    }
    
    /// <summary>
    /// 设置平滑追踪模式
    /// </summary>
    /// <param name="enable">是否启用平滑追踪</param>
    public void SetSmoothTracking(bool enable)
    {
        useSmoothTracking = enable;
    }
    
    /// <summary>
    /// 切换平滑追踪模式
    /// </summary>
    [ContextMenu("切换平滑追踪模式")]
    public void ToggleSmoothTracking()
    {
        useSmoothTracking = !useSmoothTracking;
    }
    
    /// <summary>
    /// 进入跟随状态
    /// 在此状态下，物体会使用插值平滑追踪目标位置（根据useSmoothTracking设置）
    /// </summary>
    [ContextMenu("进入跟随状态")]
    public void EnterFollowingState()
    {
        currentState = FollowState.Following;
        Debug.Log("进入跟随状态：物体将使用插值平滑追踪目标位置");
    }
    
    /// <summary>
    /// 进入不跟随状态
    /// 在此状态下，物体会直接设置为目标位置（不使用偏移量），不进行插值跟随
    /// </summary>
    [ContextMenu("进入不跟随状态")]
    public void EnterNotFollowingState()
    {
        currentState = FollowState.NotFollowing;
        Debug.Log("进入不跟随状态：物体将直接设置为目标位置（不使用偏移量）");
    }
    
    /// <summary>
    /// 获取当前跟随状态
    /// </summary>
    /// <returns>当前的跟随状态</returns>
    public FollowState GetCurrentState()
    {
        return currentState;
    }
    
}
