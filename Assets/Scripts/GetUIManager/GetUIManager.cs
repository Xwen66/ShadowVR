using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GetUIManager : MonoBehaviour
{
    private static GetUIManager _instance;
    
    public static GetUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GetUIManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GetUIManager");
                    _instance = go.AddComponent<GetUIManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public string ItemType;
    public string ItemName;
    public string ItemDescription;
    public string ItemDescription2;
    public Sprite ItemImage;
    [Header("UI Components")]
    public TextMeshProUGUI ItemTypeText;
    public TextMeshProUGUI ItemNameText;
    public TextMeshProUGUI ItemDescriptionText;
    public TextMeshProUGUI ItemDescription2Text;
    public Image ItemImageUI;


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateUI()
    {
        if (ItemTypeText != null)
            ItemTypeText.text = ItemType;
        
        if (ItemNameText != null)
            ItemNameText.text = ItemName;
        
        if (ItemDescriptionText != null)
            ItemDescriptionText.text = ItemDescription;
        
        if (ItemDescription2Text != null)
            ItemDescription2Text.text = ItemDescription2;
        
        if (ItemImageUI != null)
            ItemImageUI.sprite = ItemImage;
    }
}
