using UnityEngine;

public class ToolBoxMove : MonoBehaviour
{
    public Transform followTarget;  // 要跟随的目标
    public Vector3 offset;          // 位置偏移
    public Transform lookTarget;    // 要看向的目标
    public float moveSpeed = 5f;    // 移动速度，用于插值
    
    private Vector3 targetPosition; // 目标位置
    
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
}
