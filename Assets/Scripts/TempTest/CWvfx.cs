using UnityEngine;
using System.Collections;

public class CWvfx : MonoBehaviour
{
    private Vector3 originalScale;
    private float duration = 2f; // 总时长3秒
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalScale = transform.localScale;
        StartCoroutine(ScaleAnimation());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ScaleAnimation()
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale * 1.1f; // 从1.1倍开始
        Vector3 targetScale = originalScale * 2f; // 到1.5倍结束

        while (elapsedTime < duration)
        {
            // 使用平滑步长函数实现先快后慢的效果
            float t = elapsedTime / duration;
            float smoothT = t * t * (3f - 2f * t); // 平滑步长函数
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终达到目标大小
        transform.localScale = targetScale;
        
        // 动画完成后销毁物体
        Destroy(gameObject);
    }
}
