using UnityEngine;
using Unity.XR.CoreUtils;
using GorillaLocomotion;

public class XRDistanceConstraint : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The GameObject to maintain distance from")]
    public Transform targetObject;
    
    [Tooltip("Use this object's position as reference point (if null, uses targetObject)")]
    public Transform referencePoint;
    
    [Header("Distance Constraints")]
    [Tooltip("Minimum distance from target (0 = no minimum)")]
    public float minDistance = 0f;
    
    [Tooltip("Maximum distance from target (0 = no maximum)")]
    public float maxDistance = 5f;
    
    [Header("Constraint Type")]
    [Tooltip("Shape of the constraint boundary")]
    public ConstraintType constraintType = ConstraintType.Sphere;
    
    [Tooltip("For cylinder constraints - ignore Y axis distance")]
    public bool ignoreVerticalDistance = false;
    
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
    
    [Tooltip("Gorilla Player to constrain (auto-detected if null)")]
    public Player gorillaPlayer;
    
    [Tooltip("Apply constraints in LateUpdate for smoother integration")]
    public bool useLateUpdate = true;
    
    [Header("Constraint Modes")]
    [Tooltip("What happens when constraint is violated")]
    public ConstraintMode constraintMode = ConstraintMode.ClampPosition;
    
    [Tooltip("Push back force when using Force mode")]
    public float pushBackForce = 100f;
    
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
    private Rigidbody playerRigidbody;
    private Vector3 targetPosition;
    private Vector3 lastValidPosition;
    private bool constraintViolated = false;
    
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
        Force,          // Apply force to push back
        Velocity,       // Modify velocity to prevent violation
        Warning         // Only show warnings, don't constrain
    }
    
    void Start()
    {
        InitializeComponents();
        if (targetObject != null)
            lastValidPosition = GetConstrainedPosition();
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
        
        // Auto-detect Gorilla Player if not assigned
        if (gorillaPlayer == null)
            gorillaPlayer = Player.Instance;
        
        if (gorillaPlayer != null)
            playerRigidbody = gorillaPlayer.GetComponent<Rigidbody>();
        
        // Use reference point or target object
        if (referencePoint == null && targetObject != null)
            referencePoint = targetObject;
        
        if (showDebugInfo)
        {
            Debug.Log($"XRDistanceConstraint initialized:");
            Debug.Log($"  XR Origin: {(xrOrigin != null ? xrOrigin.name : "Not found")}");
            Debug.Log($"  Gorilla Player: {(gorillaPlayer != null ? gorillaPlayer.name : "Not found")}");
            Debug.Log($"  Target: {(targetObject != null ? targetObject.name : "Not assigned")}");
        }
    }
    
    void Update()
    {
        if (!useLateUpdate)
            ApplyConstraints();
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
            
            switch (constraintMode)
            {
                case ConstraintMode.ClampPosition:
                    ApplyPositionCorrection(currentPosition, constrainedPosition);
                    break;
                    
                case ConstraintMode.Force:
                    ApplyForceCorrection(currentPosition);
                    break;
                    
                case ConstraintMode.Velocity:
                    ApplyVelocityCorrection(currentPosition);
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
        if (xrOriginTransform != null)
            return xrOriginTransform.position;
        else if (gorillaPlayer != null)
            return gorillaPlayer.transform.position;
        else
            return transform.position;
    }
    
    Vector3 GetConstrainedPosition()
    {
        Vector3 currentPos = GetCurrentPlayerPosition();
        Vector3 targetPos = referencePoint.position;
        
        Vector3 directionToTarget = currentPos - targetPos;
        float distance = GetDistanceToTarget(currentPos);
        
        // Apply minimum distance constraint
        if (minDistance > 0 && distance < minDistance)
        {
            Vector3 correctedDirection = GetConstraintDirection(directionToTarget);
            return targetPos + correctedDirection.normalized * minDistance;
        }
        
        // Apply maximum distance constraint
        if (maxDistance > 0 && distance > maxDistance)
        {
            Vector3 correctedDirection = GetConstraintDirection(directionToTarget);
            return targetPos + correctedDirection.normalized * maxDistance;
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
                direction = Vector3.ClampMagnitude(direction, maxDistance);
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
        if (minDistance > 0 && distance < minDistance)
            return true;
        if (maxDistance > 0 && distance > maxDistance)
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
        
        Vector3 newPosition = currentPosition + correctionVector;
        
        // Apply to appropriate system
        if (xrOriginTransform != null)
        {
            xrOriginTransform.position = newPosition;
        }
        else if (gorillaPlayer != null)
        {
            gorillaPlayer.transform.position = newPosition;
        }
    }
    
    void ApplyForceCorrection(Vector3 currentPosition)
    {
        if (playerRigidbody == null)
            return;
        
        Vector3 targetPos = referencePoint.position;
        Vector3 directionToTarget = (currentPosition - targetPos).normalized;
        float distance = GetDistanceToTarget(currentPosition);
        
        Vector3 forceDirection = Vector3.zero;
        
        if (minDistance > 0 && distance < minDistance)
        {
            forceDirection = directionToTarget; // Push away from target
        }
        else if (maxDistance > 0 && distance > maxDistance)
        {
            forceDirection = -directionToTarget; // Pull toward target
        }
        
        if (forceDirection != Vector3.zero)
        {
            playerRigidbody.AddForce(forceDirection * pushBackForce * Time.deltaTime, ForceMode.Force);
        }
    }
    
    void ApplyVelocityCorrection(Vector3 currentPosition)
    {
        if (playerRigidbody == null)
            return;
        
        Vector3 velocity = playerRigidbody.linearVelocity;
        Vector3 targetPos = referencePoint.position;
        Vector3 directionToTarget = (currentPosition - targetPos).normalized;
        
        // If moving away from valid area, reduce velocity in that direction
        if (Vector3.Dot(velocity.normalized, directionToTarget) > 0)
        {
            velocity = Vector3.ProjectOnPlane(velocity, directionToTarget);
            playerRigidbody.linearVelocity = velocity;
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
        minDistance = min;
        maxDistance = max;
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
    
    // Gizmo visualization
    void OnDrawGizmos()
    {
        if (!showGizmos || referencePoint == null)
            return;
        
        Gizmos.color = gizmoColor;
        Vector3 center = referencePoint.position;
        
        // Draw minimum distance boundary
        if (minDistance > 0)
        {
            Gizmos.color = Color.red;
            DrawConstraintShape(center, minDistance);
        }
        
        // Draw maximum distance boundary
        if (maxDistance > 0)
        {
            Gizmos.color = gizmoColor;
            DrawConstraintShape(center, maxDistance);
        }
        
        // Draw current player position
        Vector3 playerPos = GetCurrentPlayerPosition();
        Gizmos.color = constraintViolated ? Color.red : Color.green;
        Gizmos.DrawWireSphere(playerPos, 0.1f);
        
        // Draw line from target to player
        Gizmos.color = Color.white;
        Gizmos.DrawLine(center, playerPos);
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