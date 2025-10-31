using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


public class ShadowFloorManager : MonoBehaviour
{
    public List<XRSocketInteractor> sockets;
    public GameObject floorMesh;
    public AudioSource audioSource;
    public AudioClip floorOutSound;
    
    void Start()
    {
        // 为每个插槽添加事件监听
        foreach (var socket in sockets)
        {
            socket.selectEntered.AddListener(OnSocketSelectEntered);
            socket.selectExited.AddListener(OnSocketSelectExited);
        }
    }

    void OnDestroy()
    {
        // 清理事件监听
        foreach (var socket in sockets)
        {
            socket.selectEntered.RemoveListener(OnSocketSelectEntered);
            socket.selectExited.RemoveListener(OnSocketSelectExited);
        }
    }

    // 当物品插入插槽时调用
    private void OnSocketSelectEntered(SelectEnterEventArgs args)
    {
        CheckBookCondition();
    }

    // 当物品从插槽移出时调用
    private void OnSocketSelectExited(SelectExitEventArgs args)
    {
        CheckBookCondition();
    }

    // 检查是否所有插槽都插入了包含"Book"的物品
    private void CheckBookCondition()
    {
        // 如果插槽数量不是3个，直接返回
        if (sockets.Count != 3)
            return;

        int bookCount = 0;
        
        // 检查每个插槽
        foreach (var socket in sockets)
        {
            // 检查插槽是否有物品
            if (socket.hasSelection)
            {
                // 获取插入的物体
                GameObject selectedObject = socket.firstInteractableSelected.transform.gameObject;
                
                // 检查物体名称是否包含"Book"
                if (selectedObject.name.Contains("Book"))
                {
                    bookCount++;
                }
            }
        }

        // 如果所有三个插槽都插入了包含"Book"的物品
        if (bookCount == 3)
        {
            Debug.Log("哈哈哈");
            floorMesh.SetActive(true);
            audioSource.PlayOneShot(floorOutSound);
        }
    }
}
