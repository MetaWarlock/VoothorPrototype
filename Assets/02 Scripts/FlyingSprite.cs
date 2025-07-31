using UnityEngine;
using UnityEngine.InputSystem;

public class FlyingSprite : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float acceleration = 1f;
    public float lateralAcceleration = 5f;

    // Public properties to access input values
    public float VerticalInput { get; private set; }
    public float HorizontalInput { get; private set; }

    private Vector2 move;
    private Rigidbody2D rb;
    private Vector2 lateralVelocity;
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    void FixedUpdate()
    {
        HandleLateralMovement();
        HandleVerticalMovement();
        
        // Update input values for propellers
        HorizontalInput = move.x;
        VerticalInput = move.y;
        
        // Flip the entire player object based on horizontal movement
        if (move.x > 0.1f && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(originalScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (move.x < -0.1f && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-originalScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    private void HandleVerticalMovement()
    {
        if (move.y > 0.01)
        {
            // ����������� ������������ �������� �� ��������� � ������ ���������
            if (rb.linearVelocity.y < maxSpeed)
            {
                rb.linearVelocity += Vector2.up * acceleration * Time.deltaTime;
            }
        }
        else if (move.y < -0.01)
        {
            // �������� �������
            rb.linearVelocity += Vector2.down * acceleration * Time.deltaTime;
        }
    }

    private void HandleLateralMovement()
    {
        if (move.x > 0.01)
        {
            lateralVelocity += Vector2.right * lateralAcceleration * Time.deltaTime;
        }
        else if (move.x < -0.01)
        {
            lateralVelocity += Vector2.left * lateralAcceleration * Time.deltaTime;
        }

        lateralVelocity = Vector2.ClampMagnitude(lateralVelocity, maxSpeed);
        rb.linearVelocity = new Vector2(lateralVelocity.x, rb.linearVelocity.y);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.x > 0.5f)  // ������������ � ����� �������
            {
                if (lateralVelocity.x < 0) lateralVelocity.x = 0;
            }
        }
    }

    // OnCollisionEnter2D() has been removed.
    // Landing and collision logic is now handled in HelicopterHealthIntegration.cs
    // to ensure correct execution order with damage calculation.

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

}