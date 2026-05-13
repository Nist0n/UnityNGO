using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GameStateUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject waitingRoot;
    [SerializeField] private GameObject resultsRoot;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private TMP_Text matchTimerText;
    [SerializeField] private TMP_Text resultsText;

    private bool _bound;

    private void Start()
    {
        if (!gameManager) gameManager = GameManager.Instance;

        if (!gameManager) gameManager = FindFirstObjectByType<GameManager>();

        Bind();
        RefreshAll();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void Bind()
    {
        if (_bound || !gameManager) return;

        gameManager.CurrentState.OnChange += OnStateChanged;
        gameManager.ConnectedPlayers.OnChange += OnConnectedPlayersChanged;
        gameManager.MatchTimer.OnChange += OnMatchTimerChanged;
        _bound = true;
    }

    private void Unbind()
    {
        if (!_bound || !gameManager) return;

        gameManager.CurrentState.OnChange -= OnStateChanged;
        gameManager.ConnectedPlayers.OnChange -= OnConnectedPlayersChanged;
        gameManager.MatchTimer.OnChange -= OnMatchTimerChanged;
        _bound = false;
    }

    private void OnStateChanged(GameManager.GameState prev, GameManager.GameState next, bool asServer)
    {
        RefreshAll();
    }

    private void OnConnectedPlayersChanged(int prev, int next, bool asServer)
    {
        RefreshWaitingOrTimer();
    }

    private void OnMatchTimerChanged(float prev, float next, bool asServer)
    {
        RefreshWaitingOrTimer();
    }

    private void RefreshAll()
    {
        if (!gameManager) return;

        GameManager.GameState state = gameManager.CurrentState.Value;

        if (waitingRoot) waitingRoot.SetActive(state == GameManager.GameState.WaitingForPlayers);
        if (resultsRoot) resultsRoot.SetActive(state == GameManager.GameState.ShowingResults);

        RefreshWaitingOrTimer();

        if (state == GameManager.GameState.ShowingResults) RefreshResultsText();
    }

    private void RefreshWaitingOrTimer()
    {
        if (!gameManager)
            return;

        if (waitingText)
        {
            waitingText.text = $"Ожидание игроков: {gameManager.ConnectedPlayers.Value}/{gameManager.RequiredPlayersForUi}";
        }

        if (matchTimerText)
        {
            bool show = gameManager.CurrentState.Value == GameManager.GameState.InProgress;
            matchTimerText.gameObject.SetActive(show);
            if (show) matchTimerText.text = $"Матч: {Mathf.Max(0f, gameManager.MatchTimer.Value):F1} с";
        }
    }

    private void RefreshResultsText()
    {
        if (!resultsText) return;

        var list = new List<PlayerNetwork>(FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None));
        list.Sort((a, b) => b.Score.Value.CompareTo(a.Score.Value));

        var sb = new StringBuilder();
        sb.AppendLine("Результаты");
        foreach (PlayerNetwork pn in list) sb.AppendLine($"{pn.Nickname.Value}: {pn.Score.Value} попаданий");

        resultsText.text = sb.ToString();
    }
}
