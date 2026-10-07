using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador para a Cena de Menu Principal.
/// Contém as funções chamadas pelos botões na UI.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        // Se houver um GameManager ativo na transição, garante que a pontuação/vidas recomecem zeradas
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetSession();
        }

        // Carrega direto pelo Índice 1 do Build Settings, ignorando o nome
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
