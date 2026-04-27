using FishNet.Object;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private int damage = 20;

    private void Update()
    {
        transform.Translate(Vector3.forward * (speed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;
        if (!other.TryGetComponent(out PlayerNetwork target)) return;
        if (target.OwnerId == OwnerId) return;

        int newHp = Mathf.Max(0, target.Hp.Value - damage);
        target.Hp.Value = newHp;
        Despawn(DespawnType.Destroy);
    }
}
