using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float cooldown = 0.4f;

    private float _lastShotTime;
    private PlayerNetwork _playerNetwork;
    
    public int MaxAmmo = 10;
    public NetworkVariable<int> CurrentAmmo;

    public override void OnNetworkSpawn()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
        if (!IsServer) return;
        CurrentAmmo.Value = MaxAmmo;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Mouse.current.leftButton.wasPressedThisFrame) ShootServerRpc(firePoint.position, firePoint.forward);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir, ServerRpcParams rpc = default)
    {
        if (!_playerNetwork.IsAlive.Value) return;
        
        if (CurrentAmmo.Value <= 0) return;
        
        if (Time.time < _lastShotTime + cooldown) return;

        _lastShotTime = Time.time;
        CurrentAmmo.Value--;

        var go = Instantiate(projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        
        var no = go.GetComponent<NetworkObject>();
        no.SpawnWithOwnership(rpc.Receive.SenderClientId);
    }
}
