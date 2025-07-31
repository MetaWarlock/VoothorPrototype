using UnityEngine;

/// <summary>
/// Handles helicopter physics including thrust, friction, gravity, and buoyancy.
/// Applies different physics parameters based on current helicopter state.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class HelicopterPhysics : MonoBehaviour, IHelicopterPhysicsSystem
{
    [Header("Physics Debug")]
    [Tooltip("Show current velocity in inspector")]
    [SerializeField] private Vector2 debugCurrentVelocity;
    
    [Tooltip("Show current friction in inspector")]
    [SerializeField] private float debugCurrentFriction;
    
    // Components
    private Rigidbody2D rb;
    private HelicopterSettings settings;
    
    // Physics state
    private Vector2 targetVelocity;
    private float currentHorizontalMaxSpeed;
    private float currentVerticalMaxSpeed;
    private float currentHorizontalAcceleration;
    private float currentVerticalAcceleration;
    private float currentFriction;
    
    public Vector2 CurrentVelocity => rb ? rb.linearVelocity : Vector2.zero;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError($"[HelicopterPhysics] Rigidbody2D not found on {gameObject.name}!");
        }
    }
    
    /// <summary>
    /// Initialize physics system with settings
    /// </summary>
    public void Initialize(HelicopterSettings settings)
    {
        this.settings = settings;
        
        // Set initial physics parameters (using Flying state defaults)
        currentHorizontalMaxSpeed = settings.horizontalMaxSpeed;
        currentVerticalMaxSpeed = settings.verticalMaxSpeed;
        currentHorizontalAcceleration = settings.horizontalAcceleration;
        currentVerticalAcceleration = settings.verticalAcceleration;
        currentFriction = 0.02f; // Default flying friction
        
        if (settings.showDebug)
        {
            Debug.Log("[HelicopterPhysics] Physics system initialized");
        }
    }
    
    /// <summary>
    /// Update physics based on input, current state, and state data
    /// </summary>
    public void UpdatePhysics(Vector2 input, HelicopterState currentState, HelicopterStateData stateData)
    {
        if (rb == null || settings == null) return;
        
        // Get state parameters
        StateParameters stateParams = GetStateParameters(currentState);
        if (stateParams == null) return;
        
        // Update physics parameters based on state
        UpdatePhysicsParameters(stateParams);
        
        // Apply thrust based on input and state
        ApplyThrust(input, stateParams, stateData);
        
        // Apply friction based on current state
        ApplyFriction(input, stateParams);
        
        // Apply special effects (gravity, buoyancy, etc.)
        ApplySpecialEffects(currentState, stateParams, stateData);
        
        // Update debug display
        UpdateDebugInfo();
    }
    
    /// <summary>
    /// Get state parameters for the given state
    /// </summary>
    private StateParameters GetStateParameters(HelicopterState state)
    {
        return state switch
        {
            HelicopterState.Flying => settings.flying,
            HelicopterState.Grounded => settings.grounded,
            HelicopterState.WallContact => settings.wallContact,
            HelicopterState.Sliding => settings.sliding,
            HelicopterState.InWater => settings.inWater,
            _ => settings.flying
        };
    }
    
    /// <summary>
    /// Update physics parameters based on current state
    /// </summary>
    private void UpdatePhysicsParameters(StateParameters stateParams)
    {
        currentHorizontalMaxSpeed = stateParams.GetHorizontalMaxSpeed(settings.horizontalMaxSpeed);
        currentVerticalMaxSpeed = stateParams.GetVerticalMaxSpeed(settings.verticalMaxSpeed);
        currentHorizontalAcceleration = stateParams.GetHorizontalAcceleration(settings.horizontalAcceleration);
        currentVerticalAcceleration = stateParams.GetVerticalAcceleration(settings.verticalAcceleration);
        currentFriction = stateParams.frictionCoefficient;
    }
    
    /// <summary>
    /// Apply thrust forces based on input and state parameters
    /// </summary>
    private void ApplyThrust(Vector2 input, StateParameters stateParams, HelicopterStateData stateData)
    {
        Vector2 thrustForce = Vector2.zero;
        
        // Vertical thrust
        if (stateParams.allowVerticalThrust && Mathf.Abs(input.y) > 0.01f)
        {
            if (input.y > 0 || rb.linearVelocity.y < currentVerticalMaxSpeed)
            {
                thrustForce.y = input.y * currentVerticalAcceleration;
            }
        }
        
        // Horizontal thrust
        if (stateParams.allowHorizontalThrust && Mathf.Abs(input.x) > 0.01f)
        {
            // Check for wall contact blocking
            if (stateData.currentState == HelicopterState.WallContact)
            {
                // Don't apply thrust towards the wall
                Vector2 wallNormal = stateData.surfaceNormal;
                float thrustDotWall = Vector2.Dot(Vector2.right * input.x, wallNormal);
                
                if (thrustDotWall < 0) // Thrust is towards wall
                {
                    // Block horizontal thrust towards wall
                    thrustForce.x = 0;
                }
                else
                {
                    thrustForce.x = input.x * currentHorizontalAcceleration;
                }
            }
            else
            {
                thrustForce.x = input.x * currentHorizontalAcceleration;
            }
        }
        
        // Apply thrust force
        if (thrustForce != Vector2.zero)
        {
            rb.linearVelocity += thrustForce * Time.fixedDeltaTime;
        }
        
        // Clamp velocity to max speeds (separate for horizontal and vertical)
        Vector2 clampedVelocity = rb.linearVelocity;
        
        // Clamp horizontal velocity
        if (Mathf.Abs(clampedVelocity.x) > currentHorizontalMaxSpeed)
        {
            clampedVelocity.x = Mathf.Sign(clampedVelocity.x) * currentHorizontalMaxSpeed;
        }
        
        // Clamp vertical velocity
        if (Mathf.Abs(clampedVelocity.y) > currentVerticalMaxSpeed)
        {
            clampedVelocity.y = Mathf.Sign(clampedVelocity.y) * currentVerticalMaxSpeed;
        }
        
        rb.linearVelocity = clampedVelocity;
    }
    
    /// <summary>
    /// Apply friction forces based on state
    /// </summary>
    private void ApplyFriction(Vector2 input, StateParameters stateParams)
    {
        if (currentFriction > 0f)
        {
            Vector2 frictionForce = Vector2.zero;
            
            // Apply horizontal friction always
            frictionForce.x = -rb.linearVelocity.x * currentFriction;
            
            // Apply vertical friction only if:
            // 1. Moving upward, OR
            // 2. Have vertical input (thrust against gravity), OR  
            // 3. In special states that need vertical friction
            bool applyVerticalFriction = rb.linearVelocity.y > 0.01f ||  // Moving up
                                       Mathf.Abs(input.y) > 0.01f ||  // Have vertical input
                                       stateParams == settings.grounded ||   // Grounded state
                                       stateParams == settings.inWater;      // In water
            
            if (applyVerticalFriction)
            {
                frictionForce.y = -rb.linearVelocity.y * currentFriction;
            }
            // Otherwise let gravity do its work for natural falling
            
            rb.linearVelocity += frictionForce * Time.fixedDeltaTime;
            
            // Prevent jittering at very low speeds
            if (rb.linearVelocity.magnitude < 0.01f)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    /// <summary>
    /// Apply special effects like buoyancy in water
    /// </summary>
    private void ApplySpecialEffects(HelicopterState currentState, StateParameters stateParams, HelicopterStateData stateData)
    {
        // Apply buoyancy in water
        if (currentState == HelicopterState.InWater && stateParams.buoyancy > 0f)
        {
            Vector2 buoyancyForce = Vector2.up * stateParams.buoyancy;
            rb.linearVelocity += buoyancyForce * Time.fixedDeltaTime;
        }
        
        // Apply separation force if stuck in wall
        if (currentState == HelicopterState.WallContact && stateData.surfaceNormal != Vector2.zero)
        {
            // Small force to prevent sticking
            Vector2 separationForce = stateData.surfaceNormal * 0.1f;
            rb.linearVelocity += separationForce * Time.fixedDeltaTime;
        }
    }
    
    /// <summary>
    /// Update debug information displayed in inspector
    /// </summary>
    private void UpdateDebugInfo()
    {
        debugCurrentVelocity = rb.linearVelocity;
        debugCurrentFriction = currentFriction;
    }
    
    /// <summary>
    /// Manually set velocity (for external systems like landing)
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
    }
    
    /// <summary>
    /// Add impulse force (for external impacts)
    /// </summary>
    public void AddImpulse(Vector2 impulse)
    {
        if (rb != null)
        {
            rb.linearVelocity += impulse;
        }
    }
    
    /// <summary>
    /// Get current physics parameters for debugging
    /// </summary>
    public (float horizontalMaxSpeed, float verticalMaxSpeed, float horizontalAcceleration, float verticalAcceleration, float friction) GetCurrentParameters()
    {
        return (currentHorizontalMaxSpeed, currentVerticalMaxSpeed, currentHorizontalAcceleration, currentVerticalAcceleration, currentFriction);
    }
}
