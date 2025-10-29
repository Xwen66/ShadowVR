using System;
using UnityEngine;
using VInspector;

public class PressAUI : MonoBehaviour
{
    public AudioSource audioSource;
    public Transform pressAUIChinese;
    public Transform pressAUIEnglish;
    public Vector3 offset;
    public Transform lookTarget;
    public Transform followTarget;
    public bool isFoxMode;

    private bool useChinese = true; // 默认使用中文
    private bool isInitialized = false; // 标记是否已初始化
    private bool changePersonEventValue = false; // 存储OnChangePersonEvent传入的值
    private bool hasPlayedAudio = false; // 标记是否已播放过音频

    void Start()
    {
        // 监听语言切换事件
        GlobalEvent.OnLanguageChangeEvent.AddListener(OnLanguageChanged);
        GlobalEvent.OnPressAUIEvent.AddListener(OnPressAUIEvent);
        GlobalEvent.OnChangePersonEvent.AddListener(OnChangePersonEvent);

        // 游戏开始时，确保两个UI都是不显示的
        if (pressAUIChinese != null)
            pressAUIChinese.gameObject.SetActive(false);
        if (pressAUIEnglish != null)
            pressAUIEnglish.gameObject.SetActive(false);
    }

    private void OnChangePersonEvent(bool arg0)
    {
        // 存储传入的值
        changePersonEventValue = arg0;
        // 更新UI显示状态
        if (isInitialized)
        {
            UpdateUIVisibility();
        }
    }

    private void OnPressAUIEvent()
    {
        // 只有在第一次接收到事件时才进行初始化
        if (!isInitialized)
        {
            isInitialized = true;
            UpdateUIVisibility();
            audioSource.Play();
        }
    }

    void OnDestroy()
    {
        // 移除事件监听，避免内存泄漏
        GlobalEvent.OnLanguageChangeEvent.RemoveListener(OnLanguageChanged);
        GlobalEvent.OnPressAUIEvent.RemoveListener(OnPressAUIEvent);
    }

    // Update is called once per frame
    void Update()
    {
        // 只更新当前激活的UI位置和朝向
        Transform activeUI = useChinese ? pressAUIChinese : pressAUIEnglish;
        if (activeUI != null)
        {
            activeUI.position = followTarget.position + offset;
            activeUI.LookAt(lookTarget);
        }
    }

    /// <summary>
    /// 语言切换事件处理函数
    /// </summary>
    /// <param name="isChinese">是否切换到中文</param>
    private void OnLanguageChanged(bool isChinese)
    {
        useChinese = isChinese;
        // 只有在已经初始化的情况下才更新UI显示状态
        if (isInitialized)
        {
            UpdateUIVisibility();
        }
    }

    /// <summary>
    /// 更新UI显示状态
    /// </summary>
    private void UpdateUIVisibility()
    {
        // 判断是否应该显示UI
        bool shouldShowUI = (changePersonEventValue != isFoxMode);

        if (pressAUIChinese != null)
            pressAUIChinese.gameObject.SetActive(shouldShowUI && useChinese);

        if (pressAUIEnglish != null)
            pressAUIEnglish.gameObject.SetActive(shouldShowUI && !useChinese);

        // // 如果UI应该显示且还未播放过音频，则播放音频
        // if (shouldShowUI && !hasPlayedAudio && audioSource != null)
        // {
        //     audioSource.Play();
        //     hasPlayedAudio = true;
        // }
    }










    //test 
    [Button]
    public void Test()
    {
        GlobalEvent.OnPressAUIEvent.Invoke();
    }
}
