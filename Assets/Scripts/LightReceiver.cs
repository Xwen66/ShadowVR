using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class LightReceiver : MonoBehaviour
{
    [Header("Multi-Light Settings")]
    [Tooltip("Automatically find all light sources in scene")]
    public bool autoFindLightSources = true;
    
    [Tooltip("Manually assigned light sources (optional)")]
    public List<LightSource> lightSources = new List<LightSource>();
    
    [Tooltip("Number of light sources needed to be fully lit")]
    [Range(1, 10)]
    public int requiredLightSources = 1;
    
    [Tooltip("Show debug info for multi-light calculation")]
    public bool showDebugInfo = false;
    
    [Header("Status")]
    public bool IsLit = false;
    
    [Tooltip("How many light sources are currently lighting this receiver")]
    public int currentLightingCount = 0;
    
    // Private variables
    private List<LightSource> activeLightSources = new List<LightSource>();

    //event that when the light receiver is lit
    public UnityEvent OnLightLit;
    private bool isInvoked = false;
    
    void Start()
    {
        if (autoFindLightSources)
        {
            FindAllLightSources();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckMultiLightStatus();
        if (IsLit)
        {
           //make sure only invode once
           if (!isInvoked)
           {
                OnLightLit.Invoke();
                isInvoked = true;
           }
        }
        else
        {
            isInvoked = false;
        }
    }
    
    void FindAllLightSources()
    {
        // Find all LightSource components in the scene
        LightSource[] foundLightSources = FindObjectsOfType<LightSource>();
        
        foreach (LightSource lightSource in foundLightSources)
        {
            // Only add light sources that target this receiver or have no specific target
            if (lightSource.lightReceiver == this || lightSource.lightReceiver == null)
            {
                if (!lightSources.Contains(lightSource))
                {
                    lightSources.Add(lightSource);
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"LightReceiver '{name}' found {lightSources.Count} light sources");
        }
    }
    
    void CheckMultiLightStatus()
    {
        activeLightSources.Clear();
        currentLightingCount = 0;
        
        // Check each light source that could affect this receiver
        foreach (LightSource lightSource in lightSources)
        {
            if (lightSource == null) continue;
            
            // Check if this light source can reach this receiver
            if (CheckLightReachesReceiver(lightSource))
            {
                activeLightSources.Add(lightSource);
                currentLightingCount++;
            }
        }
        
        // Determine if receiver is lit based on required number of light sources
        bool wasLit = IsLit;
        IsLit = currentLightingCount >= requiredLightSources;
        
        // Debug logging when status changes
        if (showDebugInfo && wasLit != IsLit)
        {
            Debug.Log($"LightReceiver '{name}' is now {(IsLit ? "LIT" : "DARK")} " +
                     $"({currentLightingCount}/{requiredLightSources} light sources active)");
        }
    }
    
    bool CheckLightReachesReceiver(LightSource lightSource)
    {
        Vector3 lightPosition = lightSource.transform.position;
        Vector3 receiverPosition = transform.position;
        Vector3 direction = (receiverPosition - lightPosition).normalized;
        float distance = Vector3.Distance(lightPosition, receiverPosition);
        
        // Use multiple rays like the improved LightSource
        int raysHitting = 0;
        int totalRays = lightSource.numberOfRays;
        
        for (int i = 0; i < totalRays; i++)
        {
            Vector3 rayStart = lightPosition;
            Vector3 rayTarget = receiverPosition;
            
            // Add spread for multiple rays (same pattern as LightSource)
            if (totalRays > 1 && i > 0)
            {
                Vector3 spreadOffset = GetSpreadOffset(i, totalRays);
                rayTarget += spreadOffset * lightSource.raySpread;
            }
            
            Vector3 rayDirection = (rayTarget - rayStart).normalized;
            float rayDistance = Vector3.Distance(rayStart, rayTarget);
            
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(rayStart, rayDirection, out hit, rayDistance);
            
            if (!hitSomething)
            {
                // No obstruction, light reaches receiver
                raysHitting++;
            }
            else if (hit.collider.gameObject == gameObject)
            {
                // Ray hit this receiver directly
                raysHitting++;
            }
            // If ray hit something else, it's blocked
        }
        
        // Check if enough rays hit to meet the light source's threshold
        float hitPercentage = (float)raysHitting / totalRays;
        return hitPercentage >= lightSource.lightThreshold;
    }
    
    // Same spread pattern as LightSource for consistency
    Vector3 GetSpreadOffset(int index, int totalRays)
    {
        if (totalRays <= 1) return Vector3.zero;
        
        float angle = (2f * Mathf.PI * index) / (totalRays - 1);
        float x = Mathf.Cos(angle);
        float y = Mathf.Sin(angle);
        
        return new Vector3(x, y, 0f);
    }
    
    // Public methods for external queries
    public List<LightSource> GetActiveLightSources()
    {
        return new List<LightSource>(activeLightSources);
    }
    
    public bool IsLitByLightSource(LightSource lightSource)
    {
        return activeLightSources.Contains(lightSource);
    }
    
    public float GetLightingPercentage()
    {
        return requiredLightSources > 0 ? (float)currentLightingCount / requiredLightSources : 0f;
    }
}
