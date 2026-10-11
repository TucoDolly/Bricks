using UnityEngine;

/// <summary>Tijolo resistente: 2 vidas, muda de cor ao ser atingido e vale 20 pontos.</summary>
public class StrongBrickController : BrickController
{
    public static readonly Color FullHealthColor = new Color(0.92f, 0.15f, 1.0f); // Roxo / Magenta Neon Arcade 80s (#EA26FF)
    public static readonly Color DamagedColor    = new Color(1.0f, 0.92f, 0.1f);  // Amarelo Neon (#FFEB1A)

    private SpriteRenderer _spriteRenderer;

    public override float InitialHealth => 2f;
    public override int Points => 20;

    protected override void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (_spriteRenderer != null) _spriteRenderer.color = FullHealthColor;
    }

    protected override void OnDamaged()
    {
        if (_spriteRenderer != null) _spriteRenderer.color = DamagedColor;
    }
}
