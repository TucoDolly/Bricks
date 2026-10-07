using System;
using UnityEngine;

/// <summary>
/// Singleton persistente (DontDestroyOnLoad) com os dados globais da sessão:
/// Pontuação, Vidas e Nome do Jogador. Não conhece nenhuma View.
/// </summary>
public class GameManager : MonoBehaviour
{
    public const int InitialLives = 3;

    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Lives { get; private set; } = InitialLives;
    public string PlayerName { get; private set; } = "Jogador";

    public event Action<int> OnScoreChanged;
    public event Action<int> OnLifeChanged;
    public event Action OnGameOver;
    public event Action OnVictory;

    /// <summary>Garante que a instância exista mesmo se não houver GameManager na cena.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void LoseLife()
    {
        if (Lives <= 0) return;
        Lives--;
        OnLifeChanged?.Invoke(Lives);
        if (Lives <= 0)
        {
            OnGameOver?.Invoke();
        }
    }

    public void SetPlayerName(string playerName)
    {
        PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Jogador" : playerName;
    }

    public void WinMatch()
    {
        OnVictory?.Invoke();
    }

    /// <summary>Reinicia a sessão (novo jogo). Mantém o nome do jogador.</summary>
    public void ResetSession()
    {
        Score = 0;
        Lives = InitialLives;
        OnScoreChanged?.Invoke(Score);
        OnLifeChanged?.Invoke(Lives);
    }

    /// <summary>Reseta a pontuação e vidas, e recarrega a cena atual.</summary>
    public void RestartMatch()
    {
        ResetSession();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
