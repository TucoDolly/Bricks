using UnityEngine;

/// <summary>Tijolo resistente: 2 vidas, muda de cor ao ser atingido e vale 20 pontos.</summary>
public class StrongBrickController : BrickController
{
    private static readonly Color FullHealthColor = new Color(0.85f, 0.25f, 0.25f);
    private static readonly Color DamagedColor    = new Color(1f, 0.85f, 0.35f);

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
