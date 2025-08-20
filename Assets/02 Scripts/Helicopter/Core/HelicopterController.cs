using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main helicopter controller that coordinates all helicopter systems.
/// This is the central hub that manages state, physics, and input.
/// </summary>
public class HelicopterController : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Helicopter configuration settings")]
    public HelicopterSettings settings;
    
    // System references
    private HelicopterStateManager stateManager;
    private HelicopterPhysics physics;
    private HelicopterCollisionDetector collisionDetector;
    
    // Input
    private Vector2 inputVector;
    
    public void Awake()
    {
        // Get or create components
        stateManager = GetComponent<HelicopterStateManager>();
        physics = GetComponent<HelicopterPhysics>();
        collisionDetector = GetComponent<HelicopterCollisionDetector>();
        
        // Settings will be created automatically in InitializeSystems if needed
    }
    
    public void Start()
    {
        // Initialize all systems
        InitializeSystems();
    }
    
    public void FixedUpdate()
    {
        // Main update loop
        UpdateSystems();
    }
    
    /// <summary>
    /// Initialize all helicopter systems
    /// </summary>
    private void InitializeSystems()
    {
        // Create default settings if none assigned
        if (settings == null)
        {
            settings = CreateDefaultSettings();
        }
        
        if (stateManager != null)
        {
            stateManager.Initialize(settings);
        }
        
        if (physics != null)
        {
            physics.Initialize(settings);
        }
        
        if (collisionDetector != null)
        {
            collisionDetector.Initialize(settings);
        }
        
        if (settings.showDebug)
        {
            Debug.Log("[HelicopterController] All systems initialized successfully");
        }
    }
    
    /// <summary>
    /// Update all helicopter systems in correct order
    /// </summary>
    private void UpdateSystems()
    {
        // 1. Get input
        // Input is handled via OnMove callback from Unity Input System
        // 2. Update collision detection
        if (collisionDetector != null)
        {
            collisionDetector.UpdateDetection();
        }
        
        // 3. Update state based on collisions and input
        if (stateManager != null)
        {
            stateManager.UpdateState(inputVector, collisionDetector);
        }
        
        // 4. Apply physics based on current state
        if (physics != null && stateManager != null)
        {
            physics.UpdatePhysics(inputVector, stateManager.CurrentState, stateManager.CurrentStateData);
        }
    }

    /// <summary>
    /// Handle input from Unity Input System (called by Unity Events)
    /// This maintains compatibility with mobile gamepad and Input System
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        // Read input value from Unity Input System
        inputVector = context.ReadValue<Vector2>();
        
        // Debug logging for input tracking (can be disabled in production)
        if (settings != null && settings.showDebug)
        {
            Debug.Log($"[HelicopterController] Input: {inputVector}");
        }
    }
    
    /// <summary>
    /// Create default settings if none provided
    /// </summary>
    private HelicopterSettings CreateDefaultSettings()
    {
        var defaultSettings = ScriptableObject.CreateInstance<HelicopterSettings>();
        
        // Set default values - separate horizontal and vertical parameters
        defaultSettings.horizontalMaxSpeed = 5f;      // Left/right movement
        defaultSettings.verticalMaxSpeed = 6f;        // Up/down movement (slightly higher for better control)
        defaultSettings.horizontalAcceleration = 12f; // Horizontal thrust
        defaultSettings.verticalAcceleration = 18f;   // Vertical thrust (stronger to overcome gravity)
        
        // Flying state (default)
        defaultSettings.flying = new StateParameters
        {
            maxSpeedMultiplier = 1f,
            accelerationMultiplier = 1f,
            frictionCoefficient = 0.02f,
            allowVerticalThrust = true,
            allowHorizontalThrust = true
        };
        
        // Grounded state (high friction, limited movement)
        defaultSettings.grounded = new StateParameters
        {
            maxSpeedMultiplier = 0.1f,
            accelerationMultiplier = 0.3f,
            frictionCoefficient = 0.8f,
            allowVerticalThrust = true,
            allowHorizontalThrust = false
        };
        
        
        
        
        
        // Collision detection settings
        defaultSettings.raycastDistance = 0.6f;
        defaultSettings.raycastCount = 3;
        
        
        // Debug settings
        defaultSettings.showDebug = false;
        defaultSettings.showRaycastGizmos = false;
        

        return defaultSettings;
    }
    
    /// <summary>
    /// Get current helicopter state for external systems
    /// </summary>
    public HelicopterState GetCurrentState()
    {
        return stateManager != null ? stateManager.CurrentState : HelicopterState.Flying;
    }
    
    /// <summary>
    /// Get current velocity for external systems
    /// </summary>
    public Vector2 GetCurrentVelocity()
    {
        return physics != null ? physics.CurrentVelocity : Vector2.zero;
    }
    
    /// <summary>
    /// Get current input vector for external systems (like propellers)
    /// </summary>
    public Vector2 GetCurrentInput()
    {
        return inputVector;
    }
}
