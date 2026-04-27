using System;
using FishNet;
using FishNet.Managing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private TMP_InputField nicknameInputField;

    public static event Action<string> OnNameChanged;

    public static string PlayerNickname { get; private set; } = "Player";

    private void Start()
    {
        if (!networkManager) networkManager = InstanceFinder.NetworkManager;

        startHostButton.onClick.AddListener(StartHost);
        startClientButton.onClick.AddListener(StartClient);
        nicknameInputField.onValueChanged.AddListener(ChangeName);
    }

    private void StartClient()
    {
        SaveNickname();
        if (networkManager) networkManager.ClientManager.StartConnection();
        DeactivateButtons();
    }

    private void StartHost()
    {
        SaveNickname();
        if (!networkManager) return;
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection();
        DeactivateButtons();
    }

    private void DeactivateButtons()
    {
        startHostButton.interactable = false;
        startClientButton.interactable = false;
    }

    private void SaveNickname()
    {
        nicknameInputField.DeactivateInputField(true);

        string rawValue = nicknameInputField.text != null ? nicknameInputField.text : string.Empty;
        if (string.IsNullOrWhiteSpace(rawValue)) PlayerNickname = "Player";
        else PlayerNickname = rawValue.Trim();
    }

    private void ChangeName(string playerName)
    {
        PlayerNickname = string.IsNullOrEmpty(playerName) ? "Player" : playerName.Trim();
        OnNameChanged?.Invoke(playerName);
    }
}
