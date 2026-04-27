using System.Collections;
using FishNet;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float respawnDelay = 10f;

    private bool _serverSpawnedPickups;

    private void Start()
    {
        if (!InstanceFinder.NetworkManager) return; InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.NetworkManager) InstanceFinder.NetworkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
    }

    private void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped) _serverSpawnedPickups = false;
        if (args.ConnectionState != LocalConnectionState.Started) return;
        if (_serverSpawnedPickups) return;
        _serverSpawnedPickups = true;
        SpawnAll();
    }

    private void SpawnAll()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
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
        if (!healthPickupPrefab) return;
        if (!InstanceFinder.ServerManager) return;
        if (!InstanceFinder.IsServerStarted) return;

        var go = Instantiate(healthPickupPrefab, position, Quaternion.identity);
        if (!go.TryGetComponent(out HealthPickup pickup))
        {
            Destroy(go); 
            return;
        }
        
        pickup.Init(this);
        
        if (!go.TryGetComponent(out NetworkObject networkObject))
        {
            Destroy(go); 
            return;
        }
        
        InstanceFinder.ServerManager.Spawn(networkObject);
    }
}
