using UnityEngine;

public class LightSource : MonoBehaviour
{
    public LightReceiver lightReceiver;
    
    [Header("Light Range")]
    [Tooltip("Maximum distance the light can reach")]
    [Range(0.1f, 50f)]
    public float maxRange = 10f;
    
    [Tooltip("Show the light range in scene view")]
    public bool showRangeGizmo = true;
    
    [Header("Multi-Ray Settings")]
    [Tooltip("Number of rays to cast for better shadow detection")]
    [Range(1, 10)]
    public int numberOfRays = 5;
    
    [Tooltip("Spread radius for multiple rays")]
    [Range(0.1f, 2f)]
    public float raySpread = 0.5f;
    
    [Tooltip("Percentage of rays that must hit to consider receiver lit")]
    [Range(0.1f, 1f)]
    public float lightThreshold = 0.6f;
    
    [Header("Debug")]
    public bool showDebugRay = true;
    public Color rayColor = Color.yellow;
    
    //this is a light source that will do a raycast to the light receiver, if the 
    //light is received, the light receiver will be lit
    public void LightUp()
    {
        lightReceiver.IsLit = true;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckLineOfSight();
    }
    
    void CheckLineOfSight()
    {
        // Check if lightReceiver is assigned
        if (lightReceiver == null)
        {
            Debug.LogWarning("LightReceiver is not assigned to " + gameObject.name);
            return;
        }
        
        Vector3 receiverPosition = lightReceiver.transform.position;
        float distance = Vector3.Distance(transform.position, receiverPosition);
        
        // Check if receiver is within range
        if (distance > maxRange)
        {
            lightReceiver.IsLit = false;
            if (showDebugRay)
            {
                // Draw a red line to show out-of-range
                Debug.DrawLine(transform.position, receiverPosition, Color.gray, 0.1f);
            }
            return;
        }
        
        int hitsOnReceiver = 0;
        
        // Cast multiple rays for better shadow detection
        for (int i = 0; i < numberOfRays; i++)
        {
            Vector3 rayStart = transform.position;
            Vector3 rayTarget = receiverPosition;
            
            // Add spread for multiple rays (except the center ray)
            if (numberOfRays > 1 && i > 0)
            {
                // Create spread pattern around the receiver
                Vector3 randomOffset = GetSpreadOffset(i, numberOfRays);
                rayTarget += randomOffset * raySpread;
            }
            
            Vector3 direction = (rayTarget - rayStart).normalized;
            float rayDistance = Vector3.Distance(rayStart, rayTarget);
            
            // Perform raycast
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(rayStart, direction, out hit, rayDistance);
            
            if (!hitSomething)
            {
                // No obstruction, light reaches this point
                hitsOnReceiver++;
            }
            else if (hit.collider.gameObject == lightReceiver.gameObject)
            {
                // Ray hit the receiver directly
                hitsOnReceiver++;
            }
            // If ray hit something else, it's blocked
            
            // Debug visualization
            if (showDebugRay)
            {
                Color debugColor = (!hitSomething || hit.collider.gameObject == lightReceiver.gameObject) ? Color.green : Color.red;
                Debug.DrawRay(rayStart, direction * rayDistance, debugColor, 0.1f);
            }
        }
        
        // Determine if receiver is lit based on threshold
        float hitPercentage = (float)hitsOnReceiver / numberOfRays;
        lightReceiver.IsLit = hitPercentage >= lightThreshold;
    }
    
    // Generate spread offsets for multiple rays
    Vector3 GetSpreadOffset(int index, int totalRays)
    {
        if (totalRays <= 1) return Vector3.zero;
        
        // Create circular pattern around center
        float angle = (2f * Mathf.PI * index) / (totalRays - 1);
        float x = Mathf.Cos(angle);
        float y = Mathf.Sin(angle);
        
        return new Vector3(x, y, 0f);
    }
    
    // Draw gizmos to visualize light range
    void OnDrawGizmos()
    {
        if (!showRangeGizmo) return;
        
        // Draw range sphere
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f); // Semi-transparent yellow
        Gizmos.DrawSphere(transform.position, maxRange);
        
        // Draw range wireframe
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxRange);
        
        // Draw line to receiver if assigned and show range status
        if (lightReceiver != null)
        {
            float distance = Vector3.Distance(transform.position, lightReceiver.transform.position);
            
            if (distance <= maxRange)
            {
                // In range - green line
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, lightReceiver.transform.position);
            }
            else
            {
                // Out of range - red line
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, lightReceiver.transform.position);
                
                // Draw where the range ends
                Vector3 direction = (lightReceiver.transform.position - transform.position).normalized;
                Vector3 rangeEndPoint = transform.position + direction * maxRange;
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(rangeEndPoint, 0.2f);
            }
        }
    }
    
    // Additional helper methods for range checking
    public bool IsReceiverInRange()
    {
        if (lightReceiver == null) return false;
        float distance = Vector3.Distance(transform.position, lightReceiver.transform.position);
        return distance <= maxRange;
    }
    
    public float GetDistanceToReceiver()
    {
        if (lightReceiver == null) return float.MaxValue;
        return Vector3.Distance(transform.position, lightReceiver.transform.position);
    }
    
    public float GetRemainingRange()
    {
        if (lightReceiver == null) return maxRange;
        float distance = Vector3.Distance(transform.position, lightReceiver.transform.position);
        return Mathf.Max(0f, maxRange - distance);
    }
    
    // Test functions for debugging
    [ContextMenu("Test Light Range")]
    public void TestLightRange()
    {
        if (lightReceiver == null)
        {
            Debug.Log("No light receiver assigned to test range!");
            return;
        }
        
        float distance = GetDistanceToReceiver();
        bool inRange = IsReceiverInRange();
        
        Debug.Log($"Light Source '{gameObject.name}' Range Test:\n" +
                  $"Distance to receiver: {distance:F2} units\n" +
                  $"Max range: {maxRange:F2} units\n" +
                  $"In range: {inRange}\n" +
                  $"Remaining range: {GetRemainingRange():F2} units");
    }

}
