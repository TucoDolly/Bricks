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
        if (string.IsNullOrEmpty(gameScenePath))
        {
            Debug.LogError("A cena do jogo precisa estar salva nos Assets antes de gerar o Menu.");
            return;
        }

        // 2. Criar nova cena para o menu
        Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Câmera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
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
        // Usar Reflection ou SerializedObject para setar o nome da cena de jogo caso seja private
        SerializedObject so = new SerializedObject(menuController);
        so.FindProperty("gameplaySceneName").stringValue = activeScene.name;
        so.ApplyModifiedProperties();

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
        GameObject playBtnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        playBtnObj.name = "Btn_Jogar";
        playBtnObj.transform.SetParent(canvasObj.transform, false);
        RectTransform playRt = playBtnObj.GetComponent<RectTransform>();
        playRt.anchoredPosition = new Vector2(0, 0);
        playRt.sizeDelta = new Vector2(200, 60);

        // Trocar Text nativo por TextMeshProUGUI no botão
        Object.DestroyImmediate(playBtnObj.GetComponentInChildren<Text>());
        GameObject playTextObj = new GameObject("Text (TMP)");
        playTextObj.transform.SetParent(playBtnObj.transform, false);
        TextMeshProUGUI playTmp = playTextObj.AddComponent<TextMeshProUGUI>();
        playTmp.text = "JOGAR";
        playTmp.color = Color.black;
        playTmp.alignment = TextAlignmentOptions.Center;
        playTextObj.GetComponent<RectTransform>().sizeDelta = playRt.sizeDelta;

        // Configurar OnClick do Jogar
        Button playButton = playBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(playButton.onClick, menuController.StartGame);

        // 7. Botão Sair
        GameObject quitBtnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        quitBtnObj.name = "Btn_Sair";
        quitBtnObj.transform.SetParent(canvasObj.transform, false);
        RectTransform quitRt = quitBtnObj.GetComponent<RectTransform>();
        quitRt.anchoredPosition = new Vector2(0, -80);
        quitRt.sizeDelta = new Vector2(200, 60);

        Object.DestroyImmediate(quitBtnObj.GetComponentInChildren<Text>());
        GameObject quitTextObj = new GameObject("Text (TMP)");
        quitTextObj.transform.SetParent(quitBtnObj.transform, false);
        TextMeshProUGUI quitTmp = quitTextObj.AddComponent<TextMeshProUGUI>();
        quitTmp.text = "SAIR";
        quitTmp.color = Color.black;
        quitTmp.alignment = TextAlignmentOptions.Center;
        quitTextObj.GetComponent<RectTransform>().sizeDelta = quitRt.sizeDelta;

        // Configurar OnClick do Sair
        Button quitButton = quitBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitButton.onClick, menuController.QuitGame);

        // 8. Salvar cena de Menu
        string menuScenePath = "Assets/Scenes/MainMenu.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(menuScene, menuScenePath);

        // 9. Configurar Build Settings
        EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
        
        bool menuExists = originalScenes.Any(s => s.path == menuScenePath);
        bool gameExists = originalScenes.Any(s => s.path == gameScenePath);

        var newScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        
        // Adiciona Menu como Index 0
        newScenes.Add(new EditorBuildSettingsScene(menuScenePath, true));
        // Adiciona Jogo como Index 1
        newScenes.Add(new EditorBuildSettingsScene(gameScenePath, true));

        // Adiciona o restante das cenas originais que não sejam essas duas
        foreach (var s in originalScenes)
        {
            if (s.path != menuScenePath && s.path != gameScenePath)
                newScenes.Add(s);
        }

        EditorBuildSettings.scenes = newScenes.ToArray();

        Debug.Log("[MainMenuSetup] Tela de abertura gerada com sucesso e adicionada ao Build Settings!");
    }
}
