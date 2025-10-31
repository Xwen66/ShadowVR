using UnityEngine;

public class RedShadow : MonoBehaviour
{
    [SerializeField]
    private float breathFrequency = 1.0f; // 呼吸频率（每秒呼吸次数）
    
    private Material material;
    private float maxAlpha;
    private float minAlpha = 0.1f; // 最小阿尔法值
    private float breathSpeed; // 呼吸速度（根据频率计算）
    private float currentAlpha;
    private bool increasing = true; // 控制阿尔法值是增加还是减少
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取物体的材质
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            // 记录当前材质的阿尔法值作为最大值
            maxAlpha = material.color.a;
            currentAlpha = maxAlpha;
            
            // 根据频率计算呼吸速度
            // 完整呼吸周期（从最大值到最小值再回到最大值）的时间 = 1 / frequency
            // 单程（从最大值到最小值或从最小值到最大值）的时间 = (1 / frequency) / 2
            // 速度 = 阿尔法值变化范围 / 单程时间
            breathSpeed = (maxAlpha - minAlpha) * breathFrequency * 2;
        }
        else
        {
            Debug.LogError("未找到Renderer组件！");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (material != null)
        {
            // 实现呼吸效果
            if (increasing)
            {
                currentAlpha += breathSpeed * Time.deltaTime;
                if (currentAlpha >= maxAlpha)
                {
                    currentAlpha = maxAlpha;
                    increasing = false;
                }
            }
            else
            {
                currentAlpha -= breathSpeed * Time.deltaTime;
                if (currentAlpha <= minAlpha)
                {
                    currentAlpha = minAlpha;
                    increasing = true;
                }
            }
            
            // 应用新的阿尔法值
            Color color = material.color;
            color.a = currentAlpha;
            material.color = color;
        }
    }
}
