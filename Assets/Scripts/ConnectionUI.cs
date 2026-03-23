using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private TMP_InputField nicknameInputField;
    
    public static event Action<string> OnNameChanged;
    
    public static string PlayerNickname { get; private set; } = "Player";
    
    private void Start()
    {
        startHostButton.onClick.AddListener(StartHost);
        startClientButton.onClick.AddListener(StartClient);
        nicknameInputField.onValueChanged.AddListener(ChangeName);
    }

    private void StartClient()
    {
        SaveNickname();
        NetworkManager.Singleton.StartClient();
        DeactivateButtons();
    }

    private void StartHost()
    {
        SaveNickname();
        NetworkManager.Singleton.StartHost();
        DeactivateButtons();
    }

    private void DeactivateButtons()
    {
        startHostButton.interactable = false;
        startClientButton.interactable = false;
    }
    
    private void SaveNickname()
    {
        string rawValue;
        if (nicknameInputField.text != null) rawValue = nicknameInputField.text;
        else rawValue = string.Empty;
        
        if (string.IsNullOrWhiteSpace(rawValue)) PlayerNickname = "Player";
        else PlayerNickname = rawValue.Trim();
    }

    private void ChangeName(string playerName)
    {
        PlayerNickname = playerName.Trim();
        OnNameChanged?.Invoke(playerName);
    }
}
