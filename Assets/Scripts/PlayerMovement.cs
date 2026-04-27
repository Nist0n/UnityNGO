using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using FishNet.Utility.Template;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public struct MoveData : IReplicateData
{
    public float Horizontal;
    public float Vertical;

    private uint _tick;

    public void Dispose() { }

    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

public struct ReconcileData : IReconcileData
{
    public Vector3 Position;

    private uint _tick;

    public void Dispose() { }

    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

public class PlayerMovement : TickNetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool useClientSidePrediction = true;

    private PlayerNetwork _playerNetwork;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _playerNetwork = GetComponent<PlayerNetwork>();

        if (useClientSidePrediction)
            SetTickCallbacks(TickCallback.Tick | TickCallback.PostTick);
        else
            SetTickCallbacks(TickCallback.None);
    }

    private void Update()
    {
        if (useClientSidePrediction)
            return;
        if (!IsOwner || !IsSpawned)
            return;
        if (!_playerNetwork || !_playerNetwork.IsAlive.Value)
            return;

        var multiplier = speed * Time.deltaTime;
        if (Keyboard.current == null) return;
        if (Keyboard.current.aKey.isPressed) transform.position += new Vector3(-multiplier, 0, 0);
        if (Keyboard.current.dKey.isPressed) transform.position += new Vector3(multiplier, 0, 0);
        if (Keyboard.current.wKey.isPressed) transform.position += new Vector3(0, 0, multiplier);
        if (Keyboard.current.sKey.isPressed) transform.position += new Vector3(0, 0, -multiplier);
    }

    protected override void TimeManager_OnTick()
    {
        if (!useClientSidePrediction) return;
        PerformReplicate(BuildMoveData());
    }

    protected override void TimeManager_OnPostTick()
    {
        if (!useClientSidePrediction) return;
        CreateReconcile();
    }

    public override void CreateReconcile()
    {
        if (!useClientSidePrediction) return;
        ReconcileData rd = new() { Position = transform.position };
        PerformReconcile(rd);
    }

    private MoveData BuildMoveData()
    {
        if (!IsOwner) return default;
        if (!_playerNetwork || !_playerNetwork.IsAlive.Value) return default;

        if (Keyboard.current == null) return default;

        float h = 0f, v = 0f;
        if (Keyboard.current.aKey.isPressed) h -= 1f;
        if (Keyboard.current.dKey.isPressed) h += 1f;
        if (Keyboard.current.wKey.isPressed) v += 1f;
        if (Keyboard.current.sKey.isPressed) v -= 1f;

        return new MoveData { Horizontal = h, Vertical = v };
    }

    [Replicate]
    private void PerformReplicate(
        MoveData md,
        ReplicateState state = ReplicateState.Invalid,
        Channel channel = Channel.Unreliable)
    {
        if (!_playerNetwork || !_playerNetwork.IsAlive.Value)
            return;

        float delta = (float)TimeManager.TickDelta;
        Vector3 move = new Vector3(md.Horizontal, 0f, md.Vertical);
        if (move.sqrMagnitude > 1f) move.Normalize();
        transform.position += move * speed * delta;
    }

    [Reconcile]
    private void PerformReconcile(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
    }
}
