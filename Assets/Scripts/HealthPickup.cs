using FishNet.Object;
using UnityEngine;

public class HealthPickup : NetworkBehaviour
{
    [SerializeField] private int healAmount = 40;

    private PickupManager _manager;
    private Vector3 _spawnPosition;

    public void Init(PickupManager manager)
    {
        _manager = manager;
        _spawnPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;
        if (!other.TryGetComponent(out PlayerNetwork player)) return;
        if (!player.IsAlive.Value) return;
        if (player.Hp.Value >= 100) return;

        player.Hp.Value = Mathf.Min(100, player.Hp.Value + healAmount);

        _manager?.OnPickedUp(_spawnPosition);
        Despawn(DespawnType.Destroy);
    }
}
