using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Abstração de um tijolo. Define o fluxo comum (receber dano, ser destruído,
/// reiniciar a fase ao quebrar o último) e deixa para as subclasses o que varia:
/// vida inicial, pontuação e reação ao dano.
/// </summary>
public class BrickController : MonoBehaviour
{
    protected BrickModel _brickModel;

    /// <summary>Vida inicial do tijolo.</summary>
    public virtual float InitialHealth => 1f;

    /// <summary>Pontos concedidos ao destruir o tijolo.</summary>
    public virtual int Points => 10;

    /// <summary>Chamado quando o tijolo sofre dano e continua vivo.</summary>
    protected virtual void OnDamaged() { }

    /// <summary>Chamado imediatamente antes do tijolo ser destruído.</summary>
    protected virtual void OnBrickDestroyed() { }

    protected virtual void Awake()
    {
        _brickModel = GetComponent<BrickModel>();
        _brickModel.Health = InitialHealth;
        _brickModel.Points = Points;
    }

    /// <summary>Aplica dano ao tijolo. Retorna true se ele foi destruído.</summary>
    public bool TakeDamage(float damage)
    {
        _brickModel.Health -= damage;
        if (_brickModel.Health > 0f)
        {
            OnDamaged();
            return false;
        }

        OnBrickDestroyed();

        int activeBricks = 0;
        foreach (var brick in FindObjectsByType<BrickController>(FindObjectsInactive.Include))
        {
            if (brick != this && brick.gameObject.activeInHierarchy && brick._brickModel.Health > 0f)
                activeBricks++;
        }
        bool ultimo = activeBricks == 0;
        
        if (GameManager.Instance != null) GameManager.Instance.AddScore(Points);
        
        Destroy(gameObject);
        if (ultimo)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.WinMatch();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // fallback
        }
        return true;
    }
}
