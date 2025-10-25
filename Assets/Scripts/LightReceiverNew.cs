using UnityEngine;
using UnityEngine.Events;

public class LightReceiverNew : MonoBehaviour
{
    public Light light;
    
    [Header("Status")]
    public bool IsLit = false;
    
    [Tooltip("Event that when the light receiver is lit")]
    public UnityEvent OnLightLit;
    private bool isInvoked = false;
    
    [Header("Screen Effect")]
    public VRScreenEffect screenEffect;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 检查灯光是否为点光源
        if (light != null && light.type != LightType.Point)
        {
            Debug.LogWarning("指定的灯光不是点光源！");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 确保灯光存在且是点光源
        if (light != null && light.type == LightType.Point)
        {
            // 计算当前对象与灯光之间的距离
            float distance = Vector3.Distance(transform.position, light.transform.position);
            
            // 首先检查是否在范围内
            bool inRange = distance < light.range;
            
            // 如果在范围内，再进行射线检测
            bool wasLit = IsLit;
            IsLit = false;
            
            if (inRange)
            {
                // 进行射线检测
                IsLit = CheckLineOfSight();
            }
            
            if (IsLit)
            {
                Debug.Log("被照亮了！");
                
                // 确保只触发一次事件
                if (!isInvoked)
                {
                    // 触发光照事件
                    OnLightLit.Invoke();
                    
                    // 显示白光效果
                    if (screenEffect != null)
                    {
                        screenEffect.ShowInLightFlash();
                    }
                    
                    isInvoked = true;
                }
            }
            else
            {
                isInvoked = false;
            }
        }
    }
    
    // 检查光线是否能到达接收器
    bool CheckLineOfSight()
    {
        Vector3 lightPosition = light.transform.position;
        Vector3 receiverPosition = transform.position;
        Vector3 direction = (receiverPosition - lightPosition).normalized;
        float distance = Vector3.Distance(lightPosition, receiverPosition);
        
        // 发射射线
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(lightPosition, direction, out hit, distance);
        
        if (!hitSomething)
        {
            // 没有碰到任何东西，光线可以直接到达接收器
            return true;
        }
        else if (hit.collider.gameObject == gameObject)
        {
            // 射线碰到了接收器本身
            return true;
        }
        else
        {
            // 射线碰到了其他物体，被阻挡
            return false;
        }
    }
}
