
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public static class GlobalEvent
{
    //sample event
    public static UnityEvent LoadEvent = new UnityEvent();
    public static UnityEvent OnModeButtonPokeEvent = new UnityEvent();
    public static UnityEvent<bool> OnIsPlayChangeEvent = new UnityEvent<bool>();
    public static UnityEvent<bool> OnIsAlignChangeEvent = new UnityEvent<bool>();

    //apple event

    //对话事件
    public static UnityEvent<int> nextDialogueEvent = new UnityEvent<int>();

    //语言切换事件
    public static UnityEvent<bool> OnLanguageChangeEvent = new UnityEvent<bool>();

    //出现PressAUI提示事件
    public static UnityEvent OnPressAUIEvent = new UnityEvent();

    //角色切换事件（bool = true  代表刺猬模式   false 代表狐狸模式）
    public static UnityEvent<bool> OnChangePersonEvent = new UnityEvent<bool>();

    //玩家完成移动教学事件
    public static UnityEvent OnCompleteMoveTeachingEvent = new UnityEvent();






    

}
