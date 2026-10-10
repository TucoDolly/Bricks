using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class VictorySetup
{
    [MenuItem("Ferramentas/Gerar Tela de Vitória")]
    public static void SetupVictory()
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
        GameObject panelObj = new GameObject("VictoryPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.88f); // Fundo escuro
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        // Título Vitória
        GameObject titleObj = new GameObject("Text_Victory");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "VITÓRIA";
        titleText.fontSize = 42;
        titleText.color = Color.yellow;
        titleText.alignment = TextAlignmentOptions.Center;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 150);
        titleRt.sizeDelta = new Vector2(600, 70);

        // Nome do Jogador
        GameObject playerObj = new GameObject("Text_VictoryPlayer");
        playerObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI playerText = playerObj.AddComponent<TextMeshProUGUI>();
        playerText.text = "...";
        playerText.fontSize = 28;
        playerText.color = Color.white;
        playerText.alignment = TextAlignmentOptions.Center;
        RectTransform playerRt = playerObj.GetComponent<RectTransform>();
        playerRt.anchoredPosition = new Vector2(0, 95);
        playerRt.sizeDelta = new Vector2(600, 45);

        // Pontuação Final / da Fase
        GameObject scoreObj = new GameObject("Text_VictoryScore");
        scoreObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "0";
        scoreText.fontSize = 30;
        scoreText.color = Color.yellow;
        scoreText.alignment = TextAlignmentOptions.Center;
        RectTransform scoreRt = scoreObj.GetComponent<RectTransform>();
        scoreRt.anchoredPosition = new Vector2(0, 50);
        scoreRt.sizeDelta = new Vector2(600, 45);

        // Botão Continuar (Próxima Fase)
        GameObject nextBtn = CreateButton(panelObj.transform, "Btn_Continuar", "CONTINUAR", new Vector2(0, -10));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.GetComponent<Button>().onClick, uiManager.Btn_NextLevel);

        // Botão Recomeçar
        GameObject restartBtn = CreateButton(panelObj.transform, "Btn_Recomecar", "RECOMEÇAR", new Vector2(0, -80));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.GetComponent<Button>().onClick, uiManager.Btn_RestartGame);

        // Botão Menu Inicial
        GameObject menuBtn = CreateButton(panelObj.transform, "Btn_Menu", "MENU INICIAL", new Vector2(0, -150));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(menuBtn.GetComponent<Button>().onClick, uiManager.Btn_LoadMainMenu);

        // Linkar o painel criado no UIManager
        SerializedObject so = new SerializedObject(uiManager);
        so.FindProperty("victoryPanel").objectReferenceValue = panelObj;
        so.ApplyModifiedProperties();

        // Esconder por padrão
        panelObj.SetActive(false);

        UnityEditor.Undo.RegisterCreatedObjectUndo(panelObj, "Criar Tela de Vitória");
        UnityEditor.EditorUtility.SetDirty(uiManager);
        Debug.Log("[VictorySetup] Tela de Vitória gerada com sucesso e linkada ao UIManager!");
    }

    private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position)
    {
        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = name;
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(300, 55);

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
