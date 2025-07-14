using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    //singleton
    public static UIManager Instance;
    public TextMeshProUGUI PopupText;
    public TextMeshProUGUI MemoryShardsText;
    [SerializeField] private float popupTextDuration = 1f;  
    private Vector2 _popupTextOriginalPosition;
    public GameObject LargePlayerUICanvas;
    public GameObject SmallPlayerUICanvas;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
    }


    public void ShowPopupText(string text)
    {
        PopupText.text = text;
        PopupText.gameObject.SetActive(true);
        StartCoroutine(AnimatePopupText());
    }

    private IEnumerator AnimatePopupText()
    {
        Vector3 startPos = _popupTextOriginalPosition;
        Vector3 targetPos = startPos + Vector3.up * 100f;
        Color startColor = PopupText.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        float duration = popupTextDuration;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // Smooth easing (similar to InOutSine)
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            
            PopupText.transform.position = Vector3.Lerp(startPos, targetPos, easedT);
            PopupText.color = Color.Lerp(startColor, targetColor, easedT);
            
            yield return null;
        }
        
        HidePopupText();
    }

    public void HidePopupText() 
    {
        PopupText.gameObject.SetActive(false);
        // Reset alpha for next use
        Color resetColor = PopupText.color;
        PopupText.color = new Color(resetColor.r, resetColor.g, resetColor.b, 1f);
    }

    [ContextMenu("Test Show Popup")]
    public void TestShowPopup()
    {
        ShowPopupText("Hello World");
    }

    public void UpdateCurrentMemoryShards(int currentMemoryShards)
    {
        MemoryShardsText.text = $"Memory Shards: {currentMemoryShards}/{GameManager.Instance.TotalMemoryShards}";
    }
    
    public void ToggleUICanvas(int index)
    {
        if(index == 0)
        {
            if(LargePlayerUICanvas.activeSelf)
            {
                LargePlayerUICanvas.SetActive(false);
            }
            else
            {
                LargePlayerUICanvas.SetActive(true);
            }
        }
        else
        {
            if(SmallPlayerUICanvas.activeSelf)
            {
                SmallPlayerUICanvas.SetActive(false);
            }
            else
            {
                SmallPlayerUICanvas.SetActive(true);
            }
        }
    }
}

