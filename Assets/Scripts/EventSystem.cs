
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
    public static UnityEvent<int> nextDialogueEvent = new UnityEvent<int>();
    public static UnityEvent firstDialogueCloseEvent = new UnityEvent();

    //语言切换事件
    public static UnityEvent<bool> OnLanguageChangeEvent = new UnityEvent<bool>();

    //出现PressAUI提示事件
    public static UnityEvent OnPressAUIEvent = new UnityEvent();





    

}
