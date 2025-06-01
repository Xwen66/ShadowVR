using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

namespace ShadowSystem
{
    public class RaycastLight : MonoBehaviour
    {
        [Header("Light Settings")]
        [Tooltip("Enable/disable the light")]
        public bool isLightOn = true;
        
        [Tooltip("Light component for visual representation")]
        public Light lightComponent;
        
        [Tooltip("Intensity of the light")]
        [Range(0f, 10f)]
        public float lightIntensity = 3f;
        
        [Tooltip("Range of the light")]
        [Range(1f, 50f)]
        public float lightRange = 15f;
        
        [Header("Raycast Settings")]
        [Tooltip("Maximum distance for raycast")]
        [Range(1f, 100f)]
        public float shootingDistance = 20f;
        
        [Tooltip("LayerMask for raycast targets")]
        public LayerMask raycastLayerMask = -1;
        
        [Tooltip("How often to perform raycasts per second")]
        [Range(1f, 60f)]
        public float raycastFrequency = 30f;
        
        [Header("Alternative Detection")]
        [Tooltip("Use sphere overlap as backup detection method")]
        public bool useBackupDetection = true;
        
        [Tooltip("Radius for backup sphere detection")]
        [Range(0.1f, 2f)]
        public float detectionRadius = 0.5f;
        
        [Header("VR Controller")]
        [Tooltip("XR Controller for input")]
        public XRController vrController;
        
        [Header("Visual Feedback")]
        [Tooltip("Show debug raycast lines")]
        public bool showDebugRays = true;
        
        [Tooltip("Color for raycast when hitting puzzle")]
        public Color hitColor = Color.green;
        
        [Tooltip("Color for raycast when not hitting puzzle")]
        public Color missColor = Color.red;
        
        [Tooltip("Width of debug ray line")]
        [Range(0.01f, 0.1f)]
        public float debugRayWidth = 0.02f;
        
        // Private variables
        private float raycastTimer = 0f;
        private List<ShadowPuzzle> currentlyHitPuzzles = new List<ShadowPuzzle>();
        private RaycastHit currentHit;
        private bool hasHit = false;
        private LineRenderer debugLineRenderer;
        private bool previousButtonState = false;
        
        void Start()
        {
            SetupLight();
            SetupDebugVisualization();
        }
        
        void SetupLight()
        {
            // Auto-find or create light component
            if (lightComponent == null)
            {
                lightComponent = GetComponent<Light>();
                if (lightComponent == null)
                {
                    lightComponent = gameObject.AddComponent<Light>();
                }
            }
            
            // Configure the light
            lightComponent.type = LightType.Spot;
            lightComponent.intensity = lightIntensity;
            lightComponent.range = lightRange;
            lightComponent.shadows = LightShadows.Soft;
            lightComponent.enabled = isLightOn;
            
            Debug.Log("Raycast Light initialized");
        }
        
        void SetupDebugVisualization()
        {
            if (!showDebugRays) return;
            
            // Create line renderer for debug visualization
            debugLineRenderer = gameObject.AddComponent<LineRenderer>();
            debugLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            debugLineRenderer.startWidth = debugRayWidth;
            debugLineRenderer.endWidth = debugRayWidth;
            debugLineRenderer.positionCount = 2;
            debugLineRenderer.useWorldSpace = true;
        }
        
        void Update()
        {
            HandleInput();
            UpdateLight();
            
            // Throttle raycast checks
            raycastTimer += Time.deltaTime;
            if (raycastTimer >= 1f / raycastFrequency)
            {
                raycastTimer = 0f;
                if (isLightOn)
                {
                    PerformRaycast();
                }
                else
                {
                    ClearAllHits();
                }
            }
            
            UpdateDebugVisualization();
        }
        
        void HandleInput()
        {
            // Handle VR controller input
            if (vrController != null)
            {
                bool triggerPressed = false;
                if (vrController.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerPressed))
                {
                    if (triggerPressed && !previousButtonState)
                    {
                        ToggleLight();
                    }
                    previousButtonState = triggerPressed;
                }
            }
            
            // Handle keyboard input for testing
            if (Input.GetKeyDown(KeyCode.L))
            {
                ToggleLight();
            }
        }
        
        void UpdateLight()
        {
            if (lightComponent != null)
            {
                lightComponent.intensity = lightIntensity;
                lightComponent.range = lightRange;
                lightComponent.enabled = isLightOn;
            }
        }
        
        void PerformRaycast()
        {
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = transform.forward;
            
            // Debug: Log raycast attempt
            if (showDebugRays && Time.frameCount % 300 == 0) // Every 5 seconds
            {
                Debug.Log($"RaycastLight: Performing raycast from {rayOrigin} in direction {rayDirection}");
                Debug.Log($"LayerMask: {raycastLayerMask.value}, Distance: {shootingDistance}");
            }
            
            // Perform the primary raycast
            hasHit = Physics.Raycast(rayOrigin, rayDirection, out currentHit, shootingDistance, raycastLayerMask);
            
            List<ShadowPuzzle> newHitPuzzles = new List<ShadowPuzzle>();
            
            if (hasHit)
            {
                if (showDebugRays && Time.frameCount % 300 == 0)
                {
                    Debug.Log($"Raycast HIT: {currentHit.collider.name} at distance {currentHit.distance:F2}");
                }
                
                // Check hit object and its parents for ShadowPuzzle component
                ShadowPuzzle hitPuzzle = GetShadowPuzzleComponent(currentHit.collider);
                if (hitPuzzle != null)
                {
                    newHitPuzzles.Add(hitPuzzle);
                    hitPuzzle.OnRaycastHit(rayDirection, rayOrigin);
                    
                    if (showDebugRays && Time.frameCount % 300 == 0)
                    {
                        Debug.Log($"Found ShadowPuzzle: {hitPuzzle.puzzleName}");
                    }
                }
                else if (showDebugRays && Time.frameCount % 300 == 0)
                {
                    Debug.Log($"Hit object '{currentHit.collider.name}' has no ShadowPuzzle component");
                }
            }
            else if (showDebugRays && Time.frameCount % 300 == 0)
            {
                Debug.Log("Raycast MISSED - no objects hit");
            }
            
            // Backup detection using sphere overlap
            if (useBackupDetection && newHitPuzzles.Count == 0)
            {
                Vector3 sphereCenter = rayOrigin + rayDirection * (shootingDistance * 0.5f);
                Collider[] overlapping = Physics.OverlapSphere(sphereCenter, detectionRadius, raycastLayerMask);
                
                foreach (Collider col in overlapping)
                {
                    ShadowPuzzle puzzle = GetShadowPuzzleComponent(col);
                    if (puzzle != null && !newHitPuzzles.Contains(puzzle))
                    {
                        newHitPuzzles.Add(puzzle);
                        puzzle.OnRaycastHit(rayDirection, rayOrigin);
                        
                        if (showDebugRays && Time.frameCount % 300 == 0)
                        {
                            Debug.Log($"Backup detection found: {puzzle.puzzleName}");
                        }
                    }
                }
            }
            
            // Handle puzzles that are no longer being hit
            foreach (ShadowPuzzle puzzle in currentlyHitPuzzles)
            {
                if (!newHitPuzzles.Contains(puzzle))
                {
                    puzzle.OnRaycastExit();
                }
            }
            
            // Update the list
            currentlyHitPuzzles = newHitPuzzles;
        }
        
        // Helper method to find ShadowPuzzle component on object or its parents
        ShadowPuzzle GetShadowPuzzleComponent(Collider collider)
        {
            // First check the collider's GameObject
            ShadowPuzzle puzzle = collider.GetComponent<ShadowPuzzle>();
            if (puzzle != null) return puzzle;
            
            // Then check parent objects
            Transform current = collider.transform.parent;
            while (current != null)
            {
                puzzle = current.GetComponent<ShadowPuzzle>();
                if (puzzle != null) return puzzle;
                current = current.parent;
            }
            
            return null;
        }
        
        void ClearAllHits()
        {
            // Inform all currently hit puzzles that they're no longer being hit
            foreach (ShadowPuzzle puzzle in currentlyHitPuzzles)
            {
                puzzle.OnRaycastExit();
            }
            currentlyHitPuzzles.Clear();
            hasHit = false;
        }
        
        void UpdateDebugVisualization()
        {
            if (!showDebugRays || debugLineRenderer == null) return;
            
            Vector3 startPos = transform.position;
            Vector3 endPos;
            Color rayColor;
            
            if (isLightOn)
            {
                if (hasHit)
                {
                    endPos = currentHit.point;
                    rayColor = currentlyHitPuzzles.Count > 0 ? hitColor : missColor;
                }
                else
                {
                    endPos = startPos + transform.forward * shootingDistance;
                    rayColor = missColor;
                }
                
                debugLineRenderer.enabled = true;
                debugLineRenderer.SetPosition(0, startPos);
                debugLineRenderer.SetPosition(1, endPos);
                debugLineRenderer.startColor = rayColor;
                debugLineRenderer.endColor = rayColor;
            }
            else
            {
                debugLineRenderer.enabled = false;
            }
        }
        
        public void ToggleLight()
        {
            isLightOn = !isLightOn;
            Debug.Log($"Raycast Light {(isLightOn ? "ON" : "OFF")}");
            
            if (!isLightOn)
            {
                ClearAllHits();
            }
        }
        
        public void SetIntensity(float intensity)
        {
            lightIntensity = Mathf.Clamp(intensity, 0f, 10f);
        }
        
        public void SetRange(float range)
        {
            lightRange = Mathf.Clamp(range, 1f, 50f);
        }
        
        public void SetShootingDistance(float distance)
        {
            shootingDistance = Mathf.Clamp(distance, 1f, 100f);
        }
        
        // Get current light state
        public bool IsOn()
        {
            return isLightOn;
        }
        
        public Vector3 GetPosition()
        {
            return transform.position;
        }
        
        public Vector3 GetDirection()
        {
            return transform.forward;
        }
        
        public List<ShadowPuzzle> GetCurrentlyHitPuzzles()
        {
            return new List<ShadowPuzzle>(currentlyHitPuzzles);
        }
        
        public bool IsHittingPuzzle()
        {
            return currentlyHitPuzzles.Count > 0;
        }
        
        public float GetHitDistance()
        {
            return hasHit ? currentHit.distance : shootingDistance;
        }
        
        // Helper methods for setup
        [ContextMenu("Test Raycast")]
        public void TestRaycast()
        {
            Debug.Log("=== RAYCAST TEST ===");
            PerformRaycast();
            
            Debug.Log($"Light On: {isLightOn}");
            Debug.Log($"LayerMask Value: {raycastLayerMask.value}");
            Debug.Log($"Shooting Distance: {shootingDistance}");
            
            if (hasHit)
            {
                Debug.Log($"✅ Raycast HIT: {currentHit.collider.name} at distance {currentHit.distance:F2}");
                Debug.Log($"Hit Point: {currentHit.point}");
                Debug.Log($"Hit Object Layer: {currentHit.collider.gameObject.layer}");
                
                ShadowPuzzle puzzle = GetShadowPuzzleComponent(currentHit.collider);
                if (puzzle != null)
                {
                    Debug.Log($"✅ Found ShadowPuzzle: {puzzle.puzzleName}");
                    Debug.Log($"Currently hitting {currentlyHitPuzzles.Count} puzzles");
                }
                else
                {
                    Debug.Log($"❌ No ShadowPuzzle component found on {currentHit.collider.name}");
                    Debug.Log("Checking all components on hit object:");
                    Component[] components = currentHit.collider.GetComponents<Component>();
                    foreach (Component comp in components)
                    {
                        Debug.Log($"  - {comp.GetType().Name}");
                    }
                }
            }
            else
            {
                Debug.Log("❌ Raycast MISSED - no objects hit");
            }
        }
        
        void OnDrawGizmosSelected()
        {
            if (!showDebugRays) return;
            
            Vector3 startPos = transform.position;
            Vector3 direction = transform.forward;
            
            // Draw raycast direction
            Gizmos.color = isLightOn ? (IsHittingPuzzle() ? hitColor : missColor) : Color.gray;
            
            if (hasHit && isLightOn)
            {
                Gizmos.DrawLine(startPos, currentHit.point);
                Gizmos.DrawWireSphere(currentHit.point, 0.1f);
            }
            else
            {
                Gizmos.DrawRay(startPos, direction * shootingDistance);
            }
            
            // Draw shooting distance indicator
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(startPos + direction * shootingDistance, 0.2f);
            
            // Draw backup detection sphere
            if (useBackupDetection)
            {
                Vector3 sphereCenter = startPos + direction * (shootingDistance * 0.5f);
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Transparent yellow
                Gizmos.DrawSphere(sphereCenter, detectionRadius);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(sphereCenter, detectionRadius);
            }
        }
        
        void OnDestroy()
        {
            // Clean up when destroyed
            ClearAllHits();
        }
    }
} 