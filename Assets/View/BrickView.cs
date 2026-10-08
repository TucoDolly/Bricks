using UnityEngine;

public class BrickView : MonoBehaviour
{
    private BrickController _brickController;

    void Awake()
    {
        _brickController = GetComponent<StrongBrickController>();
        if (_brickController == null) _brickController = GetComponent<NormalBrickController>();
        if (_brickController == null) _brickController = GetComponent<BrickController>();

        // Fallback absoluto caso o gerador falhe ou um tijolo seja arrastado manualmente sem controller
        if (_brickController == null)
        {
            _brickController = gameObject.AddComponent<NormalBrickController>();
        }
    }

    public int Points => _brickController != null ? _brickController.Points : 0;

    /// <summary>Aplica dano ao tijolo. Retorna true se ele foi destruído.</summary>
    public bool PerformTakeDamage(float damage)
    {
        if (_brickController == null)
        {
            Debug.LogError($"'{name}' não possui BrickController. Use Ferramentas > Converter Tijolos Existentes.", this);
            return false;
        }
        return _brickController.TakeDamage(damage);
    }   
}