using UnityEngine;

public class FallKillZone : MonoBehaviour
{
    public float belowDistance = 6f;
    Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (transform.position.y < cam.position.y - belowDistance)
        {
            if (GameManager.I)
                GameManager.I.GameOver("fell below camera");
            else
            {
                Debug.Log("Game Over: fell below camera");
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
