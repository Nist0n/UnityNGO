using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 18f;
    [SerializeField] private int damage = 20;

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var target = other.GetComponent<PlayerNetwork>();
        if (!target) return;
        
        if (target.OwnerClientId == OwnerClientId) return;

        int newHp = Mathf.Max(0, target.Hp.Value - damage);
        target.Hp.Value = newHp;

        NetworkObject.Despawn(destroy: true);
    }
}
