using UnityEngine;

public class Pickupable : MonoBehaviour
{
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    public bool isInTray = false;
    public bool isInToolbox = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _originalScale = transform.localScale;
        _originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetOriginalScale()
    {
        return _originalScale;
    }

    public Vector3 GetOriginalPosition()
    {
        return _originalPosition;
    }
}
