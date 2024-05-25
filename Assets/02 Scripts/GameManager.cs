using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject passengerPrefab;
    public List<Portal> portals;
    public float spawnInterval = 5f;

    private int passengerCount = 0;
    private const int maxPassengers = 2;

    void Start()
    {
        StartCoroutine(SpawnPassengers());
    }

    IEnumerator SpawnPassengers()
    {
        while (true)
        {
            if (passengerCount < maxPassengers)
            {
                Portal randomPortal = portals[Random.Range(0, portals.Count)];
                randomPortal.SpawnPassenger(passengerPrefab);
                passengerCount++;
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void PassengerExited()
    {
        passengerCount--;
    }
}
