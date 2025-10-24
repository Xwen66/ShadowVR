using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketForceInsertExample : MonoBehaviour
{
    [Header("插槽设置")]
    public XRSocketInteractor socketInteractor;
    
    [Header("要插入的对象")]
    public GameObject targetObject;
    
    [Header("自动吸附")]
    public bool snapToSocket = true;
    
    [Header("调试控制")]
    public bool insertOnStart = false;
    public KeyCode insertKey = KeyCode.Space;

    void Start()
    {
        if (insertOnStart)
        {
            ForceInsertObject();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(insertKey))
        {
            ForceInsertObject();
        }
    }

    /// <summary>
    /// 强制插入对象到插槽
    /// </summary>
    public void ForceInsertObject()
    {
        if (socketInteractor == null)
        {
            Debug.LogError("请先设置 socketInteractor");
            return;
        }

        if (targetObject == null)
        {
            Debug.LogError("请先设置 targetObject");
            return;
        }

        bool success = socketInteractor.ForceInsert(targetObject, snapToSocket);
        
        if (success)
        {
            Debug.Log($"成功将 {targetObject.name} 插入到插槽");
        }
        else
        {
            Debug.LogWarning($"插入 {targetObject.name} 失败");
        }
    }

    /// <summary>
    /// 通过代码设置要插入的对象
    /// </summary>
    public void SetTargetObject(GameObject newTarget)
    {
        targetObject = newTarget;
    }

    /// <summary>
    /// 通过代码设置插槽交互器
    /// </summary>
    public void SetSocketInteractor(XRSocketInteractor newSocket)
    {
        socketInteractor = newSocket;
    }
}