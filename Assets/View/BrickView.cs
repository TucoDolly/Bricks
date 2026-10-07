using UnityEngine;

public class BrickView : MonoBehaviour
{
    private BrickController _brickController;

    void Awake()
    {
        _brickController = GetComponent<BrickController>();

        // Fallback: tijolos antigos sem controller recebem um automaticamente.
        if (_brickController == null)
        {
            float topY = float.MinValue;
            foreach (BrickView v in FindObjectsByType<BrickView>(FindObjectsInactive.Include))
                topY = Mathf.Max(topY, v.transform.position.y);

            bool isTopRow = transform.position.y >= topY - 0.01f;
            _brickController = isTopRow
                ? (BrickController)gameObject.AddComponent<StrongBrickController>()
                : gameObject.AddComponent<NormalBrickController>();
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