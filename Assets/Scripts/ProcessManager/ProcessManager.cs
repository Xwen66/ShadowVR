using UnityEngine;

public class ProcessManager : MonoBehaviour
{
    // 单例实例
    private static ProcessManager _instance;
    
    // 公共访问属性
    public static ProcessManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ProcessManager>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("ProcessManager");
                    _instance = singletonObject.AddComponent<ProcessManager>();
                }
            }
            return _instance;
        }
    }
    
    // 确保单例在场景切换时不被销毁
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
