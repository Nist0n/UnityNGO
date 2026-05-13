using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float cooldown = 0.4f;

    private float _lastShotTime;
    private PlayerNetwork _playerNetwork;

    public int MaxAmmo = 10;
    public readonly SyncVar<int> CurrentAmmo = new(0, new());

    public override void OnStartServer()
    {
        base.OnStartServer();
        _playerNetwork = GetComponent<PlayerNetwork>();
        CurrentAmmo.Value = MaxAmmo;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!GameManager.IsMatchInProgress) return;
        if (Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame) ShootServerRpc(firePoint.position, firePoint.forward);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir)
    {
        if (!GameManager.IsMatchInProgress) return;
        if (!_playerNetwork) _playerNetwork = GetComponent<PlayerNetwork>();
        
        if (!_playerNetwork.IsAlive.Value) return;
        if (CurrentAmmo.Value <= 0) return;
        if (Time.time < _lastShotTime + cooldown) return;

        _lastShotTime = Time.time;
        CurrentAmmo.Value--;

        if (!projectilePrefab) return;
        var go = Instantiate(projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        if (!go.TryGetComponent<NetworkObject>(out var networkObject)) return;
        ServerManager.Spawn(networkObject, Owner);
    }
}
