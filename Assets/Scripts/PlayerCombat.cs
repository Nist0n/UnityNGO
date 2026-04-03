using System;
using Unity.Netcode;
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
        if (IsOwner && !playerCamera)
        {
            playerCamera = Camera.main;
        }
        
        if (playerLayer == 0)
        {
            playerLayer = LayerMask.GetMask("Player");
        }
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryAttackByClick();
        }
    }
    
    private void TryAttackByClick()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, playerLayer))
        {
            PlayerNetwork target = hit.collider.GetComponent<PlayerNetwork>();
            
            if (target && target != playerNetwork)
            {
                TryAttack(target);
            }
        }
    }
    
    private void TryAttack(PlayerNetwork target)
    {
        if (!IsOwner || !target)
            return;
        
        if (target == playerNetwork)
        {
            return;
        }
        
        DealDamageServerRpc(target.NetworkObjectId, damage);
    }
    
    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId, int damage)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
            return;
            
        PlayerNetwork targetPlayer = targetObject.GetComponent<PlayerNetwork>();
        if (!targetPlayer || targetPlayer == playerNetwork)
            return;
        
        int nextHp = Mathf.Max(0, targetPlayer.Hp.Value - damage);
        targetPlayer.Hp.Value = nextHp;
    }
}
