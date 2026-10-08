using UnityEngine;

/// <summary>Tijolo comum: 1 vida e 10 pontos.</summary>
public class NormalBrickController : BrickController
{
    public override float InitialHealth => 1f;
    public override int Points => 10;

    protected override void Awake()
    {
        base.Awake();
        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = UnityEngine.Color.white;
    }
}
