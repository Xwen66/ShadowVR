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

    [Header("UI Components Canvas")]
    public GameObject GetUICanvas;


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
    
    /// <summary>
    /// 显示UI并在5秒后自动隐藏
    /// </summary>
    public void ShowUIForFiveSeconds()
    {
        if (GetUICanvas != null)
        {
            // 激活Canvas
            GetUICanvas.SetActive(true);
            
            // 启动协程，5秒后隐藏
            StartCoroutine(HideUIAfterDelay(5f));
        }
    }
    
    /// <summary>
    /// 协程：延迟指定时间后隐藏UI
    /// </summary>
    /// <param name="delay">延迟时间（秒）</param>
    private System.Collections.IEnumerator HideUIAfterDelay(float delay)
    {
        // 等待指定时间
        yield return new WaitForSeconds(delay);
        
        // 隐藏Canvas
        if (GetUICanvas != null)
        {
            GetUICanvas.SetActive(false);
        }
    }
}
