using UnityEngine;

public class ProjectileScore : MonoBehaviour
{
    private ScoreManager scoreManager;

    void Start()
    {
        scoreManager = Object.FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.RegisterShot();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (scoreManager != null && collision.gameObject.CompareTag("Enemy"))
        {
            scoreManager.RegisterHit();
        }
        Destroy(gameObject);
    }
}