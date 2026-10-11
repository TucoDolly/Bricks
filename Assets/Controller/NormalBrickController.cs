using UnityEngine;

/// <summary>Tijolo comum: 1 vida e 10 pontos. Cores estilizadas Neon Arcade 80s.</summary>
public class NormalBrickController : BrickController
{
    public override float InitialHealth => 1f;
    public override int Points => 10;

    /// <summary>
    /// Retorna a cor neon correspondente à linha do bloco (Arcade 80s: Azul, Laranja, Amarelo, Roxo).
    /// </summary>
    public static Color GetNeonColorForRow(float y)
    {
        if (y >= 3.8f || (y >= 0.8f && y <= 1.2f))
            return new Color(0.85f, 0.15f, 1f); // Roxo/Magenta Neon (#D926FF)
        if ((y >= 3.3f && y < 3.8f) || (y >= 0.3f && y < 0.8f))
            return new Color(0f, 0.9f, 1f);     // Azul Ciano Neon (#00E5FF)
        if ((y >= 2.8f && y < 3.3f) || (y >= -0.2f && y < 0.3f))
            return new Color(1f, 0.45f, 0f);    // Laranja Neon (#FF7300)
        return new Color(1f, 0.92f, 0.1f);      // Amarelo Neon (#FFEB1A)
    }

    protected override void Awake()
    {
        base.Awake();
        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Se a cor for branca (padrão antigo) ou não definida, aplica a cor da paleta neon arcade 80s por linha
            if (spriteRenderer.color == Color.white || spriteRenderer.color == Color.clear)
            {
                spriteRenderer.color = GetNeonColorForRow(transform.position.y);
            }
        }
    }
}
