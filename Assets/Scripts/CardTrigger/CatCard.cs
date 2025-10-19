using UnityEngine;

public class CatCard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision other)
    {
        Debug.LogError("collider碰到东西了");
        //检测layer 是不是“GorillaRig”
        if(other.gameObject.layer == LayerMask.NameToLayer("GorillaRig"))
        {
            Debug.LogError("collider玩家碰撞到卡片");
        }
    }


        void OnTriggerEnter(Collider other)
    {
        Debug.LogError("trigger碰到东西了");
        //检测layer 是不是“GorillaRig”
        if(other.gameObject.layer == LayerMask.NameToLayer("GorillaRig"))
        {
            Debug.LogError("trigger玩家碰撞到卡片");
        }
    }
}
