using UnityEngine;

public class Helicopter : MonoBehaviour
{
    public bool isOnPlatform = false;

    void Update()
    {
        // �������� �� �������� ���������
        if (Mathf.Approximately(GetComponent<Rigidbody2D>().velocity.y, 0))
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
