using UnityEngine;

/// <summary>
/// Integrates helicopter with portal system for passenger boarding/delivery
/// Attach this to the helicopter GameObject
/// </summary>
public class HelicopterPortalIntegration : MonoBehaviour
{
    [Header("Integration Settings")]
    public float portalDetectionRadius = 2f;
    public LayerMask portalLayerMask = -1;
    
    // Component references
    private HelicopterStateManager stateManager;
    private HelicopterController controller;
    
    // Current state
    private Portal nearbyPortal;
    private Passenger currentPassenger; // Passenger currently in helicopter
    
    void Start()
    {
        // Get helicopter components
        stateManager = GetComponent<HelicopterStateManager>();
        controller = GetComponent<HelicopterController>();
        
        if (stateManager == null)
        {
            Debug.LogError("HelicopterPortalIntegration: HelicopterStateManager not found!");
        }
        
        if (controller == null)
        {
            Debug.LogError("HelicopterPortalIntegration: HelicopterController not found!");
        }
    }
    
    void Update()
    {
        CheckPortalProximity();
        HandlePassengerBoarding();
    }
    
    private void CheckPortalProximity()
    {
        // Find nearby portals
        Collider2D[] portals = Physics2D.OverlapCircleAll(transform.position, portalDetectionRadius, portalLayerMask);
        Portal closestPortal = null;
        float closestDistance = float.MaxValue;
        
        foreach (var collider in portals)
        {
            Portal portal = collider.GetComponent<Portal>();
            if (portal != null)
            {
                float distance = Vector2.Distance(transform.position, portal.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPortal = portal;
                }
            }
        }
        
        // Update nearby portal
        if (closestPortal != nearbyPortal)
        {
            nearbyPortal = closestPortal;
            
            // If we landed at a portal with a passenger, try to deliver them
            if (nearbyPortal != null && currentPassenger != null && IsGrounded())
            {
                TryDeliverPassenger();
            }
        }
    }
    
    private void HandlePassengerBoarding()
    {
        // Only handle boarding when grounded and no passenger aboard
        if (!IsGrounded() || currentPassenger != null) return;
        
        // Check for passengers wanting to board
        Collider2D[] passengers = Physics2D.OverlapCircleAll(transform.position, portalDetectionRadius);
        
        foreach (var collider in passengers)
        {
            Passenger passenger = collider.GetComponent<Passenger>();
            if (passenger != null && 
                passenger.currentState == PortalPassengerState.MovingToHelicopter &&
                Vector2.Distance(transform.position, passenger.transform.position) < 1f)
            {
                BoardPassenger(passenger);
                break; // Only one passenger at a time
            }
        }
    }
    
    private void BoardPassenger(Passenger passenger)
    {
        currentPassenger = passenger;
        
        // Notify systems
        PortalManager.Instance?.OnPassengerBoarded(passenger);
        
        Debug.Log($"Passenger boarded! Destination: Floor {passenger.destinationFloor}");
    }
    
    private void TryDeliverPassenger()
    {
        if (nearbyPortal == null || currentPassenger == null) return;
        
        // Check if this is the correct destination
        if (nearbyPortal.GetPortalNumber() == currentPassenger.destinationFloor)
        {
            // Correct destination - deliver passenger
            PortalManager.Instance?.OnHelicopterLandedAtPortal(nearbyPortal);
            currentPassenger = null;
            
            Debug.Log($"Passenger delivered to floor {nearbyPortal.GetPortalNumber()}!");
        }
        else
        {
            Debug.Log($"Wrong floor! Passenger wants floor {currentPassenger.destinationFloor}, but this is floor {nearbyPortal.GetPortalNumber()}");
        }
    }
    
    private bool IsGrounded()
    {
        if (stateManager == null) return false;
        
        HelicopterState state = stateManager.CurrentState;
        return state == HelicopterState.Grounded;
    }
    
    // Public getters for UI
    public bool HasPassenger() => currentPassenger != null;
    public int GetPassengerDestination() => currentPassenger?.destinationFloor ?? 0;
    public Portal GetNearbyPortal() => nearbyPortal;
    
    // Visual debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, portalDetectionRadius);
        
        if (nearbyPortal != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nearbyPortal.transform.position);
        }
    }
}
