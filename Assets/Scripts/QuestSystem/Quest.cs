using UnityEngine;

public abstract class Quest : MonoBehaviour
{
    public string QuestName;
    public string QuestDescription;
    private bool _isActive;
    private bool _isCompleted;
    private string _questHintText;
    public abstract void GetHintText();
    
}
