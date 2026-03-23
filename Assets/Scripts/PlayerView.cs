using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork playerNetwork;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text hpText;

    public override void OnNetworkSpawn()
    {
        playerNetwork.nickname.OnValueChanged += OnNicknameChanged;
        playerNetwork.hp.OnValueChanged += OnHpChanged;
        
        OnNicknameChanged(default, playerNetwork.nickname.Value);
        OnHpChanged(0, playerNetwork.hp.Value);
    }

    public override void OnNetworkDespawn()
    {
        playerNetwork.nickname.OnValueChanged -= OnNicknameChanged;
        playerNetwork.hp.OnValueChanged -= OnHpChanged;
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        nicknameText.text = newValue.ToString();
    }

    private void OnHpChanged(int oldValue, int newValue)
    {
        hpText.text = $"HP: {newValue}";
    }
}
