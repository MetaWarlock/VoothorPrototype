using UnityEngine;

/// <summary>
/// Test script for helicopter systems - helps debug and validate state transitions
/// Attach this to a GameObject with HelicopterController to see system status in inspector
/// </summary>
public class HelicopterTester : MonoBehaviour
{
    [Header("Test Controls")]
    [Tooltip("Force a specific state for testing")]
    [SerializeField] private HelicopterState forceState = HelicopterState.Flying;
    
    [Tooltip("Apply forced state on button press")]
    [SerializeField] private bool applyForceState = false;
    
    [Header("Current Status (Read Only)")]
    [Space(10)]
    [SerializeField] private HelicopterState currentState;
    [SerializeField] private HelicopterState previousState;
    [SerializeField] private float timeInCurrentState;
    [SerializeField] private Vector2 currentVelocity;
    [SerializeField] private Vector2 inputVector;
    
    [Header("Collision Status (Read Only)")]
    [Space(10)]
    [SerializeField] private bool isGrounded;


    [SerializeField] private float distanceToGround;
    [SerializeField] private Vector2 surfaceNormal;
    
    [Header("Physics Parameters (Read Only)")]
    [Space(10)]
    [SerializeField] private float currentMaxSpeed;
    [SerializeField] private float currentAcceleration;
    [SerializeField] private float currentFriction;
    
    // Component references
    private HelicopterController controller;
    private HelicopterStateManager stateManager;
    private HelicopterPhysics physics;
    private HelicopterCollisionDetector collisionDetector;
    
    void Awake()
    {
        // Get components
        controller = GetComponent<HelicopterController>();
        stateManager = GetComponent<HelicopterStateManager>();
        physics = GetComponent<HelicopterPhysics>();
        collisionDetector = GetComponent<HelicopterCollisionDetector>();
        
        // Subscribe to events
        HelicopterEvents.OnStateChanged.AddListener(OnStateChanged);
        
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        HelicopterEvents.OnStateChanged.RemoveListener(OnStateChanged);
        
    }
    
    void Update()
    {
        // Update display values
        UpdateDisplayValues();
        
        // Handle test controls
        HandleTestControls();
    }
    
    /// <summary>
    /// Update inspector display values
    /// </summary>
    private void UpdateDisplayValues()
    {
        if (stateManager != null)
        {
            var stateData = stateManager.CurrentStateData;
            currentState = stateData.currentState;
            previousState = stateData.previousState;
            timeInCurrentState = stateData.timeInCurrentState;
            
            isGrounded = stateData.isGrounded;
            
            surfaceNormal = stateData.surfaceNormal;
            distanceToGround = stateData.distanceToGround;
        }
        
        if (physics != null)
        {
            currentVelocity = physics.CurrentVelocity;
            var physicsParams = physics.GetCurrentParameters();
            currentMaxSpeed = Mathf.Max(physicsParams.horizontalMaxSpeed, physicsParams.verticalMaxSpeed); // Show the higher of the two
            currentAcceleration = Mathf.Max(physicsParams.horizontalAcceleration, physicsParams.verticalAcceleration); // Show the higher of the two
            currentFriction = physicsParams.friction;
        }
        
        if (collisionDetector != null)
        {
            distanceToGround = collisionDetector.DistanceToGround;
        }
        
        // Get input for display
        inputVector = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
    }
    
    /// <summary>
    /// Handle test controls from inspector
    /// </summary>
    private void HandleTestControls()
    {
        if (applyForceState)
        {
            applyForceState = false; // Reset button
            
            if (stateManager != null)
            {
                stateManager.ForceState(forceState);
                Debug.Log("[HelicopterTester] Forced state to: " + forceState);
            }
        }
        
        // Keyboard shortcuts for quick testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ForceStateByKey(HelicopterState.Flying);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ForceStateByKey(HelicopterState.Grounded);
        }

    }
    
    /// <summary>
    /// Force state via keyboard shortcut
    /// </summary>
    private void ForceStateByKey(HelicopterState state)
    {
        if (stateManager != null)
        {
            stateManager.ForceState(state);
        }
    }
    
    /// <summary>
    /// Handle state change events
    /// </summary>
    private void OnStateChanged(HelicopterState newState, HelicopterState previousState)
    {
        // Log state-specific information
        switch (newState)
        {
            case HelicopterState.Flying:

                break;
            case HelicopterState.Grounded:

                break;

        }
    }
    
    /// <summary>
    /// Display help in inspector
    /// </summary>
    [Space(20)]
    [Header("Test Help")]
    [Space(5)]
    [TextArea(8, 10)]
    public string helpText = "HELICOPTER STATE TESTER\n\n" +
        "Keyboard Shortcuts:\n" +
        "1 - Force Flying State\n" +
        "2 - Force Grounded State\n" +

        "Inspector Controls:\n" +
        "- Set Force State dropdown\n" +
        "- Click Apply Force State\n\n" +
        "Watch Console for state transition logs!";
}
