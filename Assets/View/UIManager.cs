using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// View da HUD: escuta os eventos do Singleton GameManager e atualiza os textos de
/// Pontuação, Vidas e Nome do Jogador. Também gerencia as telas de Game Over, Vitória e Pause.
/// </summary>
public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private GameObject livesContainer;
    [SerializeField] private Image[] lifeHeartImages;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject topBar;

    private bool isPaused = false;
    private int _lastScreenWidth;
    private int _lastScreenHeight;

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
        if (playerNameText == null) playerNameText = FindByName("TextoJogador") ?? FindByName("TextoNome");

        // Procura container de corações se ainda não atribuído
        if (livesContainer == null)
        {
            GameObject containerFound = GameObject.Find("LivesContainer");
            if (containerFound != null) livesContainer = containerFound;
        }

        if (livesContainer != null && (lifeHeartImages == null || lifeHeartImages.Length == 0))
        {
            lifeHeartImages = livesContainer.GetComponentsInChildren<Image>(true);
            // Se o próprio container tiver Image (não deveria, mas por segurança filtra apenas os filhos)
            var list = new System.Collections.Generic.List<Image>();
            foreach (var img in lifeHeartImages)
            {
                if (img.gameObject != livesContainer) list.Add(img);
            }
            if (list.Count > 0) lifeHeartImages = list.ToArray();
        }

        // Garante que os painéis comecem ocultos durante a gameplay, mesmo que você os deixe ativos no editor para posicionar
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        EnsureTopBarUI();
        EnsurePauseUI();
        AdjustTopWallToTopBar();

        // Criação dinâmica caso o texto do jogador não exista na HUD da fase atual
        if (playerNameText == null && (scoreText != null || livesText != null))
        {
            CreatePlayerNameTextRuntime();
        }

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        gm.OnScoreChanged      += UpdateScore;
        gm.OnLifeChanged       += UpdateLives;
        gm.OnPlayerNameChanged += UpdatePlayerName;
        gm.OnGameOver          += ShowGameOver;
        gm.OnVictory           += ShowVictory;

        UpdateScore(gm.Score);
        UpdateLives(gm.Lives);
        UpdatePlayerName(gm.PlayerName);

        StartCoroutine(LevelStartSequence());
    }

    private void OnDestroy()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;
        gm.OnScoreChanged      -= UpdateScore;
        gm.OnLifeChanged       -= UpdateLives;
        gm.OnPlayerNameChanged -= UpdatePlayerName;
        gm.OnGameOver          -= ShowGameOver;
        gm.OnVictory           -= ShowVictory;
    }

    private void CreatePlayerNameTextRuntime()
    {
        Transform parent = scoreText != null ? scoreText.transform.parent : livesText.transform.parent;
        GameObject playerObj = new GameObject("TextoJogador");
        playerObj.transform.SetParent(parent, false);

        playerNameText = playerObj.AddComponent<TextMeshProUGUI>();
        playerNameText.text = GameManager.Instance != null ? GameManager.Instance.PlayerName : "";
        playerNameText.fontSize = scoreText != null ? scoreText.fontSize : 24f;
        playerNameText.color = Color.white;
        playerNameText.alignment = TextAlignmentOptions.Top;

        RectTransform rt = playerNameText.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(350f, 50f);
    }

    private static TextMeshProUGUI FindByName(string objName)
    {
        foreach (TextMeshProUGUI t in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
            if (t.name == objName) return t;
        return null;
    }

#if UNITY_EDITOR
    /// <summary>Cria no editor um texto de vidas separado (clone do de pontos) para posicionar à mão.</summary>
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

    /// <summary>Cria no editor um texto do jogador centralizado no topo para posicionar à mão.</summary>
    [ContextMenu("Criar Texto do Jogador na Cena")]
    private void CreatePlayerNameTextInEditor()
    {
        if (playerNameText != null) return;
        Transform parent = transform.parent != null ? transform.parent : FindAnyObjectByType<Canvas>()?.transform;
        if (parent == null && scoreText != null) parent = scoreText.transform.parent;
        if (parent == null) return;

        GameObject obj = new GameObject("TextoJogador");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = "...";
        tmp.fontSize = 24f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Top;

        RectTransform rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(350f, 50f);

        playerNameText = tmp;
        UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "Criar TextoJogador");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString();
    }

    private void UpdateLives(int lives)
    {
        // Se corações neon estiverem disponíveis, controla a visibilidade de cada um
        if (lifeHeartImages != null && lifeHeartImages.Length > 0)
        {
            for (int i = 0; i < lifeHeartImages.Length; i++)
            {
                if (lifeHeartImages[i] != null)
                {
                    bool active = i < lives;
                    lifeHeartImages[i].gameObject.SetActive(active);
                }
            }
            if (livesText != null)
            {
                livesText.gameObject.SetActive(false);
            }
        }
        else if (livesText != null)
        {
            livesText.text = "Vidas: " + lives;
        }
    }

    private void UpdatePlayerName(string playerName)
    {
        if (playerNameText != null) playerNameText.text = playerName;
    }

    private void Update()
    {
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            AdjustTopWallToTopBar();
        }

        // Clique do mouse direto sobre o botão de pause (não depende do EventSystem/raycast da UI)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
            pauseButton != null && pauseButton.gameObject.activeInHierarchy && !isPaused)
        {
            RectTransform pauseRt = (RectTransform)pauseButton.transform;
            Canvas pauseCanvas = pauseButton.GetComponentInParent<Canvas>();
            Camera uiCam = (pauseCanvas != null && pauseCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? pauseCanvas.worldCamera : null;
            if (RectTransformUtility.RectangleContainsScreenPoint(pauseRt, Mouse.current.position.ReadValue(), uiCam))
            {
                TogglePause();
            }
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                if (buildIndex != 0) // Não pausa no menu principal
                {
                    TogglePause();
                }
            }
        }
    }

    private void ShowGameOver()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);

        if (gameOverPanel == null)
        {
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
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
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);

        if (victoryPanel == null)
        {
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
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

            GameManager gm = GameManager.Instance;
            string pName = gm != null ? gm.PlayerName : "Jogador";
            int finalScore = gm != null ? gm.Score : 0;

            int currentBuildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isLastStage = (currentBuildIndex >= UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings - 1)
                               || currentSceneName == "Fase3";

            // O título é mantido exatamente como foi configurado no Canvas (ex: "VITÓRIA!")

            // Atualiza ou cria Texto do Jogador na tela de vitória
            TextMeshProUGUI playerTxt = victoryPanel.transform.Find("Text_VictoryPlayer")?.GetComponent<TextMeshProUGUI>();
            if (playerTxt == null)
            {
                playerTxt = CreateVictoryText(victoryPanel.transform, "Text_VictoryPlayer", new Vector2(0, 90), 30);
            }
            if (playerTxt != null)
            {
                playerTxt.text = pName;
            }

            // Atualiza ou cria Texto da Pontuação na tela de vitória
            TextMeshProUGUI scoreTxt = victoryPanel.transform.Find("Text_VictoryScore")?.GetComponent<TextMeshProUGUI>();
            if (scoreTxt == null)
            {
                scoreTxt = CreateVictoryText(victoryPanel.transform, "Text_VictoryScore", new Vector2(0, 50), 32);
            }
            if (scoreTxt != null)
            {
                scoreTxt.text = finalScore.ToString();
            }

            // Se for a última fase (Fase 3), esconde o botão "Continuar"
            Transform continueBtn = victoryPanel.transform.Find("Btn_Continuar");
            if (continueBtn != null)
            {
                continueBtn.gameObject.SetActive(!isLastStage);
            }

            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // Pausa o jogo
        }
        else
        {
            Debug.LogWarning("[UIManager] Painel de Vitória não encontrado. Reiniciando via fallback.");
            GameManager.Instance.RestartMatch();
        }
    }

    private TextMeshProUGUI CreateVictoryText(Transform parent, string name, Vector2 pos, float fontSize)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = fontSize;
        NeonUIStyle.StyleTextByRole(tmp, NeonAssetProvider.Runtime); // cor neon pelo papel do texto
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(600, 40);
        return tmp;
    }

    public void Btn_NextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            // Requisito: cada avanço de fase deve ganhar uma vida, a não ser que já tenha 3
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddLife(1);
            }
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
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if (buildIndex == 0 && currentSceneName == "MainMenu") yield break; // Não faz isso no menu

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) yield break;

        // Pausa o jogo
        Time.timeScale = 0f;

        // Criar o texto "Fase X"
        GameObject textObj = new GameObject("LevelStartText");
        textObj.transform.SetParent(canvas.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

        bool isFase3 = buildIndex == 3 || currentSceneName == "Fase3";
        int displayPhase = buildIndex > 0 ? buildIndex : (isFase3 ? 3 : (currentSceneName == "Fase2" ? 2 : 1));
        tmp.text = "FASE " + displayPhase;
        tmp.fontSize = isFase3 ? 70 : 80;
        tmp.alignment = TextAlignmentOptions.Center;
        NeonUIStyle.StyleTextByRole(tmp, NeonAssetProvider.Runtime); // ciano neon pulsante
        RectTransform rt = textObj.GetComponent<RectTransform>();

        if (isFase3)
        {
            // Posiciona exatamente no vão livre entre as duas porções de tijolos (Y mundial = 2.25)
            Camera cam = Camera.main;
            if (cam != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                cam.WorldToScreenPoint(new Vector3(0, 2.25f, 0)),
                null,
                out Vector2 localPoint))
            {
                rt.anchoredPosition = new Vector2(0, localPoint.y);
            }
            else
            {
                rt.anchoredPosition = new Vector2(0, 75f);
            }
        }
        else
        {
            rt.anchoredPosition = Vector2.zero;
        }

        rt.sizeDelta = new Vector2(600, 200);

        // Espera 2 segundos reais (já que timeScale = 0)
        yield return new WaitForSecondsRealtime(2f);

        // Destrói o texto e despausa (caso não esteja pausado pelo jogador)
        Destroy(textObj);
        if (!isPaused)
        {
            Time.timeScale = 1f;
        }
    }

    private int _lastToggleFrame = -1;

    public void TogglePause()
    {
        if (_lastToggleFrame == Time.frameCount) return; // evita toggle duplo (clique + onClick)
        _lastToggleFrame = Time.frameCount;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        // Não pausa se já estiver no Game Over ou na tela de Vitória
        if ((gameOverPanel != null && gameOverPanel.activeSelf) ||
            (victoryPanel != null && victoryPanel.activeSelf))
        {
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
        Time.timeScale = 1f; // Retorna no exato ponto onde foi paralisado
    }

    public void Btn_ResumeGame()
    {
        ResumeGame();
    }

    public void Btn_QuitGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Volta ao Menu Principal
    }

    public void Btn_RestartGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Despausa
        GameManager.Instance.RestartMatch();
    }

    public void Btn_LoadMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f; // Despausa
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Index 0 é o Menu
    }

    private void EnsurePauseUI()
    {
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if (buildIndex == 0) return; // Não faz nada no menu principal

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. Procura ou cria o botão de Pause
        if (pauseButton == null)
        {
            Transform pBtnTrans = (topBar != null ? topBar.transform.Find("Btn_Pause") : null) 
                                  ?? canvas.transform.Find("Btn_Pause");
            if (pBtnTrans != null)
            {
                pauseButton = pBtnTrans.GetComponent<Button>();
            }
            else
            {
                pauseButton = CreatePauseButton(topBar != null ? topBar.transform : canvas.transform);
            }
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
            pauseButton.onClick.AddListener(TogglePause);
            pauseButton.gameObject.SetActive(true);
        }

        // 2. Procura ou cria o painel de Pause
        if (pausePanel == null)
        {
            Transform pPanelTrans = canvas.transform.Find("PausePanel");
            if (pPanelTrans != null)
            {
                pausePanel = pPanelTrans.gameObject;
            }
            else
            {
                pausePanel = CreatePausePanel(canvas.transform);
            }
        }

        if (pausePanel != null)
        {
            Button btnCont = pausePanel.transform.Find("Btn_Continuar")?.GetComponent<Button>();
            if (btnCont != null)
            {
                btnCont.onClick.RemoveListener(Btn_ResumeGame);
                btnCont.onClick.AddListener(Btn_ResumeGame);
            }

            Button btnRec = pausePanel.transform.Find("Btn_Recomecar")?.GetComponent<Button>()
                            ?? pausePanel.transform.Find("Btn_Reiniciar")?.GetComponent<Button>();
            if (btnRec != null)
            {
                btnRec.onClick.RemoveListener(Btn_RestartGame);
                btnRec.onClick.AddListener(Btn_RestartGame);
            }

            Button btnSair = pausePanel.transform.Find("Btn_Sair")?.GetComponent<Button>()
                             ?? pausePanel.transform.Find("Btn_Menu")?.GetComponent<Button>();
            if (btnSair != null)
            {
                btnSair.onClick.RemoveListener(Btn_QuitGame);
                btnSair.onClick.AddListener(Btn_QuitGame);
            }

            pausePanel.SetActive(false);
        }
    }

    private Button CreatePauseButton(Transform parent)
    {
        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = "Btn_Pause";
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-12f, 0f);
        rt.sizeDelta = new Vector2(34f, 24f);

        Text defaultText = btnObj.GetComponentInChildren<Text>();
        if (defaultText != null) Destroy(defaultText.gameObject);

        GameObject textObj = new GameObject("Text (TMP)");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "PAUSE";
        tmp.fontSize = 13f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button pauseBtn = btnObj.GetComponent<Button>();
        NeonUIStyle.StyleButton(pauseBtn, NeonUIStyle.Cyan, NeonAssetProvider.Runtime);
        return pauseBtn;
    }

    private void EnsureTopBarUI()
    {
        int buildIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if (buildIndex == 0) return; // Não cria no menu

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        bool createdTopBar = false;

        // Procura ou cria TopBar
        if (topBar == null)
        {
            Transform topBarTrans = canvas.transform.Find("TopBar");
            if (topBarTrans != null)
            {
                topBar = topBarTrans.gameObject;
            }
            else
            {
                topBar = new GameObject("TopBar");
                topBar.transform.SetParent(canvas.transform, false);
                topBar.transform.SetAsFirstSibling();

                Image barImg = topBar.AddComponent<Image>();
                barImg.color = new Color(0.04f, 0.035f, 0.1f, 0.95f);

                RectTransform barRt = topBar.GetComponent<RectTransform>();
                barRt.anchorMin = new Vector2(0f, 1f);
                barRt.anchorMax = new Vector2(1f, 1f);
                barRt.pivot = new Vector2(0.5f, 1f);
                barRt.anchoredPosition = Vector2.zero;
                barRt.sizeDelta = new Vector2(0f, 50f);

                // Linha de borda inferior (separador com cor neon ciano)
                GameObject borderLine = new GameObject("BorderLine");
                borderLine.transform.SetParent(topBar.transform, false);
                Image lineImg = borderLine.AddComponent<Image>();
                lineImg.color = new Color(0f, 0.9f, 1f, 1f);

                RectTransform lineRt = borderLine.GetComponent<RectTransform>();
                lineRt.anchorMin = new Vector2(0f, 0f);
                lineRt.anchorMax = new Vector2(1f, 0f);
                lineRt.pivot = new Vector2(0.5f, 0f);
                lineRt.anchoredPosition = Vector2.zero;
                lineRt.sizeDelta = new Vector2(0f, 3f);

                createdTopBar = true;
            }
        }

        // Se a TopBar foi recém-criada em runtime e os elementos estavam soltos fora dela,
        // agrupamos dentro da barra. Mas se o usuário já posicionou ou salvou na cena, preserva!
        if (createdTopBar)
        {
            if (scoreText != null && scoreText.transform.parent != topBar.transform)
            {
                scoreText.transform.SetParent(topBar.transform, false);
                RectTransform sRt = scoreText.rectTransform;
                sRt.anchorMin = new Vector2(0f, 0.5f);
                sRt.anchorMax = new Vector2(0f, 0.5f);
                sRt.pivot = new Vector2(0f, 0.5f);
                sRt.anchoredPosition = new Vector2(15f, 0f);
                sRt.sizeDelta = new Vector2(120f, 40f);
                scoreText.alignment = TextAlignmentOptions.MidlineLeft;
                scoreText.fontSize = 20f;
                scoreText.color = new Color(0f, 0.95f, 1f);
            }

            if (playerNameText != null && playerNameText.transform.parent != topBar.transform)
            {
                playerNameText.transform.SetParent(topBar.transform, false);
                RectTransform pRt = playerNameText.rectTransform;
                pRt.anchorMin = new Vector2(0.5f, 0.5f);
                pRt.anchorMax = new Vector2(0.5f, 0.5f);
                pRt.pivot = new Vector2(0.5f, 0.5f);
                pRt.anchoredPosition = new Vector2(0f, 0f);
                pRt.sizeDelta = new Vector2(240f, 40f);
                playerNameText.alignment = TextAlignmentOptions.Center;
                playerNameText.fontSize = 22f;
                playerNameText.color = Color.white;
            }

            if (livesText != null && livesText.transform.parent != topBar.transform)
            {
                livesText.transform.SetParent(topBar.transform, false);
                RectTransform lRt = livesText.rectTransform;
                lRt.anchorMin = new Vector2(1f, 0.5f);
                lRt.anchorMax = new Vector2(1f, 0.5f);
                lRt.pivot = new Vector2(1f, 0.5f);
                lRt.anchoredPosition = new Vector2(-80f, 0f);
                lRt.sizeDelta = new Vector2(90f, 40f);
                livesText.alignment = TextAlignmentOptions.MidlineRight;
                livesText.fontSize = 18f;
                livesText.color = new Color(1f, 0.2f, 0.5f);
            }

            if (pauseButton != null && pauseButton.transform.parent != topBar.transform)
            {
                pauseButton.transform.SetParent(topBar.transform, false);
                RectTransform bRt = pauseButton.GetComponent<RectTransform>();
                bRt.anchorMin = new Vector2(1f, 0.5f);
                bRt.anchorMax = new Vector2(1f, 0.5f);
                bRt.pivot = new Vector2(1f, 0.5f);
                bRt.anchoredPosition = new Vector2(-12f, 0f);
                bRt.sizeDelta = new Vector2(34f, 24f);
            }
        }

        // Garante a existência do LivesContainer com os corações de neon na TopBar
        EnsureLivesHeartsUI();
    }

    private void EnsureLivesHeartsUI()
    {
        if (topBar == null) return;

        Vector2 livesPos = new Vector2(-58f, 0f);
        if (livesText != null)
        {
            livesPos = livesText.rectTransform.anchoredPosition;
        }

        if (livesContainer == null)
        {
            Transform existing = topBar.transform.Find("LivesContainer");
            if (existing != null)
            {
                livesContainer = existing.gameObject;
            }
            else
            {
                livesContainer = new GameObject("LivesContainer");
                livesContainer.transform.SetParent(topBar.transform, false);

                RectTransform lcRt = livesContainer.AddComponent<RectTransform>();
                lcRt.anchorMin = new Vector2(1f, 0.5f);
                lcRt.anchorMax = new Vector2(1f, 0.5f);
                lcRt.pivot = new Vector2(1f, 0.5f);
                lcRt.anchoredPosition = livesPos;
                lcRt.sizeDelta = new Vector2(58f, 20f);

                HorizontalLayoutGroup hlg = livesContainer.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleRight;
                hlg.spacing = 3f;
                hlg.childControlWidth = false;
                hlg.childControlHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
            }
        }
        else
        {
            RectTransform lcRt = livesContainer.GetComponent<RectTransform>();
            if (lcRt != null)
            {
                // Se estiver com posição antiga (-80), atualiza para -58 (ao lado do pause)
                if (Mathf.Approximately(lcRt.anchoredPosition.x, -80f))
                    lcRt.anchoredPosition = new Vector2(-58f, 0f);
                lcRt.sizeDelta = new Vector2(58f, 20f);
            }
        }

        // Garante os 3 corações dentro do LivesContainer
        Sprite heartSprite = NeonAssetProvider.Runtime.HeartSprite;
        var images = new System.Collections.Generic.List<Image>();

        for (int i = 0; i < 3; i++)
        {
            string heartName = "Heart_" + (i + 1);
            Transform hTrans = livesContainer.transform.Find(heartName);
            Image img;
            if (hTrans != null)
            {
                img = hTrans.GetComponent<Image>();
                RectTransform hRt = img.rectTransform;
                if (hRt != null) hRt.sizeDelta = new Vector2(16f, 16f);
            }
            else
            {
                GameObject hObj = new GameObject(heartName);
                hObj.transform.SetParent(livesContainer.transform, false);
                RectTransform hRt = hObj.AddComponent<RectTransform>();
                hRt.sizeDelta = new Vector2(16f, 16f);
                img = hObj.AddComponent<Image>();
            }

            if (img != null)
            {
                if (heartSprite != null) img.sprite = heartSprite;
                img.color = NeonUIStyle.Pink;
                img.raycastTarget = false;
                images.Add(img);
            }
        }

        lifeHeartImages = images.ToArray();

        // Oculta o texto numérico antigo se o container com corações foi criado
        if (livesText != null)
        {
            livesText.gameObject.SetActive(false);
        }

        if (GameManager.Instance != null)
        {
            UpdateLives(GameManager.Instance.Lives);
        }
    }

    public void AdjustTopWallToTopBar()
    {
        if (topBar == null) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        RectTransform topBarRt = topBar.GetComponent<RectTransform>();
        if (topBarRt == null) return;

        Vector3[] corners = new Vector3[4];
        topBarRt.GetWorldCorners(corners);
        // Em ScreenSpaceOverlay, corners[0].y é a coordenada em pixels da face inferior da barra
        float screenBottomY = corners[0].y;

        // Converte pixel Y da tela para coordenadas de mundo da câmera
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, screenBottomY, -cam.transform.position.z));
        float borderWorldY = worldPoint.y;

        // Procura a TopWall (filha de Borders ou solta na cena)
        Transform topWall = GameObject.Find("TopWall")?.transform 
                            ?? GameObject.Find("Borders")?.transform.Find("TopWall");
        if (topWall != null)
        {
            BoxCollider2D col = topWall.GetComponent<BoxCollider2D>();
            float thickness = col != null ? col.size.y : 1f;

            // O centro da parede deve ser ajustado para que sua borda inferior fique exatamente na linha inferior da barra
            Vector3 pos = topWall.position;
            pos.y = borderWorldY + thickness * 0.5f;
            topWall.position = pos;
        }
    }

    private GameObject CreatePausePanel(Transform parent)
    {
        GameObject panelObj = new GameObject("PausePanel");
        panelObj.transform.SetParent(parent, false);
        Image bg = panelObj.AddComponent<Image>();
        bg.color = new Color(NeonUIStyle.DeepSpace.r, NeonUIStyle.DeepSpace.g, NeonUIStyle.DeepSpace.b, 0.82f);

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        // Título "PAUSE"
        GameObject titleObj = new GameObject("Text_PauseTitle");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "PAUSE";
        titleTmp.fontSize = 54f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        NeonUIStyle.StyleTextByRole(titleTmp, NeonAssetProvider.Runtime);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0f, 130f);
        titleRt.sizeDelta = new Vector2(600f, 70f);

        // Botão CONTINUAR
        CreatePanelButton(panelObj.transform, "Btn_Continuar", "CONTINUAR", new Vector2(0f, 30f), Btn_ResumeGame);

        // Botão REINICIAR
        CreatePanelButton(panelObj.transform, "Btn_Reiniciar", "REINICIAR", new Vector2(0f, -40f), Btn_RestartGame);

        // Botão SAIR
        CreatePanelButton(panelObj.transform, "Btn_Sair", "SAIR", new Vector2(0f, -110f), Btn_QuitGame);

        return panelObj;
    }

    private GameObject CreatePanelButton(Transform parent, string name, string text, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = name;
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(280f, 52f);

        Text defaultText = btnObj.GetComponentInChildren<Text>();
        if (defaultText != null) Destroy(defaultText.gameObject);

        GameObject textObj = new GameObject("Text (TMP)");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        Button b = btnObj.GetComponent<Button>();
        if (action != null) b.onClick.AddListener(action);
        NeonUIStyle.StyleButton(b, NeonUIStyle.ButtonColorFor(name), NeonAssetProvider.Runtime);
        return btnObj;
    }
}
