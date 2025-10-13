using UnityEngine;
using Unity.XR.CoreUtils;

public class XRDistanceConstraint : MonoBehaviour
{
    [Header("Sphere Constraint Settings")]
    [Tooltip("The GameObject to maintain distance from (center of sphere)")]
    public Transform targetObject;
    
    [Tooltip("Use this object's position as reference point (if null, uses targetObject)")]
    public Transform referencePoint;
    
    [Header("Sphere Range")]
    [Tooltip("Sphere radius - maximum distance camera can be from target")]
    [Range(0.1f, 50f)]
    public float sphereRadius = 5f;
    
    [Tooltip("Inner dead zone radius (0 = no minimum distance)")]
    [Range(0f, 10f)]
    public float innerRadius = 0f;
    
    [Header("Camera Constraint")]
    [Tooltip("Constrain based on camera position instead of XR Origin position")]
    public bool useCameraPosition = true;
    
    [Tooltip("Shape of the constraint boundary")]
    public ConstraintType constraintType = ConstraintType.Sphere;
    
    [Header("Movement Correction")]
    [Tooltip("How smoothly to correct position when violating constraints")]
    [Range(0.1f, 1f)]
    public float correctionStrength = 0.8f;
    
    [Tooltip("Use smooth correction instead of instant snapping")]
    public bool useSmoothCorrection = true;
    
    [Tooltip("Correction speed for smooth mode")]
    public float correctionSpeed = 10f;
    
    [Header("Integration Settings")]
    [Tooltip("XR Origin to constrain (auto-detected if null)")]
    public XROrigin xrOrigin;
    
    [Tooltip("Apply constraints in LateUpdate for smoother integration")]
    public bool useLateUpdate = true;
    
    [Header("Constraint Modes")]
    [Tooltip("What happens when constraint is violated")]
    public ConstraintMode constraintMode = ConstraintMode.ClampPosition;
    
    [Header("Debug")]
    [Tooltip("Show debug information")]
    public bool showDebugInfo = true;
    
    [Tooltip("Show constraint boundaries in scene view")]
    public bool showGizmos = true;
    
    [Tooltip("Color for constraint gizmos")]
    public Color gizmoColor = Color.cyan;
    
    // Private variables
    private Transform xrOriginTransform;
    private Transform cameraTransform;
    private CharacterController characterController;
    private Vector3 targetPosition;
    private Vector3 lastValidPosition;
    private bool constraintViolated = false;
    private float gameStartTime = 0f;
    private bool canLogConstraint = false;
    private bool hasTriggeredDialogue = false;
    private float constraintTouchTime = 0f;
    private bool isWaitingForDialogue = false;
    
    // Enums
    public enum ConstraintType
    {
        Sphere,         // Distance in all directions
        Cylinder,       // Distance ignoring Y axis
        Box,            // Rectangular boundary
        HorizontalOnly  // Only X and Z axes
    }
    
    public enum ConstraintMode
    {
        ClampPosition,  // Directly modify position
        Warning         // Only show warnings, don't constrain
    }
    
    void Start()
    {
        InitializeComponents();
        if (targetObject != null)
            lastValidPosition = GetConstrainedPosition();
        
        gameStartTime = Time.time;
        canLogConstraint = false;
    }
    
    void InitializeComponents()
    {
        // Auto-detect XR Origin if not assigned
        if (xrOrigin == null)
            xrOrigin = FindObjectOfType<XROrigin>();
        
        if (xrOrigin != null)
        {
            xrOriginTransform = xrOrigin.transform;
            cameraTransform = xrOrigin.Camera?.transform;
            characterController = xrOrigin.GetComponent<CharacterController>();
        }
        
        // Use reference point or target object
        if (referencePoint == null && targetObject != null)
            referencePoint = targetObject;
        
        if (showDebugInfo)
        {
            Debug.Log($"XRDistanceConstraint initialized:");
            Debug.Log($"  XR Origin: {(xrOrigin != null ? xrOrigin.name : "Not found")}");
            Debug.Log($"  Target: {(targetObject != null ? targetObject.name : "Not assigned")}");
        }
    }
    
    void Update()
    {
        if (!useLateUpdate)
            ApplyConstraints();
        
        // 检查是否已经过了5秒
        if (!canLogConstraint && Time.time - gameStartTime >= 5f)
        {
            canLogConstraint = true;
            if (showDebugInfo)
                Debug.Log("距离约束日志功能已启用（游戏开始5秒后）");
        }
        
        // 检查是否等待对话触发且已经过了3秒
        if (isWaitingForDialogue && Time.time - constraintTouchTime >= 3f)
        {
            TriggerDialogue(1);
            DialogueManager.Instance.SetNextButtonMode(true);//关闭对话框模式
            hasTriggeredDialogue = true;
            isWaitingForDialogue = false;
            if (showDebugInfo)
                Debug.Log("3秒等待结束，触发对话");
        }
    }
    
    void LateUpdate()
    {
        if (useLateUpdate)
            ApplyConstraints();
    }
    
    void ApplyConstraints()
    {
        if (targetObject == null || referencePoint == null)
            return;
        
        Vector3 currentPosition = GetCurrentPlayerPosition();
        Vector3 constrainedPosition = GetConstrainedPosition();
        
        float currentDistance = GetDistanceToTarget(currentPosition);
        bool isViolating = IsConstraintViolated(currentDistance);
        
        if (isViolating)
        {
            constraintViolated = true;
            
            // 添加触碰限制时的日志输出（仅在游戏开始5秒后且未触发过对话）
            if (showDebugInfo && canLogConstraint && !hasTriggeredDialogue && !isWaitingForDialogue)
            {
                string violationType = "";
                if (innerRadius > 0 && currentDistance < innerRadius)
                    violationType = "太靠近目标";
                else if (sphereRadius > 0 && currentDistance > sphereRadius)
                    violationType = "离目标太远";
                
                Debug.Log($"玩家触碰到距离限制！类型: {violationType}, 当前距离: {currentDistance:F2}, 限制范围: [{innerRadius:F2}, {sphereRadius:F2}]");
                
                // 开始3秒等待计时
                constraintTouchTime = Time.time;
                isWaitingForDialogue = true;
                if (showDebugInfo)
                    Debug.Log("开始3秒等待，之后将触发对话");
            }
            
            switch (constraintMode)
            {
                case ConstraintMode.ClampPosition:
                    ApplyPositionCorrection(currentPosition, constrainedPosition);
                    break;
                    
                case ConstraintMode.Warning:
                    if (showDebugInfo)
                        Debug.LogWarning($"Distance constraint violated! Distance: {currentDistance:F2}");
                    break;
            }
        }
        else
        {
            constraintViolated = false;
            lastValidPosition = currentPosition;
        }
    }
    
    Vector3 GetCurrentPlayerPosition()
    {
        // Use camera position when enabled for true VR camera constraint
        if (useCameraPosition && cameraTransform != null)
            return cameraTransform.position;
        else if (xrOriginTransform != null)
            return xrOriginTransform.position;
        else
            return transform.position;
    }
    
    Vector3 GetConstrainedPosition()
    {
        Vector3 currentPos = GetCurrentPlayerPosition();
        Vector3 targetPos = referencePoint.position;
        
        Vector3 directionToTarget = currentPos - targetPos;
        float distance = GetDistanceToTarget(currentPos);
        
        // Apply minimum distance constraint (inner radius)
        if (innerRadius > 0 && distance < innerRadius)
        {
            Vector3 correctedDirection = GetConstraintDirection(directionToTarget);
            return targetPos + correctedDirection.normalized * innerRadius;
        }
        
        // Apply maximum distance constraint (sphere radius)
        if (sphereRadius > 0 && distance > sphereRadius)
        {
            Vector3 correctedDirection = GetConstraintDirection(directionToTarget);
            return targetPos + correctedDirection.normalized * sphereRadius;
        }
        
        return currentPos;
    }
    
    Vector3 GetConstraintDirection(Vector3 direction)
    {
        switch (constraintType)
        {
            case ConstraintType.Cylinder:
            case ConstraintType.HorizontalOnly:
                direction.y = 0;
                break;
                
            case ConstraintType.Box:
                // For box constraints, clamp each axis separately
                direction = Vector3.ClampMagnitude(direction, sphereRadius);
                break;
        }
        
        return direction;
    }
    
    float GetDistanceToTarget(Vector3 position)
    {
        Vector3 diff = position - referencePoint.position;
        
        switch (constraintType)
        {
            case ConstraintType.Sphere:
                return diff.magnitude;
                
            case ConstraintType.Cylinder:
            case ConstraintType.HorizontalOnly:
                diff.y = 0;
                return diff.magnitude;
                
            case ConstraintType.Box:
                return Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y), Mathf.Abs(diff.z));
                
            default:
                return diff.magnitude;
        }
    }
    
    bool IsConstraintViolated(float distance)
    {
        if (innerRadius > 0 && distance < innerRadius)
            return true;
        if (sphereRadius > 0 && distance > sphereRadius)
            return true;
        return false;
    }
    
    void ApplyPositionCorrection(Vector3 currentPosition, Vector3 constrainedPosition)
    {
        Vector3 correctionVector = constrainedPosition - currentPosition;
        
        if (useSmoothCorrection)
        {
            correctionVector *= correctionSpeed * Time.deltaTime;
        }
        else
        {
            correctionVector *= correctionStrength;
        }
        
        // For camera-based constraints, adjust XR Origin to compensate for head tracking
        if (useCameraPosition && cameraTransform != null && xrOriginTransform != null)
        {
            // Calculate offset from XR Origin to camera
            Vector3 headOffset = cameraTransform.position - xrOriginTransform.position;
            
            // Target position for camera
            Vector3 targetCameraPosition = currentPosition + correctionVector;
            
            // Move XR Origin so camera ends up at target position
            Vector3 newXROriginPosition = targetCameraPosition - headOffset;
            xrOriginTransform.position = newXROriginPosition;
        }
        else
        {
            // Standard XR Origin constraint
            Vector3 newPosition = currentPosition + correctionVector;
            
            if (xrOriginTransform != null)
            {
                xrOriginTransform.position = newPosition;
            }
        }
    }
    
    // Public methods for external control
    public void SetTargetObject(Transform newTarget)
    {
        targetObject = newTarget;
        referencePoint = newTarget;
    }
    
    public void SetDistanceConstraints(float min, float max)
    {
        innerRadius = min;
        sphereRadius = max;
    }
    
    public void EnableConstraint(bool enable)
    {
        enabled = enable;
    }
    
    public bool IsConstraintCurrentlyViolated()
    {
        return constraintViolated;
    }
    
    public float GetCurrentDistance()
    {
        return GetDistanceToTarget(GetCurrentPlayerPosition());
    }

    // 触发对话方法
    private void TriggerDialogue(int dialogueNumber)
    {
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.GoToDialogue(dialogueNumber);
            Debug.Log($"触发了对话 #{dialogueNumber}");
        }
        else
        {
            Debug.LogWarning("未找到 DialogueManager 组件");
        }
    }
    
    // Gizmo visualization
    void OnDrawGizmos()
    {
        if (!showGizmos || referencePoint == null)
            return;
        
        Gizmos.color = gizmoColor;
        Vector3 center = referencePoint.position;
        
        // Draw minimum distance boundary
        if (innerRadius > 0)    
        {
            Gizmos.color = Color.red;
            DrawConstraintShape(center, innerRadius);
        }
        
        // Draw maximum distance boundary
        if (sphereRadius > 0)
        {
            Gizmos.color = gizmoColor;
            DrawConstraintShape(center, sphereRadius);
        }
        
        // Draw current player/camera position
        Vector3 playerPos = GetCurrentPlayerPosition();
        Gizmos.color = constraintViolated ? Color.red : Color.green;
        Gizmos.DrawWireSphere(playerPos, 0.1f);
        
        // Draw line from target to player/camera
        Gizmos.color = Color.white;
        Gizmos.DrawLine(center, playerPos);
        
        // If using camera position, also show XR Origin position
        if (useCameraPosition && cameraTransform != null && xrOriginTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(xrOriginTransform.position, Vector3.one * 0.2f);
            Gizmos.DrawLine(xrOriginTransform.position, cameraTransform.position);
        }
    }
    
    void DrawConstraintShape(Vector3 center, float radius)
    {
        switch (constraintType)
        {
            case ConstraintType.Sphere:
                Gizmos.DrawWireSphere(center, radius);
                break;
                
            case ConstraintType.Cylinder:
                // Draw cylinder (simplified as circles)
                DrawWireCylinder(center, radius, 4f);
                break;
                
            case ConstraintType.HorizontalOnly:
                Gizmos.DrawWireSphere(new Vector3(center.x, GetCurrentPlayerPosition().y, center.z), radius);
                break;
                
            case ConstraintType.Box:
                Gizmos.DrawWireCube(center, Vector3.one * radius * 2f);
                break;
        }
    }
    
    void DrawWireCylinder(Vector3 center, float radius, float height)
    {
        // Top circle
        DrawWireCircle(center + Vector3.up * height * 0.5f, radius, Vector3.up);
        // Bottom circle
        DrawWireCircle(center - Vector3.up * height * 0.5f, radius, Vector3.up);
        
        // Vertical lines
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(center + Vector3.up * height * 0.5f + offset, 
                           center - Vector3.up * height * 0.5f + offset);
        }
    }
    
    void DrawWireCircle(Vector3 center, float radius, Vector3 normal)
    {
        Vector3 forward = Vector3.Slerp(Vector3.forward, -normal, 0.5f);
        Vector3 right = Vector3.Cross(normal, forward);
        
        for (int i = 0; i < 32; i++)
        {
            float angle1 = i * 360f / 32f * Mathf.Deg2Rad;
            float angle2 = (i + 1) * 360f / 32f * Mathf.Deg2Rad;
            
            Vector3 pos1 = center + (Mathf.Cos(angle1) * right + Mathf.Sin(angle1) * forward) * radius;
            Vector3 pos2 = center + (Mathf.Cos(angle2) * right + Mathf.Sin(angle2) * forward) * radius;
            
            Gizmos.DrawLine(pos1, pos2);
        }
    }
} 