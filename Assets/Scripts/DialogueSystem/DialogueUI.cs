using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button languageToggleButton;
    [SerializeField] private TMP_FontAsset fontAssetEnglish;
    [SerializeField] private TMP_FontAsset fontAssetChinese;
    
    [Header("Animation Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;
    
    [Header("Default Settings")]
    [SerializeField] private Sprite defaultCharacterSprite;
    
    // Private variables
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    
    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
        
        // Initialize fonts based on current language
        if (DialogueManager.Instance != null)
        {
            RefreshFonts();
        }
    }
    
    private void InitializeUI()
    {
        // Hide dialogue panel initially
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        // Set default character image
        if (characterImage != null && defaultCharacterSprite != null)
            characterImage.sprite = defaultCharacterSprite;
    }
    
    private void SetupEventListeners()
    {
        // Subscribe to DialogueManager events
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart.AddListener(OnDialogueStart);
            DialogueManager.Instance.OnDialogueDisplay.AddListener(OnDialogueDisplay);
            DialogueManager.Instance.OnDialogueEnd.AddListener(OnDialogueEnd);
        }
        
        // Setup button listeners
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        
        if (languageToggleButton != null)
            languageToggleButton.onClick.AddListener(OnLanguageToggleClicked);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart.RemoveListener(OnDialogueStart);
            DialogueManager.Instance.OnDialogueDisplay.RemoveListener(OnDialogueDisplay);
            DialogueManager.Instance.OnDialogueEnd.RemoveListener(OnDialogueEnd);
        }
    }
    
    #region Event Handlers
    
    private void OnDialogueStart(DialogueEntry dialogue)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        Debug.Log("Dialogue UI: Dialogue started");
    }
    
    private void OnDialogueDisplay(DialogueEntry dialogue)
    {
        if (dialogue == null) return;
        
        UpdateCharacterImage(dialogue.CharacterID);
        UpdateCharacterName(dialogue);
        UpdateDialogueText(dialogue);
    }
    
    private void OnDialogueEnd()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        // Stop any active typewriter effect
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        isTyping = false;
        
        Debug.Log("Dialogue UI: Dialogue ended");
    }
    
    private void OnNextButtonClicked()
    {
        if (isTyping)
        {
            // If typing, complete the text immediately
            CompleteTypewriter();
        }
        else
        {
            // Move to next dialogue
            DialogueManager.Instance.NextDialogue();
        }
    }
    
    private void OnCloseButtonClicked()
    {
        DialogueManager.Instance.EndDialogue();
    }
    
    private void OnLanguageToggleClicked()
    {
        DialogueManager.Instance.ToggleLanguage();
    }
    
    #endregion
    
    #region UI Update Methods
    
    private void UpdateCharacterImage(string characterID)
    {
        if (characterImage == null) return;
        
        Sprite characterSprite = DialogueManager.Instance.GetCharacterImage(characterID);
        
        if (characterSprite != null)
        {
            characterImage.sprite = characterSprite;
        }
        else
        {
            // Use default sprite if character image not found
            if (defaultCharacterSprite != null)
                characterImage.sprite = defaultCharacterSprite;
            
            Debug.LogWarning($"Character image not found for ID: {characterID} (English name)");
        }
    }
    
    private void UpdateCharacterName(DialogueEntry dialogue)
    {
        if (characterNameText == null) return;
        
        // Update font asset for character name text
        UpdateCharacterNameFont();
        
        string characterName = dialogue.GetCharacterName(DialogueManager.Instance.IsChinese);
        characterNameText.text = characterName;
    }
    
    private void UpdateCharacterNameFont()
    {
        if (characterNameText == null) return;
        
        bool isChinese = DialogueManager.Instance.IsChinese;
        
        if (isChinese && fontAssetChinese != null)
        {
            characterNameText.font = fontAssetChinese;
        }
        else if (!isChinese && fontAssetEnglish != null)
        {
            characterNameText.font = fontAssetEnglish;
        }
    }
    
    private void UpdateDialogueText(DialogueEntry dialogue)
    {
        if (dialogueText == null) return;
        
        // Update font asset based on language
        UpdateDialogueTextFont();
        
        string text = dialogue.GetDialogueContent(DialogueManager.Instance.IsChinese);
        
        if (useTypewriterEffect)
        {
            StartTypewriter(text);
        }
        else
        {
            dialogueText.text = text;
        }
    }
    
    private void UpdateDialogueTextFont()
    {
        if (dialogueText == null) return;
        
        bool isChinese = DialogueManager.Instance.IsChinese;
        
        if (isChinese && fontAssetChinese != null)
        {
            dialogueText.font = fontAssetChinese;
        }
        else if (!isChinese && fontAssetEnglish != null)
        {
            dialogueText.font = fontAssetEnglish;
        }
        else
        {
            // Fallback warning if appropriate font asset is missing
            if (isChinese && fontAssetChinese == null)
            {
                Debug.LogWarning("Chinese font asset not assigned! Please assign fontAssetChinese in DialogueUI.");
            }
            else if (!isChinese && fontAssetEnglish == null)
            {
                Debug.LogWarning("English font asset not assigned! Please assign fontAssetEnglish in DialogueUI.");
            }
        }
    }
    
    #endregion
    
    #region Typewriter Effect
    
    private void StartTypewriter(string text)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
    }
    
    private System.Collections.IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        typewriterCoroutine = null;
    }
    
    private void CompleteTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        // Get current dialogue and display full text
        var currentDialogue = DialogueManager.Instance.CurrentDialogue;
        if (currentDialogue != null)
        {
            dialogueText.text = currentDialogue.GetDialogueContent(DialogueManager.Instance.IsChinese);
        }
        
        isTyping = false;
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Manually show dialogue panel
    /// </summary>
    public void ShowDialoguePanel()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }
    
    /// <summary>
    /// Manually hide dialogue panel
    /// </summary>
    public void HideDialoguePanel()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    /// <summary>
    /// Set typewriter speed
    /// </summary>
    public void SetTypewriterSpeed(float speed)
    {
        typewriterSpeed = Mathf.Max(0.01f, speed);
    }
    
    /// <summary>
    /// Toggle typewriter effect
    /// </summary>
    public void SetTypewriterEffect(bool enabled)
    {
        useTypewriterEffect = enabled;
    }
    
    /// <summary>
    /// Manually refresh fonts for all text components based on current language
    /// </summary>
    public void RefreshFonts()
    {
        UpdateCharacterNameFont();
        UpdateDialogueTextFont();
        
        Debug.Log($"Fonts refreshed for language: {(DialogueManager.Instance.IsChinese ? "Chinese" : "English")}");
    }
    
    /// <summary>
    /// Set font assets for both languages
    /// </summary>
    public void SetFontAssets(TMP_FontAsset englishFont, TMP_FontAsset chineseFont)
    {
        fontAssetEnglish = englishFont;
        fontAssetChinese = chineseFont;
        RefreshFonts();
    }
    
    #endregion
}
