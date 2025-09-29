using UnityEngine;

public class ClampedFollowCam : MonoBehaviour
{
    public Transform target;   
    
    public float yOffset = 2f;           
    public float deadZoneHalfHeight = 1.25f;

    public float smoothUp = 6f;          
    public float smoothDown = 10f;       

    public bool lockToHighest = false;

    float camY;                          
    float highestY;                      

    void Start()
    {
        if (!target) target = GameObject.FindGameObjectWithTag("Player")?.transform;
        camY = transform.position.y;
        highestY = camY;
    }

    void LateUpdate()
    {
        if (!target) return;
        float targetY = target.position.y + yOffset;

        if (lockToHighest)
        {
            if (targetY > highestY)
                highestY = Mathf.Lerp(highestY, targetY, 1f - Mathf.Exp(-smoothUp * Time.deltaTime));
            camY = Mathf.Max(camY, highestY);
        }
        else
        {
            float topBound = camY + deadZoneHalfHeight;
            float bottomBound = camY - deadZoneHalfHeight;

            if (targetY > topBound)
            {
                float goal = targetY - deadZoneHalfHeight;
                camY = Mathf.Lerp(camY, goal, 1f - Mathf.Exp(-smoothUp * Time.deltaTime));
            }
            else if (targetY < bottomBound)
            {
                float goal = targetY + deadZoneHalfHeight;
                camY = Mathf.Lerp(camY, goal, 1f - Mathf.Exp(-smoothDown * Time.deltaTime));
            }
        }

        var p = transform.position;
        p.y = camY;
        transform.position = p;
    }
}