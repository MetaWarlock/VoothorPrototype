using UnityEngine;

public class Passenger : MonoBehaviour
{
    public float speed = 2f;
    public float boardingSpeed = 1f;
    public float boardingRadius = 3f;
    private Vector3 destination;

    private bool atExitPoint = false;
    private bool isBoarding = false;
    private Transform helicopter;
    public string PlayerTag = "Player";

    void Update()
    {
        if (isBoarding)
        {
            MoveTowardsHelicopter();
        }
        else if (!atExitPoint)
        {
            MoveTowardsDestination();
        }
        else
        {
            CheckForHelicopter();
        }
    }

    public void SetDestination(Vector3 exitPoint)
    {
        destination = exitPoint;
    }

    private void MoveTowardsDestination()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, destination) < 0.1f)
        {
            atExitPoint = true;
            
        }
    }

    private void CheckForHelicopter()
    {
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, boardingRadius);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, boardingRadius);

        if (hitRight.collider != null && hitRight.collider.CompareTag(PlayerTag))
        {
            Helicopter helicopterScript = hitRight.collider.GetComponent<Helicopter>();
            if (helicopterScript != null && helicopterScript.IsOnPlatform())
            {
                isBoarding = true;
                helicopter = hitRight.collider.transform;
            }
        }
        else if (hitLeft.collider != null && hitLeft.collider.CompareTag(PlayerTag))
        {
            Helicopter helicopterScript = hitLeft.collider.GetComponent<Helicopter>();
            if (helicopterScript != null && helicopterScript.IsOnPlatform())
            {
                isBoarding = true;
                helicopter = hitLeft.collider.transform;
            }
        }
    }

    private void MoveTowardsHelicopter()
    {
        if (helicopter != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, helicopter.position, boardingSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, helicopter.position) < 0.1f)
            {
                // Удалите пассажира или добавьте логику посадки
                // The passenger has boarded. We can add scoring logic here later.
                Destroy(gameObject);
            }
        }
    }
}
