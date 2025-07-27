using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterZone : MonoBehaviour
{
    [Header("Water Physics Settings")]
    [Tooltip("Force multiplier for pushing objects out of water")]
    public float buoyancyForce = 15f;
    
    [Tooltip("Maximum force that can be applied")]
    public float maxBuoyancyForce = 25f;
    
    [Tooltip("Resistance applied to movement in water")]
    public float waterDrag = 2f;
    
    [Tooltip("Speed multiplier for entry velocity calculation")]
    public float entryVelocityMultiplier = 1.5f;
    
    [Tooltip("Maximum depth an object can dive (in Unity units)")]
    public float maxDiveDepth = 3f;
    
    [Tooltip("Minimum velocity required to start diving")]
    public float minDiveVelocity = 5f;

    [Header("Visual Settings")]
    [Tooltip("GameObject marking the water surface position (optional - if null, will auto-calculate)")]
    public Transform waterSurfaceMarker;
    
    [Tooltip("Manual water surface Y position (used if waterSurfaceMarker is null)")]
    public float waterSurfaceY;
    
    [Tooltip("Enable debug visualization")]
    public bool showDebug = false;

    [Header("Surface Behavior")]
    [Tooltip("Keep objects floating on surface instead of bouncing")]
    public bool preventSurfaceBouncing = true;
    
    [Tooltip("Velocity threshold for surface floating")]
    public float surfaceFloatThreshold = 1f;
    
    [Tooltip("Force multiplier when object is near surface")]
    public float surfaceStabilization = 0.5f;

    private Collider2D waterCollider;
    private float actualWaterSurfaceY;

    void Start()
    {
        // Get and setup water collider as trigger
        waterCollider = GetComponent<Collider2D>();
        waterCollider.isTrigger = true;
        
        // Calculate actual water surface position
        UpdateWaterSurfacePosition();
    }
    
    void Update()
    {
        // Update water surface position if using marker
        if (waterSurfaceMarker != null)
        {
            UpdateWaterSurfacePosition();
        }
    }
    
    private void UpdateWaterSurfacePosition()
    {
        if (waterSurfaceMarker != null)
        {
            actualWaterSurfaceY = waterSurfaceMarker.position.y;
        }
        else if (waterSurfaceY != 0f)
        {
            actualWaterSurfaceY = waterSurfaceY;
        }
        else
        {
            // Auto-calculate from collider bounds (top edge)
            actualWaterSurfaceY = transform.position.y + waterCollider.bounds.size.y * 0.5f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        WaterBody waterBody = other.GetComponent<WaterBody>();
        if (waterBody == null)
        {
            // Add WaterBody component if object doesn't have one
            waterBody = other.gameObject.AddComponent<WaterBody>();
        }

        // Calculate entry velocity for initial force
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float entrySpeed = rb.linearVelocity.magnitude;
            waterBody.SetEntryVelocity(entrySpeed * entryVelocityMultiplier);
            
            if (showDebug)
            {
                Debug.Log($"{other.name} entered water at speed: {entrySpeed}");
            }
        }

        waterBody.EnterWater(this);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        WaterBody waterBody = other.GetComponent<WaterBody>();
        if (waterBody != null && waterBody.IsInWater)
        {
            ApplyWaterPhysics(other, waterBody);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        WaterBody waterBody = other.GetComponent<WaterBody>();
        if (waterBody != null)
        {
            waterBody.ExitWater();
            
            // Reset velocity when exiting water to prevent bouncing
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null && preventSurfaceBouncing)
            {
                // Only reset velocity if object is moving upward (exiting from surface)
                if (rb.linearVelocity.y > 0 && other.transform.position.y >= actualWaterSurfaceY - 0.5f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    
                    if (showDebug)
                    {
                        Debug.Log($"{other.name} velocity reset on water exit");
                    }
                }
            }
            
            if (showDebug)
            {
                Debug.Log($"{other.name} exited water");
            }
        }
    }

    private void ApplyWaterPhysics(Collider2D other, WaterBody waterBody)
    {
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        float objectY = other.transform.position.y;
        float depthBelowSurface = actualWaterSurfaceY - objectY;
        bool isNearSurface = depthBelowSurface < surfaceFloatThreshold;

        // Disable gravity when in water to prevent bouncing
        if (preventSurfaceBouncing)
        {
            waterBody.SetGravityScale(0f);
        }

        // Apply buoyancy force if object is below water surface
        if (depthBelowSurface > 0)
        {
            // Calculate buoyancy based on depth and entry velocity
            float entryBonus = waterBody.GetEntryVelocity() * 0.1f;
            float buoyancy = (buoyancyForce + entryBonus) * depthBelowSurface;
            buoyancy = Mathf.Min(buoyancy, maxBuoyancyForce);

            // Check if object should be able to dive deeper
            bool canDive = rb.linearVelocity.y < -minDiveVelocity && depthBelowSurface < maxDiveDepth;
            
            if (!canDive)
            {
                // Apply stabilization force near surface to prevent bouncing
                if (isNearSurface && preventSurfaceBouncing)
                {
                    buoyancy *= surfaceStabilization;
                    
                    // Additional surface stabilization
                    if (rb.linearVelocity.y > 0 && depthBelowSurface < 0.2f)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 
                                                       Mathf.Min(rb.linearVelocity.y, 1f));
                    }
                }
                
                // Apply upward buoyancy force
                Vector2 buoyancyVector = Vector2.up * buoyancy;
                rb.AddForce(buoyancyVector, ForceMode2D.Force);
            }
        }
        else if (depthBelowSurface < -0.1f && preventSurfaceBouncing)
        {
            // Object is above water surface - apply gentle downward force to keep it on surface
            Vector2 surfaceAttraction = Vector2.down * buoyancyForce * 0.3f;
            rb.AddForce(surfaceAttraction, ForceMode2D.Force);
        }

        // Apply water drag
        Vector2 drag = -rb.linearVelocity * waterDrag;
        rb.AddForce(drag, ForceMode2D.Force);

        // Reduce entry velocity over time
        waterBody.ReduceEntryVelocity(Time.fixedDeltaTime);

        if (showDebug)
        {
            Debug.DrawLine(other.transform.position, 
                          other.transform.position + Vector3.up * depthBelowSurface, 
                          Color.blue);
                          
            if (isNearSurface)
            {
                Debug.DrawLine(other.transform.position, 
                              other.transform.position + Vector3.right * 0.5f, 
                              Color.yellow);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (showDebug)
        {
            // Update surface position for gizmos
            if (Application.isPlaying)
            {
                UpdateWaterSurfacePosition();
            }
            else
            {
                // In edit mode, use marker or manual position
                if (waterSurfaceMarker != null)
                {
                    actualWaterSurfaceY = waterSurfaceMarker.position.y;
                }
                else if (waterSurfaceY != 0f)
                {
                    actualWaterSurfaceY = waterSurfaceY;
                }
                else
                {
                    var col = GetComponent<Collider2D>();
                    if (col != null)
                    {
                        actualWaterSurfaceY = transform.position.y + col.bounds.size.y * 0.5f;
                    }
                }
            }
            
            Vector3 center = transform.position;
            Vector3 size = GetComponent<Collider2D>().bounds.size;
            
            // Draw water surface line
            Gizmos.color = Color.cyan;
            Vector3 surfaceStart = new Vector3(center.x - size.x * 0.5f, actualWaterSurfaceY, center.z);
            Vector3 surfaceEnd = new Vector3(center.x + size.x * 0.5f, actualWaterSurfaceY, center.z);
            Gizmos.DrawLine(surfaceStart, surfaceEnd);
            
            // Draw surface float threshold
            Gizmos.color = Color.yellow;
            Vector3 floatStart = new Vector3(center.x - size.x * 0.5f, actualWaterSurfaceY - surfaceFloatThreshold, center.z);
            Vector3 floatEnd = new Vector3(center.x + size.x * 0.5f, actualWaterSurfaceY - surfaceFloatThreshold, center.z);
            Gizmos.DrawLine(floatStart, floatEnd);
            
            // Draw max dive depth
            Gizmos.color = Color.red;
            Vector3 diveStart = new Vector3(center.x - size.x * 0.5f, actualWaterSurfaceY - maxDiveDepth, center.z);
            Vector3 diveEnd = new Vector3(center.x + size.x * 0.5f, actualWaterSurfaceY - maxDiveDepth, center.z);
            Gizmos.DrawLine(diveStart, diveEnd);
            
            // Draw marker position if exists
            if (waterSurfaceMarker != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(waterSurfaceMarker.position, 0.2f);
            }
        }
    }
}
