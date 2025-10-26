using UnityEngine;
using UnityEngine.UI;

public class UITesttt : MonoBehaviour
{
    [SerializeField] private Image targetImage; // 目标图片引用
    [SerializeField] private float breathSpeed = 1.0f; // 呼吸速度，默认为1秒一个周期
    
    private float currentTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 如果没有设置图片，尝试从当前GameObject获取
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
        
        // 确保图片存在
        if (targetImage == null)
        {
            Debug.LogWarning("未找到目标图片组件，请在Inspector中设置或确保当前GameObject有Image组件");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetImage == null) return;
        
        // 更新时间
        currentTime += Time.deltaTime * breathSpeed;
        
        // 使用正弦函数计算透明度，范围从0到1
        float alpha = (Mathf.Sin(currentTime) + 1f) / 2f;
        
        // 获取当前颜色并更新透明度
        Color currentColor = targetImage.color;
        currentColor.a = alpha;
        targetImage.color = currentColor;
    }
}
