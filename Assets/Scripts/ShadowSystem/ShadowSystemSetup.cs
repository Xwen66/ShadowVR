using UnityEngine;

namespace ShadowSystem
{
    public class ShadowSystemSetup : MonoBehaviour
    {
        [Header("Setup Configuration")]
        [Tooltip("Automatically setup the scene on Start")]
        public bool autoSetup = true;
        
        [Header("Layer Management")]
        [Tooltip("Unity Physics Layer for puzzle objects (used by raycast)")]
        public string puzzleLayerName = "Puzzles";
        
        [Tooltip("XR Interaction Layer for grabbing (0 = Default, 1 = Custom)")]
        [Range(0, 31)]
        public int xrInteractionLayer = 0; // Default interaction layer
        
        [Tooltip("Automatically manage both layer systems")]
        public bool autoManageLayers = true;
        
        [Header("Raycast Light")]
        [Tooltip("VR Controller to attach raycast light to")]
        public Transform vrController;
        
        [Tooltip("Create raycast light if none exists")]
        public bool createRaycastLight = true;
        
        [Header("Test Puzzles")]
        [Tooltip("Create test shadow puzzle objects")]
        public bool createTestPuzzles = true;
        
        [Tooltip("Number of test puzzles to create")]
        [Range(1, 5)]
        public int testPuzzleCount = 2;
        
        // Private variables
        private int puzzleLayer = 8; // Default to layer 8
        
        void Start()
        {
            if (autoSetup)
            {
                SetupShadowSystem();
            }
        }
        
        [ContextMenu("Setup Shadow System")]
        public void SetupShadowSystem()
        {
            Debug.Log("Setting up integrated Shadow System with dual layer support...");
            
            // Setup layers first (both Unity Physics and XR Interaction layers)
            if (autoManageLayers)
            {
                SetupIntegratedLayers();
            }
            
            // Setup raycast light
            if (createRaycastLight)
            {
                SetupRaycastLight();
            }
            
            // Create test puzzles
            if (createTestPuzzles)
            {
                CreateTestPuzzles();
            }
            
            Debug.Log("Integrated Shadow System setup complete!");
        }
        
        void SetupIntegratedLayers()
        {
            // Setup Unity Physics Layer for raycast targeting
            puzzleLayer = LayerMask.NameToLayer(puzzleLayerName);
            
            if (puzzleLayer == -1)
            {
                Debug.LogWarning($"Unity Physics Layer '{puzzleLayerName}' not found!");
                Debug.LogWarning("Please create it manually:");
                Debug.LogWarning("1. Edit → Project Settings → Tags and Layers");
                Debug.LogWarning($"2. Add '{puzzleLayerName}' to an empty User Layer slot");
                Debug.LogWarning("3. Re-run setup");
                
                // Fallback to Default layer
                puzzleLayer = 0;
                puzzleLayerName = "Default";
            }
            else
            {
                Debug.Log($"✅ Unity Physics Layer: '{puzzleLayerName}' (Layer {puzzleLayer}) - for raycast targeting");
            }
            
            // Log XR Interaction Layer info
            Debug.Log($"✅ XR Interaction Layer: {xrInteractionLayer} - for VR grabbing");
            Debug.Log("📋 Integration: Objects will use BOTH layer systems simultaneously");
        }
        
        void SetupRaycastLight()
        {
            // Find existing raycast light
            RaycastLight existingLight = FindObjectOfType<RaycastLight>();
            
            if (existingLight == null)
            {
                // Create new raycast light
                GameObject lightGO = new GameObject("Raycast Light");
                
                // Attach to VR controller if specified
                if (vrController != null)
                {
                    lightGO.transform.SetParent(vrController);
                    lightGO.transform.localPosition = Vector3.forward * 0.1f;
                    lightGO.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    // Position in world if no controller specified
                    lightGO.transform.position = new Vector3(0, 1.5f, 0);
                }
                
                // Add raycast light component
                RaycastLight raycastLight = lightGO.AddComponent<RaycastLight>();
                raycastLight.shootingDistance = 20f;
                raycastLight.raycastFrequency = 10f;
                raycastLight.showDebugRays = true;
                
                // Set LayerMask to target puzzle layer (Unity Physics Layer)
                raycastLight.raycastLayerMask = 1 << puzzleLayer;
                
                Debug.Log($"Created Raycast Light targeting '{puzzleLayerName}' layer (Physics)");
            }
            else
            {
                // Update existing light's LayerMask
                existingLight.raycastLayerMask = 1 << puzzleLayer;
                Debug.Log($"Updated Raycast Light to target '{puzzleLayerName}' layer (Physics)");
            }
        }
        
        void CreateTestPuzzles()
        {
            for (int i = 0; i < testPuzzleCount; i++)
            {
                // Create test puzzle object
                GameObject puzzleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                puzzleObj.name = $"TestPuzzle_{i + 1}";
                
                // Set Unity Physics Layer (for raycast targeting)
                puzzleObj.layer = puzzleLayer;
                
                // Position objects
                Vector3 startPosition = new Vector3(i * 2f - 1f, 1f, 3f);
                puzzleObj.transform.position = startPosition;
                puzzleObj.transform.localScale = Vector3.one * 0.7f;
                
                // Add Rigidbody for interaction
                Rigidbody rb = puzzleObj.AddComponent<Rigidbody>();
                rb.mass = 1f;
                
                // Add XRGrabInteractable for VR grabbing
                var grabInteractable = puzzleObj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                
                // Configure XR Interaction Layer (separate from Unity Physics Layer)
                grabInteractable.interactionLayers = 1 << xrInteractionLayer; // Convert to LayerMask
                
                // Configure grab settings
                grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
                grabInteractable.trackPosition = true;
                grabInteractable.trackRotation = true;
                grabInteractable.throwOnDetach = true;
                grabInteractable.retainTransformParent = true;
                
                // Add ShadowPuzzle component
                ShadowPuzzle shadowPuzzle = puzzleObj.AddComponent<ShadowPuzzle>();
                shadowPuzzle.puzzleName = puzzleObj.name;
                
                // Configure puzzle requirements
                shadowPuzzle.requiredPosition = startPosition + Vector3.forward * 1.5f; // Move 1.5m forward
                shadowPuzzle.positionTolerance = 0.5f;
                
                shadowPuzzle.fromRaycastDirection = Vector3.down; // Light should come from above
                shadowPuzzle.directionTolerance = 45f;
                
                shadowPuzzle.requiredRotation = new Vector3(0, 45f * i, 0); // Different rotation for each
                shadowPuzzle.rotationTolerance = 20f;
                
                shadowPuzzle.raycastSourceTargetPosition = new Vector3(0, 3f, 2f); // Light source position
                shadowPuzzle.sourcePositionTolerance = 2f;
                
                shadowPuzzle.holdTime = 1f;
                shadowPuzzle.showDebugInfo = true;
                
                // Create position markers automatically
                shadowPuzzle.CreatePositionMarkers();
                
                Debug.Log($"✅ Created {puzzleObj.name}:");
                Debug.Log($"   - Unity Layer: {puzzleLayerName} (for raycast)");
                Debug.Log($"   - XR Interaction Layer: {xrInteractionLayer} (for grabbing)");
            }
        }
        
        [ContextMenu("Find VR Controller")]
        public void FindVRController()
        {
            // Try to find Right Controller
            GameObject rightController = GameObject.Find("Right Controller");
            if (rightController != null)
            {
                vrController = rightController.transform;
                Debug.Log("Found Right Controller");
                return;
            }
            
            // Try to find any XR Controller
            UnityEngine.XR.Interaction.Toolkit.XRController[] controllers = 
                FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.XRController>();
            
            if (controllers.Length > 0)
            {
                vrController = controllers[0].transform;
                Debug.Log($"Found XR Controller: {vrController.name}");
            }
            else
            {
                Debug.LogWarning("No VR Controller found");
            }
        }
        
        [ContextMenu("Clear Test Puzzles")]
        public void ClearTestPuzzles()
        {
            // Find and destroy test puzzles
            ShadowPuzzle[] testPuzzles = FindObjectsOfType<ShadowPuzzle>();
            
            foreach (ShadowPuzzle puzzle in testPuzzles)
            {
                if (puzzle.name.Contains("TestPuzzle"))
                {
                    DestroyImmediate(puzzle.gameObject);
                }
            }
            
            Debug.Log("Cleared all test puzzles");
        }
        
        [ContextMenu("Snap All Puzzles to Required")]
        public void SnapAllPuzzlesToRequired()
        {
            ShadowPuzzle[] puzzles = FindObjectsOfType<ShadowPuzzle>();
            
            foreach (ShadowPuzzle puzzle in puzzles)
            {
                puzzle.SnapToRequired();
            }
            
            Debug.Log($"Snapped {puzzles.Length} puzzles to required positions");
        }
        
        [ContextMenu("Fix Existing Puzzles for Integration")]
        public void FixExistingPuzzlesForIntegration()
        {
            // Ensure layers are set up
            SetupIntegratedLayers();
            
            ShadowPuzzle[] puzzles = FindObjectsOfType<ShadowPuzzle>();
            int fixedCount = 0;
            
            foreach (ShadowPuzzle puzzle in puzzles)
            {
                bool hasChanges = false;
                
                // Set Unity Physics Layer
                if (puzzle.gameObject.layer != puzzleLayer)
                {
                    puzzle.gameObject.layer = puzzleLayer;
                    hasChanges = true;
                }
                
                // Add/Configure XRGrabInteractable
                var grabInteractable = puzzle.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = puzzle.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                    hasChanges = true;
                }
                
                // Configure XR Interaction Layer
                int expectedInteractionMask = 1 << xrInteractionLayer;
                if (grabInteractable.interactionLayers != expectedInteractionMask)
                {
                    grabInteractable.interactionLayers = expectedInteractionMask;
                    hasChanges = true;
                }
                
                // Configure grab settings
                grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
                grabInteractable.trackPosition = true;
                grabInteractable.trackRotation = true;
                grabInteractable.throwOnDetach = true;
                grabInteractable.retainTransformParent = true;
                
                // Ensure Rigidbody exists
                if (puzzle.GetComponent<Rigidbody>() == null)
                {
                    puzzle.gameObject.AddComponent<Rigidbody>().mass = 1f;
                    hasChanges = true;
                }
                
                if (hasChanges)
                {
                    fixedCount++;
                    Debug.Log($"Fixed {puzzle.puzzleName} for integrated layers");
                }
            }
            
            // Update RaycastLight
            RaycastLight light = FindObjectOfType<RaycastLight>();
            if (light != null)
            {
                light.raycastLayerMask = 1 << puzzleLayer;
                Debug.Log($"Updated RaycastLight to target {puzzleLayerName} layer");
            }
            
            Debug.Log($"🎉 Integration complete! Fixed {fixedCount} puzzle(s)");
            Debug.Log($"📋 Puzzles now use:");
            Debug.Log($"   - Unity Physics Layer '{puzzleLayerName}' for raycast detection");
            Debug.Log($"   - XR Interaction Layer {xrInteractionLayer} for VR grabbing");
        }
        
        [ContextMenu("Show Integration Status")]
        public void ShowIntegrationStatus()
        {
            Debug.Log("=== INTEGRATION STATUS ===");
            
            // Check Unity Physics Layer
            int layerCheck = LayerMask.NameToLayer(puzzleLayerName);
            if (layerCheck == -1)
            {
                Debug.LogError($"❌ Unity Physics Layer '{puzzleLayerName}' does NOT exist!");
            }
            else
            {
                Debug.Log($"✅ Unity Physics Layer '{puzzleLayerName}' exists (Layer {layerCheck})");
            }
            
            // Check XR Interaction Manager
            var xrManager = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            if (xrManager != null)
            {
                Debug.Log($"✅ XR Interaction Manager found");
                Debug.Log($"📋 Using XR Interaction Layer: {xrInteractionLayer}");
            }
            else
            {
                Debug.LogWarning("⚠️ No XR Interaction Manager found - VR grabbing may not work");
            }
            
            // Check RaycastLight configuration
            RaycastLight light = FindObjectOfType<RaycastLight>();
            if (light != null)
            {
                bool targetsCorrectLayer = (light.raycastLayerMask.value & (1 << layerCheck)) != 0;
                Debug.Log($"RaycastLight LayerMask: {light.raycastLayerMask.value}");
                Debug.Log($"Targets puzzle layer: {(targetsCorrectLayer ? "✅ YES" : "❌ NO")}");
            }
            
            // Check puzzle objects
            ShadowPuzzle[] puzzles = FindObjectsOfType<ShadowPuzzle>();
            Debug.Log($"\nFound {puzzles.Length} puzzle(s):");
            
            foreach (ShadowPuzzle puzzle in puzzles)
            {
                string layerName = LayerMask.LayerToName(puzzle.gameObject.layer);
                bool correctUnityLayer = puzzle.gameObject.layer == layerCheck;
                
                var grabInteractable = puzzle.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                bool hasGrabComponent = grabInteractable != null;
                bool correctXRLayer = hasGrabComponent && (grabInteractable.interactionLayers & (1 << xrInteractionLayer)) != 0;
                
                Debug.Log($"  {puzzle.puzzleName}:");
                Debug.Log($"    Unity Layer: {puzzle.gameObject.layer} ({layerName}) {(correctUnityLayer ? "✅" : "❌")}");
                Debug.Log($"    XR Grabbable: {(hasGrabComponent ? "✅" : "❌")}");
                Debug.Log($"    XR Layer: {(correctXRLayer ? "✅" : "❌")}");
            }
        }
        
        void OnDrawGizmosSelected()
        {
            if (!createTestPuzzles) return;
            
            // Draw test puzzle setup visualization
            Gizmos.color = Color.yellow;
            for (int i = 0; i < testPuzzleCount; i++)
            {
                Vector3 startPos = new Vector3(i * 2f - 1f, 1f, 3f);
                Vector3 targetPos = startPos + Vector3.forward * 1.5f;
                
                // Draw start position
                Gizmos.DrawWireCube(startPos, Vector3.one * 0.7f);
                
                // Draw target position
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(targetPos, 0.5f);
                
                // Draw connection
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(startPos, targetPos);
                
                Gizmos.color = Color.yellow;
            }
            
            // Draw raycast light position
            if (createRaycastLight)
            {
                Gizmos.color = Color.red;
                Vector3 lightPos = vrController != null ? 
                    vrController.position + vrController.forward * 0.1f : 
                    new Vector3(0, 1.5f, 0);
                Gizmos.DrawWireSphere(lightPos, 0.3f);
                Gizmos.DrawRay(lightPos, Vector3.forward * 5f);
            }
        }
    }
} 