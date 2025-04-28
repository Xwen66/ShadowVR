using UnityEngine;
using UnityEngine.Events;

namespace OculusInteraction
{
    public class InteractableObject : MonoBehaviour
    {
        public Color normalColor = Color.white;
        public Color hoveredColor = Color.yellow;
        public Color selectedColor = Color.green;
        
        public bool canBeGrabbed = true;
        public bool canBeTeleported = false;
        
        public UnityEvent OnHoverEnter;
        public UnityEvent OnHoverExit;
        public UnityEvent OnSelect;
        public UnityEvent OnDeselect;
        
        private Renderer objectRenderer;
        private Color originalColor;
        private MaterialPropertyBlock propBlock;
        private bool isHovered = false;
        private bool isSelected = false;
        
        private void Awake()
        {
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                originalColor = objectRenderer.material.color;
                propBlock = new MaterialPropertyBlock();
            }
            
            // Initialize events if null
            if (OnHoverEnter == null)
                OnHoverEnter = new UnityEvent();
            if (OnHoverExit == null)
                OnHoverExit = new UnityEvent();
            if (OnSelect == null)
                OnSelect = new UnityEvent();
            if (OnDeselect == null)
                OnDeselect = new UnityEvent();
        }
        
        public void OnHovered(bool isBeingHovered)
        {
            if (isBeingHovered && !isHovered)
            {
                isHovered = true;
                UpdateVisual();
                OnHoverEnter.Invoke();
            }
            else if (!isBeingHovered && isHovered)
            {
                isHovered = false;
                UpdateVisual();
                OnHoverExit.Invoke();
            }
        }
        
        public void OnSelected(bool isBeingSelected)
        {
            if (isBeingSelected && !isSelected)
            {
                isSelected = true;
                UpdateVisual();
                OnSelect.Invoke();
            }
            else if (!isBeingSelected && isSelected)
            {
                isSelected = false;
                UpdateVisual();
                OnDeselect.Invoke();
            }
        }
        
        private void UpdateVisual()
        {
            if (objectRenderer == null) return;
            
            Color targetColor = normalColor;
            
            if (isSelected)
            {
                targetColor = selectedColor;
            }
            else if (isHovered)
            {
                targetColor = hoveredColor;
            }
            
            objectRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", targetColor);
            objectRenderer.SetPropertyBlock(propBlock);
        }
        
        // Reset the object's visual state
        public void ResetVisualState()
        {
            isHovered = false;
            isSelected = false;
            
            if (objectRenderer != null)
            {
                objectRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", normalColor);
                objectRenderer.SetPropertyBlock(propBlock);
            }
        }
    }
}