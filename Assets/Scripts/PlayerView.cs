using System.Collections;
using FishNet.Object;
using TMPro;
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

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (!playerNetwork) return;

        playerNetwork.Nickname.OnChange += OnNicknameChanged;
        playerNetwork.Hp.OnChange += OnHpChanged;

        OnNicknameChanged(string.Empty, playerNetwork.Nickname.Value, false);
        OnHpChanged(0, playerNetwork.Hp.Value, false);

        if (!Owner.IsLocalClient) return;

        if (canvasObject) canvasObject.SetActive(true);

        playerNetwork.IsAlive.OnChange += OnIsAliveChanged;
        OnIsAliveChanged(true, playerNetwork.IsAlive.Value, false);

        if (!playerShooting) return;
        playerShooting.CurrentAmmo.OnChange += OnBulletsCountChanged;
        int ammoDisplay = playerShooting.CurrentAmmo.Value > 0 ? playerShooting.CurrentAmmo.Value : playerShooting.MaxAmmo;
        OnBulletsCountChanged(0, ammoDisplay, false);
    }

    public override void OnStopNetwork()
    {
        if (playerNetwork)
        {
            playerNetwork.Nickname.OnChange -= OnNicknameChanged;
            playerNetwork.Hp.OnChange -= OnHpChanged;
            playerNetwork.IsAlive.OnChange -= OnIsAliveChanged;
        }

        if (playerShooting)
            playerShooting.CurrentAmmo.OnChange -= OnBulletsCountChanged;

        base.OnStopNetwork();
    }

    private void OnNicknameChanged(string oldValue, string newValue, bool asServer)
    {
        if (nicknameText) nicknameText.text = newValue;
    }

    private void OnHpChanged(int oldValue, int newValue, bool asServer)
    {
        if (hpText) hpText.text = $"HP: {newValue}";
    }

    private void OnBulletsCountChanged(int oldValue, int newValue, bool asServer)
    {
        if (bulletsCount) bulletsCount.text = $"Bullets: {newValue}";
    }

    private void OnIsAliveChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (newValue)
        {
            if (_respawnTimerCoroutine != null)
            {
                StopCoroutine(_respawnTimerCoroutine);
                if (respawnTimerText) respawnTimerText.text = "";
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
