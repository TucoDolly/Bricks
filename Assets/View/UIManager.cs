using UnityEngine;
using TMPro;

/// <summary>
/// View da HUD: apenas escuta os eventos do GameManager e atualiza os textos.
/// Se não houver texto de vidas atribuído, cria um clonando o texto de pontos.
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<UIManager>() != null) return;
        // Só se auto-cria se existir a UI do jogo na cena
        if (FindByName("TextoPontos") == null) return;
        new GameObject("UIManager").AddComponent<UIManager>();
    }

    private void Start()
    {
        // Fallback por nome, para não roubar textos do Menu Principal
        if (scoreText == null) scoreText = FindByName("TextoPontos");
        if (livesText == null) livesText = FindByName("TextoVidas");

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        gm.OnScoreChanged += UpdateScore;
        gm.OnLifeChanged  += UpdateLives;
        gm.OnGameOver     += ShowGameOver;
        gm.OnVictory      += ShowVictory;
        UpdateScore(gm.Score);
        UpdateLives(gm.Lives);

        StartCoroutine(LevelStartSequence());
    }

    private void OnDestroy()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;
        gm.OnScoreChanged -= UpdateScore;
        gm.OnLifeChanged  -= UpdateLives;
        gm.OnGameOver     -= ShowGameOver;
        gm.OnVictory      -= ShowVictory;
    }

    private static TextMeshProUGUI FindByName(string objName)
    {
        foreach (TextMeshProUGUI t in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
            if (t.name == objName) return t;
        return null;
    }

#if UNITY_EDITOR
    /// <summary>Cria no editor um texto de vidas separado (clone do de pontos) para você posicionar à mão.</summary>
    [ContextMenu("Criar Texto de Vidas na Cena")]
    private void CreateLivesTextInEditor()
    {
        if (scoreText == null) scoreText = FindByName("TextoPontos") ?? FindAnyObjectByType<TextMeshProUGUI>();
        if (scoreText == null || livesText != null) return;

        TextMeshProUGUI clone = Instantiate(scoreText, scoreText.transform.parent);
        clone.name = "TextoVidas";
        clone.text = "Vidas: 3";
        clone.rectTransform.anchoredPosition = scoreText.rectTransform.anchoredPosition + new Vector2(0f, -scoreText.fontSize * 1.3f);
        livesText = clone;
        UnityEditor.Undo.RegisterCreatedObjectUndo(clone.gameObject, "Criar TextoVidas");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "Pontuação: " + score;
    }

    private void UpdateLives(int lives)
    {
        if (livesText != null) livesText.text = "Vidas: " + lives;
    }

    private void ShowGameOver()
    {
        // Fallback blindado: se a variável perdeu a referência, procura o objeto desativado pelo nome
        if (gameOverPanel == null)
        {
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name == "GameOverPanel")
                {
                    gameOverPanel = t.gameObject;
                    break;
                }
            }
        }

        if (gameOverPanel != null)
        {
            Debug.Log("[UIManager] Painel de Game Over encontrado e ativado!");
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
        }
        else
        {
            Debug.LogWarning("[UIManager] Painel não foi encontrado nem pelo nome. Reiniciando via fallback.");
            GameManager.Instance.RestartMatch();
        }
    }

    private void ShowVictory()
    {
        if (victoryPanel == null)
        {
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name == "VictoryPanel")
                {
                    victoryPanel = t.gameObject;
                    break;
                }
            }
        }

        if (victoryPanel != null)
        {
            Debug.Log("[UIManager] Painel de Vitória encontrado e ativado!");
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
        }
        else
        {
            Debug.LogWarning("[UIManager] Painel de Vitória não encontrado. Reiniciando via fallback.");
            GameManager.Instance.RestartMatch();
        }
    }

    public void Btn_NextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextIndex);
        }
        else
        {
            // Se não houver mais fases, volta para o menu
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private System.Collections.IEnumerator LevelStartSequence()
    {
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if (buildIndex == 0) yield break; // Não faz isso no menu

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) yield break;

        // Pausa o jogo
        Time.timeScale = 0f;

        // Criar o texto "Fase X"
        GameObject textObj = new GameObject("LevelStartText");
        textObj.transform.SetParent(canvas.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "FASE " + buildIndex;
        tmp.fontSize = 80;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(600, 200);

        // Espera 2 segundos reais (já que timeScale = 0)
        yield return new WaitForSecondsRealtime(2f);

        // Destrói o texto e despausa
        Destroy(textObj);
        Time.timeScale = 1f;
    }

    public void Btn_RestartGame()
    {
        Time.timeScale = 1f; // Despausa
        GameManager.Instance.RestartMatch();
    }

    public void Btn_LoadMainMenu()
    {
        Time.timeScale = 1f; // Despausa
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Index 0 é o Menu
    }
}
