using UnityEngine;

public class GameManager : MonoBehaviour

{

    [SerializeField] private GameObject _largePlayer;
    [SerializeField] public GameObject FoxPrefab;
    [SerializeField] private GameObject _smallPlayer;
    [SerializeField] private float _cooldown = 1f;
    private float _cooldownTimer = 0f;
    [SerializeField] private int _totalMemoryShards = 5;
    private int _currentMemoryShards = 0;
    //singleton
    public static GameManager Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _cooldownTimer += Time.deltaTime;
        if (_cooldownTimer < _cooldown) return;
        // Check if the left controller's trigger (index) button is pressed using XR Input
        if (UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand)
            .TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isPressed) && isPressed || Input.GetKeyDown(KeyCode.Space))
        {
            //small player mode
            if (_largePlayer.activeSelf)
            {
                _largePlayer.SetActive(false);
                FoxPrefab.SetActive(true);
                _smallPlayer.SetActive(true);
                _cooldownTimer = 0f;
            }
            else
            {
                //large player mode
                _largePlayer.SetActive(true);
                FoxPrefab.SetActive(false);
                _smallPlayer.SetActive(false);
                _cooldownTimer = 0f;
            }
        }
    }

    public void AddMemoryShard()
    {
        _currentMemoryShards++;
        UIManager.Instance.UpdateCurrentMemoryShards(_currentMemoryShards);
        if (_currentMemoryShards >= _totalMemoryShards)
        {
            //win
        }
    }
    public int TotalMemoryShards
    {
        get { return _totalMemoryShards; }
    }
    public int CurrentMemoryShards
    {
        get { return _currentMemoryShards; }
    }
}
