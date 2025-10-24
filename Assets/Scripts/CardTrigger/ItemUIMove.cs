using System;
using Unity.VisualScripting;
using UnityEngine;
using VInspector;

public class ItemUIMove : MonoBehaviour
{
    public Transform uiTransform;
    public Vector3 offset;
    public Transform lookTarget;
    public Transform followTarget;

    [Header("Position Settings")]
    public float distanceFromTarget = 2f; // UI距离目标的水平距离
    public float yOffset = 1.5f; // UI的Y轴偏移量（相对于目标的高度）

    [Header("Smoothing Settings")]
    public float positionSmoothTime = 0.1f; // 位置平滑时间
    public float rotationSmoothTime = 0.1f; // 旋转平滑时间

    private Vector3 positionVelocity = Vector3.zero;
    private float rotationVelocity = 0f;

    void Awake()
    {
        uiTransform = this.transform;
    }

    void OnEnable()
    {
        TeleportToTarget();
    }

    // Update is called once per frame
    void Update()
    {
        // 更新UI位置和朝向
        if (uiTransform != null && followTarget != null)
        {
            UpdateUIPositionAndRotation(uiTransform);
        }
    }

    /// <summary>
    /// 更新UI的位置和旋转，使其始终在目标前方并朝向目标
    /// </summary>
    private void UpdateUIPositionAndRotation(Transform uiTransform)
    {
        // 获取目标的水平方向前向向量（忽略Y轴）
        Vector3 targetForward = followTarget.forward;
        targetForward.y = 0;
        targetForward.Normalize();

        // 计算目标位置：目标位置 + 水平前向方向 * 距离 + Y轴偏移
        Vector3 targetPosition = followTarget.position + targetForward * distanceFromTarget;
        targetPosition.y = followTarget.position.y + yOffset; // Y轴跟随目标高度并加上偏移量

        // 使用平滑阻尼移动UI到目标位置
        uiTransform.position = Vector3.SmoothDamp(uiTransform.position, targetPosition, ref positionVelocity, positionSmoothTime);

        // 计算朝向目标的旋转（只考虑水平方向）
        Vector3 directionToTarget = followTarget.position - uiTransform.position;
        directionToTarget.y = 0; // 只考虑水平方向
        if (directionToTarget.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-directionToTarget);

            // 使用平滑阻尼旋转UI朝向目标
            uiTransform.rotation = Quaternion.Euler(0f,
                Mathf.SmoothDampAngle(uiTransform.eulerAngles.y, targetRotation.eulerAngles.y, ref rotationVelocity, rotationSmoothTime),
                0f);
        }
    }
    
    /// <summary>
    /// 瞬间将UI移动到目标位置，不使用插值
    /// </summary>
    /// 
    public void TeleportToTarget()
    {
        if (uiTransform != null && followTarget != null)
        {
            // 获取目标的水平方向前向向量（忽略Y轴）
            Vector3 targetForward = followTarget.forward;
            targetForward.y = 0;
            targetForward.Normalize();
            
            // 计算目标位置：目标位置 + 水平前向方向 * 距离 + Y轴偏移
            Vector3 targetPosition = followTarget.position + targetForward * distanceFromTarget;
            targetPosition.y = followTarget.position.y + yOffset; // Y轴跟随目标高度并加上偏移量
            
            // 直接设置位置，不使用插值
            uiTransform.position = targetPosition;
            
            // 计算朝向目标的旋转（只考虑水平方向）
            Vector3 directionToTarget = followTarget.position - uiTransform.position;
            directionToTarget.y = 0; // 只考虑水平方向
            if (directionToTarget.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-directionToTarget);
                
                // 直接设置旋转，不使用插值
                uiTransform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
            }
            
            // 重置速度，避免下次插值时出现异常
            positionVelocity = Vector3.zero;
            rotationVelocity = 0f;
        }
    }
}
