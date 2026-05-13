using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(NetworkObject))]
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public static bool IsMatchInProgress =>
        !Instance || Instance.CurrentState.Value == GameState.InProgress;

    [SerializeField] private int requiredPlayers = 2;
    [SerializeField] private float matchDurationSeconds = 60f;
    [SerializeField] private float resultsScreenSeconds = 5f;

    public int RequiredPlayersForUi => requiredPlayers;

    public readonly SyncVar<GameState> CurrentState =
        new(GameState.WaitingForPlayers, new SyncTypeSettings(0.5f));

    public readonly SyncVar<int> ConnectedPlayers = new(0, new SyncTypeSettings(0.25f));

    public readonly SyncVar<float> MatchTimer = new(60f, new SyncTypeSettings(0.25f));

    private Coroutine _resetLobbyCoroutine;
    private float _recountCooldown;

    public enum GameState
    {
        WaitingForPlayers,
        InProgress,
        ShowingResults
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Instance = this;
    }

    public override void OnStopNetwork()
    {
        if (Instance == this) Instance = null;

        base.OnStopNetwork();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        ServerManager.OnAuthenticationResult += OnAuthenticationResult;
        NetworkManager.ClientManager.OnClientConnectionState += OnLocalClientConnectionState;

        RecountConnectedPlayers();
        TryStartMatchIfReady();
    }

    public override void OnStopServer()
    {
        ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        ServerManager.OnAuthenticationResult -= OnAuthenticationResult;
        NetworkManager.ClientManager.OnClientConnectionState -= OnLocalClientConnectionState;

        if (_resetLobbyCoroutine != null)
        {
            StopCoroutine(_resetLobbyCoroutine);
            _resetLobbyCoroutine = null;
        }

        base.OnStopServer();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (!IsServerInitialized) return;

        RecountConnectedPlayers();
        TryStartMatchIfReady();
    }

    private void OnLocalClientConnectionState(ClientConnectionStateArgs args)
    {
        if (!IsServerInitialized) return;

        RecountConnectedPlayers();
        TryStartMatchIfReady();
    }

    private void OnAuthenticationResult(NetworkConnection conn, bool authenticated)
    {
        if (!IsServerInitialized) return;

        RecountConnectedPlayers();
        TryStartMatchIfReady();
    }

    private void RecountConnectedPlayers()
    {
        int count = ServerManager.Clients.Count;
        if (NetworkManager.IsHostStarted) count++;

        ConnectedPlayers.Value = count;
    }

    private void TryStartMatchIfReady()
    {
        if (CurrentState.Value != GameState.WaitingForPlayers) return;

        if (ConnectedPlayers.Value < requiredPlayers) return;

        StartMatch();
    }

    private void StartMatch()
    {
        MatchTimer.Value = matchDurationSeconds;
        CurrentState.Value = GameState.InProgress;
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        if (CurrentState.Value == GameState.WaitingForPlayers)
        {
            _recountCooldown -= Time.unscaledDeltaTime;
            if (_recountCooldown <= 0f)
            {
                _recountCooldown = 0.25f;
                RecountConnectedPlayers();
            }
        }

        if (CurrentState.Value != GameState.InProgress) return;

        MatchTimer.Value -= Time.deltaTime;

        if (MatchTimer.Value <= 0f) EndMatch();
    }

    private void EndMatch()
    {
        if (CurrentState.Value != GameState.InProgress) return;

        CurrentState.Value = GameState.ShowingResults;

        if (_resetLobbyCoroutine != null) StopCoroutine(_resetLobbyCoroutine);

        _resetLobbyCoroutine = StartCoroutine(ResetToLobbyAfterDelay());
    }

    private IEnumerator ResetToLobbyAfterDelay()
    {
        yield return new WaitForSeconds(resultsScreenSeconds);
        _resetLobbyCoroutine = null;
        ResetToLobby();
    }

    private void ResetToLobby()
    {
        foreach (NetworkConnection conn in ServerManager.Clients.Values) ResetPlayersForConnection(conn);

        if (NetworkManager.IsHostStarted && NetworkManager.ClientManager.Connection.IsValid) ResetPlayersForConnection(NetworkManager.ClientManager.Connection);

        MatchTimer.Value = matchDurationSeconds;
        CurrentState.Value = GameState.WaitingForPlayers;

        TryStartMatchIfReady();
    }

    private static void ResetPlayersForConnection(NetworkConnection conn)
    {
        foreach (NetworkObject nob in conn.Objects)
        {
            if (!nob.TryGetComponent(out PlayerNetwork pn)) continue;

            pn.Hp.Value = 100;
            pn.IsAlive.Value = true;
            pn.Score.Value = 0;
            pn.RespawnPlayer();

            if (nob.TryGetComponent(out PlayerShooting shooting)) shooting.CurrentAmmo.Value = shooting.MaxAmmo;
        }
    }
}
