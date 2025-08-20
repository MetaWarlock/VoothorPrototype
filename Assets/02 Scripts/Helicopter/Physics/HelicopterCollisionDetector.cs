using UnityEngine;

/// <summary>
/// Detects collisions and environmental conditions around the helicopter.
/// Uses raycast-based detection to prevent sticking and provide accurate state information.
/// </summary>
public class HelicopterCollisionDetector : MonoBehaviour, IHelicopterCollisionSystem
{
    [Header("Collision Debug")]
    [Tooltip("Show collision state in inspector")]
    [SerializeField] private bool debugIsGrounded = false;

    [SerializeField] private float debugDistanceToGround = 0f;
    
    // Settings and components
    private HelicopterSettings settings;
    private Rigidbody2D rb;
    
    // Collision state
    private bool isGrounded = false;
    private Vector2 groundNormal = Vector2.up;
    private float distanceToGround = float.MaxValue;
    
    // Raycast directions (8-directional detection)
    private readonly Vector2[] raycastDirections = {
        Vector2.down,         // Bottom  
        Vector2.down + Vector2.left,  // Bottom-Left
        Vector2.down + Vector2.right  // Bottom-Right
    };
    
    // Interface properties
    public bool IsGrounded => isGrounded;
    public Vector2 GroundNormal => groundNormal;
    public float DistanceToGround => distanceToGround;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError($"[HelicopterCollisionDetector] Rigidbody2D not found on {gameObject.name}!");
        }
    }
    
    /// <summary>
    /// Initialize collision detection system with settings
    /// </summary>
    public void Initialize(HelicopterSettings settings)
    {
        this.settings = settings;
        
        if (settings.showDebug)
        {
            Debug.Log("[HelicopterCollisionDetector] Collision detection initialized");
        }
    }
    
    /// <summary>
    /// Update collision detection - called every FixedUpdate
    /// </summary>
    public void UpdateDetection()
    {
        if (settings == null) return;
        
        // Reset collision state
        ResetCollisionState();
        
        // Perform raycast-based detection
        PerformRaycastDetection();
        
        // Update debug info
        UpdateDebugInfo();
    }
    
    /// <summary>
    /// Reset collision state before detection
    /// </summary>
    private void ResetCollisionState()
    {
        isGrounded = false;
        distanceToGround = float.MaxValue;
        groundNormal = Vector2.up;
    }
    
    /// <summary>
    /// Perform raycast-based collision detection in all directions
    /// </summary>
    private void PerformRaycastDetection()
    {
        float rayDistance = settings.raycastDistance;
        int rayCount = settings.raycastCount;
        
        // Track closest hits for separation vectors
        float closestGroundDistance = float.MaxValue;
        
        
        
        // Cast rays in all directions
        foreach (Vector2 direction in raycastDirections)
        {
            Vector2 normalizedDirection = direction.normalized;
            
            // Cast multiple rays per direction for better accuracy
            for (int i = 0; i < rayCount; i++)
            {
                Vector2 rayOrigin = (Vector2)transform.position + GetRayOffset(i, rayCount);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, normalizedDirection, rayDistance, GetCollisionLayerMask());
                
                if (hit.collider != null)
                {
                    ProcessRaycastHit(hit, normalizedDirection, ref closestGroundDistance);
                }
                
                // Draw debug rays if enabled
                if (settings.showRaycastGizmos)
                {
                    Color rayColor = hit.collider != null ? Color.red : Color.green;
                    Debug.DrawRay(rayOrigin, normalizedDirection * rayDistance, rayColor, Time.fixedDeltaTime);
                }
            }
        }
        
        // Determine final collision states
        DetermineCollisionStates(closestGroundDistance);
    }
    
    /// <summary>
    /// Get ray offset for multiple rays per direction
    /// </summary>
    private Vector2 GetRayOffset(int rayIndex, int totalRays)
    {
        if (totalRays <= 1) return Vector2.zero;
        
        float offsetRange = 0.2f; // Spread rays over 0.2 units
        float step = offsetRange / (totalRays - 1);
        float offset = -offsetRange * 0.5f + rayIndex * step;
        
        return Vector2.right * offset; // Spread horizontally
    }
    
    /// <summary>
    /// Get collision layer mask for raycast detection
    /// </summary>
    private int GetCollisionLayerMask()
    {
        // Include all solid layers that should block helicopter movement
        // You may need to adjust layer numbers based on your project setup
        return Physics2D.AllLayers; // For now, detect all layers
    }
    
    /// <summary>
    /// Process a raycast hit and update collision state
    /// </summary>
    private void ProcessRaycastHit(RaycastHit2D hit, Vector2 rayDirection, ref float closestGroundDistance)
    {
        string hitTag = hit.collider.tag;
        Vector2 hitNormal = hit.normal;
        float hitDistance = hit.distance;
        
        // Skip water collisions (handled separately)
        if (hitTag == "Water") return;
        
        // Check if this is a damaging surface
        bool isDamagingSurface = IsDamagingSurface(hit.collider.gameObject);
        if (!isDamagingSurface) return;
        
        // Determine collision type based on ray direction and normal
        // Downward and bottom-diagonal rays for ground detection
        if (hitNormal.y > 0.5f && hitDistance < closestGroundDistance) // Surface is mostly upward-facing
        {
            closestGroundDistance = hitDistance;
            groundNormal = hitNormal;
        }
    }
    
    /// <summary>
    /// Determine final collision states based on raycast results
    /// </summary>
    private void DetermineCollisionStates(float closestGroundDistance)
    {
        // Determine grounded state
        isGrounded = closestGroundDistance < settings.raycastDistance * 0.8f;
        
        distanceToGround = closestGroundDistance;
        
        if (settings.showDebug && isGrounded)
        {
            Debug.Log($"[HelicopterCollisionDetector] Grounded dist: {closestGroundDistance:F2}");
        }
    }
    
    /// <summary>
    /// Check if object is a damaging/solid surface
    /// </summary>
    private bool IsDamagingSurface(GameObject obj)
    {
        // Use the same tags as damage system
        string[] damageTags = { "Ground", "Wall", "Obstacle" };
        
        foreach (string tag in damageTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Update debug information in inspector
    /// </summary>
    private void UpdateDebugInfo()
    {
        debugIsGrounded = isGrounded;
        
    
        debugDistanceToGround = distanceToGround;
    }
    
    /// <summary>
    /// Get collision information for debugging
    /// </summary>
    public (bool grounded, float groundDist) GetCollisionInfo()
    {
        return (isGrounded, distanceToGround);
    }
    
    /// <summary>
    /// Force collision state (for testing)
    /// </summary>
    public void ForceCollisionState(bool grounded)
    {
        isGrounded = grounded;
        
        if (settings.showDebug)
        {
            Debug.Log($"[HelicopterCollisionDetector] Forced state - Ground: {grounded}");
        }
    }
    
    /// <summary>
    /// Draw debug gizmos in scene view
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (settings == null || !settings.showRaycastGizmos) return;
        
        // Draw collision detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, settings.raycastDistance);
        
        // Draw ground normal
        if (isGrounded)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, groundNormal * 1f);
        }
        
        // Draw state indicators
        if (isGrounded)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + Vector3.down * 0.5f, 0.1f);
        }
        
        
    }
}
