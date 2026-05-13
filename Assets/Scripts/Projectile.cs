using FishNet.Connection;
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

        int prevHp = target.Hp.Value;
        if (prevHp <= 0) return;

        int newHp = Mathf.Max(0, prevHp - damage);
        target.Hp.Value = newHp;

        if (TryGetAttackerPlayer(out PlayerNetwork attacker) && attacker != target)
            attacker.Score.Value++;

        Despawn(DespawnType.Destroy);
    }

    private bool TryGetAttackerPlayer(out PlayerNetwork attacker)
    {
        attacker = null;
        NetworkConnection owner = Owner;
        if (!owner.IsValid) return false;

        foreach (NetworkObject nob in owner.Objects)
        {
            if (nob.TryGetComponent(out PlayerNetwork pn))
            {
                attacker = pn;
                return true;
            }
        }

        return false;
    }
}
