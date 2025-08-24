using UnityEngine;
using TMPro;

[System.Serializable]
public class FontSettings
{
    [Header("Font Assets")]
    public TMP_FontAsset chineseFontAsset;
    public TMP_FontAsset englishFontAsset;
    public TMP_FontAsset fallbackFontAsset;
    
    [Header("Auto-Switch Settings")]
    public bool autoSwitchFontByLanguage = true;
}

/// <summary>
/// Helper component to manage Chinese font compatibility in TextMeshPro
/// </summary>
public class ChineseFontHelper : MonoBehaviour
{
    [SerializeField] private FontSettings fontSettings;
    
    private TextMeshProUGUI[] textComponents;
    
    private void Start()
    {
        CacheTextComponents();
        SetupFonts();
        
        // Subscribe to language changes if DialogueManager exists
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueDisplay.AddListener(OnLanguageChanged);
        }
    }
    
    private void CacheTextComponents()
    {
        // Find all TextMeshPro components in children
        textComponents = GetComponentsInChildren<TextMeshProUGUI>();
        Debug.Log($"Found {textComponents.Length} TextMeshPro components");
    }
    
    private void SetupFonts()
    {
        if (fontSettings.chineseFontAsset == null)
        {
            Debug.LogWarning("No Chinese font asset assigned! Please create one using Window > TextMeshPro > Font Asset Creator");
            return;
        }
        
        foreach (var textComponent in textComponents)
        {
            SetupFallbackFonts(textComponent);
        }
    }
    
    private void SetupFallbackFonts(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) return;
        
        // Set main font based on language or use universal font
        if (fontSettings.autoSwitchFontByLanguage)
        {
            bool isChinese = DialogueManager.Instance?.IsChinese ?? false;
            textComponent.font = isChinese ? fontSettings.chineseFontAsset : fontSettings.englishFontAsset;
        }
        else if (fontSettings.chineseFontAsset != null)
        {
            // Use Chinese font as main (it usually includes English)
            textComponent.font = fontSettings.chineseFontAsset;
        }
        
        Debug.Log($"Applied font to {textComponent.name}: {textComponent.font?.name}");
    }
    
    private void OnLanguageChanged(DialogueEntry dialogue)
    {
        if (!fontSettings.autoSwitchFontByLanguage) return;
        
        bool isChinese = DialogueManager.Instance.IsChinese;
        TMP_FontAsset targetFont = isChinese ? fontSettings.chineseFontAsset : fontSettings.englishFontAsset;
        
        if (targetFont == null)
        {
            Debug.LogWarning($"No {(isChinese ? "Chinese" : "English")} font asset assigned!");
            return;
        }
        
        foreach (var textComponent in textComponents)
        {
            if (textComponent != null)
            {
                textComponent.font = targetFont;
            }
        }
        
        Debug.Log($"Switched to {(isChinese ? "Chinese" : "English")} font: {targetFont.name}");
    }
    
    /// <summary>
    /// Manually switch all text components to Chinese font
    /// </summary>
    [ContextMenu("Switch to Chinese Font")]
    public void SwitchToChineseFont()
    {
        if (fontSettings.chineseFontAsset == null)
        {
            Debug.LogError("No Chinese font asset assigned!");
            return;
        }
        
        foreach (var textComponent in textComponents)
        {
            if (textComponent != null)
            {
                textComponent.font = fontSettings.chineseFontAsset;
            }
        }
        
        Debug.Log("Switched all text to Chinese font");
    }
    
    /// <summary>
    /// Manually switch all text components to English font
    /// </summary>
    [ContextMenu("Switch to English Font")]
    public void SwitchToEnglishFont()
    {
        if (fontSettings.englishFontAsset == null)
        {
            Debug.LogError("No English font asset assigned!");
            return;
        }
        
        foreach (var textComponent in textComponents)
        {
            if (textComponent != null)
            {
                textComponent.font = fontSettings.englishFontAsset;
            }
        }
        
        Debug.Log("Switched all text to English font");
    }
    
    /// <summary>
    /// Test font compatibility with sample Chinese text
    /// </summary>
    [ContextMenu("Test Chinese Characters")]
    public void TestChineseCharacters()
    {
        string testText = "你好世界！这是测试。Hello World! This is a test.";
        
        foreach (var textComponent in textComponents)
        {
            if (textComponent != null)
            {
                string originalText = textComponent.text;
                textComponent.text = testText;
                
                Debug.Log($"Testing Chinese characters on {textComponent.name}");
                
                // Restore original text after 3 seconds
                Invoke(nameof(RestoreOriginalTexts), 3f);
            }
        }
    }
    
    private void RestoreOriginalTexts()
    {
        foreach (var textComponent in textComponents)
        {
            if (textComponent != null && textComponent.name.Contains("Test"))
            {
                textComponent.text = "Original text restored";
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueDisplay.RemoveListener(OnLanguageChanged);
        }
    }
}
