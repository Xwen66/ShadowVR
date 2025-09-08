using UnityEngine;
using UnityEngine.UI;
public class QuestHintUI : MonoBehaviour
{
    [SerializeField] private GameObject questHintPanel;
    [SerializeField] private TMPro.TextMeshProUGUI questHintText;
    [SerializeField] private Button HideButton;
    [SerializeField] public Quest Quest;

    //QuestHintUi class is to controll showing the quest hint text, button that hide and unhide the panel
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Quest = GetComponentInParent<Quest>();
        questHintText.text = Quest.GetHintText();
        HideButton.onClick.AddListener(HideUnhidePanel);
    }

  

    void HideUnhidePanel()
    {
        if (questHintPanel.activeSelf)  
            questHintPanel.SetActive(false);
        else
        {
            questHintPanel.SetActive(true);
        }
    }

}
