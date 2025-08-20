using UnityEngine;

/// <summary>
/// Manages helicopter state transitions based on collision data and input.
/// Implements state machine logic for Flying and Grounded states.
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
    
        stateData.surfaceNormal = collisionSystem.GroundNormal;
        stateData.distanceToGround = collisionSystem.DistanceToGround;
    }
    
    /// <summary>
    /// Update state-specific timers
    /// </summary>
    // No per-state timers needed after simplification
    private void UpdateStateTimers() { }
    
    /// <summary>
    /// Determine what the new state should be based on current conditions
    /// </summary>
    private HelicopterState DetermineNewState(Vector2 input, IHelicopterCollisionSystem collisionSystem)
    {

                // Grounded check
        if (stateData.isGrounded)
        {
            return HelicopterState.Grounded;
        }

        // Default: flying
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
