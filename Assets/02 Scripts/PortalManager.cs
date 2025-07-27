using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject passengerPrefab;
    public float spawnInterval = 10f;

    [Header("Portal and Game References")]
    public List<Portal> portals = new List<Portal>();


    void Start()
    {
        if (passengerPrefab == null)
        {
            Debug.LogError("Passenger Prefab is not assigned in the PortalManager inspector!");
            return;
        }

        StartCoroutine(SpawnPassengers());
    }

    private IEnumerator SpawnPassengers()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            Portal portalToSpawn = GetRandomPortal();
            if (portalToSpawn != null)
            {
                SpawnPassengerInPortal(portalToSpawn, passengerPrefab);
            }
            else
            {
                Debug.LogWarning("No portals available in PortalManager to spawn a passenger.");
            }
        }
    }

    private void SpawnPassengerInPortal(Portal portal, GameObject passengerPrefab)
    {
        if (portal != null)
        {
            portal.SpawnPassenger(passengerPrefab);
        }
    }

    private Portal GetRandomPortal()
    {
        if (portals.Count == 0)
        {
            return null;
        }
        return portals[Random.Range(0, portals.Count)];
    }
}
