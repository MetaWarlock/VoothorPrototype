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
    private HelicopterController helicopterController;

    private void Start()
    {
        helicopterController = GetComponentInParent<HelicopterController>();
        if (helicopterController == null)
        {
            Debug.LogError($"[PropellerRotation] HelicopterController not found on parent of {gameObject.name}!");
        }
    }

    private void Update()
    {
        if (helicopterController == null) return;

        // Get current input from helicopter controller
        Vector2 currentInput = helicopterController.GetCurrentInput();
        
        // Get target speed based on input
        float targetSpeed = 0f;
        
        if (propellerType == PropellerType.Top && Mathf.Abs(currentInput.y) > 0.01f)
        {
            targetSpeed = maxRotationSpeed * Mathf.Abs(currentInput.y);
        }
        else if (propellerType == PropellerType.Back && Mathf.Abs(currentInput.x) > 0.01f)
        {
            // Flip rotation direction based on movement direction for back propeller
            if (currentInput.x > 0 && !rotateClockwise || 
                currentInput.x < 0 && rotateClockwise)
            {
                rotateClockwise = !rotateClockwise;
            }
            targetSpeed = maxRotationSpeed * Mathf.Abs(currentInput.x);
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
