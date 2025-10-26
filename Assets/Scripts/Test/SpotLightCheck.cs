using UnityEngine;
using UnityEngine.Events;

public class SpotLightCheck : MonoBehaviour
{
    public Light spotLight;
    
    [Header("Status")]
    public bool IsLit = false;
    
    [Tooltip("Event that when the light receiver is lit")]
    public UnityEvent OnLightLit;
    private bool isInvoked = false;
    
    [Header("Screen Effect")]
    public VRScreenEffect screenEffect;
    
    void Start()
    {
        // 确保灯光是聚光灯类型
        if (spotLight != null && spotLight.type != LightType.Spot)
        {
            Debug.LogWarning("指定的灯光不是聚光灯类型！");
        }
    }

    void Update()
    {
        // 确保灯光存在且是聚光灯
        if (spotLight != null && spotLight.type == LightType.Spot)
        {
            // 保存之前的状态
            bool wasLit = IsLit;
            IsLit = false;
            
            // 检查当前物体是否在聚光灯照射范围内
            IsLit = IsInSpotLightRange();
            
            if (IsLit)
            {
                Debug.Log("被聚光灯照亮了！");
                
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
    
    /// <summary>
    /// 检查当前物体是否在聚光灯照射范围内
    /// </summary>
    /// <returns>如果在范围内返回true，否则返回false</returns>
    bool IsInSpotLightRange()
    {
        // 获取聚光灯的位置和方向
        Vector3 lightPosition = spotLight.transform.position;
        Vector3 lightDirection = spotLight.transform.forward;
        
        // 计算从灯光到当前物体的方向
        Vector3 toObjectDirection = transform.position - lightPosition;
        float distanceToObject = toObjectDirection.magnitude;
        
        // 检查距离是否在聚光灯范围内
        if (distanceToObject > spotLight.range)
        {
            return false;
        }
        
        // 归一化方向向量
        toObjectDirection.Normalize();
        
        // 计算灯光方向与物体方向之间的角度
        float angle = Vector3.Angle(lightDirection, toObjectDirection);
        
        // 检查角度是否在聚光灯的聚光角度范围内
        if (angle > spotLight.spotAngle / 2f)
        {
            return false;
        }
        
        // 进行射线检测，检查是否有障碍物阻挡
        RaycastHit hit;
        if (Physics.Raycast(lightPosition, toObjectDirection, out hit, distanceToObject))
        {
            // 如果射线击中的不是当前物体，说明有障碍物阻挡
            if (hit.collider.gameObject != gameObject)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 在Scene视图中绘制辅助线，便于调试
    /// </summary>
    void OnDrawGizmos()
    {
        if (spotLight == null || spotLight.type != LightType.Spot)
            return;
            
        // 绘制从灯光到物体的连线
        Gizmos.color = IsInSpotLightRange() ? Color.green : Color.red;
        Gizmos.DrawLine(spotLight.transform.position, transform.position);
        
        // 绘制聚光灯范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spotLight.transform.position, spotLight.range);
    }
}
