using UnityEngine;

public class PropellerRotation : MonoBehaviour
{
    public enum PropellerType { Top, Back }

    [Tooltip("Type of the propeller")]
    public PropellerType propellerType = PropellerType.Top;

    [Tooltip("Maximum rotation speed in degrees per second")]
    public float maxRotationSpeed = 720f;
    
    [Tooltip("Smoothing factor for rotation speed changes")]
    public float rotationSmoothing = 5f;

    [Tooltip("If true, the propeller will rotate clockwise. If false, counter-clockwise")]
    public bool rotateClockwise = true;

    private float currentRotationSpeed = 0f;
    private FlyingSprite flyingSprite;

    private void Start()
    {
        flyingSprite = GetComponentInParent<FlyingSprite>();
    }

    private void Update()
    {
        if (flyingSprite == null) return;

        // Get target speed based on input
        float targetSpeed = 0f;
        
        if (propellerType == PropellerType.Top && flyingSprite.VerticalInput != 0)
        {
            targetSpeed = maxRotationSpeed * Mathf.Abs(flyingSprite.VerticalInput);
        }
        else if (propellerType == PropellerType.Back && flyingSprite.HorizontalInput != 0)
        {
            // Flip rotation direction based on movement direction for back propeller
            if (flyingSprite.HorizontalInput > 0 && !rotateClockwise || 
                flyingSprite.HorizontalInput < 0 && rotateClockwise)
            {
                rotateClockwise = !rotateClockwise;
            }
            targetSpeed = maxRotationSpeed * Mathf.Abs(flyingSprite.HorizontalInput);
        }

        // Smoothly adjust current rotation speed
        currentRotationSpeed = Mathf.Lerp(
            currentRotationSpeed, 
            targetSpeed, 
            rotationSmoothing * Time.deltaTime
        );

        // Apply rotation if speed is above threshold
        if (currentRotationSpeed > 1f)
        {
            float rotationAmount = currentRotationSpeed * Time.deltaTime;
            if (!rotateClockwise) rotationAmount = -rotationAmount;

            switch (propellerType)
            {
                case PropellerType.Top:
                    transform.Rotate(0f, rotationAmount, 0f);
                    break;
                case PropellerType.Back:
                    transform.Rotate(0f, 0f, rotationAmount);
                    break;
            }
        }
    }
}
