using UnityEngine;

public class DialogueMove : MonoBehaviour
{
    public enum UIMode
    {
        Mode1,//刺猬模式
        Mode2 //狐狸模式
    }

    [Header("Sound About")]
    public AudioSource audioSource;
    public AudioClip audioClipThisActive;
    public AudioClip audioClipNext;

    [Header("Mode Selection")]
    public UIMode currentMode = UIMode.Mode1;

    public Transform targetTransform;

    [Header("Mode 1 Settings")]
    public Transform targetTransform1;
    public float Scale1;

    [Header("Position Settings Mode 1")]
    public float distanceFromTarget1 = 2f; // UI距离目标的水平距离
    public float yOffset1 = 1.5f; // UI的Y轴偏移量（相对于目标的高度）

    [Header("Mode 2 Settings")]
    public Transform targetTransform2;
    public float Scale2;

    [Header("Position Settings Mode 2")]
    public float distanceFromTarget2 = 2f; // UI距离目标的水平距离
    public float yOffset2 = 1.5f; // UI的Y轴偏移量（相对于目标的高度）

    [Header("Smoothing Settings")]
    public float positionSmoothTime = 0.1f; // 位置平滑时间
    public float rotationSmoothTime = 0.1f; // 旋转平滑时间

    private Vector3 positionVelocity = Vector3.zero;
    private float rotationVelocity = 0f;
    private bool wasDialogueActive = false; // 用于跟踪对话状态变化

    void Awake()
    {
        // 初始化时传送到目标位置
        TeleportToTarget();
    }

    void OnEnable()
    {
        // 每次启用时传送到目标位置
        TeleportToTarget();
        
        // 订阅对话管理器事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart.AddListener(OnDialogueStart);
            DialogueManager.Instance.OnDialogueDisplay.AddListener(OnDialogueDisplay);
            DialogueManager.Instance.OnDialogueEnd.AddListener(OnDialogueEnd);
        }
    }

    void OnDisable()
    {
        // 取消订阅对话管理器事件
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart.RemoveListener(OnDialogueStart);
            DialogueManager.Instance.OnDialogueDisplay.RemoveListener(OnDialogueDisplay);
            DialogueManager.Instance.OnDialogueEnd.RemoveListener(OnDialogueEnd);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 根据当前模式获取相应的目标
        Transform currentTarget = currentMode == UIMode.Mode1 ? targetTransform1 : targetTransform2;

        // 更新UI位置和朝向
        if (currentTarget != null)
        {
            UpdateUIPositionAndRotation();
            UpdateUIScale();
        }
    }


    /// <summary>
    /// 更新UI的位置和旋转，使其始终在目标前方并朝向目标
    /// </summary>
    private void UpdateUIPositionAndRotation()
    {
        // 根据当前模式获取相应的参数
        Transform currentTarget = currentMode == UIMode.Mode1 ? targetTransform1 : targetTransform2;
        float currentDistance = currentMode == UIMode.Mode1 ? distanceFromTarget1 : distanceFromTarget2;
        float currentYOffset = currentMode == UIMode.Mode1 ? yOffset1 : yOffset2;

        if (currentTarget == null)
            return;

        // 获取目标的水平方向前向向量（忽略Y轴）
        Vector3 targetForward = currentTarget.forward;
        targetForward.y = 0;
        targetForward.Normalize();

        // 计算目标位置：目标位置 + 水平前向方向 * 距离 + Y轴偏移
        Vector3 targetPosition = currentTarget.position + targetForward * currentDistance;
        targetPosition.y = currentTarget.position.y + currentYOffset; // Y轴跟随目标高度并加上偏移量

        // 使用平滑阻尼移动UI到目标位置
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, positionSmoothTime);

        // 计算朝向目标的旋转（只考虑水平方向）
        Vector3 directionToTarget = currentTarget.position - transform.position;
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
    /// 更新UI的缩放
    /// </summary>
    private void UpdateUIScale()
    {
        float targetScale = currentMode == UIMode.Mode1 ? Scale1 : Scale2;
        transform.localScale = Vector3.one * targetScale;
    }

    /// <summary>
    /// 瞬间将UI移动到目标位置，不使用插值
    /// </summary>
    public void TeleportToTarget()
    {
        // 根据当前模式获取相应的目标
        Transform currentTarget = currentMode == UIMode.Mode1 ? targetTransform1 : targetTransform2;

        if (currentTarget != null)
        {
            // 根据当前模式获取相应的参数
            float currentDistance = currentMode == UIMode.Mode1 ? distanceFromTarget1 : distanceFromTarget2;
            float currentYOffset = currentMode == UIMode.Mode1 ? yOffset1 : yOffset2;

            // 获取目标的水平方向前向向量（忽略Y轴）
            Vector3 targetForward = currentTarget.forward;
            targetForward.y = 0;
            targetForward.Normalize();

            // 计算目标位置：目标位置 + 水平前向方向 * 距离 + Y轴偏移
            Vector3 targetPosition = currentTarget.position + targetForward * currentDistance;
            targetPosition.y = currentTarget.position.y + currentYOffset; // Y轴跟随目标高度并加上偏移量

            // 直接设置位置，不使用插值
            transform.position = targetPosition;

            // 计算朝向目标的旋转（只考虑水平方向）
            Vector3 directionToTarget = currentTarget.position - transform.position;
            directionToTarget.y = 0; // 只考虑水平方向
            if (directionToTarget.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-directionToTarget);

                // 直接设置旋转，不使用插值
                transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
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
            targetTransform = currentMode == UIMode.Mode1 ? targetTransform1 : targetTransform2;
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

    /// <summary>
    /// 设置目标Transform（兼容旧版本）
    /// </summary>
    public void SetTarget(Transform target)
    {
        targetTransform = target;
        // 同时更新当前模式对应的目标
        if (currentMode == UIMode.Mode1)
            targetTransform1 = target;
        else
            targetTransform2 = target;
    }

    /// <summary>
    /// 设置模式1的目标Transform
    /// </summary>
    public void SetTarget1(Transform target)
    {
        targetTransform1 = target;
        if (currentMode == UIMode.Mode1)
            targetTransform = target;
    }

    /// <summary>
    /// 设置模式2的目标Transform
    /// </summary>
    public void SetTarget2(Transform target)
    {
        targetTransform2 = target;
        if (currentMode == UIMode.Mode2)
            targetTransform = target;
    }

    /// <summary>
    /// 设置UI距离目标的距离（兼容旧版本）
    /// </summary>
    public void SetDistance(float distance)
    {
        if (currentMode == UIMode.Mode1)
            distanceFromTarget1 = distance;
        else
            distanceFromTarget2 = distance;
    }

    /// <summary>
    /// 设置UI的Y轴偏移量（兼容旧版本）
    /// </summary>
    public void SetYOffset(float offset)
    {
        if (currentMode == UIMode.Mode1)
            yOffset1 = offset;
        else
            yOffset2 = offset;
    }

    #region 音效事件处理

    /// <summary>
    /// 对话开始事件处理 - 播放激活音效
    /// </summary>
    private void OnDialogueStart(DialogueEntry dialogue)
    {
        PlayActiveSound();
    }

    /// <summary>
    /// 对话显示事件处理 - 播放下一句音效（仅在对话已激活时）
    /// </summary>
    private void OnDialogueDisplay(DialogueEntry dialogue)
    {
        // 只有在对话已经激活的情况下才播放下一句音效
        // 这样可以避免在对话开始时同时播放两个音效
        if (wasDialogueActive)
        {
            PlayNextSound();
        }
        wasDialogueActive = true;
    }

    /// <summary>
    /// 对话结束事件处理 - 重置状态
    /// </summary>
    private void OnDialogueEnd()
    {
        wasDialogueActive = false;
    }

    /// <summary>
    /// 播放UI激活音效
    /// </summary>
    private void PlayActiveSound()
    {
        if (audioSource != null && audioClipThisActive != null)
        {
            audioSource.PlayOneShot(audioClipThisActive);
            Debug.Log("Playing active sound for dialogue UI");
        }
        else
        {
            Debug.LogWarning("Cannot play active sound: AudioSource or AudioClip is missing");
        }
    }

    /// <summary>
    /// 播放下一句对话音效
    /// </summary>
    private void PlayNextSound()
    {
        if (audioSource != null && audioClipNext != null)
        {
            audioSource.PlayOneShot(audioClipNext);
            Debug.Log("Playing next sound for dialogue");
        }
        else
        {
            Debug.LogWarning("Cannot play next sound: AudioSource or AudioClip is missing");
        }
    }

    #endregion
}
