using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork playerNetwork;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Camera playerCamera;

    private void Start()
    {
        if (Owner.IsLocalClient && !playerCamera) playerCamera = Camera.main;
        if (playerLayer == 0) playerLayer = LayerMask.GetMask("Player");
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!GameManager.IsMatchInProgress) return;
        if (Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame) TryAttackByClick();
    }

    private void TryAttackByClick()
    {
        if (!playerCamera) return;
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, playerLayer))
        {
            if (hit.collider.TryGetComponent(out PlayerNetwork target) && target != playerNetwork) TryAttack(target);
        }
    }

    private void TryAttack(PlayerNetwork target)
    {
        if (!IsOwner || !target) return;
        if (target == playerNetwork) return;
        DealDamageServerRpc(target.ObjectId, damage);
    }

    [ServerRpc]
    private void DealDamageServerRpc(int targetObjectId, int damage)
    {
        if (!ServerManager) return;
        if (!GameManager.IsMatchInProgress) return;
        if (!playerNetwork) playerNetwork = GetComponent<PlayerNetwork>();
        
        if (!ServerManager.Objects.Spawned.TryGetValue(targetObjectId, out NetworkObject targetObject)) return;
        
        if (!targetObject.TryGetComponent(out PlayerNetwork targetPlayer) || targetPlayer == playerNetwork) return;

        int prevHp = targetPlayer.Hp.Value;
        if (prevHp <= 0) return;

        int nextHp = Mathf.Max(0, prevHp - damage);
        targetPlayer.Hp.Value = nextHp;
        playerNetwork.Score.Value++;
    }
}
