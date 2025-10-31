using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class StartImage : MonoBehaviour
{
    public SpriteRenderer startImage;
    public SpriteRenderer startImageText;
    public SpriteRenderer startImage2;

    public AudioSource audioSource;
    public AudioClip backgroundMusic;
    public AudioClip backgroundMusic2;


    private void Awake()
    {
        // 将所有图片的透明度设置为0（完全透明）
        SetAlpha(startImage, 0f);
        SetAlpha(startImageText, 0f);
        SetAlpha(startImage2, 0f);
    }

    void Start()
    {
        // 启动图片序列协程
        StartCoroutine(ImageSequenceCoroutine());
    }

    private IEnumerator ImageSequenceCoroutine()
    {
        // 等待5秒
        yield return new WaitForSeconds(2f);

        // 显示第一张图（2秒淡入）并播放背景音乐
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
        yield return StartCoroutine(FadeIn(startImage, 2f));

        // 等待2秒
        yield return new WaitForSeconds(2f);

        // 显示文字图片（2秒淡入）
        yield return StartCoroutine(FadeIn(startImageText, 2f));

        // 等待3秒
        yield return new WaitForSeconds(3f);

        // 第一张图和文字图片一起淡出（2秒）
        yield return StartCoroutine(FadeOutMultiple(new SpriteRenderer[] { startImage, startImageText }, 2f));

        // 等待2秒
        yield return new WaitForSeconds(2f);

        // 显示第二张图（2秒淡入）并播放第二首背景音乐
        if (audioSource != null && backgroundMusic2 != null)
        {
            audioSource.clip = backgroundMusic2;
            audioSource.Play();
        }
        yield return StartCoroutine(FadeIn(startImage2, 2f));

        // 等待3秒
        yield return new WaitForSeconds(3f);

        // 第二张图淡出（2秒）
        yield return StartCoroutine(FadeOut(startImage2, 2f));

        // 等待2秒后跳转到下一个场景
        yield return new WaitForSeconds(2f);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        // 获取当前场景的索引
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // 加载下一个场景（如果当前是最后一个场景，则循环到第一个）
        int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextSceneIndex);
    }

    private IEnumerator FadeIn(SpriteRenderer renderer, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = renderer.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            renderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        renderer.color = targetColor;
    }

    private IEnumerator FadeOut(SpriteRenderer renderer, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = renderer.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            renderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        renderer.color = targetColor;
    }

    private IEnumerator FadeOutMultiple(SpriteRenderer[] renderers, float duration)
    {
        float elapsedTime = 0f;
        Color[] startColors = new Color[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            startColors[i] = renderers[i].color;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            for (int i = 0; i < renderers.Length; i++)
            {
                Color targetColor = new Color(startColors[i].r, startColors[i].g, startColors[i].b, 0f);
                renderers[i].color = Color.Lerp(startColors[i], targetColor, t);
            }
            
            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Color targetColor = new Color(startColors[i].r, startColors[i].g, startColors[i].b, 0f);
            renderers[i].color = targetColor;
        }
    }

    private void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        Color color = renderer.color;
        color.a = alpha;
        renderer.color = color;
    }
}
