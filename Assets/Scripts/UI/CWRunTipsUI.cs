using System;
using UnityEngine;
using VInspector;

public class CWRunTipsUI : MonoBehaviour
{
    public Transform pressAUIChinese;
    public Transform pressAUIEnglish;
    public Vector3 offset;
    public Transform lookTarget;
    public Transform followTarget;
    
    [Header("Position Settings")]
    public float distanceFromTarget = 2f; // UI距离目标的水平距离
    public float yOffset = 1.5f; // UI的Y轴偏移量（相对于目标的高度）
    
    [Header("Smoothing Settings")]
    public float positionSmoothTime = 0.1f; // 位置平滑时间
    public float rotationSmoothTime = 0.1f; // 旋转平滑时间
    
    private bool useChinese = true; // 默认使用中文
    private bool shouldShowUI = false; // 控制UI是否显示
    
    private Vector3 positionVelocity = Vector3.zero;
    private float rotationVelocity = 0f;
    
    void Start()
    {
        // 监听语言切换事件
        GlobalEvent.OnLanguageChangeEvent.AddListener(OnLanguageChanged);
        GlobalEvent.OnChangePersonEvent.AddListener(OnChangePersonEvent);
        
        // 游戏开始时，确保两个UI都是不显示的
        if (pressAUIChinese != null)
            pressAUIChinese.gameObject.SetActive(false);
        if (pressAUIEnglish != null)
            pressAUIEnglish.gameObject.SetActive(false);
    }


    private void OnChangePersonEvent(bool showUI)
    {
        shouldShowUI = showUI;
        UpdateUIVisibility();
    }

    void OnDestroy()
    {
        // 移除事件监听，避免内存泄漏
        GlobalEvent.OnLanguageChangeEvent.RemoveListener(OnLanguageChanged);
        GlobalEvent.OnChangePersonEvent.RemoveListener(OnChangePersonEvent);
    }

    // Update is called once per frame
    void Update()
    {
        // 只更新当前激活的UI位置和朝向
        Transform activeUI = useChinese ? pressAUIChinese : pressAUIEnglish;
        if (activeUI != null && followTarget != null)
        {
            UpdateUIPositionAndRotation(activeUI);
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
    /// 语言切换事件处理函数
    /// </summary>
    /// <param name="isChinese">是否切换到中文</param>
    private void OnLanguageChanged(bool isChinese)
    {
        useChinese = isChinese;
        UpdateUIVisibility();
    }
    
    /// <summary>
    /// 更新UI显示状态
    /// </summary>
    private void UpdateUIVisibility()
    {
        if (pressAUIChinese != null)
            pressAUIChinese.gameObject.SetActive(shouldShowUI && useChinese);
            
        if (pressAUIEnglish != null)
            pressAUIEnglish.gameObject.SetActive(shouldShowUI && !useChinese);
    }










    //test 
    [Button]
    public void Test()
    {
        GlobalEvent.OnChangePersonEvent.Invoke(true);
    }
}
