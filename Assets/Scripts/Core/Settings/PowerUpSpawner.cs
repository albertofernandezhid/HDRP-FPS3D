using UnityEngine;
using System.Collections;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Configuración de Respawn")]
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private float respawnDelay = 30f;

    private bool isRespawning = false;

    void Start()
    {
        Spawn();
    }

    void Update()
    {
        if (transform.childCount == 0 && !isRespawning)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);
        Spawn();
        isRespawning = false;
    }

    private void Spawn()
    {
        GameObject newInstance = Instantiate(powerUpPrefab, transform.position, transform.rotation);
        newInstance.transform.SetParent(this.transform);
    }
}