using UnityEngine;

public class ItemEffectObject : MonoBehaviour
{
    public Transform moveTarget;
    public Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (moveTarget != null)
        {
            transform.position = moveTarget.position + offset;
        }
        
    }

    public void OnGet()
    {
        // 销毁场景中的实例对象
        Destroy(gameObject);
    }
}
