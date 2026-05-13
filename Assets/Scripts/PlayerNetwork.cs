using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private PlayerShooting playerShooting;
    

    public readonly SyncVar<string> Nickname = new("Player", new());
    public readonly SyncVar<int> Hp = new(100, new());
    public readonly SyncVar<bool> IsAlive = new(true, new());
    public readonly SyncVar<int> Score = new(0, new());

    private GameObject[] _spawnPoints;

    private void Awake()
    {
        Hp.OnChange += Hp_OnChange;
        IsAlive.OnChange += IsAlive_OnChange;
    }

    private void Start()
    {
        _spawnPoints = GameObject.FindGameObjectsWithTag("Spawner");
    }
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!Owner.IsLocalClient) return;
        
        ConnectionUI.OnNameChanged -= SubmitNicknameServerRpc;
        ConnectionUI.OnNameChanged += SubmitNicknameServerRpc;
        SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
    }

    public override void OnStopClient()
    {
        ConnectionUI.OnNameChanged -= SubmitNicknameServerRpc;
        base.OnStopClient();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nick)
    {
        string safeValue = string.IsNullOrWhiteSpace(nick)
            ? $"Player_{OwnerId}"
            : nick.Trim();
        Nickname.Value = safeValue;
    }

    private void Hp_OnChange(int prev, int next, bool asServer)
    {
        if (!asServer)
            return;

        if (next <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    public void RespawnPlayer() => StartCoroutine(RespawnRoutine());

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            Hp.Value = 100;
            IsAlive.Value = true;
            yield break;
        }

        int idx = Random.Range(0, _spawnPoints.Length);
        Vector3 spawnPos = _spawnPoints[idx].transform.position + new Vector3(0, 3, 0);

        transform.position = spawnPos;

        Hp.Value = 100;
        playerShooting.CurrentAmmo.Value = playerShooting.MaxAmmo;
        IsAlive.Value = true;
    }

    private void IsAlive_OnChange(bool prev, bool next, bool asServer)
    {
        if (meshRenderer) meshRenderer.enabled = next;
    }
}
