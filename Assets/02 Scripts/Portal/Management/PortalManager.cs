using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }
    
    [Header("Spawn Settings")]
    public int maxWaitingPassengers = 2;
    public float spawnDelay = 5f; // Delay between spawns
    public float deliveryRespawnDelay = 5f; // Delay after delivery before spawning new passenger
    
    [Header("Portal References")]
    public List<Portal> portals = new List<Portal>();
    
    [Header("Events")]
    public UnityEvent<int> OnPassengerCountChanged;
    public UnityEvent<int, int> PassengerDeliveredEvent; // delivered count, total count
    public UnityEvent<int, int> PassengerLostEvent; // lost count, total count
    
    // Current state
    private List<Passenger> activePassengers = new List<Passenger>();
    private Dictionary<Portal, Passenger> passengerInTransit = new Dictionary<Portal, Passenger>(); // Portal -> Passenger being delivered
    
    // Statistics
    private int totalSpawned = 0;
    private int totalDelivered = 0;
    private int totalLost = 0;
    
    // Spawn timing
    private bool canSpawn = true;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple PortalManager instances detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        ValidateConfiguration();
        StartCoroutine(SpawnManager());
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    private void ValidateConfiguration()
    {
        // Auto-discover portals if list is empty
        if (portals.Count == 0)
        {
            Portal[] foundPortals = Object.FindObjectsByType<Portal>(FindObjectsSortMode.None);
            portals.AddRange(foundPortals);

        }
        
        if (portals.Count == 0)
        {
            Debug.LogError("PortalManager: No portals found on scene!");
            return;
        }
        

    }
    
    private IEnumerator SpawnManager()
    {
        // Initial spawn
        yield return new WaitForSeconds(1f);
        
        while (true)
        {
            // Check if we can spawn more passengers
            if (CanSpawnNewPassenger())
            {
                TrySpawnPassenger();
                yield return new WaitForSeconds(spawnDelay);
            }
            else
            {
                // Wait a bit before checking again
                yield return new WaitForSeconds(1f);
            }
        }
    }
    
    private bool CanSpawnNewPassenger()
    {
        return canSpawn && 
               activePassengers.Count < maxWaitingPassengers && 
               GetAvailableSpawnPortals().Count > 0;
    }
    
    private void TrySpawnPassenger()
    {
        Portal spawnPortal = GetRandomSpawnPortal();
        if (spawnPortal != null)
        {
            spawnPortal.SpawnPassenger();
        }
    }
    
    private Portal GetRandomSpawnPortal()
    {
        List<Portal> availablePortals = GetAvailableSpawnPortals();
        if (availablePortals.Count == 0) return null;
        
        return availablePortals[Random.Range(0, availablePortals.Count)];
    }
    
    private List<Portal> GetAvailableSpawnPortals()
    {
        List<Portal> available = new List<Portal>();
        
        foreach (Portal portal in portals)
        {
            if (portal != null && portal.CanSpawnPassenger())
            {
                available.Add(portal);
            }
        }
        
        return available;
    }
    
    public int GetRandomDestination(int excludePortalNumber)
    {
        List<Portal> destinationPortals = new List<Portal>();
        
        foreach (Portal portal in portals)
        {
            if (portal != null && 
                portal.GetPortalNumber() != excludePortalNumber && 
                portal.CanReceivePassenger())
            {
                destinationPortals.Add(portal);
            }
        }
        
        if (destinationPortals.Count == 0)
        {
            Debug.LogWarning($"No valid destinations for portal {excludePortalNumber}");
            return excludePortalNumber; // Fallback
        }
        
        Portal chosen = destinationPortals[Random.Range(0, destinationPortals.Count)];
        return chosen.GetPortalNumber();
    }
    
    // Called by Passenger when spawned
    public void OnPassengerSpawned(Passenger passenger, Portal spawnPortal)
    {
        activePassengers.Add(passenger);
        totalSpawned++;
        
        Debug.Log($"Passenger spawned at portal {spawnPortal.GetPortalNumber()}. Active: {activePassengers.Count}/{maxWaitingPassengers}");
        
        OnPassengerCountChanged?.Invoke(activePassengers.Count);
    }
    
    // Called by Passenger when boarding helicopter
    public void OnPassengerBoarded(Passenger passenger)
    {
        if (activePassengers.Contains(passenger))
        {
            activePassengers.Remove(passenger);
            
            // Find destination portal
            Portal destinationPortal = GetPortalByNumber(passenger.destinationFloor);
            if (destinationPortal != null)
            {
                passengerInTransit[destinationPortal] = passenger;
            }
            
            Debug.Log($"Passenger boarded, destination: floor {passenger.destinationFloor}. Active: {activePassengers.Count}");
            OnPassengerCountChanged?.Invoke(activePassengers.Count);
        }
    }
    
    // Called by helicopter when landing at destination
    public void OnHelicopterLandedAtPortal(Portal portal)
    {
        if (passengerInTransit.ContainsKey(portal))
        {
            Passenger passenger = passengerInTransit[portal];
            passengerInTransit.Remove(portal);
            
            // Tell portal to receive the passenger
            portal.ReceivePassenger(passenger);
        }
    }
    
    // Called by Portal/Passenger when delivery is complete
    public void OnPassengerDelivered(Passenger passenger)
    {
        totalDelivered++;
        
        Debug.Log($"Passenger delivered! Total delivered: {totalDelivered}");
        PassengerDeliveredEvent?.Invoke(totalDelivered, totalSpawned);
        
        // Schedule respawn after delay
        StartCoroutine(DelayedRespawn());
    }
    
    // Called by Passenger when lost (drowned, etc.)
    public void OnPassengerLost(Passenger passenger)
    {
        if (activePassengers.Contains(passenger))
        {
            activePassengers.Remove(passenger);
        }
        
        // Remove from transit if applicable
        Portal transitPortal = null;
        foreach (var kvp in passengerInTransit)
        {
            if (kvp.Value == passenger)
            {
                transitPortal = kvp.Key;
                break;
            }
        }
        if (transitPortal != null)
        {
            passengerInTransit.Remove(transitPortal);
        }
        
        totalLost++;
        
        Debug.Log($"Passenger lost! Active: {activePassengers.Count}, Total lost: {totalLost}");
        OnPassengerCountChanged?.Invoke(activePassengers.Count);
        PassengerLostEvent?.Invoke(totalLost, totalSpawned);
        
        // Schedule respawn after delay
        StartCoroutine(DelayedRespawn());
    }
    
    private IEnumerator DelayedRespawn()
    {
        canSpawn = false;
        yield return new WaitForSeconds(deliveryRespawnDelay);
        canSpawn = true;
    }
    
    private Portal GetPortalByNumber(int portalNumber)
    {
        foreach (Portal portal in portals)
        {
            if (portal != null && portal.GetPortalNumber() == portalNumber)
            {
                return portal;
            }
        }
        return null;
    }
    
    // Public getters for UI
    public int GetActivePassengerCount() => activePassengers.Count;
    public int GetTotalSpawned() => totalSpawned;
    public int GetTotalDelivered() => totalDelivered;
    public int GetTotalLost() => totalLost;
    public float GetDeliveryRate() => totalSpawned > 0 ? (float)totalDelivered / totalSpawned : 0f;
    
    // Debug info
    void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUI.Box(new Rect(10, 10, 200, 120), "Portal Manager Debug");
            GUI.Label(new Rect(15, 30, 190, 20), $"Active: {activePassengers.Count}/{maxWaitingPassengers}");
            GUI.Label(new Rect(15, 50, 190, 20), $"Spawned: {totalSpawned}");
            GUI.Label(new Rect(15, 70, 190, 20), $"Delivered: {totalDelivered}");
            GUI.Label(new Rect(15, 90, 190, 20), $"Lost: {totalLost}");
            GUI.Label(new Rect(15, 110, 190, 20), $"Rate: {GetDeliveryRate():P1}");
        }
    }
}
