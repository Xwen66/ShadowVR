using UnityEngine;

public class DialogueMove : MonoBehaviour
{
    public Transform targetTransform;
    [Header("Position Settings")]
    public float distanceFromTarget = 2f; // UI距离目标的水平距离
    public float yOffset = 1.5f; // UI的Y轴偏移量
    
    [Header("Smoothing Settings")]
    public float positionSmoothTime = 0.1f; // 位置平滑时间
    public float rotationSmoothTime = 0.1f; // 旋转平滑时间
    
    private Vector3 positionVelocity = Vector3.zero;
    private float rotationVelocity = 0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (targetTransform == null)
            return;
            
        UpdateUIPositionAndRotation();
    }
    
    /// <summary>
    /// 更新UI的位置和旋转，使其始终在目标前方并朝向目标
    /// </summary>
    private void UpdateUIPositionAndRotation()
    {
        // 获取目标的水平方向前向向量（忽略Y轴）
        Vector3 targetForward = targetTransform.forward;
        targetForward.y = 0;
        targetForward.Normalize();
        
        // 计算目标位置：目标位置 + 水平前向方向 * 距离 + Y轴偏移
        Vector3 targetPosition = targetTransform.position + targetForward * distanceFromTarget;
        targetPosition.y = yOffset; // 保持固定的Y轴位置
        
        // 使用平滑阻尼移动UI到目标位置
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, positionSmoothTime);
        
        // 计算朝向目标的旋转（只考虑水平方向）
        Vector3 directionToTarget = targetTransform.position - transform.position;
        directionToTarget.y = 0; // 只考虑水平方向
        if (directionToTarget.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-directionToTarget);
            
            // 使用平滑阻尼旋转UI朝向目标
            transform.rotation = Quaternion.Euler(0f,
                Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation.eulerAngles.y, ref rotationVelocity, rotationSmoothTime),
                0f);
        }
    }
    
    /// <summary>
    /// 设置目标Transform
    /// </summary>
    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }
    
    /// <summary>
    /// 设置UI距离目标的距离
    /// </summary>
    public void SetDistance(float distance)
    {
        distanceFromTarget = distance;
    }
    
    /// <summary>
    /// 设置UI的Y轴偏移量
    /// </summary>
    public void SetYOffset(float offset)
    {
        yOffset = offset;
    }
}
