using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Configuration")]
    public int portalNumber = 1;
    public PortalMode mode = PortalMode.Both;
    public DestinationType destinationType = DestinationType.Random;
    public int specificDestination = 1;
    
    [Header("Passenger Filters")]
    public PassengerType allowedPassengerTypes = PassengerType.Man | PassengerType.Woman | PassengerType.Elderly;
    
    [Header("Water Settings")]
    public bool disableInWater = true;
    private bool isInWater = false;
    
    [Header("References")]
    public Transform exitPoint;
    public Animator doorAnimator;
    public Transform doorPosition; // Where passengers spawn behind the door
    
    [Header("Passenger Prefabs")]
    public GameObject manPassengerPrefab;
    public GameObject womanPassengerPrefab;
    public GameObject elderlyPassengerPrefab;
    
    // Current state
    private bool isActive = true;
    
    void Start()
    {
        ValidateConfiguration();
    }
    
    void Update()
    {
        CheckWaterContact();
    }
    
    private void ValidateConfiguration()
    {
        if (exitPoint == null)
        {
            Debug.LogError($"Portal {portalNumber}: exitPoint not assigned!");
        }
        
        if (doorPosition == null)
        {
            doorPosition = transform; // Use portal position as default
        }
        
        if (manPassengerPrefab == null || womanPassengerPrefab == null || elderlyPassengerPrefab == null)
        {
            Debug.LogWarning($"Portal {portalNumber}: Some passenger prefabs not assigned!");
        }
    }
    
    private void CheckWaterContact()
    {
        if (!disableInWater) return;
        
        // Check if portal is touching water
        Collider2D waterCollider = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Water"));
        bool newWaterState = waterCollider != null;
        
        if (newWaterState != isInWater)
        {
            isInWater = newWaterState;
            isActive = !isInWater;
            
            if (isInWater)
            {
                Debug.Log($"Portal {portalNumber} disabled due to water contact");
            }
            else
            {
                Debug.Log($"Portal {portalNumber} re-enabled");
            }
        }
    }
    
    public bool CanSpawnPassenger()
    {
        return isActive && (mode == PortalMode.SendOnly || mode == PortalMode.Both);
    }
    
    public bool CanReceivePassenger()
    {
        return isActive && (mode == PortalMode.ReceiveOnly || mode == PortalMode.Both);
    }
    
    public void SpawnPassenger()
    {
        if (!CanSpawnPassenger())
        {
            Debug.LogWarning($"Portal {portalNumber} cannot spawn passengers (mode: {mode}, active: {isActive})");
            return;
        }
        
        // Choose passenger type
        PassengerType chosenType = ChoosePassengerType();
        if (chosenType == 0)
        {
            Debug.LogWarning($"Portal {portalNumber}: No allowed passenger types!");
            return;
        }
        
        // Choose destination
        int destination = ChooseDestination();
        if (destination == portalNumber)
        {
            Debug.LogWarning($"Portal {portalNumber}: Destination same as origin, skipping spawn");
            return;
        }
        
        // Get appropriate prefab
        GameObject prefab = GetPassengerPrefab(chosenType);
        if (prefab == null)
        {
            Debug.LogError($"Portal {portalNumber}: No prefab for passenger type {chosenType}");
            return;
        }
        
        // Spawn passenger
        StartCoroutine(SpawnPassengerSequence(prefab, chosenType, destination));
    }
    
    private System.Collections.IEnumerator SpawnPassengerSequence(GameObject prefab, PassengerType type, int destination)
    {
        // 1. Open door
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
            yield return new WaitForSeconds(0.5f); // Wait for door to open
        }
        
        // 2. Spawn passenger behind door
        GameObject passengerObj = Instantiate(prefab, doorPosition.position, Quaternion.identity);
        Passenger passenger = passengerObj.GetComponent<Passenger>();
        
        if (passenger != null)
        {
            // Configure passenger
            passenger.passengerType = type;
            passenger.destinationFloor = destination;
            passenger.SetDestination(exitPoint.position, destination);
            
            // Notify PortalManager
            PortalManager.Instance?.OnPassengerSpawned(passenger, this);
        }
        
        // 3. Wait for passenger to move to exit point
        yield return new WaitForSeconds(2f);
        
        // 4. Close door
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");
        }
    }
    
    public void ReceivePassenger(Passenger passenger)
    {
        if (!CanReceivePassenger())
        {
            Debug.LogWarning($"Portal {portalNumber} cannot receive passengers (mode: {mode}, active: {isActive})");
            return;
        }
        
        StartCoroutine(ReceivePassengerSequence(passenger));
    }
    
    private System.Collections.IEnumerator ReceivePassengerSequence(Passenger passenger)
    {
        // 1. Open door
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 2. Passenger emerges and moves to door
        passenger.EmergeAtDestination(doorPosition);
        
        // 3. Wait for passenger to reach door
        yield return new WaitForSeconds(3f);
        
        // 4. Close door
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");
        }
    }
    
    private PassengerType ChoosePassengerType()
    {
        var availableTypes = System.Enum.GetValues(typeof(PassengerType));
        var validTypes = new System.Collections.Generic.List<PassengerType>();
        
        foreach (PassengerType type in availableTypes)
        {
            if (type != 0 && (allowedPassengerTypes & type) == type)
            {
                validTypes.Add(type);
            }
        }
        
        if (validTypes.Count == 0) return 0;
        return validTypes[Random.Range(0, validTypes.Count)];
    }
    
    private int ChooseDestination()
    {
        if (destinationType == DestinationType.Specific)
        {
            return specificDestination;
        }
        
        // Random destination - ask PortalManager
        return PortalManager.Instance?.GetRandomDestination(portalNumber) ?? 1;
    }
    
    private GameObject GetPassengerPrefab(PassengerType type)
    {
        switch (type)
        {
            case PassengerType.Man: return manPassengerPrefab;
            case PassengerType.Woman: return womanPassengerPrefab;
            case PassengerType.Elderly: return elderlyPassengerPrefab;
            default: return null;
        }
    }
    
    // Public getters
    public int GetPortalNumber() => portalNumber;
    public bool IsActive() => isActive;
    public PortalMode GetMode() => mode;
}
