using UnityEngine;

[CreateAssetMenu(fileName = "PassengerSettings", menuName = "Helicopter Taxi/Passenger Settings")]
public class PassengerSettings : ScriptableObject
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float swimSpeed = 1f;
    
    [Header("Water Survival")]
    public float waterSurvivalTime = 10f; // seconds
    
    [Header("Detection")]
    public float helicopterDetectionRadius = 3f;
    
    [Header("Boarding")]
    public float boardingSpeed = 1f;
    public float boardingRadius = 1.5f;
}
