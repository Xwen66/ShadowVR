using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip UIOut;
    public GameObject UIcanvas;
    public QuestUIMove questUIMove;
    public TextMeshProUGUI ItemTypeText;
    public Canvas questCanvas;
    public string ItemQuest1;
    public string ItemQuest2;
    public string ItemQuest3;

    public string shadowQuest1;
    public string shadowQuest2;

    private static QuestUIManager _instance;

    public static QuestUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<QuestUIManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("QuestUIManager");
                    _instance = go.AddComponent<QuestUIManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

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

    private void OnEnable()
    {
        // 检查GameManager中的玩家形态状态
        if (GameManager.Instance != null)
        {
            // 获取QuestUIMove组件
            QuestUIMove questUIMove = GetComponent<QuestUIMove>();
            if (questUIMove != null)
            {
                // 如果是大型玩家（人类形态），切换到Mode2（狐狸模式）
                // 如果是小型玩家（动物形态），切换到Mode1（刺猬模式）
                if (GameManager.Instance.isLargePlayer)
                {
                    questUIMove.SwitchToMode2(); // 人类形态 -> Mode2
                }
                else
                {
                    questUIMove.SwitchToMode1(); // 动物形态 -> Mode1
                }
            }
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

    // 方法1：设置ItemQuest1到ItemTypeText，显示Canvas并在3秒后隐藏
    public void SetItemQuest1Text()
    {
        // 检查并更新UI模式
        CheckAndUpdateUIMode();
        
        if (questCanvas != null)
        {
            questCanvas.gameObject.SetActive(true);
            // 播放UI出现音效
            PlayUIOutSound();
        }
        
        if (ItemTypeText != null)
        {
            ItemTypeText.text = ItemQuest1;
        }
        
        Invoke("HideQuestCanvas", 3f);
    }

    // 方法2：设置ItemQuest2到ItemTypeText，显示Canvas并在3秒后隐藏
    public void SetItemQuest2Text()
    {
        // 检查并更新UI模式
        CheckAndUpdateUIMode();
        
        if (questCanvas != null)
        {
            questCanvas.gameObject.SetActive(true);
            // 播放UI出现音效
            PlayUIOutSound();
        }
        
        if (ItemTypeText != null)
        {
            ItemTypeText.text = ItemQuest2;
        }
        
        Invoke("HideQuestCanvas", 3f);
    }

    // 方法3：设置ItemQuest3到ItemTypeText，显示Canvas并在3秒后隐藏
    public void SetItemQuest3Text()
    {
        // 检查并更新UI模式
        CheckAndUpdateUIMode();
        
        if (questCanvas != null)
        {
            questCanvas.gameObject.SetActive(true);
            // 播放UI出现音效
            PlayUIOutSound();
        }
        
        if (ItemTypeText != null)
        {
            ItemTypeText.text = ItemQuest3;
        }
        
        Invoke("HideQuestCanvas", 3f);
    }

    // 方法4：设置shadowQuest1到ItemTypeText，显示Canvas并在3秒后隐藏
    public void SetShadowQuest1Text()
    {
        // 检查并更新UI模式
        CheckAndUpdateUIMode();
        
        if (questCanvas != null)
        {
            questCanvas.gameObject.SetActive(true);
            // 播放UI出现音效
            PlayUIOutSound();
        }
        
        if (ItemTypeText != null)
        {
            ItemTypeText.text = shadowQuest1;
        }
        
        Invoke("HideQuestCanvas", 3f);
    }

    // 方法5：设置shadowQuest2到ItemTypeText，显示Canvas并在3秒后隐藏
    public void SetShadowQuest2Text()
    {
        // 检查并更新UI模式
        CheckAndUpdateUIMode();
        
        if (questCanvas != null)
        {
            questCanvas.gameObject.SetActive(true);
            // 播放UI出现音效
            PlayUIOutSound();
        }
        
        if (ItemTypeText != null)
        {
            ItemTypeText.text = shadowQuest2;
        }
        
        Invoke("HideQuestCanvas", 3f);
    }

    // 隐藏Quest Canvas的方法
    private void HideQuestCanvas()
    {
        if (questCanvas != null)
        {
            questCanvas.gameObject.SetActive(false);
        }
    }

    // 检查并更新UI模式的辅助方法
    private void CheckAndUpdateUIMode()
    {
        // 检查GameManager中的玩家形态状态
        if (GameManager.Instance != null)
        {
            // 获取QuestUIMove组件
            // QuestUIMove questUIMove = GetComponentInChildren<QuestUIMove>();
            if (questUIMove != null)
            {
                // 如果是大型玩家（人类形态），切换到Mode2（狐狸模式）
                // 如果是小型玩家（动物形态），切换到Mode1（刺猬模式）
                if (GameManager.Instance.isLargePlayer)
                {
                    questUIMove.SwitchToMode2(); // 人类形态 -> Mode2
                }
                else
                {
                    questUIMove.SwitchToMode1(); // 动物形态 -> Mode1
                }
            }
        }
    }

    /// <summary>
    /// 播放UI出现音效
    /// </summary>
    private void PlayUIOutSound()
    {
        if (audioSource != null && UIOut != null)
        {
            audioSource.PlayOneShot(UIOut);
            Debug.Log("Playing UI out sound for Quest UI");
        }
        else
        {
            Debug.LogWarning("Cannot play UI out sound: AudioSource or UIOut AudioClip is missing");
        }
    }
}
