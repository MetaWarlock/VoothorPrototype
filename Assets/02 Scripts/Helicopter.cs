using UnityEngine;

public class Helicopter : MonoBehaviour
{
    public bool isOnPlatform = false;

    void Update()
    {
        // пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ пїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅпїЅ
        if (Mathf.Approximately(GetComponent<Rigidbody2D>().linearVelocity.y, 0))
        {
            isOnPlatform = true;
        }
        else
        {
            isOnPlatform = false;
        }
    }

    public bool IsOnPlatform()
    {
        return isOnPlatform;
    }
}
