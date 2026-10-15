using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controlador para a Cena de Menu Principal.
/// Gerencia os botões do menu e a tela/popup para inserção do nome do jogador.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private TMP_InputField nameInputField;

    private void Start()
    {
        AudioController.Instance?.RegisterAllButtonsInScene();

        if (nameInputPanel == null)
        {
            FindPanelReferences();
        }

        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }

        if (nameInputField != null)
        {
            nameInputField.onSubmit.AddListener(_ => ConfirmNameAndStart());
        }
    }

    private void FindPanelReferences()
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            if (t.name == "NameInputPanel")
            {
                nameInputPanel = t.gameObject;
                break;
            }
        }

        if (nameInputPanel != null && nameInputField == null)
        {
            nameInputField = nameInputPanel.GetComponentInChildren<TMP_InputField>(true);
        }
    }

    /// <summary>
    /// Chamado ao clicar no botão "JOGAR" no menu principal.
    /// Abre a tela para o usuário colocar seu nome.
    /// </summary>
    public void StartGame()
    {
        OpenNameInput();
    }

    public void OpenNameInput()
    {
        if (nameInputPanel == null)
        {
            FindPanelReferences();
        }

        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            AudioController.Instance?.RegisterAllButtonsInScene();
            if (nameInputField == null)
            {
                nameInputField = nameInputPanel.GetComponentInChildren<TMP_InputField>(true);
            }

            if (nameInputField != null)
            {
                nameInputField.text = "";
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
        }
        else
        {
            // Fallback direto caso o painel não exista na cena
            ConfirmNameAndStart();
        }
    }

    public void CloseNameInput()
    {
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Chamado ao clicar no botão de confirmar o nome ou ao pressionar Enter.
    /// Salva o nome no Singleton GameManager e inicia a Fase 1.
    /// </summary>
    public void ConfirmNameAndStart()
    {
        string playerName = nameInputField != null ? nameInputField.text : "";
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Jogador";
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerName(playerName);
            GameManager.Instance.ResetSession();
        }

        // Carrega a Fase 1 (Índice 1 no Build Settings)
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
