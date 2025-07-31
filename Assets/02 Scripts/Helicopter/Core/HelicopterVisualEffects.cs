using UnityEngine;

/// <summary>
/// Handles visual effects for the helicopter, including body rotation and sprite flipping.
/// This replaces the visual logic that was previously in FlyingSprite.cs
/// </summary>
public class HelicopterVisualEffects : MonoBehaviour
{
    [Header("Body Rotation")]
    [Tooltip("Maximum angle to rotate the helicopter body when moving horizontally")]
    [Range(0f, 45f)]
    public float maxBodyTiltAngle = 15f;
    
    [Tooltip("Speed of body rotation animation")]
    [Range(1f, 20f)]
    public float bodyRotationSpeed = 8f;
    
    [Header("Sprite Flipping")]
    [Tooltip("Enable horizontal sprite flipping when changing direction")]
    public bool enableSpriteFlipping = true;
    
    [Tooltip("Input threshold for sprite flipping")]
    [Range(0.01f, 0.5f)]
    public float flipThreshold = 0.1f;
    
    // Components
    private HelicopterController helicopterController;
    private Transform bodyTransform;
    private Vector3 originalScale;
    
    // Current rotation state
    private float currentBodyAngle = 0f;
    
    void Awake()
    {
        helicopterController = GetComponent<HelicopterController>();
        bodyTransform = transform; // Use main transform for body rotation
        originalScale = transform.localScale;
    }
    
    void Start()
    {
        if (helicopterController == null)
        {
            Debug.LogError($"[HelicopterVisualEffects] HelicopterController not found on {gameObject.name}!");
            enabled = false;
        }
    }
    
    void Update()
    {
        if (helicopterController == null) return;
        
        // Get current input
        Vector2 currentInput = helicopterController.GetCurrentInput();
        
        // Update body rotation
        UpdateBodyRotation(currentInput);
        
        // Update sprite flipping
        if (enableSpriteFlipping)
        {
            UpdateSpriteFlipping(currentInput);
        }
    }
    
    /// <summary>
    /// Update helicopter body rotation based on horizontal input
    /// </summary>
    private void UpdateBodyRotation(Vector2 input)
    {
        // Calculate target angle based on horizontal input
        float targetAngle = -input.x * maxBodyTiltAngle; // Negative for realistic tilt direction
        
        // Smoothly interpolate to target angle
        currentBodyAngle = Mathf.LerpAngle(currentBodyAngle, targetAngle, bodyRotationSpeed * Time.deltaTime);
        
        // Apply rotation (rotate around Z-axis for 2D tilt)
        bodyTransform.rotation = Quaternion.Euler(0f, 0f, currentBodyAngle);
    }
    
    /// <summary>
    /// Update sprite flipping based on horizontal movement direction
    /// </summary>
    private void UpdateSpriteFlipping(Vector2 input)
    {
        // Flip sprite based on horizontal input direction
        if (input.x > flipThreshold && transform.localScale.x < 0)
        {
            // Moving right - use original scale (facing right)
            transform.localScale = new Vector3(originalScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (input.x < -flipThreshold && transform.localScale.x > 0)
        {
            // Moving left - flip scale (facing left)
            transform.localScale = new Vector3(-originalScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    
    /// <summary>
    /// Reset all visual effects to default state
    /// </summary>
    public void ResetVisualEffects()
    {
        currentBodyAngle = 0f;
        bodyTransform.rotation = Quaternion.identity;
        transform.localScale = originalScale;
    }
    
    /// <summary>
    /// Set body tilt manually (for external systems)
    /// </summary>
    public void SetBodyTilt(float angle)
    {
        currentBodyAngle = Mathf.Clamp(angle, -maxBodyTiltAngle, maxBodyTiltAngle);
        bodyTransform.rotation = Quaternion.Euler(0f, 0f, currentBodyAngle);
    }
}
