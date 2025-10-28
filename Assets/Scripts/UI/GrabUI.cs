using UnityEngine;

public class GrabUI : MonoBehaviour
{
    [Header("呼吸效果参数")]
    [Tooltip("呼吸效果的缩放速度")]
    public float breathSpeed = 2.0f;
    
    [Tooltip("最小缩放比例")]
    public float minScale = 0.8f;
    
    [Tooltip("最大缩放比例")]
    public float maxScale = 1.2f;
    
    [Tooltip("是否启用呼吸效果")]
    public bool enableBreathing = true;
    
    [Header("抓取检测参数")]
    [Tooltip("等待时间（秒）后开始检测抓取键")]
    public float waitTime = 2.0f;
    
    [Tooltip("是否启用抓取检测")]
    public bool enableGrabDetection = true;
    
    private Vector3 originalScale;
    private float time;
    private float elapsedTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 保存原始缩放值
        originalScale = transform.localScale;
        time = 0f;
        elapsedTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // 更新经过的时间
        elapsedTime += Time.deltaTime;
        
        // 呼吸效果
        if (enableBreathing)
        {
            // 更新时间
            time += Time.deltaTime * breathSpeed;
            
            // 使用正弦函数计算缩放因子 (范围从-1到1)
            float scaleFactor = Mathf.Sin(time);
            
            // 将缩放因子从[-1,1]映射到[minScale, maxScale]
            float normalizedScale = (scaleFactor + 1f) / 2f; // 映射到[0,1]
            float targetScale = Mathf.Lerp(minScale, maxScale, normalizedScale);
            
            // 应用缩放
            transform.localScale = originalScale * targetScale;
        }
        
        // 抓取检测
        if (enableGrabDetection && elapsedTime >= waitTime)
        {
            CheckGrabInput();
        }
    }
    
    /// <summary>
    /// 检查手柄的抓取键是否被按下
    /// </summary>
    private void CheckGrabInput()
    {
        // 检查右手控制器的抓取键（gripButton）
        bool rightHandGrab = false;
        var rightHandDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
        if (rightHandDevice.isValid)
        {
            rightHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out rightHandGrab);
        }
        
        // 检查左手控制器的抓取键（gripButton）
        bool leftHandGrab = false;
        var leftHandDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
        if (leftHandDevice.isValid)
        {
            leftHandDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out leftHandGrab);
        }
        
        // 如果任意一只手的抓取键被按下，销毁当前UI
        if (rightHandGrab || leftHandGrab)
        {
            Destroy(gameObject);
            Debug.Log("GrabUI: 抓取键被按下，销毁UI");
        }
    }
    
    // 公共方法：启用/禁用呼吸效果
    public void SetBreathingEnabled(bool enabled)
    {
        enableBreathing = enabled;
        
        // 如果禁用，恢复原始大小
        if (!enabled)
        {
            transform.localScale = originalScale;
        }
    }
    
    // 公共方法：设置呼吸速度
    public void SetBreathSpeed(float speed)
    {
        breathSpeed = Mathf.Max(0.1f, speed); // 确保速度不为负或零
    }
    
    // 公共方法：启用/禁用抓取检测
    public void SetGrabDetectionEnabled(bool enabled)
    {
        enableGrabDetection = enabled;
    }
    
    // 公共方法：设置等待时间
    public void SetWaitTime(float time)
    {
        waitTime = Mathf.Max(0f, time); // 确保时间不为负
    }
}
