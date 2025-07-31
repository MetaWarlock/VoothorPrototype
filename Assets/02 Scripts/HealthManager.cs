using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages health for game objects, particularly the helicopter player.
/// Handles damage calculation based on collision speed and provides events for UI updates.
/// </summary>
public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health value")]
    public float maxHealth = 100f;
    
    [Tooltip("Current health value")]
    [SerializeField] private float currentHealth;
    
    [Header("Damage Settings")]
    [Tooltip("Minimum collision speed to cause damage")]
    public float minDamageSpeed = 0.8f;
    
    [Tooltip("Maximum collision speed for damage calculation")]
    public float maxDamageSpeed = 7f;
    
    [Tooltip("Minimum damage percentage (0-50%)")]
    public float minDamagePercent = 5f;
    
    [Tooltip("Maximum damage percentage (0-50%)")]
    public float maxDamagePercent = 50f;
    
    [Header("Invincibility")]
    [Tooltip("Time in seconds of invincibility after taking damage")]
    public float invincibilityTime = 1f;
    
    [Tooltip("Enable debug logging")]
    public bool showDebug = false;
    
    // Events for UI and other systems
    [System.Serializable]
    public class HealthEvent : UnityEvent<float, float> { } // current, max
    
    [Header("Events")]
    public HealthEvent onHealthChanged;
    public UnityEvent onDeath;
    public UnityEvent onDamageTaken;
    public UnityEvent onHealthRestored;
    
    // Private variables
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private Rigidbody2D rb;
    
    // Properties
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public bool IsAlive => currentHealth > 0f;
    public bool IsInvincible => isInvincible;
    public bool IsAtFullHealth => Mathf.Approximately(currentHealth, maxHealth);
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Initialize health to maximum
        currentHealth = maxHealth;
        
        // Initialize events if null
        if (onHealthChanged == null) onHealthChanged = new HealthEvent();
        if (onDeath == null) onDeath = new UnityEvent();
        if (onDamageTaken == null) onDamageTaken = new UnityEvent();
        if (onHealthRestored == null) onHealthRestored = new UnityEvent();
    }
    
    void Start()
    {
        // Notify initial health state
        onHealthChanged.Invoke(currentHealth, maxHealth);
    }
    
    void Update()
    {
        // Handle invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (showDebug)
                {
                    Debug.Log($"{name}: Invincibility ended");
                }
            }
        }
    }
    
    /// <summary>
    /// Apply damage based on collision speed
    /// </summary>
    /// <param name="collisionSpeed">Speed of collision</param>
    /// <returns>Actual damage dealt</returns>
    public float TakeDamageFromCollision(float collisionSpeed)
    {
        if (!IsAlive || isInvincible)
        {
            return 0f;
        }
        
        // Calculate damage based on speed
        float damagePercent = CalculateDamagePercent(collisionSpeed);
        
        if (damagePercent > 0f)
        {
            float damage = maxHealth * (damagePercent / 100f);
            return TakeDamage(damage);
        }
        
        return 0f;
    }
    
    /// <summary>
    /// Apply raw damage amount
    /// </summary>
    /// <param name="damage">Damage amount</param>
    /// <returns>Actual damage dealt</returns>
    public float TakeDamage(float damage)
    {
        if (!IsAlive || isInvincible)
        {
            return 0f;
        }
        
        float actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth = Mathf.Max(0f, currentHealth - actualDamage);
        
        // Start invincibility period
        StartInvincibility();
        
        // Trigger events
        onHealthChanged.Invoke(currentHealth, maxHealth);
        onDamageTaken.Invoke();
        
        if (showDebug)
        {
            Debug.Log($"{name}: Took {actualDamage:F1} damage. Health: {currentHealth:F1}/{maxHealth:F1}");
        }
        
        // Check for death
        if (!IsAlive)
        {
            HandleDeath();
        }
        
        return actualDamage;
    }
    
    /// <summary>
    /// Restore health
    /// </summary>
    /// <param name="amount">Amount to heal</param>
    /// <returns>Actual amount healed</returns>
    public float Heal(float amount)
    {
        if (!IsAlive) return 0f;
        
        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        float actualHealing = currentHealth - oldHealth;
        
        if (actualHealing > 0f)
        {
            onHealthChanged.Invoke(currentHealth, maxHealth);
            onHealthRestored.Invoke();
            
            if (showDebug)
            {
                Debug.Log($"{name}: Healed {actualHealing:F1}. Health: {currentHealth:F1}/{maxHealth:F1}");
            }
        }
        
        return actualHealing;
    }
    
    /// <summary>
    /// Fully restore health
    /// </summary>
    public void FullHeal()
    {
        Heal(maxHealth);
    }
    
    /// <summary>
    /// Set health to specific value
    /// </summary>
    /// <param name="health">New health value</param>
    public void SetHealth(float health)
    {
        float oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        
        if (!Mathf.Approximately(oldHealth, currentHealth))
        {
            onHealthChanged.Invoke(currentHealth, maxHealth);
            
            if (currentHealth > oldHealth)
            {
                onHealthRestored.Invoke();
            }
            else if (currentHealth < oldHealth)
            {
                onDamageTaken.Invoke();
            }
            
            if (!IsAlive && oldHealth > 0f)
            {
                HandleDeath();
            }
        }
    }
    
    /// <summary>
    /// Calculate damage percentage based on collision speed
    /// </summary>
    private float CalculateDamagePercent(float speed)
    {
        if (showDebug)
        {
            Debug.Log($"[DAMAGE CALC] Checking speed: {speed:F2} against minDamageSpeed: {minDamageSpeed:F2}");
        }

        if (speed < minDamageSpeed)
        {
            if (showDebug && speed > 0.1f)
            {
                Debug.Log("[DAMAGE CALC] Speed is too low, no damage.");
            }
            return 0f;
        }
        
        // Normalize speed between min and max damage speeds
        float normalizedSpeed = Mathf.InverseLerp(minDamageSpeed, maxDamageSpeed, speed);
        normalizedSpeed = Mathf.Clamp01(normalizedSpeed);
        
        // Calculate damage percentage
        return Mathf.Lerp(minDamagePercent, maxDamagePercent, normalizedSpeed);
    }
    
    /// <summary>
    /// Start invincibility period
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        
        if (showDebug)
        {
            Debug.Log($"{name}: Started invincibility for {invincibilityTime}s");
        }
    }
    
    /// <summary>
    /// Handle death logic
    /// </summary>
    private void HandleDeath()
    {
        if (showDebug)
        {
            Debug.Log($"{name}: Died!");
        }
        
        onDeath.Invoke();
    }
    
    /// <summary>
    /// Get collision speed from Collision2D
    /// </summary>
    /// <param name="collision">Collision data</param>
    /// <returns>Collision speed magnitude</returns>
    public float GetCollisionSpeed(Collision2D collision)
    {
        if (rb != null)
        {
            return rb.linearVelocity.magnitude;
        }
        
        // Fallback: use relative velocity magnitude
        return collision.relativeVelocity.magnitude;
    }
    
    /// <summary>
    /// Handle collision for automatic damage calculation
    /// Should be called from OnCollisionEnter2D
    /// </summary>
    /// <param name="collision">Collision data</param>
    /// <param name="damageableTag">Tag that can cause damage (optional)</param>
    public void HandleCollision(Collision2D collision, string damageableTag = null)
    {
        // Check if this collision should cause damage
        if (!string.IsNullOrEmpty(damageableTag) && !collision.gameObject.CompareTag(damageableTag))
        {
            return;
        }
        
        float speed = GetCollisionSpeed(collision);
        TakeDamageFromCollision(speed);
    }
    
    #region Debug Info
    /// <summary>
    /// Get debug information about current health state
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Health: {currentHealth:F1}/{maxHealth:F1} ({HealthPercentage:P1}) - " +
               $"Alive: {IsAlive}, Invincible: {IsInvincible}" +
               (isInvincible ? $" ({invincibilityTimer:F1}s)" : "");
    }
    
    void OnGUI()
    {
        if (showDebug && Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label($"Health Debug: {name}");
            GUILayout.Label(GetDebugInfo());
            GUILayout.EndArea();
        }
    }
    #endregion
}
