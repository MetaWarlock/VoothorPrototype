using UnityEngine;

/// <summary>
/// Component that tracks an object's interaction with water zones.
/// Automatically added to objects when they enter water.
/// </summary>
public class WaterBody : MonoBehaviour
{
    [Header("Water State")]
    [SerializeField] private bool isInWater = false;
    [SerializeField] private float entryVelocity = 0f;
    [SerializeField] private float entryVelocityDecay = 2f;

    private WaterZone currentWaterZone;
    private Rigidbody2D rb;
    private float originalGravityScale = -1f;

    public bool IsInWater => isInWater;
    public WaterZone CurrentWaterZone => currentWaterZone;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Called when object enters a water zone
    /// </summary>
    /// <param name="waterZone">The water zone being entered</param>
    public void EnterWater(WaterZone waterZone)
    {
        isInWater = true;
        currentWaterZone = waterZone;
        
        // Store original drag values if we haven't already
        if (rb != null && !HasStoredOriginalDrag())
        {
            StoreOriginalDrag();
        }
    }

    /// <summary>
    /// Called when object exits water zone
    /// </summary>
    public void ExitWater()
    {
        isInWater = false;
        currentWaterZone = null;
        entryVelocity = 0f;
        
        // Restore original values
        if (rb != null)
        {
            RestoreOriginalDrag();
            RestoreOriginalGravity();
        }
    }

    /// <summary>
    /// Sets the entry velocity for buoyancy calculations
    /// </summary>
    /// <param name="velocity">Entry velocity magnitude</param>
    public void SetEntryVelocity(float velocity)
    {
        entryVelocity = Mathf.Max(entryVelocity, velocity);
    }

    /// <summary>
    /// Gets current entry velocity (decays over time)
    /// </summary>
    /// <returns>Current entry velocity</returns>
    public float GetEntryVelocity()
    {
        return entryVelocity;
    }

    /// <summary>
    /// Reduces entry velocity over time
    /// </summary>
    /// <param name="deltaTime">Time since last frame</param>
    public void ReduceEntryVelocity(float deltaTime)
    {
        if (entryVelocity > 0f)
        {
            entryVelocity = Mathf.Max(0f, entryVelocity - entryVelocityDecay * deltaTime);
        }
    }

    /// <summary>
    /// Check if object is diving (moving downward in water)
    /// </summary>
    /// <returns>True if diving</returns>
    public bool IsDiving()
    {
        return isInWater && rb != null && rb.linearVelocity.y < -1f;
    }

    /// <summary>
    /// Get depth below water surface
    /// </summary>
    /// <returns>Depth in Unity units, 0 if not in water or above surface</returns>
    public float GetDepthBelowSurface()
    {
        if (!isInWater || currentWaterZone == null) return 0f;
        
        // Use the actual water surface Y from the zone
        float surfaceY = currentWaterZone.waterSurfaceMarker != null ? 
                        currentWaterZone.waterSurfaceMarker.position.y : 
                        currentWaterZone.waterSurfaceY;
        float objectY = transform.position.y;
        return Mathf.Max(0f, surfaceY - objectY);
    }
    
    /// <summary>
    /// Set gravity scale for the object (used by water zones)
    /// </summary>
    /// <param name="gravityScale">New gravity scale value</param>
    public void SetGravityScale(float gravityScale)
    {
        if (rb == null) return;
        
        // Store original gravity scale if not already stored
        if (originalGravityScale < 0f)
        {
            originalGravityScale = rb.gravityScale;
        }
        
        rb.gravityScale = gravityScale;
    }

    #region Physics Values Management
    private float originalDrag = -1f;
    private float originalAngularDrag = -1f;

    private bool HasStoredOriginalDrag()
    {
        return originalDrag >= 0f;
    }

    private void StoreOriginalDrag()
    {
        if (rb != null)
        {
            originalDrag = rb.linearDamping;
            originalAngularDrag = rb.angularDamping;
        }
    }

    private void RestoreOriginalDrag()
    {
        if (rb != null && HasStoredOriginalDrag())
        {
            rb.linearDamping = originalDrag;
            rb.angularDamping = originalAngularDrag;
            
            // Reset stored values
            originalDrag = -1f;
            originalAngularDrag = -1f;
        }
    }
    
    private void RestoreOriginalGravity()
    {
        if (rb != null && originalGravityScale >= 0f)
        {
            rb.gravityScale = originalGravityScale;
            originalGravityScale = -1f;
        }
    }
    #endregion

    void OnDestroy()
    {
        // Ensure we clean up if object is destroyed while in water
        if (isInWater)
        {
            RestoreOriginalDrag();
            RestoreOriginalGravity();
        }
    }

    #region Debug Info
    public string GetDebugInfo()
    {
        if (!isInWater) return "Not in water";
        
        string gravityInfo = rb != null ? $", Gravity: {rb.gravityScale:F2}" : "";
        return $"In Water - Depth: {GetDepthBelowSurface():F2}, Entry Velocity: {entryVelocity:F2}, Diving: {IsDiving()}{gravityInfo}";
    }
    #endregion
}
