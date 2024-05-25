using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform exitPoint;

    public void SpawnPassenger(GameObject passengerPrefab)
    {
        GameObject passenger = Instantiate(passengerPrefab, transform.position, Quaternion.identity);
        Passenger passengerScript = passenger.GetComponent<Passenger>();
        passengerScript.SetDestination(exitPoint.position);
        passengerScript.SetGameManager(FindObjectOfType<GameManager>());
    }
}
