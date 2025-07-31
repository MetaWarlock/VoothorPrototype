using UnityEngine;

/// <summary>
/// Manages helicopter state transitions based on collision data and input.
/// Implements state machine logic for Flying, Grounded, WallContact, Sliding, and InWater states.
/// </summary>
public class HelicopterStateManager : MonoBehaviour, IHelicopterStateListener
{
    [Header("State Debug")]
    [Tooltip("Show current state in inspector")]
    [SerializeField] private HelicopterState debugCurrentState = HelicopterState.Flying;
    
    // Settings reference
    private HelicopterSettings settings;
    
    // State data
    private HelicopterStateData stateData;
    
    // State transition timers
    private float wallContactTimer = 0f;
    private float slidingTimer = 0f;
    
    public HelicopterState CurrentState => stateData.currentState;
    public HelicopterStateData CurrentStateData => stateData;
    
    void Awake()
    {
        // Initialize state data
        stateData = new HelicopterStateData();
        
        // Subscribe to helicopter events
        HelicopterEvents.OnStateChanged.AddListener(OnStateChanged);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        HelicopterEvents.OnStateChanged.RemoveListener(OnStateChanged);
    }
    
    /// <summary>
    /// Initialize the state manager with settings
    /// </summary>
    public void Initialize(HelicopterSettings settings)
    {
        this.settings = settings;
        
        // Start in Flying state
        ChangeState(HelicopterState.Flying);
        
        if (settings.showDebug)
        {
            Debug.Log("[HelicopterStateManager] Initialized in Flying state");
        }
    }
    
    /// <summary>
    /// Update state based on input and collision data
    /// </summary>
    public void UpdateState(Vector2 input, IHelicopterCollisionSystem collisionSystem)
    {
        if (settings == null || collisionSystem == null) return;
        
        // Update state data from collision system
        UpdateStateData(collisionSystem);
        
        // Update timers
        stateData.timeInCurrentState += Time.fixedDeltaTime;
        UpdateStateTimers();
        
        // Determine new state based on current conditions
        HelicopterState newState = DetermineNewState(input, collisionSystem);
        
        // Change state if needed
        if (newState != stateData.currentState)
        {
            ChangeState(newState);
        }
        
        // Update debug display
        debugCurrentState = stateData.currentState;
    }
    
    /// <summary>
    /// Update state data from collision system
    /// </summary>
    private void UpdateStateData(IHelicopterCollisionSystem collisionSystem)
    {
        stateData.isGrounded = collisionSystem.IsGrounded;
        stateData.isTouchingWall = collisionSystem.IsTouchingWall;
        stateData.isInWater = collisionSystem.IsInWater;
        stateData.surfaceNormal = collisionSystem.GroundNormal;
        stateData.distanceToGround = collisionSystem.DistanceToGround;
    }
    
    /// <summary>
    /// Update state-specific timers
    /// </summary>
    private void UpdateStateTimers()
    {
        switch (stateData.currentState)
        {
            case HelicopterState.WallContact:
                wallContactTimer += Time.fixedDeltaTime;
                break;
            case HelicopterState.Sliding:
                slidingTimer += Time.fixedDeltaTime;
                break;
            default:
                wallContactTimer = 0f;
                slidingTimer = 0f;
                break;
        }
    }
    
    /// <summary>
    /// Determine what the new state should be based on current conditions
    /// </summary>
    private HelicopterState DetermineNewState(Vector2 input, IHelicopterCollisionSystem collisionSystem)
    {
        // Priority 1: Water (highest priority - safe zone)
        if (stateData.isInWater)
        {
            return HelicopterState.InWater;
        }
        
        // Priority 2: Wall contact
        if (stateData.isTouchingWall)
        {
            // Timeout check for wall contact
            if (wallContactTimer > settings.wallContactTimeout)
            {
                return HelicopterState.Flying; // Force escape from wall
            }
            return HelicopterState.WallContact;
        }
        
        // Priority 3: Grounded
        if (stateData.isGrounded)
        {
            // Check if we should be sliding instead
            var physics = GetComponent<HelicopterPhysics>();
            if (physics != null && physics.CurrentVelocity.magnitude > settings.slidingToGroundedThreshold)
            {
                return HelicopterState.Sliding;
            }
            return HelicopterState.Grounded;
        }
        
        // Priority 4: Sliding to Grounded transition
        if (stateData.currentState == HelicopterState.Sliding)
        {
            var physics = GetComponent<HelicopterPhysics>();
            if (physics != null && physics.CurrentVelocity.magnitude <= settings.slidingToGroundedThreshold)
            {
                return HelicopterState.Grounded;
            }
            // Continue sliding if still moving
            return HelicopterState.Sliding;
        }
        
        // Default: Flying
        return HelicopterState.Flying;
    }
    
    /// <summary>
    /// Change to a new state
    /// </summary>
    private void ChangeState(HelicopterState newState)
    {
        HelicopterState previousState = stateData.currentState;
        
        // Update state data
        stateData.previousState = previousState;
        stateData.currentState = newState;
        stateData.timeInCurrentState = 0f;
        
        // Reset state-specific timers
        wallContactTimer = 0f;
        slidingTimer = 0f;
        
        // Fire state change event
        HelicopterEvents.OnStateChanged.Invoke(newState, previousState);
        
        // Debug logging
        if (settings != null && settings.showDebug)
        {
            Debug.Log($"[HelicopterStateManager] State changed: {previousState} → {newState}");
        }
    }
    
    /// <summary>
    /// Handle state change events
    /// </summary>
    public void OnStateChanged(HelicopterState newState, HelicopterState previousState)
    {
        // Handle any state change specific logic here
        // This is called after the state has already changed
        
        // Special handling for water entry/exit
        if (newState == HelicopterState.InWater && previousState != HelicopterState.InWater)
        {
            HelicopterEvents.OnWaterStateChanged.Invoke(true);
        }
        else if (newState != HelicopterState.InWater && previousState == HelicopterState.InWater)
        {
            HelicopterEvents.OnWaterStateChanged.Invoke(false);
        }
    }
    
    /// <summary>
    /// Get state parameters for current state
    /// </summary>
    public StateParameters GetCurrentStateParameters()
    {
        if (settings == null) return null;
        
        return stateData.currentState switch
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
    /// Force a specific state (for testing or special cases)
    /// </summary>
    public void ForceState(HelicopterState state)
    {
        if (settings.showDebug)
        {
            Debug.Log($"[HelicopterStateManager] Force state: {state}");
        }
        
        ChangeState(state);
    }
}
