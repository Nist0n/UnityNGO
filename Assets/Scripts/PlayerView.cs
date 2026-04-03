using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork playerNetwork;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text bulletsCount;
    [SerializeField] private TMP_Text respawnTimerText;
    [SerializeField] private GameObject canvasObject;

    private Coroutine _respawnTimerCoroutine;
    
    public override void OnNetworkSpawn()
    {
        if (!playerNetwork) return;
        playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;
        playerNetwork.Hp.OnValueChanged += OnHpChanged;
        
        OnNicknameChanged(default, playerNetwork.Nickname.Value);
        OnHpChanged(0, playerNetwork.Hp.Value);
        
        if (!IsLocalPlayer) return;
        
        if (canvasObject)
        {
            canvasObject.SetActive(IsLocalPlayer);
        }
        
        playerNetwork.IsAlive.OnValueChanged += OnIsAliveChanged;
        OnIsAliveChanged(true, playerNetwork.IsAlive.Value);
        
        if (!playerShooting) return;
        playerShooting.CurrentAmmo.OnValueChanged += OnBulletsCountChanged;
        
        OnBulletsCountChanged(0, playerShooting.MaxAmmo);
    }

    public override void OnNetworkDespawn()
    {
        if (!playerNetwork) return;
        playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
        playerNetwork.Hp.OnValueChanged -= OnHpChanged;
        playerNetwork.IsAlive.OnValueChanged -= OnIsAliveChanged;
        if (!playerShooting) return;
        playerShooting.CurrentAmmo.OnValueChanged -= OnBulletsCountChanged;
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        nicknameText.text = newValue.ToString();
    }

    private void OnHpChanged(int oldValue, int newValue)
    {
        hpText.text = $"HP: {newValue}";
    }
    
    private void OnBulletsCountChanged(int oldValue, int newValue)
    {
        bulletsCount.text = $"Bullets: {newValue}";
    }
    
    private void OnIsAliveChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            if (_respawnTimerCoroutine != null)
            {
                StopCoroutine(_respawnTimerCoroutine);
                respawnTimerText.text = "";
                _respawnTimerCoroutine = null;
            }
        }
        else
        {
            if (_respawnTimerCoroutine != null) StopCoroutine(_respawnTimerCoroutine);
                
            _respawnTimerCoroutine = StartCoroutine(RespawnTimerCoroutine());
        }
    }
    
    private IEnumerator RespawnTimerCoroutine()
    {
        float remainingTime = 3f;
        
        while (remainingTime > 0)
        {
            if (respawnTimerText) respawnTimerText.text = $"Respawning in {remainingTime:F1}...";
                
            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }
        
        if (respawnTimerText) respawnTimerText.text = "";
    }
}
