using UnityEngine;
using TMPro;

public class Passenger : MonoBehaviour
{
    [Header("Passenger Configuration")]
    public PassengerType passengerType = PassengerType.Man;
    public PassengerSettings settings;
    
    [Header("Destination")]
    public int destinationFloor = 1;
    
    [Header("UI Elements")]
    public Canvas floorUI;
    public TextMeshProUGUI floorText;
    
    [Header("References")]
    public Transform exitPoint;
    public Transform portalDoor;
    
    // Current state
    public PortalPassengerState currentState = PortalPassengerState.Spawning;
    
    // Internal references
    private HelicopterController helicopter;
    private HelicopterStateManager helicopterStateManager;
    
    // Movement and timing
    private Vector3 targetPosition;
    private float waterSurvivalTimer;
    private bool isInWater = false;
    
    // Constants
    private const string HELICOPTER_TAG = "Player";
    private const float ARRIVAL_THRESHOLD = 0.1f;
    
    void Start()
    {
        if (settings == null)
        {
            Debug.LogError($"PassengerSettings not assigned for {gameObject.name}");
            return;
        }
        
        // Initialize UI
        UpdateFloorUI();
        
        // Start state machine
        ChangeState(PortalPassengerState.MovingToWaitPoint);
    }
    
    void Update()
    {
        UpdateStateMachine();
        
        // Update water survival timer
        if (isInWater)
        {
            waterSurvivalTimer -= Time.deltaTime;
            if (waterSurvivalTimer <= 0)
            {
                ChangeState(PortalPassengerState.Drowned);
            }
        }
    }
    
    private void UpdateStateMachine()
    {
        switch (currentState)
        {
            case PortalPassengerState.Spawning:
                // Handled by Portal
                break;
                
            case PortalPassengerState.MovingToWaitPoint:
                MoveTowardsTarget();
                if (ReachedTarget())
                {
                    ChangeState(PortalPassengerState.Waiting);
                }
                break;
                
            case PortalPassengerState.Waiting:
                CheckForHelicopter();
                break;
                
            case PortalPassengerState.MovingToHelicopter:
                MoveTowardsTarget();
                if (ReachedTarget())
                {
                    ChangeState(PortalPassengerState.Boarding);
                }
                // Check if helicopter took off
                if (helicopter == null || !IsHelicopterGrounded())
                {
                    ChangeState(PortalPassengerState.ReturningToWaitPoint);
                }
                break;
                
            case PortalPassengerState.Boarding:
                // Passenger disappears (handled in ChangeState)
                break;
                
            case PortalPassengerState.ReturningToWaitPoint:
                MoveTowardsTarget();
                if (ReachedTarget())
                {
                    ChangeState(PortalPassengerState.Waiting);
                }
                break;
                
            case PortalPassengerState.EmergeFromHelicopter:
                // Passenger reappears at destination portal
                ChangeState(PortalPassengerState.MovingToPortalDoor);
                break;
                
            case PortalPassengerState.MovingToPortalDoor:
                MoveTowardsTarget();
                if (ReachedTarget())
                {
                    // Mission completed - notify PortalManager
                    PortalManager.Instance?.OnPassengerDelivered(this);
                    Destroy(gameObject);
                }
                break;
                
            case PortalPassengerState.InWater:
                // Floating in water, checking for helicopter
                CheckForHelicopterInWater();
                break;
                
            case PortalPassengerState.SwimmingToHelicopter:
                SwimTowardsHelicopter();
                if (ReachedTarget())
                {
                    ChangeState(PortalPassengerState.Boarding);
                }
                break;
                
            case PortalPassengerState.Drowned:
                // Mission failed - notify PortalManager
                PortalManager.Instance?.OnPassengerLost(this);
                Destroy(gameObject);
                break;
        }
    }
    
    private void ChangeState(PortalPassengerState newState)
    {
        // Exit current state
        switch (currentState)
        {
            case PortalPassengerState.Waiting:
                SetFloorUIVisible(false);
                break;
        }
        
        currentState = newState;
        
        // Enter new state
        switch (newState)
        {
            case PortalPassengerState.MovingToWaitPoint:
                targetPosition = exitPoint.position;
                break;
                
            case PortalPassengerState.Waiting:
                SetFloorUIVisible(true);
                break;
                
            case PortalPassengerState.MovingToHelicopter:
                if (helicopter != null)
                    targetPosition = helicopter.transform.position;
                break;
                
            case PortalPassengerState.Boarding:
                SetFloorUIVisible(false);
                gameObject.SetActive(false); // Passenger disappears
                break;
                
            case PortalPassengerState.ReturningToWaitPoint:
                targetPosition = exitPoint.position;
                helicopter = null;
                break;
                
            case PortalPassengerState.EmergeFromHelicopter:
                gameObject.SetActive(true); // Passenger reappears
                break;
                
            case PortalPassengerState.MovingToPortalDoor:
                if (portalDoor != null)
                    targetPosition = portalDoor.position;
                break;
                
            case PortalPassengerState.InWater:
                isInWater = true;
                waterSurvivalTimer = settings.waterSurvivalTime;
                break;
                
            case PortalPassengerState.SwimmingToHelicopter:
                if (helicopter != null)
                    targetPosition = helicopter.transform.position;
                break;
        }
    }
    
    private void MoveTowardsTarget()
    {
        float speed = isInWater ? settings.swimSpeed : settings.walkSpeed;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
    
    private void SwimTowardsHelicopter()
    {
        if (helicopter != null)
        {
            targetPosition = helicopter.transform.position;
            MoveTowardsTarget();
        }
    }
    
    private bool ReachedTarget()
    {
        return Vector3.Distance(transform.position, targetPosition) < ARRIVAL_THRESHOLD;
    }
    
    private void CheckForHelicopter()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, settings.helicopterDetectionRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag(HELICOPTER_TAG))
            {
                HelicopterController heli = collider.GetComponent<HelicopterController>();
                HelicopterStateManager stateManager = collider.GetComponent<HelicopterStateManager>();
                
                if (heli != null && stateManager != null && IsHelicopterReadyForBoarding(stateManager))
                {
                    helicopter = heli;
                    helicopterStateManager = stateManager;
                    ChangeState(PortalPassengerState.MovingToHelicopter);
                    break;
                }
            }
        }
    }
    
    private void CheckForHelicopterInWater()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, settings.helicopterDetectionRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag(HELICOPTER_TAG))
            {
                HelicopterStateManager stateManager = collider.GetComponent<HelicopterStateManager>();
                
                if (stateManager != null && stateManager.CurrentState == HelicopterState.Grounded)
                {
                    helicopter = collider.GetComponent<HelicopterController>();
                    helicopterStateManager = stateManager;
                    ChangeState(PortalPassengerState.SwimmingToHelicopter);
                    break;
                }
            }
        }
    }
    
    private bool IsHelicopterReadyForBoarding(HelicopterStateManager stateManager)
    {
        HelicopterState state = stateManager.CurrentState;
        return state == HelicopterState.Grounded;
    }
    
    private bool IsHelicopterGrounded()
    {
        if (helicopterStateManager == null) return false;
        return IsHelicopterReadyForBoarding(helicopterStateManager);
    }
    
    private void UpdateFloorUI()
    {
        if (floorText != null)
        {
            floorText.text = destinationFloor.ToString();
        }
    }
    
    private void SetFloorUIVisible(bool visible)
    {
        if (floorUI != null)
        {
            floorUI.gameObject.SetActive(visible);
        }
    }
    
    // Public methods for external systems
    public void SetDestination(Vector3 exitPointPos, int destFloor)
    {
        exitPoint.position = exitPointPos;
        destinationFloor = destFloor;
        UpdateFloorUI();
    }
    
    public void SetPortalDoor(Transform door)
    {
        portalDoor = door;
    }
    
    public void FallIntoWater()
    {
        if (currentState == PortalPassengerState.Waiting || currentState == PortalPassengerState.MovingToHelicopter)
        {
            ChangeState(PortalPassengerState.InWater);
        }
    }
    
    public void EmergeAtDestination(Transform destinationDoor)
    {
        portalDoor = destinationDoor;
        ChangeState(PortalPassengerState.EmergeFromHelicopter);
    }
    
    // Collision detection for helicopter contact in flight
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(HELICOPTER_TAG))
        {
            HelicopterStateManager stateManager = other.GetComponent<HelicopterStateManager>();
            if (stateManager != null && stateManager.CurrentState == HelicopterState.Flying)
            {
                FallIntoWater();
            }
        }
    }
}
