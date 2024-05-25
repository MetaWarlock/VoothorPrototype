using UnityEngine;
using UnityEngine.InputSystem;

public class FlyingSprite : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float acceleration = 1f;
    public float lateralAcceleration = 5f;

    private Vector2 move;

    private Rigidbody2D rb;
    private Vector2 lateralVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        HandleLateralMovement();
        HandleVerticalMovement();
    }

    private void HandleVerticalMovement()
    {
        if (move.y > 0.01)
        {
            // ”величиваем вертикальную скорость до максимума с учетом ускорени€
            if (rb.velocity.y < maxSpeed)
            {
                rb.velocity += Vector2.up * acceleration * Time.deltaTime;
            }
        }
        else if (move.y < -0.01)
        {
            // ”скор€ем падение
            rb.velocity += Vector2.down * acceleration * Time.deltaTime;
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
        rb.velocity = new Vector2(lateralVelocity.x, rb.velocity.y);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.x > 0.5f)  // —толкновение с левой стороны
            {
                if (lateralVelocity.x < 0) lateralVelocity.x = 0;
            }
            else if (contact.normal.x < -0.5f)  // —толкновение с правой стороны
            {
                if (lateralVelocity.x > 0) lateralVelocity.x = 0;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ѕри приземлении обнул€ем боковую скорость
        if (collision.gameObject.CompareTag("Ground"))
        {
            lateralVelocity = Vector2.zero;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

}