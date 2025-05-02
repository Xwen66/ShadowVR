using UnityEngine;

namespace OculusInteraction
{
    public class InteractionManager : MonoBehaviour
    {
        public OVRCameraRig cameraRig;
        public LayerMask interactableLayers = 1;
        
        private GameObject leftHandObject;
        private GameObject rightHandObject;
        private HandInteractionController leftHandController;
        private HandInteractionController rightHandController;
        
        private void Start()
        {
            if (cameraRig == null)
            {
                cameraRig = FindObjectOfType<OVRCameraRig>();
                if (cameraRig == null)
                {
                    Debug.LogError("No OVRCameraRig found in the scene!");
                    return;
                }
            }
            
            SetupHands();
        }
        
        private void SetupHands()
        {
            // Get the hand anchors from OVRCameraRig
            Transform leftHandAnchor = cameraRig.leftHandAnchor;
            Transform rightHandAnchor = cameraRig.rightHandAnchor;
            
            if (leftHandAnchor == null || rightHandAnchor == null)
            {
                Debug.LogError("Hand anchors not found in OVRCameraRig!");
                return;
            }
            
            // Create visual representations for the hands
            leftHandObject = new GameObject("LeftHandVisual");
            leftHandObject.transform.SetParent(leftHandAnchor, false);
            CreateSimpleHandVisual(leftHandObject, Color.blue);
            
            rightHandObject = new GameObject("RightHandVisual");
            rightHandObject.transform.SetParent(rightHandAnchor, false);
            CreateSimpleHandVisual(rightHandObject, Color.red);
            
            // Add interaction controllers to the hands
            leftHandController = leftHandObject.AddComponent<HandInteractionController>();
            leftHandController.handType = HandInteractionController.HandType.Left;
            leftHandController.interactionLayers = interactableLayers;
            
            rightHandController = rightHandObject.AddComponent<HandInteractionController>();
            rightHandController.handType = HandInteractionController.HandType.Right;
            rightHandController.interactionLayers = interactableLayers;
            
            // Set up controller callbacks
            SetupControllerCallbacks(leftHandController);
            SetupControllerCallbacks(rightHandController);
        }
        
        private void CreateSimpleHandVisual(GameObject handObject, Color color)
        {
            // Create a simple cube to represent the controller
            GameObject visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualCube.transform.SetParent(handObject.transform, false);
            visualCube.transform.localScale = new Vector3(0.03f, 0.01f, 0.1f);
            visualCube.transform.localPosition = new Vector3(0, 0, 0.05f);
            
            // Create a line to represent the interaction ray
            GameObject rayCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rayCylinder.transform.SetParent(handObject.transform, false);
            rayCylinder.transform.localScale = new Vector3(0.005f, 0.5f, 0.005f);
            rayCylinder.transform.localPosition = new Vector3(0, 0, 0.5f);
            rayCylinder.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            // Set the materials
            Renderer cubeRenderer = visualCube.GetComponent<Renderer>();
            Renderer rayRenderer = rayCylinder.GetComponent<Renderer>();
            
            if (cubeRenderer != null)
            {
                cubeRenderer.material = new Material(Shader.Find("Standard"));
                cubeRenderer.material.color = color;
            }
            
            if (rayRenderer != null)
            {
                rayRenderer.material = new Material(Shader.Find("Standard"));
                rayRenderer.material.color = new Color(color.r, color.g, color.b, 0.5f);
            }
        }
        
        private void SetupControllerCallbacks(HandInteractionController controller)
        {
            // Set up callbacks to interact with objects
            controller.OnObjectHovered.AddListener((obj) => {
                if (obj != null)
                {
                    InteractableObject interactable = obj.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        interactable.OnHovered(true);
                    }
                }
            });
            
            controller.OnObjectSelected.AddListener((obj) => {
                if (obj != null)
                {
                    InteractableObject interactable = obj.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        interactable.OnSelected(true);
                    }
                }
            });
            
            controller.OnObjectDeselected.AddListener((obj) => {
                if (obj != null)
                {
                    InteractableObject interactable = obj.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        interactable.OnSelected(false);
                    }
                }
            });
        }
    }
}