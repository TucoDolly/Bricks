using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class MainMenuSetup
{
    [MenuItem("Ferramentas/Gerar Tela de Abertura")]
    public static void SetupMainMenu()
    {
        // 1. Salvar a cena atual, se necessário
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.isDirty)
        {
            EditorSceneManager.SaveScene(activeScene);
        }

        string gameScenePath = activeScene.path;
        if (string.IsNullOrEmpty(gameScenePath) || gameScenePath.Contains("MainMenu"))
        {
            // Se a cena ativa for o próprio Menu ou nula, procura a Fase 1 (SampleScene)
            string sampleScenePath = "Assets/Scenes/SampleScene.unity";
            if (System.IO.File.Exists(sampleScenePath))
            {
                gameScenePath = sampleScenePath;
            }
        }

        // 2. Criar nova cena para o menu
        Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Câmera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

        // 3. Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Event System
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        // 4. Menu Manager
        GameObject managerObj = new GameObject("MenuManager");
        MainMenuController menuController = managerObj.AddComponent<MainMenuController>();

        // 5. Título
        GameObject titleObj = new GameObject("Titulo");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "BRICKS";
        titleText.fontSize = 80;
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 150);
        titleRt.sizeDelta = new Vector2(600, 100);

        // 6. Botão Jogar
        GameObject playBtnObj = CreateButton(canvasObj.transform, "Btn_Jogar", "JOGAR", new Vector2(0, 0), new Vector2(220, 60));
        Button playButton = playBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(playButton.onClick, menuController.StartGame);

        // 7. Botão Sair
        GameObject quitBtnObj = CreateButton(canvasObj.transform, "Btn_Sair", "SAIR", new Vector2(0, -80), new Vector2(220, 60));
        Button quitButton = quitBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitButton.onClick, menuController.QuitGame);

        // 8. Painel de Inserção de Nome (NameInputPanel)
        GameObject namePanelObj = new GameObject("NameInputPanel");
        namePanelObj.transform.SetParent(canvasObj.transform, false);
        Image namePanelBg = namePanelObj.AddComponent<Image>();
        namePanelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        RectTransform namePanelRt = namePanelObj.GetComponent<RectTransform>();
        namePanelRt.anchorMin = Vector2.zero;
        namePanelRt.anchorMax = Vector2.one;
        namePanelRt.offsetMin = Vector2.zero;
        namePanelRt.offsetMax = Vector2.zero;

        // Título do painel de nome
        GameObject panelTitleObj = new GameObject("Text_TituloNome");
        panelTitleObj.transform.SetParent(namePanelObj.transform, false);
        TextMeshProUGUI panelTitleText = panelTitleObj.AddComponent<TextMeshProUGUI>();
        panelTitleText.text = "DIGITE SEU NOME";
        panelTitleText.fontSize = 44;
        panelTitleText.color = Color.white;
        panelTitleText.alignment = TextAlignmentOptions.Center;
        RectTransform panelTitleRt = panelTitleObj.GetComponent<RectTransform>();
        panelTitleRt.anchoredPosition = new Vector2(0, 130);
        panelTitleRt.sizeDelta = new Vector2(500, 60);

        // Subtítulo explicativo
        GameObject panelSubObj = new GameObject("Text_Subtitulo");
        panelSubObj.transform.SetParent(namePanelObj.transform, false);
        TextMeshProUGUI panelSubText = panelSubObj.AddComponent<TextMeshProUGUI>();
        panelSubText.text = "Informe seu nome para iniciar o jogo:";
        panelSubText.fontSize = 24;
        panelSubText.color = new Color(0.8f, 0.8f, 0.8f);
        panelSubText.alignment = TextAlignmentOptions.Center;
        RectTransform panelSubRt = panelSubObj.GetComponent<RectTransform>();
        panelSubRt.anchoredPosition = new Vector2(0, 80);
        panelSubRt.sizeDelta = new Vector2(500, 40);

        // Campo de entrada (InputField TMP)
        TMP_DefaultControls.Resources tmpRes = new TMP_DefaultControls.Resources();
        GameObject inputFieldObj = TMP_DefaultControls.CreateInputField(tmpRes);
        inputFieldObj.name = "Input_Nome";
        inputFieldObj.transform.SetParent(namePanelObj.transform, false);
        RectTransform inputRt = inputFieldObj.GetComponent<RectTransform>();
        inputRt.anchoredPosition = new Vector2(0, 15);
        inputRt.sizeDelta = new Vector2(360, 55);

        TMP_InputField tmpInputField = inputFieldObj.GetComponent<TMP_InputField>();
        if (tmpInputField != null)
        {
            tmpInputField.text = "";
            if (tmpInputField.textComponent != null)
            {
                tmpInputField.textComponent.fontSize = 26;
                tmpInputField.textComponent.alignment = TextAlignmentOptions.Center;
                tmpInputField.textComponent.color = Color.black;
            }
            if (tmpInputField.placeholder is TextMeshProUGUI ph)
            {
                ph.text = "Nome do jogador...";
                ph.fontSize = 24;
                ph.alignment = TextAlignmentOptions.Center;
            }
        }

        // Botão Confirmar / Iniciar
        GameObject confirmBtnObj = CreateButton(namePanelObj.transform, "Btn_Confirmar", "INICIAR JOGO", new Vector2(0, -60), new Vector2(260, 55));
        Button confirmBtn = confirmBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(confirmBtn.onClick, menuController.ConfirmNameAndStart);

        // Botão Voltar
        GameObject backBtnObj = CreateButton(namePanelObj.transform, "Btn_Voltar", "VOLTAR", new Vector2(0, -130), new Vector2(200, 45));
        Button backBtn = backBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtn.onClick, menuController.CloseNameInput);

        // Serializar referências no MainMenuController
        SerializedObject so = new SerializedObject(menuController);
        so.FindProperty("nameInputPanel").objectReferenceValue = namePanelObj;
        so.FindProperty("nameInputField").objectReferenceValue = tmpInputField;
        so.ApplyModifiedProperties();

        // Esconder painel de nome inicialmente
        namePanelObj.SetActive(false);

        // 9. Salvar cena de Menu
        string menuScenePath = "Assets/Scenes/MainMenu.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(menuScene, menuScenePath);

        // 10. Configurar Build Settings
        EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
        var newScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        // Adiciona Menu como Index 0
        newScenes.Add(new EditorBuildSettingsScene(menuScenePath, true));
        // Adiciona Jogo como Index 1
        if (!string.IsNullOrEmpty(gameScenePath) && gameScenePath != menuScenePath)
        {
            newScenes.Add(new EditorBuildSettingsScene(gameScenePath, true));
        }

        // Adiciona o restante das cenas originais
        foreach (var s in originalScenes)
        {
            if (s.path != menuScenePath && s.path != gameScenePath)
                newScenes.Add(s);
        }

        EditorBuildSettings.scenes = newScenes.ToArray();

        Debug.Log("[MainMenuSetup] Tela de abertura gerada com sucesso e adicionada ao Build Settings!");
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
