using UnityEngine;

public class ToolBoxMove : MonoBehaviour
{
    // 状态枚举
    public enum ToolBoxState
    {
        Following,  // 跟随状态
        Hidden      // 隐藏状态
    }
    
    public Transform followTarget;  // 要跟随的目标
    public Vector3 offset;          // 位置偏移
    public Transform lookTarget;    // 要看向的目标
    public float moveSpeed = 5f;    // 移动速度，用于插值
    public Vector3 hiddenPosition = new Vector3(1000f, 1000f, 1000f); // 隐藏位置（很远的位置）
    
    private Vector3 targetPosition; // 目标位置
    private ToolBoxState currentState = ToolBoxState.Hidden; // 当前状态，默认为隐藏状态
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (followTarget != null)
        {
            // 初始化位置
            targetPosition = followTarget.position + offset;
            transform.position = targetPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case ToolBoxState.Following:
                UpdateFollowingState();
                break;
            case ToolBoxState.Hidden:
                // 隐藏状态下不需要更新位置，保持在远处
                break;
        }
    }
    
    // 跟随状态的更新逻辑
    private void UpdateFollowingState()
    {
        if (followTarget != null)
        {
            // 计算目标位置（跟随目标位置 + 偏移）
            targetPosition = followTarget.position + offset;
            
            // 使用插值平滑移动到目标位置
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            
            // 如果有看向目标，则朝向该目标
            if (lookTarget != null)
            {
                transform.LookAt(lookTarget);
            }
        }
    }
    
    // 切换到跟随状态
    public void SetFollowingState()
    {
        if (currentState != ToolBoxState.Following)
        {
            currentState = ToolBoxState.Following;
            // 如果之前是隐藏状态，先移动到跟随目标的位置，然后继续跟随
            if (followTarget != null)
            {
                targetPosition = followTarget.position + offset;
                transform.position = targetPosition;
            }
        }
    }
    
    // 切换到隐藏状态
    public void SetHiddenState()
    {
        if (currentState != ToolBoxState.Hidden)
        {
            currentState = ToolBoxState.Hidden;
            // 直接设置到很远的位置，不使用插值
            transform.position = hiddenPosition;
        }
    }
    
    // 获取当前状态
    public ToolBoxState GetCurrentState()
    {
        return currentState;
    }
    
    // 切换状态（在跟随和隐藏之间切换）
    public void ToggleState()
    {
        if (currentState == ToolBoxState.Following)
        {
            SetHiddenState();
        }
        else
        {
            SetFollowingState();
        }
    }
}
