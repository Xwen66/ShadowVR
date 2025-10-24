using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class ToolBoxManager : MonoBehaviour
{
    // 单例实例
    private static ToolBoxManager _instance;

    // 公共访问属性
    public static ToolBoxManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ToolBoxManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ToolBoxManager");
                    _instance = go.AddComponent<ToolBoxManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public Transform sockFather;
    public List<XRSocketInteractor> socketInteractorList; // 存储插槽交互器的列表
    // Awake在Start之前调用，用于初始化单例
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


    [ContextMenu("Search All Socket Interactors")]
    public void SearchAllSocketInteractors()
    {
        // 清空现有列表
        if (socketInteractorList == null)
        {
            socketInteractorList = new List<XRSocketInteractor>();
        }
        else
        {
            socketInteractorList.Clear();
        }

        // 检查sockFather是否已设置
        if (sockFather == null)
        {
            Debug.LogWarning("sockFather未设置，无法搜索SocketInteractor");
            return;
        }

        // 在sockFather的所有子物体中查找XRSocketInteractor组件
        XRSocketInteractor[] socketInteractors = sockFather.GetComponentsInChildren<XRSocketInteractor>();

        // 将找到的所有SocketInteractor添加到列表中
        foreach (XRSocketInteractor socket in socketInteractors)
        {
            socketInteractorList.Add(socket);
        }

        Debug.Log($"找到 {socketInteractorList.Count} 个SocketInteractor");
    }

    /// <summary>
    /// 强制插入对象到第一个可用的空闲插槽中
    /// </summary>
    /// <param name="targetObject">要插入的对象</param>
    /// <param name="snapToSocket">是否自动吸附到插槽位置</param>
    /// <returns>是否成功插入</returns>
    [ContextMenu("Force Insert To Specific Socket")]
    public bool ForceInsertToAvailableSocket(GameObject targetObject, bool snapToSocket = true)
    {
        // 检查目标对象是否有效
        if (targetObject == null)
        {
            Debug.LogError("目标对象为空，无法插入");
            return false;
        }

        // 检查插槽列表是否已初始化
        if (socketInteractorList == null || socketInteractorList.Count == 0)
        {
            Debug.LogWarning("插槽列表为空，请先调用 SearchAllSocketInteractors() 方法");
            return false;
        }

        // 遍历所有插槽，寻找第一个空闲的插槽
        foreach (XRSocketInteractor socket in socketInteractorList)
        {
            if (socket == null)
            {
                Debug.LogWarning("发现空的插槽引用，跳过");
                continue;
            }

            // 检查插槽是否空闲（没有插入任何对象）
            if (!socket.hasSelection)
            {
                // 尝试强制插入对象
                bool success = socket.ForceInsert(targetObject, snapToSocket);

                if (success)
                {
                    Debug.Log($"成功将 {targetObject.name} 插入到插槽 {socket.name}");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"尝试将 {targetObject.name} 插入到插槽 {socket.name} 失败，继续尝试下一个插槽");
                }
            }
        }

        // 如果所有插槽都被占用
        Debug.LogWarning($"没有可用的空闲插槽来插入 {targetObject.name}");
        return false;
    }

    /// <summary>
    /// 强制插入对象到指定的插槽中
    /// </summary>
    /// <param name="targetObject">要插入的对象</param>
    /// <param name="socketIndex">插槽索引</param>
    /// <param name="snapToSocket">是否自动吸附到插槽位置</param>
    /// <returns>是否成功插入</returns>

    public bool ForceInsertToSpecificSocket(GameObject targetObject, int socketIndex, bool snapToSocket = true)
    {
        // 检查目标对象是否有效
        if (targetObject == null)
        {
            Debug.LogError("目标对象为空，无法插入");
            return false;
        }

        // 检查插槽列表是否已初始化
        if (socketInteractorList == null || socketInteractorList.Count == 0)
        {
            Debug.LogWarning("插槽列表为空，请先调用 SearchAllSocketInteractors() 方法");
            return false;
        }

        // 检查索引是否有效
        if (socketIndex < 0 || socketIndex >= socketInteractorList.Count)
        {
            Debug.LogError($"插槽索引 {socketIndex} 超出范围，有效范围：0-{socketInteractorList.Count - 1}");
            return false;
        }

        XRSocketInteractor targetSocket = socketInteractorList[socketIndex];
        
        if (targetSocket == null)
        {
            Debug.LogError($"索引 {socketIndex} 处的插槽为空");
            return false;
        }

        // 尝试强制插入对象
        bool success = targetSocket.ForceInsert(targetObject, snapToSocket);
        
        if (success)
        {
            Debug.Log($"成功将 {targetObject.name} 插入到插槽 {targetSocket.name} (索引: {socketIndex})");
        }
        else
        {
            Debug.LogWarning($"插入 {targetObject.name} 到插槽 {targetSocket.name} (索引: {socketIndex}) 失败");
        }

        return success;
    }
}
