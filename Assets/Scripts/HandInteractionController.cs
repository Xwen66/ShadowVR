using UnityEngine;
using UnityEngine.Events;

namespace OculusInteraction
{
    public class HandInteractionController : MonoBehaviour
    {
        public enum HandType
        {
            Left,
            Right
        }

        public HandType handType;
        public Transform rayOrigin;
        public LayerMask interactionLayers = 1;
        public float maxRayDistance = 100f;
        public float grabDistance = 0.1f;
        
        public UnityEvent<GameObject> OnObjectHovered;
        public UnityEvent<GameObject> OnObjectSelected;
        public UnityEvent<GameObject> OnObjectDeselected;
        
        private GameObject currentHoveredObject;
        private GameObject currentSelectedObject;
        private bool triggerPressed;
        private OVRInput.Controller controllerMask;

        private void Start()
        {
            // Set up the correct controller mask based on hand type
            controllerMask = (handType == HandType.Left) ? 
                OVRInput.Controller.LTouch : 
                OVRInput.Controller.RTouch;
                
            // If no ray origin assigned, use this transform
            if (rayOrigin == null)
            {
                rayOrigin = transform;
            }
            
            // Initialize events if null
            if (OnObjectHovered == null)
                OnObjectHovered = new UnityEvent<GameObject>();
            if (OnObjectSelected == null)
                OnObjectSelected = new UnityEvent<GameObject>();
            if (OnObjectDeselected == null)
                OnObjectDeselected = new UnityEvent<GameObject>();
        }
        
        private void Update()
        {
            // Check for trigger press/release
            bool triggerPressedThisFrame = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controllerMask);
            
            // Handle ray interaction
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out hit, maxRayDistance, interactionLayers))
            {
                // Handle hovering
                if (currentHoveredObject != hit.collider.gameObject)
                {
                    currentHoveredObject = hit.collider.gameObject;
                    OnObjectHovered.Invoke(currentHoveredObject);
                }
                
                // Handle selection
                if (triggerPressedThisFrame && !triggerPressed)
                {
                    triggerPressed = true;
                    currentSelectedObject = hit.collider.gameObject;
                    OnObjectSelected.Invoke(currentSelectedObject);
                }
            }
            else
            {
                // Clear hovering when not pointing at anything
                if (currentHoveredObject != null)
                {
                    currentHoveredObject = null;
                    OnObjectHovered.Invoke(null);
                }
            }
            
            // Handle deselection
            if (!triggerPressedThisFrame && triggerPressed)
            {
                triggerPressed = false;
                if (currentSelectedObject != null)
                {
                    OnObjectDeselected.Invoke(currentSelectedObject);
                    currentSelectedObject = null;
                }
            }
        }
        
        // Helper method to check if an object is within grab distance
        public bool IsInGrabRange(GameObject obj)
        {
            if (obj == null) return false;
            
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            return distance <= grabDistance;
        }
        
        // Visual debug
        private void OnDrawGizmosSelected()
        {
            if (rayOrigin != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * maxRayDistance);
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, grabDistance);
            }
        }
    }
}