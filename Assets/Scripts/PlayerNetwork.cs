using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    
    public NetworkVariable<FixedString32Bytes> Nickname;
    public NetworkVariable<int> Hp = new(100);
    public NetworkVariable<bool> IsAlive = new(true);
    
    private GameObject[] _spawnPoints;

    private void Start()
    {
        _spawnPoints = GameObject.FindGameObjectsWithTag("Spawner");
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            ConnectionUI.OnNameChanged += SubmitNicknameServerRpc;
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
        Hp.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;
    }

    public override void OnNetworkDespawn()
    {
        ConnectionUI.OnNameChanged -= SubmitNicknameServerRpc;
        Hp.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nick)
    {
        string safeValue;
        if (string.IsNullOrWhiteSpace(nick)) safeValue = $"Player_{OwnerClientId}";
        else safeValue = nick.Trim();
        Nickname.Value = safeValue;
    }
    
    private void OnHpChanged(int prev, int next)
    {
        if (!IsServer) return;
        
        if (next <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);
        
        int idx = Random.Range(0, _spawnPoints.Length);
        Vector3 spawnPos = _spawnPoints[idx].transform.position + new Vector3(0, 3, 0);
        
        transform.position = spawnPos;
    
        Hp.Value = 100;
        IsAlive.Value = true;
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        meshRenderer.enabled = next;
    }
}
