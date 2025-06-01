using UnityEngine;

public class ObjectMotion : MonoBehaviour
{
    [Header("Motion Settings")]
    [Tooltip("Enable floating up and down motion")]
    public bool enableFloating = true;
    
    [Tooltip("Enable rotation/spinning motion")]
    public bool enableRotation = true;
    
    [Tooltip("Enable scaling (pulsing) motion")]
    public bool enableScaling = false;
    
    [Tooltip("Enable horizontal swaying motion")]
    public bool enableSwaying = false;
    
    [Header("Floating Motion")]
    [Tooltip("Distance to float up and down")]
    public float floatingDistance = 1f;
    
    [Tooltip("Speed of floating motion")]
    public float floatingSpeed = 1f;
    
    [Tooltip("Type of floating motion curve")]
    public FloatingType floatingType = FloatingType.SineWave;
    
    [Tooltip("Axis for floating motion")]
    public MotionAxis floatingAxis = MotionAxis.Y;
    
    [Header("Rotation Motion")]
    [Tooltip("Speed of rotation on X axis (degrees per second)")]
    public float rotationSpeedX = 0f;
    
    [Tooltip("Speed of rotation on Y axis (degrees per second)")]
    public float rotationSpeedY = 30f;
    
    [Tooltip("Speed of rotation on Z axis (degrees per second)")]
    public float rotationSpeedZ = 0f;
    
    [Tooltip("Use local or world space for rotation")]
    public bool useLocalRotation = true;
    
    [Header("Scaling Motion")]
    [Tooltip("Minimum scale multiplier")]
    public float minScale = 0.8f;
    
    [Tooltip("Maximum scale multiplier")]
    public float maxScale = 1.2f;
    
    [Tooltip("Speed of scaling motion")]
    public float scalingSpeed = 2f;
    
    [Tooltip("Scale all axes uniformly")]
    public bool uniformScaling = true;
    
    [Header("Swaying Motion")]
    [Tooltip("Distance to sway left and right")]
    public float swayingDistance = 0.5f;
    
    [Tooltip("Speed of swaying motion")]
    public float swayingSpeed = 1.5f;
    
    [Tooltip("Axis for swaying motion")]
    public MotionAxis swayingAxis = MotionAxis.X;
    
    [Header("Advanced Settings")]
    [Tooltip("Random offset for motion timing")]
    public bool useRandomOffset = true;
    
    [Tooltip("Easing type for smoother motion")]
    public EasingType easingType = EasingType.EaseInOut;
    
    [Tooltip("Motion starts automatically")]
    public bool autoStart = true;
    
    [Tooltip("Motion continues when game is paused")]
    public bool ignoreTimeScale = false;
    
    [Header("Debug")]
    [Tooltip("Show motion gizmos in scene view")]
    public bool showGizmos = true;
    
    [Tooltip("Gizmo color for motion visualization")]
    public Color gizmoColor = Color.cyan;
    
    // Private variables
    private Vector3 startPosition;
    private Vector3 startScale;
    private Quaternion startRotation;
    private float timeOffset;
    private bool isMotionActive = true;
    
    // Enums for dropdown options
    public enum FloatingType
    {
        SineWave,
        Triangle,
        Square,
        Random
    }
    
    public enum MotionAxis
    {
        X,
        Y,
        Z
    }
    
    public enum EasingType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeMotion();
    }
    
    void InitializeMotion()
    {
        // Store initial transform values
        startPosition = transform.position;
        startScale = transform.localScale;
        startRotation = transform.rotation;
        
        // Random offset for variation
        if (useRandomOffset)
        {
            timeOffset = Random.Range(0f, 2f * Mathf.PI);
        }
        
        if (!autoStart)
        {
            isMotionActive = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMotionActive) return;
        
        float deltaTime = ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        float currentTime = ignoreTimeScale ? Time.unscaledTime : Time.time;
        
        // Apply floating motion
        if (enableFloating)
        {
            ApplyFloatingMotion(currentTime);
        }
        
        // Apply rotation motion
        if (enableRotation)
        {
            ApplyRotationMotion(deltaTime);
        }
        
        // Apply scaling motion
        if (enableScaling)
        {
            ApplyScalingMotion(currentTime);
        }
        
        // Apply swaying motion
        if (enableSwaying)
        {
            ApplySwayingMotion(currentTime);
        }
    }
    
    void ApplyFloatingMotion(float time)
    {
        float motionValue = 0f;
        float adjustedTime = (time + timeOffset) * floatingSpeed;
        
        switch (floatingType)
        {
            case FloatingType.SineWave:
                motionValue = Mathf.Sin(adjustedTime);
                break;
            case FloatingType.Triangle:
                motionValue = Mathf.PingPong(adjustedTime, 2f) - 1f;
                break;
            case FloatingType.Square:
                motionValue = Mathf.Sign(Mathf.Sin(adjustedTime));
                break;
            case FloatingType.Random:
                motionValue = Mathf.PerlinNoise(adjustedTime, 0f) * 2f - 1f;
                break;
        }
        
        // Apply easing
        motionValue = ApplyEasing(motionValue);
        
        Vector3 offset = Vector3.zero;
        switch (floatingAxis)
        {
            case MotionAxis.X:
                offset.x = motionValue * floatingDistance;
                break;
            case MotionAxis.Y:
                offset.y = motionValue * floatingDistance;
                break;
            case MotionAxis.Z:
                offset.z = motionValue * floatingDistance;
                break;
        }
        
        transform.position = startPosition + offset;
    }
    
    void ApplyRotationMotion(float deltaTime)
    {
        Vector3 rotationSpeed = new Vector3(rotationSpeedX, rotationSpeedY, rotationSpeedZ);
        Vector3 rotation = rotationSpeed * deltaTime;
        
        if (useLocalRotation)
        {
            transform.Rotate(rotation, Space.Self);
        }
        else
        {
            transform.Rotate(rotation, Space.World);
        }
    }
    
    void ApplyScalingMotion(float time)
    {
        float adjustedTime = (time + timeOffset) * scalingSpeed;
        float scaleValue = Mathf.Sin(adjustedTime) * 0.5f + 0.5f; // 0 to 1
        float currentScale = Mathf.Lerp(minScale, maxScale, scaleValue);
        
        // Apply easing
        currentScale = ApplyEasing(currentScale);
        
        Vector3 newScale;
        if (uniformScaling)
        {
            newScale = startScale * currentScale;
        }
        else
        {
            newScale = new Vector3(
                startScale.x * currentScale,
                startScale.y * currentScale,
                startScale.z * currentScale
            );
        }
        
        transform.localScale = newScale;
    }
    
    void ApplySwayingMotion(float time)
    {
        float adjustedTime = (time + timeOffset) * swayingSpeed;
        float swayValue = Mathf.Sin(adjustedTime);
        
        // Apply easing
        swayValue = ApplyEasing(swayValue);
        
        Vector3 swayOffset = Vector3.zero;
        switch (swayingAxis)
        {
            case MotionAxis.X:
                swayOffset.x = swayValue * swayingDistance;
                break;
            case MotionAxis.Y:
                swayOffset.y = swayValue * swayingDistance;
                break;
            case MotionAxis.Z:
                swayOffset.z = swayValue * swayingDistance;
                break;
        }
        
        // Add sway to current position (don't reset to start position)
        Vector3 basePosition = startPosition;
        if (enableFloating)
        {
            basePosition = transform.position; // Use current position if floating is active
        }
        
        transform.position = basePosition + swayOffset;
    }
    
    float ApplyEasing(float value)
    {
        switch (easingType)
        {
            case EasingType.Linear:
                return value;
            case EasingType.EaseIn:
                return value * value;
            case EasingType.EaseOut:
                return 1f - (1f - value) * (1f - value);
            case EasingType.EaseInOut:
                return value < 0.5f ? 2f * value * value : 1f - Mathf.Pow(-2f * value + 2f, 2f) / 2f;
            case EasingType.Bounce:
                return BounceEase(value);
            default:
                return value;
        }
    }
    
    float BounceEase(float value)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;
        
        if (value < 1f / d1)
        {
            return n1 * value * value;
        }
        else if (value < 2f / d1)
        {
            return n1 * (value -= 1.5f / d1) * value + 0.75f;
        }
        else if (value < 2.5f / d1)
        {
            return n1 * (value -= 2.25f / d1) * value + 0.9375f;
        }
        else
        {
            return n1 * (value -= 2.625f / d1) * value + 0.984375f;
        }
    }
    
    // Public methods for external control
    public void StartMotion()
    {
        isMotionActive = true;
    }
    
    public void StopMotion()
    {
        isMotionActive = false;
    }
    
    public void ResetToStartPosition()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = startScale;
    }
    
    public void SetFloatingSpeed(float speed)
    {
        floatingSpeed = speed;
    }
    
    public void SetRotationSpeed(float x, float y, float z)
    {
        rotationSpeedX = x;
        rotationSpeedY = y;
        rotationSpeedZ = z;
    }
    
    public void SetRandomOffset()
    {
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }
    
    // Gizmos for visualization
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = gizmoColor;
        
        // Draw floating range
        if (enableFloating)
        {
            Vector3 floatDirection = Vector3.zero;
            switch (floatingAxis)
            {
                case MotionAxis.X:
                    floatDirection = Vector3.right;
                    break;
                case MotionAxis.Y:
                    floatDirection = Vector3.up;
                    break;
                case MotionAxis.Z:
                    floatDirection = Vector3.forward;
                    break;
            }
            
            Gizmos.DrawLine(center - floatDirection * floatingDistance, 
                          center + floatDirection * floatingDistance);
            Gizmos.DrawWireSphere(center - floatDirection * floatingDistance, 0.1f);
            Gizmos.DrawWireSphere(center + floatDirection * floatingDistance, 0.1f);
        }
        
        // Draw swaying range
        if (enableSwaying)
        {
            Vector3 swayDirection = Vector3.zero;
            switch (swayingAxis)
            {
                case MotionAxis.X:
                    swayDirection = Vector3.right;
                    break;
                case MotionAxis.Y:
                    swayDirection = Vector3.up;
                    break;
                case MotionAxis.Z:
                    swayDirection = Vector3.forward;
                    break;
            }
            
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
            Gizmos.DrawLine(center - swayDirection * swayingDistance, 
                          center + swayDirection * swayingDistance);
        }
    }
}
