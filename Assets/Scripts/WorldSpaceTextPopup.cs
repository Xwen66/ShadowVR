using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class WorldSpaceTextPopup : MonoBehaviour
{
    [Header("Text Settings")]
    [Tooltip("The text to display when popup is shown")]
    [TextArea(3, 5)]
    public string popupText = "You entered the trigger area!";
    
    [Tooltip("Text component to display the message")]
    public TextMeshProUGUI textComponent;
    
    [Header("Canvas Settings")]
    [Tooltip("Canvas component for world space rendering")]
    public Canvas worldSpaceCanvas;
    
    [Tooltip("CanvasGroup for fade effects")]
    public CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [Tooltip("Enable fade in/out animations")]
    public bool useAnimations = true;
    
    [Tooltip("Duration of fade in/out animations")]
    public float animationDuration = 0.5f;
    
    [Tooltip("Auto hide the popup after this duration (0 = don't auto hide)")]
    public float autoHideDuration = 3f;
    
    [Header("Billboard Settings")]
    [Tooltip("Make the popup always face the camera")]
    public bool faceCamera = true;
    
    [Tooltip("Camera to face (if null, will find main camera)")]
    public Camera targetCamera;
    
    [Header("Positioning")]
    [Tooltip("Offset from the trigger position")]
    public Vector3 popupOffset = new Vector3(0, 2f, 0);
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = true;
    
    // Private variables
    private bool isVisible = false;
    private Coroutine animationCoroutine;
    private Coroutine autoHideCoroutine;
    private Transform originalParent;
    
    void Start()
    {
        InitializeComponents();
        HideImmediate();
    }
    
    void InitializeComponents()
    {
        // Find camera if not assigned
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        // Setup canvas if not assigned
        if (worldSpaceCanvas == null)
            worldSpaceCanvas = GetComponentInChildren<Canvas>();
        
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
            worldSpaceCanvas.worldCamera = targetCamera;
        }
        
        // Setup canvas group if not assigned
        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        
        // Setup text component if not assigned
        if (textComponent == null)
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        
        if (textComponent != null)
            textComponent.text = popupText;
        
        originalParent = transform.parent;
        
        if (showDebugMessages)
            Debug.Log($"WorldSpaceTextPopup '{gameObject.name}' initialized");
    }
    
    void Update()
    {
        if (faceCamera && targetCamera != null && isVisible)
        {
            FaceCameraBillboard();
        }
    }
    
    void FaceCameraBillboard()
    {
        Vector3 directionToCamera = targetCamera.transform.position - transform.position;
        
        // Option 1: Full 3D billboard (current behavior)
        transform.rotation = Quaternion.LookRotation(-directionToCamera);
        
        // Option 2: Y-axis constrained billboard (uncomment to use)
        // directionToCamera.y = 0; // Keep text upright
        // if (directionToCamera != Vector3.zero)
        //     transform.rotation = Quaternion.LookRotation(-directionToCamera);
        
        // Option 3: Smooth rotation billboard (uncomment to use)
        // Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
        // transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
    
    // Public methods to be called from TriggerVolume UnityEvents
    public void ShowPopup()
    {
        if (showDebugMessages)
            Debug.Log($"Showing popup: {popupText}");
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        
        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);
        
        if (useAnimations && canvasGroup != null)
        {
            animationCoroutine = StartCoroutine(FadeIn());
        }
        else
        {
            ShowImmediate();
        }
        
        // Start auto hide timer
        if (autoHideDuration > 0)
        {
            autoHideCoroutine = StartCoroutine(AutoHide());
        }
    }
    
    public void HidePopup()
    {
        if (showDebugMessages)
            Debug.Log($"Hiding popup");
        
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        
        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);
        
        if (useAnimations && canvasGroup != null)
        {
            animationCoroutine = StartCoroutine(FadeOut());
        }
        else
        {
            HideImmediate();
        }
    }
    
    public void TogglePopup()
    {
        if (isVisible)
            HidePopup();
        else
            ShowPopup();
    }
    
    public void SetPopupText(string newText)
    {
        popupText = newText;
        if (textComponent != null)
            textComponent.text = popupText;
    }
    
    public void SetPopupPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition + popupOffset;
    }
    
    // Immediate show/hide without animations
    private void ShowImmediate()
    {
        if (worldSpaceCanvas != null)
            worldSpaceCanvas.gameObject.SetActive(true);
        
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        
        gameObject.SetActive(true);
        isVisible = true;
    }
    
    private void HideImmediate()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        if (worldSpaceCanvas != null)
            worldSpaceCanvas.gameObject.SetActive(false);
        
        isVisible = false;
    }
    
    // Animation coroutines
    private IEnumerator FadeIn()
    {
        if (worldSpaceCanvas != null)
            worldSpaceCanvas.gameObject.SetActive(true);
        
        gameObject.SetActive(true);
        isVisible = true;
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / animationDuration);
            
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / animationDuration);
            
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        if (worldSpaceCanvas != null)
            worldSpaceCanvas.gameObject.SetActive(false);
        
        isVisible = false;
    }
    
    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(autoHideDuration);
        HidePopup();
    }
    
    // Editor utility methods
    [ContextMenu("Test Show Popup")]
    public void TestShowPopup()
    {
        ShowPopup();
    }
    
    [ContextMenu("Test Hide Popup")]
    public void TestHidePopup()
    {
        HidePopup();
    }
    
    void OnDrawGizmos()
    {
        // Draw a small sphere to show the popup position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        
        // Draw offset preview
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + popupOffset);
        Gizmos.DrawWireSphere(transform.position + popupOffset, 0.05f);
    }
} 