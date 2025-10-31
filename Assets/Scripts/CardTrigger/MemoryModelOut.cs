using UnityEngine;
using System.Collections;

public class MemoryModelOut : MonoBehaviour
{
    [SerializeField]
    private float scaleSpeed = 1.0f; // 缩放插值速度，可在Inspector中调整
    
    private Vector3 originalScale; // 记录原始缩放值
    private bool isScaling = false; // 是否正在进行缩放动画
    
    void Start()
    {
        // 记录原始缩放值
        originalScale = transform.localScale;
        
        // 将缩放设置为0
        transform.localScale = Vector3.zero;
        
        // 开始缩放动画
        isScaling = true;
        
        // 启动协程，5秒后销毁对象
        StartCoroutine(DestroyAfterDelay());
    }

    void Update()
    {
        // 如果正在进行缩放动画
        if (isScaling)
        {
            // 使用插值逐渐恢复到原始缩放值
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleSpeed);
            
            // 当缩放值接近原始值时，停止动画
            if (Vector3.Distance(transform.localScale, originalScale) < 0.01f)
            {
                transform.localScale = originalScale;
                isScaling = false;
            }
        }
    }
    
    /// <summary>
    /// 协程：延迟5秒后销毁对象
    /// </summary>
    private IEnumerator DestroyAfterDelay()
    {
        // 等待5秒
        yield return new WaitForSeconds(5.0f);
        
        // 销毁对象
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 在Inspector中显示当前缩放速度的值
    /// </summary>
    void OnValidate()
    {
        // 确保缩放速度为正数
        if (scaleSpeed <= 0)
        {
            scaleSpeed = 0.1f;
        }
    }
}
