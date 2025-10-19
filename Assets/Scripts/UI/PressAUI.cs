using UnityEngine;
using VInspector;

public class PressAUI : MonoBehaviour
{
    public Transform pressAUIChinese;
    public Transform pressAUIEnglish;
    public Vector3 offset;
    public Transform lookTarget;
    public Transform followTarget;
    
    private bool useChinese = true; // 默认使用中文
    private bool isInitialized = false; // 标记是否已初始化
    
    void Start()
    {
        // 监听语言切换事件
        GlobalEvent.OnLanguageChangeEvent.AddListener(OnLanguageChanged);
        GlobalEvent.OnPressAUIEvent.AddListener(OnPressAUIEvent);
        
        // 游戏开始时，确保两个UI都是不显示的
        if (pressAUIChinese != null)
            pressAUIChinese.gameObject.SetActive(false);
        if (pressAUIEnglish != null)
            pressAUIEnglish.gameObject.SetActive(false);
    }

    private void OnPressAUIEvent()
    {
        // 只有在第一次接收到事件时才进行初始化
        if (!isInitialized)
        {
            isInitialized = true;
            UpdateUIVisibility();
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
        if (pressAUIChinese != null)
            pressAUIChinese.gameObject.SetActive(useChinese);
            
        if (pressAUIEnglish != null)
            pressAUIEnglish.gameObject.SetActive(!useChinese);
    }










    //test 
    [Button]
    public void Test()
    {
        GlobalEvent.OnPressAUIEvent.Invoke();
    }
}
