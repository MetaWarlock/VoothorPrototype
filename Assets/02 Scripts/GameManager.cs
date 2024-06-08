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
    private Queue<Portal> portalQueue;

    void Start()
    {
        portalQueue = new Queue<Portal>(portals);
        StartCoroutine(SpawnPassengers());
    }

    IEnumerator SpawnPassengers()
    {
        while (true)
        {
            if (passengerCount < maxPassengers)
            {
                Portal nextPortal = GetNextPortal();
                if (nextPortal != null)
                {
                    nextPortal.SpawnPassenger(passengerPrefab);
                    passengerCount++;
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    Portal GetNextPortal()
    {
        if (portalQueue.Count == 0)
        {
            return null; // Нет доступных порталов
        }

        Portal nextPortal = portalQueue.Dequeue();
        portalQueue.Enqueue(nextPortal);
        return nextPortal;
    }

    public void PassengerExited()
    {
        passengerCount--;
    }
}
