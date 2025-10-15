using UnityEngine;

public class PressAUI : MonoBehaviour
{
    public Transform pressAUI;
    public Vector3 offset;
    public Transform lookTarget;
    public Transform followTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pressAUI.position = followTarget.position + offset;
        pressAUI.LookAt(lookTarget);
    }
}
