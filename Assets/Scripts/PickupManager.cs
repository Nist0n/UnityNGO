using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float respawnDelay = 10f;

    private void Start()
    {
        if (NetworkManager.Singleton)
        {
            Debug.Log("NetworkManager.Singleton is called");
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }

    private void OnServerStarted()
    {
        Debug.Log("Starting server");
        SpawnAll();
    }

    private void SpawnAll()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return;
        }

        foreach (var point in spawnPoints)
        {
            if (point) SpawnPickup(point.position);
        }
    }

    public void OnPickedUp(Vector3 position)
    {
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnPickup(position);
    }

    private void SpawnPickup(Vector3 position)
    {
        if (!healthPickupPrefab)
        {
            return;
        }

        var go = Instantiate(healthPickupPrefab, position, Quaternion.identity);
        var pickup = go.GetComponent<HealthPickup>();
        
        if (!pickup)
        {
            Destroy(go);
            return;
        }
        
        pickup.Init(this);
        
        var networkObject = go.GetComponent<NetworkObject>();
        if (!networkObject)
        {
            Destroy(go);
            return;
        }
        
        networkObject.Spawn();
    }
}