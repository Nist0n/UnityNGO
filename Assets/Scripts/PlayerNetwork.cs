using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> nickname;
    public NetworkVariable<int> hp = new(100);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            ConnectionUI.OnNameChanged += SubmitNicknameServerRpc;
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    public override void OnNetworkDespawn()
    {
        ConnectionUI.OnNameChanged -= SubmitNicknameServerRpc;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nick)
    {
        string safeValue;
        if (string.IsNullOrWhiteSpace(nick)) safeValue = $"Player_{OwnerClientId}";
        else safeValue = nick.Trim();
        nickname.Value = safeValue;
    }
}
