using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Helicopter state enumeration
/// </summary>
public enum HelicopterState
{
    Flying,      // In air - full control
    Grounded,    // On ground - high friction, limited thrust
    WallContact, // Touching wall - prevent sticking
    Sliding,     // Sliding on surface - increased friction
    InWater      // In water - safe zone with altered physics
}

/// <summary>
/// Data container for current state information
/// </summary>
[System.Serializable]
public class HelicopterStateData
{
    public HelicopterState currentState;
    public HelicopterState previousState;
    public float timeInCurrentState;
    public Vector2 surfaceNormal;
    public bool isGrounded;
    public bool isTouchingWall;
    public bool isInWater;
    public float distanceToGround;
    
    public HelicopterStateData()
    {
        currentState = HelicopterState.Flying;
        previousState = HelicopterState.Flying;
        timeInCurrentState = 0f;
        surfaceNormal = Vector2.up;
        isGrounded = false;
        isTouchingWall = false;
        isInWater = false;
        distanceToGround = float.MaxValue;
    }
}

/// <summary>
/// Custom UnityEvents for helicopter state changes
/// </summary>
[System.Serializable]
public class HelicopterStateEvent : UnityEvent<HelicopterState, HelicopterState> { }

[System.Serializable]
public class HelicopterCollisionEvent : UnityEvent<Vector2, ContactPoint2D[]> { }

[System.Serializable]
public class HelicopterWaterEvent : UnityEvent<bool> { }

/// <summary>
/// Centralized event system for helicopter
/// </summary>
public static class HelicopterEvents
{
    /// <summary>
    /// Fired when helicopter state changes (newState, previousState)
    /// </summary>
    public static HelicopterStateEvent OnStateChanged = new HelicopterStateEvent();
    
    /// <summary>
    /// Fired when helicopter collides with something (velocity, contacts)
    /// </summary>
    public static HelicopterCollisionEvent OnCollision = new HelicopterCollisionEvent();
    
    /// <summary>
    /// Fired when helicopter enters/exits water (isInWater)
    /// </summary>
    public static HelicopterWaterEvent OnWaterStateChanged = new HelicopterWaterEvent();
    
    /// <summary>
    /// Clear all event listeners (useful for cleanup)
    /// </summary>
    public static void ClearAllEvents()
    {
        OnStateChanged.RemoveAllListeners();
        OnCollision.RemoveAllListeners();
        OnWaterStateChanged.RemoveAllListeners();
    }
}

/// <summary>
/// Interface for systems that need to be notified of state changes
/// </summary>
public interface IHelicopterStateListener
{
    void OnStateChanged(HelicopterState newState, HelicopterState previousState);
}

/// <summary>
/// Interface for systems that handle helicopter physics
/// </summary>
public interface IHelicopterPhysicsSystem
{
    void Initialize(HelicopterSettings settings);
    void UpdatePhysics(Vector2 input, HelicopterState currentState, HelicopterStateData stateData);
    Vector2 CurrentVelocity { get; }
}

/// <summary>
/// Interface for collision detection systems
/// </summary>
public interface IHelicopterCollisionSystem
{
    void Initialize(HelicopterSettings settings);
    void UpdateDetection();
    bool IsGrounded { get; }
    bool IsTouchingWall { get; }
    bool IsInWater { get; }
    Vector2 GroundNormal { get; }
    float DistanceToGround { get; }
}
