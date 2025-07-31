using UnityEngine;

/// <summary>
/// ScriptableObject containing all helicopter configuration parameters.
/// This allows easy tweaking of helicopter behavior without code changes.
/// </summary>
[CreateAssetMenu(fileName = "HelicopterSettings", menuName = "Helicopter/Settings")]
public class HelicopterSettings : ScriptableObject
{
    [Header("General Physics")]
    [Tooltip("Maximum horizontal speed (left/right movement)")]
    public float horizontalMaxSpeed = 5f;
    
    [Tooltip("Maximum vertical speed (up/down movement)")]
    public float verticalMaxSpeed = 6f;
    
    [Tooltip("Horizontal acceleration (left/right thrust)")]
    public float horizontalAcceleration = 10f;
    
    [Tooltip("Vertical acceleration (up/down thrust)")]
    public float verticalAcceleration = 15f;
    
    [Header("State-Specific Parameters")]
    public StateParameters flying = new StateParameters
    {
        maxSpeedMultiplier = 1f,
        accelerationMultiplier = 1f,
        frictionCoefficient = 0.02f,
        allowVerticalThrust = true,
        allowHorizontalThrust = true
    };
    
    public StateParameters grounded = new StateParameters
    {
        maxSpeedMultiplier = 0.1f,
        accelerationMultiplier = 0.3f,
        frictionCoefficient = 0.8f,
        allowVerticalThrust = true,
        allowHorizontalThrust = false
    };
    
    public StateParameters wallContact = new StateParameters
    {
        maxSpeedMultiplier = 1f,
        accelerationMultiplier = 1f,
        frictionCoefficient = 0.1f,
        allowVerticalThrust = true,
        allowHorizontalThrust = true
    };
    
    public StateParameters sliding = new StateParameters
    {
        maxSpeedMultiplier = 0.6f,
        accelerationMultiplier = 0.5f,
        frictionCoefficient = 0.6f,
        allowVerticalThrust = true,
        allowHorizontalThrust = true
    };
    
    public StateParameters inWater = new StateParameters
    {
        maxSpeedMultiplier = 0.6f,
        accelerationMultiplier = 0.4f,
        frictionCoefficient = 0.7f,
        allowVerticalThrust = true,
        allowHorizontalThrust = true,
        buoyancy = 0.2f
    };
    
    [Header("Collision Detection")]
    [Tooltip("Distance for raycast collision detection")]
    public float raycastDistance = 0.6f;
    
    [Tooltip("Number of raycasts per direction (more = better accuracy)")]
    public int raycastCount = 3;
    
    [Tooltip("Minimum speed to consider for wall sticking prevention")]
    public float minimumStickPreventionSpeed = 0.1f;
    
    [Header("State Transition")]
    [Tooltip("Minimum speed to transition from Sliding to Grounded")]
    public float slidingToGroundedThreshold = 0.1f;
    
    [Tooltip("Time before transitioning from WallContact to Flying")]
    public float wallContactTimeout = 0.5f;
    
    [Header("Debug")]
    [Tooltip("Show debug information and gizmos")]
    public bool showDebug = false;
    
    [Tooltip("Show raycast lines in scene view")]
    public bool showRaycastGizmos = false;
}

/// <summary>
/// Parameters for each helicopter state
/// </summary>
[System.Serializable]
public class StateParameters
{
    [Tooltip("Multiplier for maximum speed in this state")]
    [Range(0f, 2f)]
    public float maxSpeedMultiplier = 1f;
    
    [Tooltip("Multiplier for acceleration in this state")]
    [Range(0f, 2f)]
    public float accelerationMultiplier = 1f;
    
    [Tooltip("Friction coefficient for this state")]
    [Range(0f, 1f)]
    public float frictionCoefficient = 0.02f;
    
    [Tooltip("Allow vertical thrust in this state")]
    public bool allowVerticalThrust = true;
    
    [Tooltip("Allow horizontal thrust in this state")]
    public bool allowHorizontalThrust = true;
    
    [Tooltip("Buoyancy force (only used in water)")]
    [Range(0f, 1f)]
    public float buoyancy = 0f;
    
    /// <summary>
    /// Get effective horizontal max speed based on settings and multiplier
    /// </summary>
    public float GetHorizontalMaxSpeed(float baseHorizontalMaxSpeed) => baseHorizontalMaxSpeed * maxSpeedMultiplier;
    
    /// <summary>
    /// Get effective vertical max speed based on settings and multiplier
    /// </summary>
    public float GetVerticalMaxSpeed(float baseVerticalMaxSpeed) => baseVerticalMaxSpeed * maxSpeedMultiplier;
    
    /// <summary>
    /// Get effective horizontal acceleration based on settings and multiplier
    /// </summary>
    public float GetHorizontalAcceleration(float baseHorizontalAcceleration) => baseHorizontalAcceleration * accelerationMultiplier;
    
    /// <summary>
    /// Get effective vertical acceleration based on settings and multiplier
    /// </summary>
    public float GetVerticalAcceleration(float baseVerticalAcceleration) => baseVerticalAcceleration * accelerationMultiplier;
}
