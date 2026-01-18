using UnityEngine;
using HDRP_FPS3D.Enemy;

public class EnemyScoreLink : MonoBehaviour
{
    private EnemyHealth _health;
    private ScoreManager _scoreManager;
    private bool _hasRegisteredDeath = false;

    void Start()
    {
        _health = GetComponent<EnemyHealth>();
        _scoreManager = Object.FindFirstObjectByType<ScoreManager>();
    }

    void Update()
    {
        if (_health != null && _health.IsDead && !_hasRegisteredDeath)
        {
            if (_scoreManager != null)
            {
                _scoreManager.RegisterKill();
            }
            _hasRegisteredDeath = true;
            this.enabled = false;
        }
    }
}