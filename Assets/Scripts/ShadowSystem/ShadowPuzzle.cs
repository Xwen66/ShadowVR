using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace ShadowSystem
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ShadowPuzzle : MonoBehaviour
    {

        //require rigidbody
        [Header("Puzzle Identity")]
        [Tooltip("Name of this puzzle")]
        public string puzzleName = "Shadow Puzzle";
        
        [Header("Position GameObjects")]
        [Tooltip("GameObjects to use for positions (0=required position, 1=raycast source)")]
        [SerializeField] private GameObject[] positionObjects = new GameObject[2];
        
        [Header("Object Requirements")]
        [Tooltip("Required position for puzzle completion (auto-set from positionObjects[0])")]
        public Vector3 requiredPosition = Vector3.zero;
        
        [Tooltip("Position tolerance in units")]
        [Range(0.1f, 5f)]
        public float positionTolerance = 1f;
        
        [Header("Raycast Direction Requirements")]
        [Tooltip("Direction the raycast should come from (normalized)")]
        public Vector3 fromRaycastDirection = Vector3.down;
        
        [Tooltip("Direction tolerance in degrees")]
        [Range(1f, 90f)]
        public float directionTolerance = 60f;
        
        [Header("Rotation Requirements")]
        [Tooltip("Required rotation (Euler angles)")]
        public Vector3 requiredRotation = Vector3.zero;
        
        [Tooltip("Rotation tolerance in degrees")]
        [Range(1f, 45f)]
        public float rotationTolerance = 30f;
        
        [Tooltip("Only check Y rotation (facing direction) instead of full 3D rotation")]
        public bool onlyCheckYRotation = true;
        
        [Header("Raycast Source Requirements")]
        [Tooltip("Required position of the raycast source (auto-set from positionObjects[1])")]
        public Vector3 raycastSourceTargetPosition = Vector3.zero;
        
        [Tooltip("Raycast source position tolerance")]
        [Range(0.1f, 10f)]
        public float sourcePositionTolerance = 5f;
        
        [Header("Validation Settings")]
        [Tooltip("How long all conditions must be met before completion")]
        [Range(0f, 5f)]
        public float holdTime = 1f;
        
        [Tooltip("Use simplified validation (easier to meet)")]
        public bool useSimplifiedValidation = true;
        
        [Tooltip("Distance within which light source is considered 'close enough'")]
        [Range(1f, 20f)]
        public float lightProximityDistance = 10f;
        
        [Header("Visual Feedback")]
        [Tooltip("Show debug information")]
        public bool showDebugInfo = true;
        
        [Tooltip("Color for required position indicator")]
        public Color requiredPositionColor = Color.blue;
        
        [Tooltip("Color for raycast source indicator")]
        public Color raycastSourceColor = Color.cyan;
        
        [Header("Events")]
        [Tooltip("Called when puzzle is solved")]
        public UnityEvent OnPuzzleSolved;
        
        [Tooltip("Called when conditions change")]
        public UnityEvent<bool> OnConditionsChanged;
        
        [Tooltip("Called with progress (0-1)")]
        public UnityEvent<float> OnProgressChanged;
        
        [SerializeField] public bool ShowOnScreenDebug = false;
        
        // Private variables
        private bool isPuzzleSolved = false;
        private float conditionsMetTime = 0f;
        private bool lastConditionState = false;
        
        // Current raycast data (set by RaycastLight)
        private bool isBeingHitByRaycast = false;
        private Vector3 currentRaycastDirection = Vector3.zero;
        private Vector3 currentRaycastSourcePosition = Vector3.zero;
        private XRGrabInteractable xrGrabInteractable;
        void Start()
        {
            UpdatePositionsFromGameObjects();
            xrGrabInteractable = GetComponent<XRGrabInteractable>();
            xrGrabInteractable.useDynamicAttach = true;
            
        }
        
        void OnValidate()
        {
            // Update positions when values change in inspector
            UpdatePositionsFromGameObjects();
        }
        
        void UpdatePositionsFromGameObjects()
        {
            if (positionObjects != null)
            {
                // Required position from first GameObject

                
                if (positionObjects.Length > 0 && positionObjects[0] != null)
                {
                    requiredPosition = positionObjects[0].transform.position;
                }
                
                // Raycast source position from second GameObject
                if (positionObjects.Length > 1 && positionObjects[1] != null)
                {
                    raycastSourceTargetPosition = positionObjects[1].transform.position;
                }
            }
        }
        
        void Update()
        {
            if (isPuzzleSolved) return;
            
            ValidateConditions();
        }
        
        void ValidateConditions()
        {
            bool allConditionsMet = CheckAllConditions();
            
            if (allConditionsMet)
            {
                conditionsMetTime += Time.deltaTime;
                
                // Update progress
                float progress = holdTime > 0 ? Mathf.Clamp01(conditionsMetTime / holdTime) : 1f;
                OnProgressChanged?.Invoke(progress);
                
                // Check if held long enough
                if (conditionsMetTime >= holdTime)
                {
                    SolvePuzzle();
                }
            }
            else
            {
                conditionsMetTime = 0f;
                OnProgressChanged?.Invoke(0f);
            }
            
            // Notify if condition state changed
            if (allConditionsMet != lastConditionState)
            {
                OnConditionsChanged?.Invoke(allConditionsMet);
                lastConditionState = allConditionsMet;
                
                if (showDebugInfo)
                {
                    Debug.Log($"Puzzle '{puzzleName}' conditions: {(allConditionsMet ? "MET" : "NOT MET")}");
                }
            }
        }
        
        bool CheckAllConditions()
        {
            // Check if object is at required position
            if (!IsAtRequiredPosition())
            {
                if (showDebugInfo && Time.frameCount % 300 == 0)
                    Debug.Log($"'{puzzleName}': Object not at required position");
                return false;
            }
            
            // Check rotation
            if (!IsAtRequiredRotation())
            {
                if (showDebugInfo && Time.frameCount % 300 == 0)
                    Debug.Log($"'{puzzleName}': Object not at required rotation");
                return false;
            }
            
            // Check raycast conditions
            if (!IsRaycastConditionsMet())
            {
                if (showDebugInfo && Time.frameCount % 300 == 0)
                    Debug.Log($"'{puzzleName}': Raycast conditions not met");
                return false;
            }
            
            return true;
        }
        
        bool IsAtRequiredPosition()
        {
            float distance = Vector3.Distance(transform.position, requiredPosition);
            return distance <= positionTolerance;
        }
        
        bool IsAtRequiredRotation()
        {
            if (onlyCheckYRotation)
            {
                // Only compare Y rotation (facing direction)
                float currentY = transform.eulerAngles.y;
                float requiredY = requiredRotation.y;
                
                // Handle angle wrapping (0-360 degrees)
                float angleDifference = Mathf.DeltaAngle(currentY, requiredY);
                return Mathf.Abs(angleDifference) <= rotationTolerance;
            }
            else
            {
                // Full 3D rotation comparison
                float angleDifference = Quaternion.Angle(transform.rotation, Quaternion.Euler(requiredRotation));
                return angleDifference <= rotationTolerance;
            }
        }
        
        bool IsRaycastConditionsMet()
        {
            // Must be hit by raycast (this is the fundamental requirement)
            if (!isBeingHitByRaycast)
            {
                if (showDebugInfo && Time.frameCount % 60 == 0) // Every second at 60fps
                    Debug.Log($"'{puzzleName}': Not being hit by raycast");
                return false;
            }
            
            if (useSimplifiedValidation)
            {
                // Simplified validation: just check if light is reasonably close
                float sourceDistance = Vector3.Distance(currentRaycastSourcePosition, transform.position);
                if (sourceDistance > lightProximityDistance)
                {
                    if (showDebugInfo && Time.frameCount % 60 == 0)
                        Debug.Log($"'{puzzleName}': Light too far away {sourceDistance:F2}m > {lightProximityDistance}m");
                    return false;
                }
                
                // Optional: very loose direction check (if fromRaycastDirection is not zero)
                if (fromRaycastDirection != Vector3.zero)
                {
                    Vector3 incomingDirection = -currentRaycastDirection.normalized;
                    Vector3 requiredDirection = fromRaycastDirection.normalized;
                    float directionAngle = Vector3.Angle(incomingDirection, requiredDirection);
                    
                    // Very generous tolerance for simplified mode
                    float simplifiedTolerance = Mathf.Max(directionTolerance, 120f);
                    if (directionAngle > simplifiedTolerance)
                    {
                        if (showDebugInfo && Time.frameCount % 60 == 0)
                            Debug.Log($"'{puzzleName}': Direction too far off {directionAngle:F1}° > {simplifiedTolerance}°");
                        return false;
                    }
                }
                
                return true; // Simplified validation passed
            }
            else
            {
                // Original strict validation
                Vector3 incomingDirection = -currentRaycastDirection.normalized;
                Vector3 requiredDirection = fromRaycastDirection.normalized;
                float directionAngle = Vector3.Angle(incomingDirection, requiredDirection);
                
                if (directionAngle > directionTolerance)
                {
                    if (showDebugInfo && Time.frameCount % 60 == 0)
                        Debug.Log($"'{puzzleName}': Direction angle {directionAngle:F1}° > tolerance {directionTolerance}°");
                    return false;
                }
                
                // Check raycast source position
                float sourceDistance = Vector3.Distance(currentRaycastSourcePosition, raycastSourceTargetPosition);
                if (sourceDistance > sourcePositionTolerance)
                {
                    if (showDebugInfo && Time.frameCount % 60 == 0)
                        Debug.Log($"'{puzzleName}': Source distance {sourceDistance:F2} > tolerance {sourcePositionTolerance:F2}");
                    return false;
                }
                
                return true;
            }
        }
        
        void SolvePuzzle()
        {
            if (isPuzzleSolved) return;
            
            isPuzzleSolved = true;
            Debug.Log($"🎉 Puzzle '{puzzleName}' SOLVED!");
            
            OnPuzzleSolved?.Invoke();
            OnProgressChanged?.Invoke(1f);
        }
        
        // Called by RaycastLight when this puzzle is hit
        public void OnRaycastHit(Vector3 raycastDirection, Vector3 sourcePosition)
        {
            isBeingHitByRaycast = true;
            currentRaycastDirection = raycastDirection;
            currentRaycastSourcePosition = sourcePosition;
        }
        
        // Called by RaycastLight when raycast no longer hits this puzzle
        public void OnRaycastExit()
        {
            isBeingHitByRaycast = false;
            currentRaycastDirection = Vector3.zero;
            currentRaycastSourcePosition = Vector3.zero;
        }
        
        // Public methods for external control
        public void ResetPuzzle()
        {
            isPuzzleSolved = false;
            conditionsMetTime = 0f;
            lastConditionState = false;
            isBeingHitByRaycast = false;
            OnProgressChanged?.Invoke(0f);
            Debug.Log($"Puzzle '{puzzleName}' reset");
        }
        
        public bool IsSolved()
        {
            return isPuzzleSolved;
        }
        
        public float GetProgress()
        {
            return holdTime > 0 ? Mathf.Clamp01(conditionsMetTime / holdTime) : 0f;
        }
        
        // Helper methods for setup
        [ContextMenu("Set Current Position as Required")]
        public void SetCurrentAsRequired()
        {
            requiredPosition = transform.position;
            requiredRotation = transform.eulerAngles;
            
            // Update the GameObject reference if it exists
            if (positionObjects != null && positionObjects.Length > 0 && positionObjects[0] != null)
            {
                positionObjects[0].transform.position = requiredPosition;
                positionObjects[0].transform.rotation = Quaternion.Euler(requiredRotation);
            }
            
            Debug.Log($"Set current position and rotation as required for '{puzzleName}'");
        }
        
        [ContextMenu("Set Current Y Rotation as Required")]
        public void SetCurrentYRotationAsRequired()
        {
            // Only set the Y rotation, keep the rest
            requiredRotation = new Vector3(requiredRotation.x, transform.eulerAngles.y, requiredRotation.z);
            
            // Update the GameObject reference if it exists (only Y rotation)
            if (positionObjects != null && positionObjects.Length > 0 && positionObjects[0] != null)
            {
                Vector3 currentRot = positionObjects[0].transform.eulerAngles;
                positionObjects[0].transform.rotation = Quaternion.Euler(currentRot.x, transform.eulerAngles.y, currentRot.z);
            }
            
            Debug.Log($"Set current Y rotation ({transform.eulerAngles.y:F1}°) as required for '{puzzleName}'");
        }
        
        [ContextMenu("Create Position Markers")]
        public void CreatePositionMarkers()
        {
            if (positionObjects == null)
                positionObjects = new GameObject[2];
            
            // Create required position marker
            if (positionObjects.Length > 0 && positionObjects[0] == null)
            {
                GameObject requiredMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                requiredMarker.name = $"{puzzleName}_RequiredPosition";
                requiredMarker.transform.position = requiredPosition;
                requiredMarker.transform.rotation = Quaternion.Euler(requiredRotation);
                requiredMarker.transform.localScale = Vector3.one * 0.3f;
                
                // Make it a visual marker (no collider)
                Collider col = requiredMarker.GetComponent<Collider>();
                if (col != null) DestroyImmediate(col);
                
                // Color it blue for required
                Renderer renderer = requiredMarker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.blue;
                    renderer.material = mat;
                }
                
                positionObjects[0] = requiredMarker;
            }
            
            // Create raycast source marker
            if (positionObjects.Length > 1 && positionObjects[1] == null)
            {
                GameObject sourceMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                sourceMarker.name = $"{puzzleName}_RaycastSource";
                sourceMarker.transform.position = raycastSourceTargetPosition;
                sourceMarker.transform.localScale = Vector3.one * 0.2f;
                
                // Make it a visual marker (no collider)
                Collider col = sourceMarker.GetComponent<Collider>();
                if (col != null) DestroyImmediate(col);
                
                // Color it cyan for raycast source
                Renderer renderer = sourceMarker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = Color.cyan;
                    renderer.material = mat;
                }
                
                positionObjects[1] = sourceMarker;
            }
            
            Debug.Log($"Created position markers for '{puzzleName}'");
        }
        
        [ContextMenu("Snap to Required Position")]
        public void SnapToRequired()
        {
            transform.position = requiredPosition;
            transform.rotation = Quaternion.Euler(requiredRotation);
            Debug.Log($"Snapped '{puzzleName}' to required position and rotation");
        }
        
        // Add detailed raycast debugging method
        [ContextMenu("Debug Raycast Conditions")]
        public void DebugRaycastConditions()
        {
            Debug.Log($"=== Raycast Debug for '{puzzleName}' ===");
            Debug.Log($"Being hit by raycast: {isBeingHitByRaycast}");
            Debug.Log($"Validation mode: {(useSimplifiedValidation ? "SIMPLIFIED" : "STRICT")}");
            
            if (isBeingHitByRaycast)
            {
                Debug.Log($"Current raycast direction: {currentRaycastDirection}");
                Debug.Log($"Incoming direction (negated): {(-currentRaycastDirection).normalized}");
                Debug.Log($"Current source position: {currentRaycastSourcePosition}");
                
                if (useSimplifiedValidation)
                {
                    float sourceDistance = Vector3.Distance(currentRaycastSourcePosition, transform.position);
                    Debug.Log($"Light distance to object: {sourceDistance:F2}m (max: {lightProximityDistance}m) - {(sourceDistance <= lightProximityDistance ? "PASS" : "FAIL")}");
                    
                    if (fromRaycastDirection != Vector3.zero)
                    {
                        Vector3 incomingDir = (-currentRaycastDirection).normalized;
                        Vector3 requiredDir = fromRaycastDirection.normalized;
                        float dirAngle = Vector3.Angle(incomingDir, requiredDir);
                        float simplifiedTolerance = Mathf.Max(directionTolerance, 120f);
                        Debug.Log($"Direction angle: {dirAngle:F1}° (simplified tolerance: {simplifiedTolerance}°) - {(dirAngle <= simplifiedTolerance ? "PASS" : "FAIL")}");
                    }
                }
                else
                {
                    Debug.Log($"Required direction: {fromRaycastDirection.normalized}");
                    Vector3 incomingDir = (-currentRaycastDirection).normalized;
                    Vector3 requiredDir = fromRaycastDirection.normalized;
                    float dirAngle = Vector3.Angle(incomingDir, requiredDir);
                    Debug.Log($"Direction angle: {dirAngle:F1}° (tolerance: {directionTolerance}°) - {(dirAngle <= directionTolerance ? "PASS" : "FAIL")}");
                    
                    Debug.Log($"Required source position: {raycastSourceTargetPosition}");
                    float sourceDist = Vector3.Distance(currentRaycastSourcePosition, raycastSourceTargetPosition);
                    Debug.Log($"Source distance: {sourceDist:F2} (tolerance: {sourcePositionTolerance:F2}) - {(sourceDist <= sourcePositionTolerance ? "PASS" : "FAIL")}");
                }
            }
            else
            {
                Debug.Log("❌ Not being hit by any raycast!");
                Debug.Log("Check: Light on? Correct layer? Object has collider? LayerMask includes puzzle layer?");
            }
            
            Debug.Log($"Overall raycast conditions: {(IsRaycastConditionsMet() ? "✅ MET" : "❌ NOT MET")}");
        }
        
        void OnDrawGizmosSelected()
        {
            if (!showDebugInfo) return;
            
            // Draw required position (validation)
            Gizmos.color = requiredPositionColor;
            Gizmos.DrawWireSphere(requiredPosition, positionTolerance);
            Gizmos.DrawWireCube(requiredPosition, Vector3.one * 0.15f);
            
            // Draw raycast source target position
            Gizmos.color = raycastSourceColor;
            Gizmos.DrawWireSphere(raycastSourceTargetPosition, sourcePositionTolerance);
            
            // Draw required raycast direction
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, fromRaycastDirection * 2f);
            
            // Draw current incoming raycast direction if being hit
            if (isBeingHitByRaycast)
            {
                Vector3 incomingDirection = -currentRaycastDirection.normalized;
                float directionAngle = Vector3.Angle(incomingDirection, fromRaycastDirection.normalized);
                
                // Color based on direction validation
                Gizmos.color = directionAngle <= directionTolerance ? Color.green : Color.red;
                Gizmos.DrawRay(transform.position, incomingDirection * 2.5f);
                
                // Draw line from raycast source to this object
                Gizmos.color = Color.white;
                Gizmos.DrawLine(currentRaycastSourcePosition, transform.position);
                
                // Draw source position validation
                float sourceDistance = Vector3.Distance(currentRaycastSourcePosition, raycastSourceTargetPosition);
                Gizmos.color = sourceDistance <= sourcePositionTolerance ? Color.green : Color.red;
                Gizmos.DrawWireSphere(currentRaycastSourcePosition, 0.2f);
            }
            
            // Draw rotation visualization based on mode
            if (onlyCheckYRotation)
            {
                // Y-rotation only: draw horizontal facing directions
                Vector3 currentForwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                Vector3 requiredForwardFlat = Vector3.ProjectOnPlane(Quaternion.Euler(0, requiredRotation.y, 0) * Vector3.forward, Vector3.up).normalized;
                
                // Current facing direction (Y-only)
                Gizmos.color = IsAtRequiredRotation() ? Color.green : new Color(1f, 0.5f, 0f);
                Gizmos.DrawRay(transform.position, currentForwardFlat * 1.5f);
                
                // Required facing direction (Y-only)
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, requiredForwardFlat * 1.2f);
                
                // Draw Y-rotation tolerance arc
                float currentY = transform.eulerAngles.y;
                float requiredY = requiredRotation.y;
                
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Transparent yellow
                Vector3 center = transform.position + Vector3.up * 0.1f;
                
                // Draw tolerance arc
                for (int i = 0; i <= 20; i++)
                {
                    float angle1 = requiredY - rotationTolerance + (i * rotationTolerance * 2f / 20f);
                    float angle2 = requiredY - rotationTolerance + ((i + 1) * rotationTolerance * 2f / 20f);
                    
                    Vector3 point1 = center + Quaternion.Euler(0, angle1, 0) * Vector3.forward * 1f;
                    Vector3 point2 = center + Quaternion.Euler(0, angle2, 0) * Vector3.forward * 1f;
                    
                    Gizmos.DrawLine(point1, point2);
                }
            }
            else
            {
                // Full 3D rotation: original visualization
                Gizmos.color = IsAtRequiredRotation() ? Color.green : new Color(1f, 0.5f, 0f);
                Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
                
                Vector3 requiredForward = Quaternion.Euler(requiredRotation) * Vector3.forward;
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, requiredForward * 1.2f);
            }
            
            // Draw connection to required (validation - green/red)
            Gizmos.color = IsAtRequiredPosition() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, requiredPosition);
        }
        
        // Debug GUI
        void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 450, 350));
            GUILayout.Label($"Puzzle: {puzzleName}");
            GUILayout.Label($"Status: {(isPuzzleSolved ? "SOLVED" : "Active")}");
            GUILayout.Label($"Progress: {GetProgress():P1}");
            GUILayout.Label($"Hold Time: {conditionsMetTime:F1}s / {holdTime:F1}s");
            
            GUILayout.Space(5);
            GUILayout.Label("=== Conditions ===");
            GUILayout.Label($"Required Position: {(IsAtRequiredPosition() ? "✓" : "✗")} (dist: {Vector3.Distance(transform.position, requiredPosition):F2}m)");
            
            // Show rotation info based on mode
            if (onlyCheckYRotation)
            {
                float currentY = transform.eulerAngles.y;
                float requiredY = requiredRotation.y;
                float yDifference = Mathf.Abs(Mathf.DeltaAngle(currentY, requiredY));
                GUILayout.Label($"Y Rotation: {(IsAtRequiredRotation() ? "✓" : "✗")} (current: {currentY:F1}°, required: {requiredY:F1}°, diff: {yDifference:F1}°)");
            }
            else
            {
                float fullDifference = Quaternion.Angle(transform.rotation, Quaternion.Euler(requiredRotation));
                GUILayout.Label($"Full Rotation: {(IsAtRequiredRotation() ? "✓" : "✗")} (diff: {fullDifference:F1}°)");
            }
            
            GUILayout.Space(5);
            GUILayout.Label($"=== Raycast Status ({(useSimplifiedValidation ? "SIMPLIFIED" : "STRICT")}) ===");
            GUILayout.Label($"Being Hit by Light: {(isBeingHitByRaycast ? "✓" : "✗")}");
            
            if (isBeingHitByRaycast)
            {
                if (useSimplifiedValidation)
                {
                    float sourceDistance = Vector3.Distance(currentRaycastSourcePosition, transform.position);
                    GUILayout.Label($"Light Distance: {sourceDistance:F2}m / {lightProximityDistance}m {(sourceDistance <= lightProximityDistance ? "✓" : "✗")}");
                    
                    if (fromRaycastDirection != Vector3.zero)
                    {
                        Vector3 incomingDir = (-currentRaycastDirection).normalized;
                        Vector3 requiredDir = fromRaycastDirection.normalized;
                        float dirAngle = Vector3.Angle(incomingDir, requiredDir);
                        float simplifiedTolerance = Mathf.Max(directionTolerance, 120f);
                        GUILayout.Label($"Direction: {dirAngle:F1}° / {simplifiedTolerance}° {(dirAngle <= simplifiedTolerance ? "✓" : "✗")}");
                    }
                    else
                    {
                        GUILayout.Label("Direction: Not required ✓");
                    }
                }
                else
                {
                    Vector3 incomingDir = (-currentRaycastDirection).normalized;
                    Vector3 requiredDir = fromRaycastDirection.normalized;
                    float dirAngle = Vector3.Angle(incomingDir, requiredDir);
                    float sourceDist = Vector3.Distance(currentRaycastSourcePosition, raycastSourceTargetPosition);
                    
                    GUILayout.Label($"Direction Angle: {dirAngle:F1}° / {directionTolerance}° {(dirAngle <= directionTolerance ? "✓" : "✗")}");
                    GUILayout.Label($"Source Distance: {sourceDist:F2}m / {sourcePositionTolerance:F2}m {(sourceDist <= sourcePositionTolerance ? "✓" : "✗")}");
                }
                
                GUILayout.Label($"Light Position: {currentRaycastSourcePosition}");
            }
            else
            {
                GUILayout.Label("❌ No raycast hitting this puzzle");
                GUILayout.Label("Troubleshooting:");
                GUILayout.Label("• Is RaycastLight turned on?");
                GUILayout.Label("• Does puzzle have a Collider?");
                GUILayout.Label("• Is LayerMask set correctly?");
                GUILayout.Label("• Try 'Test Raycast' on RaycastLight");
            }
            
            GUILayout.Space(5);
            GUILayout.Label($"Overall Raycast: {(IsRaycastConditionsMet() ? "✅ MET" : "❌ NOT MET")}");
            
            if (useSimplifiedValidation)
            {
                GUILayout.Label("Using SIMPLIFIED mode (easier to solve)");
            }
            
            if (onlyCheckYRotation)
            {
                GUILayout.Label("Y-Rotation Only mode (facing direction)");
            }
            
            GUILayout.EndArea();
        }
    }
} 