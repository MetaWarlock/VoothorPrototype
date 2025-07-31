using UnityEngine;

/// <summary>
/// Trigger-based damage detection system for helicopter.
/// This script should be attached to a child GameObject with a trigger collider
/// that is slightly larger than the helicopter's main collider.
/// </summary>
public class HelicopterDamageTrigger : MonoBehaviour
{
    [Header("Components")]
    [Tooltip("Reference to the helicopter's HealthManager")]
    public HealthManager healthManager;
    
    [Tooltip("Reference to the helicopter's Rigidbody2D")]
    public Rigidbody2D helicopterRigidbody;
    
    [Header("Damage Settings")]
    [Tooltip("Tags that can cause damage")]
    public string[] damageableTags = { "Ground", "Wall", "Obstacle" };
    
    [Tooltip("Whether water causes damage")]
    public bool waterCausesDamage = true;
    
    [Header("Timing")]
    [Tooltip("Minimum time between damage applications")]
    public float damageFrequency = 0.5f;
    
    private float lastDamageTime = 0f;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (healthManager == null)
        {
            healthManager = GetComponentInParent<HealthManager>();
        }
        
        if (helicopterRigidbody == null)
        {
            helicopterRigidbody = GetComponentInParent<Rigidbody2D>();
        }
        
        // Validate setup
        if (healthManager == null)
        {
            Debug.LogError($"[HelicopterDamageTrigger] HealthManager not found on {gameObject.name}!");
        }
        
        if (helicopterRigidbody == null)
        {
            Debug.LogError($"[HelicopterDamageTrigger] Rigidbody2D not found on {gameObject.name}!");
        }
        
        // Ensure this object has a trigger collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"[HelicopterDamageTrigger] No Collider2D found on {gameObject.name}!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[HelicopterDamageTrigger] Collider on {gameObject.name} is not set as Trigger!");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if enough time has passed since last damage
        if (Time.time - lastDamageTime < damageFrequency)
        {
            return;
        }
        
        // Check if this object can cause damage
        if (!CanCauseDamage(other.gameObject))
        {
            return;
        }
        
        // Get helicopter velocity at the moment of trigger enter
        Vector2 helicopterVelocity = helicopterRigidbody.linearVelocity;
        float totalSpeed = helicopterVelocity.magnitude;
        
        // For trigger-based detection, we use total speed as impact speed
        // since we're detecting BEFORE the collision changes velocity
        float impactSpeed = totalSpeed;
        
        // Apply damage using the impact speed
        float damageDealt = healthManager.TakeDamageFromCollision(impactSpeed);
        
        if (damageDealt > 0f)
        {
            lastDamageTime = Time.time;
            
            if (healthManager.showDebug)
            {
                Debug.Log($"[TRIGGER DAMAGE] Helicopter hit {other.gameObject.name}: " +
                         $"Speed: {totalSpeed:F2}, Damage: {damageDealt:F1}");
            }
        }
        else if (healthManager.showDebug && totalSpeed > 0.1f)
        {
            Debug.Log($"[TRIGGER DAMAGE] No damage from {other.gameObject.name}: " +
                     $"Speed: {totalSpeed:F2} below threshold");
        }
    }
    
    /// <summary>
    /// Check if a GameObject can cause damage to the helicopter
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>True if it can cause damage</returns>
    private bool CanCauseDamage(GameObject obj)
    {
        // Check against damageable tags
        foreach (string tag in damageableTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        
        // Special case for water
        if (obj.CompareTag("Water") && waterCausesDamage)
        {
            return true;
        }
        
        return false;
    }
}
