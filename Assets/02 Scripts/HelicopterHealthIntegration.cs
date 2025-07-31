using UnityEngine;

/// <summary>
/// Integrates helicopter collision detection with the health system.
/// This script should be attached to the helicopter along with HealthManager.
/// </summary>
[RequireComponent(typeof(HealthManager))]
public class HelicopterHealthIntegration : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Tags that cause damage when collided with")]
    public string[] damageableTags = { "Ground", "Wall", "Enemy", "Obstacle" };
    
    [Tooltip("Should collision with water cause damage?")]
    public bool waterCausesDamage = false;
    
    [Tooltip("Minimum time between damage events")]
    public float damageFrequency = 0.5f;
    
    [Header("Visual Feedback")]
    [Tooltip("Should the helicopter flash when taking damage?")]
    public bool flashOnDamage = true;
    
    [Tooltip("Flash color when taking damage")]
    public Color damageFlashColor = Color.red;
    
    [Tooltip("Flash duration")]
    public float flashDuration = 0.2f;
    
    [Tooltip("Should screen shake on damage?")]
    public bool screenShakeOnDamage = false;
    
    [Header("Audio")]
    [Tooltip("Sound to play when taking damage")]
    public AudioClip damageSound;
    
    [Tooltip("Sound to play when dying")]
    public AudioClip deathSound;
    
    // Components
    private HealthManager healthManager;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    
    // State
    private float lastDamageTime = 0f;
    private Color originalColor;
    private bool isFlashing = false;
    private float flashTimer = 0f;
    
    void Awake()
    {
        // Get required components
        healthManager = GetComponent<HealthManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        
        // Store original color
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    void Start()
    {
        // Subscribe to health events
        if (healthManager != null)
        {
            healthManager.onDamageTaken.AddListener(OnDamageTaken);
            healthManager.onDeath.AddListener(OnDeath);
            healthManager.onHealthRestored.AddListener(OnHealthRestored);
        }
    }
    
    void Update()
    {
        // Handle damage flash effect
        if (isFlashing && spriteRenderer != null)
        {
            flashTimer += Time.deltaTime;
            
            if (flashTimer >= flashDuration)
            {
                // End flash
                isFlashing = false;
                spriteRenderer.color = originalColor;
            }
            else
            {
                // Lerp between damage color and original
                float t = flashTimer / flashDuration;
                spriteRenderer.color = Color.Lerp(damageFlashColor, originalColor, t);
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle landing logic only - damage is now handled by HelicopterDamageTrigger
        HandleLanding(collision);
    }
    
    private void HandleLanding(Collision2D collision)
    {
        // Check if landing on any solid surface
        if (IsLandingSurface(collision.gameObject))
        {
            // Check if landing on top of the surface (normal pointing upwards)
            if (collision.contacts[0].normal.y > 0.7f)
            {
                // Reset horizontal velocity for a stable landing
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
    }
    
    /// <summary>
    /// Check if this is a surface the helicopter can land on
    /// </summary>
    private bool IsLandingSurface(GameObject obj)
    {
        // Check against damageable tags (which are also landing surfaces)
        foreach (string tag in damageableTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }
    
    // OnTriggerEnter2D removed - all damage handling is now done by HelicopterDamageTrigger.cs
    // This prevents console errors from undefined tags like "Projectile" and "EnemyAttack"
    
    // Old damage calculation code has been moved to HelicopterDamageTrigger.cs
    
    /// <summary>
    /// Handle damage taken event
    /// </summary>
    private void OnDamageTaken()
    {
        // Visual feedback
        if (flashOnDamage && spriteRenderer != null)
        {
            StartDamageFlash();
        }
        
        // Audio feedback
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Screen shake (if camera controller exists)
        if (screenShakeOnDamage)
        {
            // This would require a camera controller script
            // CameraController.Instance?.Shake(0.1f, 0.2f);
        }
    }
    
    /// <summary>
    /// Handle death event
    /// </summary>
    private void OnDeath()
    {
        // Audio feedback
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Disable helicopter control
        var flyingSprite = GetComponent<FlyingSprite>();
        if (flyingSprite != null)
        {
            flyingSprite.enabled = false;
        }
        
        // Stop all propeller rotation
        var propellers = GetComponentsInChildren<PropellerRotation>();
        foreach (var propeller in propellers)
        {
            propeller.enabled = false;
        }
        
        // You can add more death effects here:
        // - Particle effects
        // - Screen effects
        // - Game over logic
        
        if (healthManager.showDebug)
        {
            Debug.Log("Helicopter has been destroyed!");
        }
    }
    
    /// <summary>
    /// Handle health restored event
    /// </summary>
    private void OnHealthRestored()
    {
        // Can add positive visual/audio feedback here
        // For example: healing particle effect, sound
    }
    
    /// <summary>
    /// Start damage flash effect
    /// </summary>
    private void StartDamageFlash()
    {
        if (!isFlashing)
        {
            isFlashing = true;
            flashTimer = 0f;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = damageFlashColor;
            }
        }
    }
    
    /// <summary>
    /// Manually trigger damage (for testing or special cases)
    /// </summary>
    /// <param name="damage">Damage amount</param>
    public void TriggerDamage(float damage)
    {
        if (healthManager != null)
        {
            healthManager.TakeDamage(damage);
        }
    }
    
    /// <summary>
    /// Manually heal the helicopter
    /// </summary>
    /// <param name="amount">Heal amount</param>
    public void TriggerHeal(float amount)
    {
        if (healthManager != null)
        {
            healthManager.Heal(amount);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (healthManager != null)
        {
            healthManager.onDamageTaken.RemoveListener(OnDamageTaken);
            healthManager.onDeath.RemoveListener(OnDeath);
            healthManager.onHealthRestored.RemoveListener(OnHealthRestored);
        }
    }
}
