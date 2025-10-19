using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VRScreenEffect : MonoBehaviour
{
    [Header("Screen Effect Components")]
    public Canvas canvas;

    [SerializeField]
    private Color flashColor = Color.red;

    public Camera vrCamera;

    public Image flashImage;

    [Header("Flash Effect Settings")]
    [Tooltip("Duration of the flash effect in seconds")]
    [Range(0.1f, 2f)]
    public float flashDuration = 0.5f;

    [Tooltip("Peak alpha value for the flash")]
    [Range(0.1f, 1f)]
    public float maxAlpha = 0.8f;

    [Tooltip("How quickly the flash fades")]
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // SetupScreenEffect();
    }

    void Update()
    {
        // Make sure the canvas always faces the camera
        if (vrCamera != null && canvas != null)
        {
            canvas.transform.LookAt(vrCamera.transform);
            canvas.transform.Rotate(0, 180, 0); // Flip to face the camera properly
        }
    }

    void SetupScreenEffect()
    {
        // Auto-find components if not assigned
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        if (flashImage == null)
            flashImage = GetComponent<Image>();

        if (vrCamera == null)
        {
            // Try to find VR camera
            vrCamera = Camera.main;
            if (vrCamera == null)
                vrCamera = FindObjectOfType<Camera>();
        }

        // Setup canvas for VR
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = vrCamera;

            // Position canvas in front of camera
            if (vrCamera != null)
            {
                canvas.transform.position = vrCamera.transform.position + vrCamera.transform.forward * 0.5f;
                canvas.transform.localScale = Vector3.one * 0.001f; // Small scale for world space
            }
        }

        // Setup flash image
        if (flashImage != null)
        {
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            flashImage.gameObject.SetActive(false);
        }

        Debug.Log("VR Screen Effect initialized");
    }

    public void ShowScreenEffect()
    {
        StartCoroutine(FlashEffect());
        Debug.Log("ShowScreenEffect");
    }

    public void ShowScreenEffect(Color customColor)
    {
        flashColor = customColor;
        StartCoroutine(FlashEffect());
    }

    public void ShowScreenEffect(float customDuration)
    {
        float originalDuration = flashDuration;
        flashDuration = customDuration;
        StartCoroutine(FlashEffect());
        flashDuration = originalDuration;
    }

    private IEnumerator FlashEffect()
    {
        if (flashImage == null)
        {
            Debug.LogWarning("Flash image is not assigned!");
            yield break;
        }

        // Enable the canvas image
        flashImage.gameObject.SetActive(true);

        // Set initial color with max alpha
        Color startColor = new Color(flashColor.r, flashColor.g, flashColor.b, maxAlpha);
        Color endColor = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        // Flash effect: quickly fade from max alpha to 0
        float elapsedTime = 0f;

        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / flashDuration;

            // Use the curve to control the fade
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            float currentAlpha = Mathf.Lerp(maxAlpha, 0f, curveValue);

            // Update the image color
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, currentAlpha);

            yield return null;
        }

        // Ensure the image is fully transparent and disabled
        flashImage.color = endColor;
        flashImage.gameObject.SetActive(false);
        GlobalEvent.OnWhiteFlashEndEvent.Invoke();
        Debug.LogError("白色闪光结束");
    }

    // Additional effect methods for different scenarios
    public void ShowDamageFlash()
    {
        flashColor = Color.red;
        ShowScreenEffect();
    }

    public void ShowHealFlash()
    {
        flashColor = Color.green;
        ShowScreenEffect();
    }

    public void ShowWarningFlash()
    {
        flashColor = Color.yellow;
        ShowScreenEffect();
    }
    public void ShowInLightFlash()
    {
        flashColor = Color.white;
        ShowScreenEffect();
    }

    public void ShowCriticalFlash()
    {
        flashColor = new Color(1f, 0f, 0f, 1f); // Bright red
        float originalDuration = flashDuration;
        flashDuration = 0.8f; // Longer for critical
        StartCoroutine(FlashEffect());
        flashDuration = originalDuration;
    }

    //test function
    [ContextMenu("Test Flash")]
    public void TestFlash()
    {
        ShowScreenEffect(Color.red);
    }
}
