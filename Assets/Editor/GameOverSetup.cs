using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class GameOverSetup
{
    [MenuItem("Ferramentas/Gerar Tela de Game Over")]
    public static void SetupGameOver()
    {
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>(FindObjectsInactive.Include);
        if (uiManager == null)
        {
            GameObject uiObj = new GameObject("UIManager");
            uiManager = uiObj.AddComponent<UIManager>();
            UnityEditor.Undo.RegisterCreatedObjectUndo(uiObj, "Criar UIManager");
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Nenhum Canvas encontrado na cena!");
            return;
        }

        // Criar Fundo (Panel)
        GameObject panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f); // Fundo escuro semi-transparente
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        // Título Game Over
        GameObject titleObj = new GameObject("Text_GameOver");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "GAME OVER";
        titleText.fontSize = 80;
        titleText.color = new Color(0.9f, 0.2f, 0.2f);
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 150);
        titleRt.sizeDelta = new Vector2(600, 100);

        // Botão Recomeçar
        GameObject restartBtn = CreateButton(panelObj.transform, "Btn_Recomecar", "RECOMEÇAR", new Vector2(0, 0));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.GetComponent<Button>().onClick, uiManager.Btn_RestartGame);

        // Botão Menu Inicial
        GameObject menuBtn = CreateButton(panelObj.transform, "Btn_Menu", "MENU INICIAL", new Vector2(0, -80));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, uiManager.Btn_LoadMainMenu);

        // Linkar o painel criado no UIManager
        SerializedObject so = new SerializedObject(uiManager);
        so.FindProperty("gameOverPanel").objectReferenceValue = panelObj;
        so.ApplyModifiedProperties();

        // Deixar o painel invisível por padrão (o jogo só mostra quando perde)
        panelObj.SetActive(false);

        UnityEditor.Undo.RegisterCreatedObjectUndo(panelObj, "Criar Tela Game Over");
        UnityEditor.EditorUtility.SetDirty(uiManager);
        Debug.Log("[GameOverSetup] Tela de Game Over gerada com sucesso e linkada ao UIManager!");
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position)
    {
        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = name;
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, 60);

        Object.DestroyImmediate(btnObj.GetComponentInChildren<Text>()); // Remove texto antigo
        GameObject textObj = new GameObject("Text (TMP)");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        textObj.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;

        return btnObj;
    }
}
