using System;
using Unity.VisualScripting;
using UnityEngine;
using VInspector;

public class QuestUIMove : MonoBehaviour
{
    public enum UIMode
    {
        Mode1,//刺猬模式
        Mode2 //狐狸模式
    }

    [Header("Mode Selection")]
    public UIMode currentMode = UIMode.Mode1;

    public Transform uiTransform;

    [Header("Mode 1 Settings")]
    public Vector3 offset1;
    public Transform lookTarget1;
    public Transform followTarget1;
    public float Scale1;

    [Header("Position Settings Mode 1")]
    public float distanceFromTarget1 = 2f; // UI距离目标的水平距离
    public float yOffset1 = 1.5f; // UI的Y轴偏移量（相对于目标的高度）

    [Header("Mode 2 Settings")]
    public Vector3 offset2;
    public Transform lookTarget2;
    public Transform followTarget2;
    public float Scale2;

    [Header("Position Settings Mode 2")]
    public float distanceFromTarget2 = 2f; // UI距离目标的水平距离
    public float yOffset2 = 1.5f; // UI的Y轴偏移量（相对于目标的高度）



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
        // 根据当前模式获取相应的目标
        Transform currentFollowTarget = currentMode == UIMode.Mode1 ? followTarget1 : followTarget2;
        
        // 更新UI位置和朝向
        if (uiTransform != null && currentFollowTarget != null)
        {
            UpdateUIPositionAndRotation(uiTransform);
            UpdateUIScale();
        }
    }

    /// <summary>
    /// 更新UI的位置和旋转，使其始终在目标前方并朝向目标
    /// </summary>
    private void UpdateUIPositionAndRotation(Transform uiTransform)
    {
        // 根据当前模式获取相应的参数
        Transform currentFollowTarget = currentMode == UIMode.Mode1 ? followTarget1 : followTarget2;
        float currentDistance = currentMode == UIMode.Mode1 ? distanceFromTarget1 : distanceFromTarget2;
        float currentYOffset = currentMode == UIMode.Mode1 ? yOffset1 : yOffset2;

        // 获取目标的水平方向前向向量（忽略Y轴）
        Vector3 targetForward = currentFollowTarget.forward;
        targetForward.y = 0;
        targetForward.Normalize();

        // 计算目标位置：目标位置 + 水平前向方向 * 距离 + Y轴偏移
        Vector3 targetPosition = currentFollowTarget.position + targetForward * currentDistance;
        targetPosition.y = currentFollowTarget.position.y + currentYOffset; // Y轴跟随目标高度并加上偏移量

        // 使用平滑阻尼移动UI到目标位置
        uiTransform.position = Vector3.SmoothDamp(uiTransform.position, targetPosition, ref positionVelocity, positionSmoothTime);

        // 计算朝向目标的旋转（只考虑水平方向）
        Vector3 directionToTarget = currentFollowTarget.position - uiTransform.position;
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
    /// 更新UI的缩放
    /// </summary>
    private void UpdateUIScale()
    {
        float targetScale = currentMode == UIMode.Mode1 ? Scale1 : Scale2;
        uiTransform.localScale = Vector3.one * targetScale;
    }

    /// <summary>
    /// 瞬间将UI移动到目标位置，不使用插值
    /// </summary>
    public void TeleportToTarget()
    {
        // 根据当前模式获取相应的目标
        Transform currentFollowTarget = currentMode == UIMode.Mode1 ? followTarget1 : followTarget2;
        
        if (uiTransform != null && currentFollowTarget != null)
        {
            // 根据当前模式获取相应的参数
            float currentDistance = currentMode == UIMode.Mode1 ? distanceFromTarget1 : distanceFromTarget2;
            float currentYOffset = currentMode == UIMode.Mode1 ? yOffset1 : yOffset2;

            // 获取目标的水平方向前向向量（忽略Y轴）
            Vector3 targetForward = currentFollowTarget.forward;
            targetForward.y = 0;
            targetForward.Normalize();

            // 计算目标位置：目标位置 + 水平前向方向 * 距离 + Y轴偏移
            Vector3 targetPosition = currentFollowTarget.position + targetForward * currentDistance;
            targetPosition.y = currentFollowTarget.position.y + currentYOffset; // Y轴跟随目标高度并加上偏移量

            // 直接设置位置，不使用插值
            uiTransform.position = targetPosition;

            // 计算朝向目标的旋转（只考虑水平方向）
            Vector3 directionToTarget = currentFollowTarget.position - uiTransform.position;
            directionToTarget.y = 0; // 只考虑水平方向
            if (directionToTarget.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-directionToTarget);

                // 直接设置旋转，不使用插值
                uiTransform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
            }

            // 设置缩放
            UpdateUIScale();

            // 重置速度，避免下次插值时出现异常
            positionVelocity = Vector3.zero;
            rotationVelocity = 0f;
        }
    }

    /// <summary>
    /// 切换UI模式
    /// </summary>
    /// <param name="mode">要切换到的模式</param>
    public void SwitchMode(UIMode mode)
    {
        if (currentMode != mode)
        {
            currentMode = mode;
            TeleportToTarget(); // 切换模式后立即传送到新位置
        }
    }

    /// <summary>
    /// 切换到模式一
    /// </summary>
    public void SwitchToMode1()
    {
        SwitchMode(UIMode.Mode1);
    }

    /// <summary>
    /// 切换到模式二
    /// </summary>
    public void SwitchToMode2()
    {
        SwitchMode(UIMode.Mode2);
    }
}
