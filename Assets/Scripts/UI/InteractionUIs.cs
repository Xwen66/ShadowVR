using UnityEngine;

public class InteractionUIs : MonoBehaviour
{
    public bool useChinese;
    private bool lastUseChinese; // 用于跟踪上一次的语言状态
    
    void Start()
    {
        lastUseChinese = useChinese; // 初始化上一次的状态
        
        // 延迟触发初始事件，确保其他脚本有时间注册监听器
        Invoke(nameof(TriggerInitialLanguageEvent), 0.1f);
    }
    
    /// <summary>
    /// 延迟触发初始语言事件
    /// </summary>
    private void TriggerInitialLanguageEvent()
    {
        GlobalEvent.OnLanguageChangeEvent.Invoke(useChinese);
    }

    void Update()
    {
        // 只有当语言状态真正改变时才触发事件
        if (useChinese != lastUseChinese)
        {
            GlobalEvent.OnLanguageChangeEvent.Invoke(useChinese);
            lastUseChinese = useChinese; // 更新上一次的状态
        }
    }
    
    /// <summary>
    /// 设置语言状态并触发事件
    /// </summary>
    /// <param name="chinese">是否使用中文</param>
    public void SetLanguage(bool chinese)
    {
        if (useChinese != chinese)
        {
            useChinese = chinese;
            GlobalEvent.OnLanguageChangeEvent.Invoke(useChinese);
            lastUseChinese = useChinese;
        }
    }
    
    /// <summary>
    /// 切换语言状态
    /// </summary>
    public void ToggleLanguage()
    {
        SetLanguage(!useChinese);
    }
}
