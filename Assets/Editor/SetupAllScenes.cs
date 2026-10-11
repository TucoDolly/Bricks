using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public static class SetupAllScenes
{
    [MenuItem("Ferramentas/Atualizar Todas as Cenas (Nome do Jogador)")]
    public static void UpdateAllScenes()
    {
        UpdateMainMenu();
        UpdateGameplayScene("Assets/Scenes/SampleScene.unity", false);
        UpdateGameplayScene("Assets/Scenes/Fase2.unity", false);
        UpdateGameplayScene("Assets/Scenes/Fase3.unity", true);

        BricksSceneBuilder.AutoConfigureBuildSettings();

        // Volta para o Menu Principal como cena ativa no editor
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        Debug.Log("[SetupAllScenes] Todas as cenas foram atualizadas com sucesso!");
    }

    private static void UpdateMainMenu()
    {
        string path = "Assets/Scenes/MainMenu.unity";
        Scene scene = EditorSceneManager.OpenScene(path);

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SetupAllScenes] Canvas não encontrado no Menu!");
            return;
        }

        MainMenuController menuController = Object.FindAnyObjectByType<MainMenuController>();
        if (menuController == null)
        {
            GameObject mgr = GameObject.Find("MenuManager") ?? new GameObject("MenuManager");
            menuController = mgr.GetComponent<MainMenuController>() ?? mgr.AddComponent<MainMenuController>();
        }

        // Procura ou cria NameInputPanel
        Transform existingPanel = canvas.transform.Find("NameInputPanel");
        GameObject panelObj;
        if (existingPanel != null)
        {
            panelObj = existingPanel.gameObject;
        }
        else
        {
            panelObj = new GameObject("NameInputPanel");
            panelObj.transform.SetParent(canvas.transform, false);
            Image bg = panelObj.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Título
            GameObject titleObj = new GameObject("Text_TituloNome");
            titleObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "DIGITE SEU NOME";
            titleText.fontSize = 44;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 130);
            titleRt.sizeDelta = new Vector2(500, 60);

            // Subtítulo
            GameObject subObj = new GameObject("Text_Subtitulo");
            subObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI subText = subObj.AddComponent<TextMeshProUGUI>();
            subText.text = "Informe seu nome para iniciar o jogo:";
            subText.fontSize = 24;
            subText.color = new Color(0.8f, 0.8f, 0.8f);
            subText.alignment = TextAlignmentOptions.Center;
            RectTransform subRt = subObj.GetComponent<RectTransform>();
            subRt.anchoredPosition = new Vector2(0, 80);
            subRt.sizeDelta = new Vector2(500, 40);

            // InputField TMP
            TMP_DefaultControls.Resources res = new TMP_DefaultControls.Resources();
            GameObject inputObj = TMP_DefaultControls.CreateInputField(res);
            inputObj.name = "Input_Nome";
            inputObj.transform.SetParent(panelObj.transform, false);
            RectTransform inputRt = inputObj.GetComponent<RectTransform>();
            inputRt.anchoredPosition = new Vector2(0, 15);
            inputRt.sizeDelta = new Vector2(360, 55);

            TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
            if (inputField != null)
            {
                if (inputField.textComponent != null)
                {
                    inputField.textComponent.fontSize = 26;
                    inputField.textComponent.alignment = TextAlignmentOptions.Center;
                    inputField.textComponent.color = Color.black;
                }
                if (inputField.placeholder is TextMeshProUGUI ph)
                {
                    ph.text = "Nome do jogador...";
                    ph.fontSize = 24;
                    ph.alignment = TextAlignmentOptions.Center;
                }
            }

            // Botão Confirmar
            GameObject confirmBtnObj = CreateButton(panelObj.transform, "Btn_Confirmar", "INICIAR JOGO", new Vector2(0, -60), new Vector2(260, 55));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(confirmBtnObj.GetComponent<Button>().onClick, menuController.ConfirmNameAndStart);

            // Botão Voltar
            GameObject backBtnObj = CreateButton(panelObj.transform, "Btn_Voltar", "VOLTAR", new Vector2(0, -130), new Vector2(200, 45));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtnObj.GetComponent<Button>().onClick, menuController.CloseNameInput);
        }

        TMP_InputField foundInputField = panelObj.GetComponentInChildren<TMP_InputField>(true);

        // Atualiza referências no MainMenuController
        SerializedObject so = new SerializedObject(menuController);
        so.FindProperty("nameInputPanel").objectReferenceValue = panelObj;
        so.FindProperty("nameInputField").objectReferenceValue = foundInputField;
        so.ApplyModifiedProperties();

        // Garante que o Btn_Jogar chama menuController.StartGame
        Button playBtn = canvas.transform.Find("Btn_Jogar")?.GetComponent<Button>();
        if (playBtn != null)
        {
            // Limpa e readiciona para garantir chamada limpa
            for (int i = playBtn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(playBtn.onClick, i);
            }
            UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, menuController.StartGame);
        }

        // Esconde o painel inicialmente
        panelObj.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupAllScenes] MainMenu atualizado!");
    }

    private static void UpdateGameplayScene(string scenePath, bool isLastStage)
    {
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogWarning("[SetupAllScenes] Cena não encontrada: " + scenePath);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(scenePath);
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SetupAllScenes] Canvas não encontrado na cena: " + scenePath);
            return;
        }

        UIManager uiManager = Object.FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);
        if (uiManager == null)
        {
            GameObject uiObj = new GameObject("UIManager");
            uiManager = uiObj.AddComponent<UIManager>();
        }

        // 1. Procura ou cria TopBar (Barra Superior com Borda)
        Transform topBarTrans = canvas.transform.Find("TopBar");
        GameObject topBarObj;
        if (topBarTrans != null)
        {
            topBarObj = topBarTrans.gameObject;
        }
        else
        {
            topBarObj = new GameObject("TopBar");
            topBarObj.transform.SetParent(canvas.transform, false);
            topBarObj.transform.SetAsFirstSibling();
            Image barImg = topBarObj.AddComponent<Image>();
            barImg.color = new Color(0.04f, 0.035f, 0.1f, 0.95f);
            RectTransform barRt = topBarObj.GetComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0f, 1f);
            barRt.anchorMax = new Vector2(1f, 1f);
            barRt.pivot = new Vector2(0.5f, 1f);
            barRt.anchoredPosition = Vector2.zero;
            barRt.sizeDelta = new Vector2(0f, 50f);

            // Linha neon ciano separadora na parte inferior da barra
            GameObject borderLine = new GameObject("BorderLine");
            borderLine.transform.SetParent(topBarObj.transform, false);
            Image lineImg = borderLine.AddComponent<Image>();
            lineImg.color = new Color(0f, 0.9f, 1f, 1f);
            RectTransform lineRt = borderLine.GetComponent<RectTransform>();
            lineRt.anchorMin = new Vector2(0f, 0f);
            lineRt.anchorMax = new Vector2(1f, 0f);
            lineRt.pivot = new Vector2(0.5f, 0f);
            lineRt.anchoredPosition = Vector2.zero;
            lineRt.sizeDelta = new Vector2(0f, 3f);
        }

        // Procura ou cria TextoJogador dentro da TopBar
        Transform playerTxtTrans = topBarObj.transform.Find("TextoJogador") ?? canvas.transform.Find("TextoJogador");
        TextMeshProUGUI playerTmp;
        if (playerTxtTrans == null)
        {
            GameObject playerObj = new GameObject("TextoJogador");
            playerObj.transform.SetParent(topBarObj.transform, false);
            playerTmp = playerObj.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            playerTmp = playerTxtTrans.GetComponent<TextMeshProUGUI>();
            playerTmp.transform.SetParent(topBarObj.transform, false);
        }
        playerTmp.text = "...";
        playerTmp.fontSize = 22f;
        playerTmp.color = Color.white;
        playerTmp.alignment = TextAlignmentOptions.Center;
        RectTransform playerRt = playerTmp.rectTransform;
        playerRt.anchorMin = new Vector2(0.5f, 0.5f);
        playerRt.anchorMax = new Vector2(0.5f, 0.5f);
        playerRt.pivot = new Vector2(0.5f, 0.5f);
        playerRt.anchoredPosition = new Vector2(0f, 0f);
        playerRt.sizeDelta = new Vector2(240f, 40f);

        // Pontuação dentro da TopBar (lado esquerdo)
        TextMeshProUGUI scoreTmp = topBarObj.transform.Find("TextoPontos")?.GetComponent<TextMeshProUGUI>()
                                   ?? canvas.transform.Find("TextoPontos")?.GetComponent<TextMeshProUGUI>();
        if (scoreTmp != null)
        {
            scoreTmp.transform.SetParent(topBarObj.transform, false);
            scoreTmp.text = "0";
            scoreTmp.fontSize = 20f;
            scoreTmp.color = new Color(0f, 0.95f, 1f, 1f); // Ciano Neon (#00F5FF)
            scoreTmp.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform sRt = scoreTmp.rectTransform;
            sRt.anchorMin = new Vector2(0f, 0.5f);
            sRt.anchorMax = new Vector2(0f, 0.5f);
            sRt.pivot = new Vector2(0f, 0.5f);
            sRt.anchoredPosition = new Vector2(15f, 0f);
            sRt.sizeDelta = new Vector2(120f, 40f);
        }

        // Vidas dentro da TopBar (lado direito em Rosa Neon)
        TextMeshProUGUI livesTmp = topBarObj.transform.Find("TextoVidas")?.GetComponent<TextMeshProUGUI>()
                                   ?? canvas.transform.Find("TextoVidas")?.GetComponent<TextMeshProUGUI>();
        if (livesTmp != null)
        {
            livesTmp.transform.SetParent(topBarObj.transform, false);
            livesTmp.fontSize = 18f;
            livesTmp.color = new Color(1f, 0.2f, 0.5f, 1f); // Rosa Neon (#FF3380)
            livesTmp.alignment = TextAlignmentOptions.MidlineRight;
            RectTransform lRt = livesTmp.rectTransform;
            lRt.anchorMin = new Vector2(1f, 0.5f);
            lRt.anchorMax = new Vector2(1f, 0.5f);
            lRt.pivot = new Vector2(1f, 0.5f);
            lRt.anchoredPosition = new Vector2(-80f, 0f);
            lRt.sizeDelta = new Vector2(90f, 40f);
        }

        // 2. Atualizar ou Criar VictoryPanel
        Transform victoryPanelTrans = canvas.transform.Find("VictoryPanel");
        GameObject victoryPanelObj;
        if (victoryPanelTrans != null)
        {
            victoryPanelObj = victoryPanelTrans.gameObject;
        }
        else
        {
            victoryPanelObj = new GameObject("VictoryPanel");
            victoryPanelObj.transform.SetParent(canvas.transform, false);
            Image img = victoryPanelObj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.88f);
            RectTransform vicRt = victoryPanelObj.GetComponent<RectTransform>();
            vicRt.anchorMin = Vector2.zero;
            vicRt.anchorMax = Vector2.one;
            vicRt.offsetMin = Vector2.zero;
            vicRt.offsetMax = Vector2.zero;
        }

        // Text_Victory
        Transform vicTitleTrans = victoryPanelObj.transform.Find("Text_Victory");
        TextMeshProUGUI vicTitleText;
        if (vicTitleTrans == null)
        {
            GameObject titleObj = new GameObject("Text_Victory");
            titleObj.transform.SetParent(victoryPanelObj.transform, false);
            vicTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 150);
            titleRt.sizeDelta = new Vector2(600, 70);
        }
        else
        {
            vicTitleText = vicTitleTrans.GetComponent<TextMeshProUGUI>();
        }
        vicTitleText.text = "VITÓRIA";
        vicTitleText.fontSize = 42;
        vicTitleText.color = Color.yellow;
        vicTitleText.alignment = TextAlignmentOptions.Center;

        // Text_VictoryPlayer
        Transform vicPlayerTrans = victoryPanelObj.transform.Find("Text_VictoryPlayer");
        TextMeshProUGUI vicPlayerText;
        if (vicPlayerTrans == null)
        {
            GameObject pObj = new GameObject("Text_VictoryPlayer");
            pObj.transform.SetParent(victoryPanelObj.transform, false);
            vicPlayerText = pObj.AddComponent<TextMeshProUGUI>();
            RectTransform pRt = pObj.GetComponent<RectTransform>();
            pRt.anchoredPosition = new Vector2(0, 95);
            pRt.sizeDelta = new Vector2(600, 45);
        }
        else
        {
            vicPlayerText = vicPlayerTrans.GetComponent<TextMeshProUGUI>();
        }
        vicPlayerText.text = "...";
        vicPlayerText.fontSize = 28;
        vicPlayerText.color = Color.white;
        vicPlayerText.alignment = TextAlignmentOptions.Center;

        // Text_VictoryScore
        Transform vicScoreTrans = victoryPanelObj.transform.Find("Text_VictoryScore");
        TextMeshProUGUI vicScoreText;
        if (vicScoreTrans == null)
        {
            GameObject sObj = new GameObject("Text_VictoryScore");
            sObj.transform.SetParent(victoryPanelObj.transform, false);
            vicScoreText = sObj.AddComponent<TextMeshProUGUI>();
            RectTransform sRt = sObj.GetComponent<RectTransform>();
            sRt.anchoredPosition = new Vector2(0, 50);
            sRt.sizeDelta = new Vector2(600, 45);
        }
        else
        {
            vicScoreText = vicScoreTrans.GetComponent<TextMeshProUGUI>();
        }
        vicScoreText.text = "0";
        vicScoreText.fontSize = 30;
        vicScoreText.color = Color.yellow;
        vicScoreText.alignment = TextAlignmentOptions.Center;

        // Botões no VictoryPanel
        Transform btnCont = victoryPanelObj.transform.Find("Btn_Continuar");
        if (btnCont == null)
        {
            GameObject btn = CreateButton(victoryPanelObj.transform, "Btn_Continuar", "CONTINUAR", new Vector2(0, -10), new Vector2(300, 55));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.GetComponent<Button>().onClick, uiManager.Btn_NextLevel);
            btnCont = btn.transform;
        }
        btnCont.gameObject.SetActive(!isLastStage);

        Transform btnRec = victoryPanelObj.transform.Find("Btn_Recomecar");
        if (btnRec == null)
        {
            GameObject btn = CreateButton(victoryPanelObj.transform, "Btn_Recomecar", isLastStage ? "JOGAR NOVAMENTE" : "RECOMEÇAR", new Vector2(0, -80), new Vector2(300, 55));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.GetComponent<Button>().onClick, uiManager.Btn_RestartGame);
        }
        else
        {
            RectTransform recRt = btnRec.GetComponent<RectTransform>();
            recRt.anchoredPosition = new Vector2(0, -80);
            var tmp = btnRec.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null && isLastStage) tmp.text = "JOGAR NOVAMENTE";
        }

        Transform btnMenu = victoryPanelObj.transform.Find("Btn_Menu");
        if (btnMenu == null)
        {
            GameObject btn = CreateButton(victoryPanelObj.transform, "Btn_Menu", "MENU INICIAL", new Vector2(0, -150), new Vector2(300, 55));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.GetComponent<Button>().onClick, uiManager.Btn_LoadMainMenu);
        }
        else
        {
            RectTransform menuRt = btnMenu.GetComponent<RectTransform>();
            menuRt.anchoredPosition = new Vector2(0, -150);
        }

        victoryPanelObj.SetActive(false);

        // 3. Botão de Pause (Btn_Pause) posicionado na TopBar ao lado de TextoVidas
        Transform pauseBtnTrans = topBarObj.transform.Find("Btn_Pause") ?? canvas.transform.Find("Btn_Pause");
        GameObject pauseBtnObj;
        Button pauseBtn;
        if (pauseBtnTrans == null)
        {
            pauseBtnObj = CreateButton(topBarObj.transform, "Btn_Pause", "PAUSE", new Vector2(-15f, 0f), new Vector2(58f, 28f));
            pauseBtn = pauseBtnObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(pauseBtn.onClick, uiManager.TogglePause);
        }
        else
        {
            pauseBtnObj = pauseBtnTrans.gameObject;
            pauseBtnObj.transform.SetParent(topBarObj.transform, false);
            pauseBtn = pauseBtnObj.GetComponent<Button>();
        }
        RectTransform btnPauseRt = pauseBtnObj.GetComponent<RectTransform>();
        btnPauseRt.anchorMin = new Vector2(1f, 0.5f);
        btnPauseRt.anchorMax = new Vector2(1f, 0.5f);
        btnPauseRt.pivot = new Vector2(1f, 0.5f);
        btnPauseRt.anchoredPosition = new Vector2(-15f, 0f);
        btnPauseRt.sizeDelta = new Vector2(58f, 28f);

        var pTmp = pauseBtnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (pTmp != null)
        {
            pTmp.fontSize = 13f;
            pTmp.fontStyle = FontStyles.Bold;
        }
        pauseBtnObj.SetActive(true);

        // 4. Painel de Pause (PausePanel)
        Transform pausePanelTrans = canvas.transform.Find("PausePanel");
        GameObject pausePanelObj;
        if (pausePanelTrans != null)
        {
            pausePanelObj = pausePanelTrans.gameObject;
        }
        else
        {
            pausePanelObj = new GameObject("PausePanel");
            pausePanelObj.transform.SetParent(canvas.transform, false);
            Image img = pausePanelObj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.85f);
            RectTransform pauseRt = pausePanelObj.GetComponent<RectTransform>();
            pauseRt.anchorMin = Vector2.zero;
            pauseRt.anchorMax = Vector2.one;
            pauseRt.offsetMin = Vector2.zero;
            pauseRt.offsetMax = Vector2.zero;

            // Text_PauseTitle
            GameObject titleObj = new GameObject("Text_PauseTitle");
            titleObj.transform.SetParent(pausePanelObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "PAUSE";
            titleText.fontSize = 54;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.yellow;
            titleText.alignment = TextAlignmentOptions.Center;
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchoredPosition = new Vector2(0, 130);
            titleRt.sizeDelta = new Vector2(600, 70);

            // Botão Continuar
            GameObject btnContinuar = CreateButton(pausePanelObj.transform, "Btn_Continuar", "CONTINUAR", new Vector2(0, 30), new Vector2(280, 52));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnContinuar.GetComponent<Button>().onClick, uiManager.Btn_ResumeGame);

            // Botão Reiniciar
            GameObject btnReiniciar = CreateButton(pausePanelObj.transform, "Btn_Reiniciar", "REINICIAR", new Vector2(0, -40), new Vector2(280, 52));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnReiniciar.GetComponent<Button>().onClick, uiManager.Btn_RestartGame);

            // Botão Sair
            GameObject btnSair = CreateButton(pausePanelObj.transform, "Btn_Sair", "SAIR", new Vector2(0, -110), new Vector2(280, 52));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSair.GetComponent<Button>().onClick, uiManager.Btn_QuitGame);
        }
        pausePanelObj.SetActive(false);

        // 5. Ajustar o teto (TopWall) para a bola bater exatamente na borda
        Transform bordersTrans = GameObject.Find("Borders")?.transform;
        Transform topWallTrans = bordersTrans != null ? bordersTrans.Find("TopWall") : GameObject.Find("TopWall")?.transform;
        if (topWallTrans != null)
        {
            Camera cam = Camera.main;
            float camY = cam != null ? cam.transform.position.y : 1f;
            float halfH = cam != null ? cam.orthographicSize : 5f;
            // 50px de barra em 600h = 0.833 unidades
            float borderY = (camY + halfH) - 0.833f;
            BoxCollider2D col = topWallTrans.GetComponent<BoxCollider2D>();
            float thickness = col != null ? col.size.y : 1f;

            Vector3 pos = topWallTrans.position;
            pos.y = borderY + thickness * 0.5f;
            topWallTrans.position = pos;
        }

        // 6. Serializar no UIManager
        SerializedObject so = new SerializedObject(uiManager);
        so.FindProperty("scoreText").objectReferenceValue = scoreTmp;
        so.FindProperty("livesText").objectReferenceValue = livesTmp;
        so.FindProperty("playerNameText").objectReferenceValue = playerTmp;
        so.FindProperty("victoryPanel").objectReferenceValue = victoryPanelObj;
        so.FindProperty("pauseButton").objectReferenceValue = pauseBtn;
        so.FindProperty("pausePanel").objectReferenceValue = pausePanelObj;
        so.FindProperty("topBar").objectReferenceValue = topBarObj;
        
        Transform goPanel = canvas.transform.Find("GameOverPanel");
        if (goPanel != null)
        {
            so.FindProperty("gameOverPanel").objectReferenceValue = goPanel.gameObject;
        }
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupAllScenes] Cena atualizada: " + scenePath);
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = name;
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Object.DestroyImmediate(btnObj.GetComponentInChildren<Text>());
        GameObject textObj = new GameObject("Text (TMP)");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        textObj.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;

        return btnObj;
    }
}
