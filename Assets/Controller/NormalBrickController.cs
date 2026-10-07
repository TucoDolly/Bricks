/// <summary>Tijolo comum: 1 vida e 10 pontos.</summary>
public class NormalBrickController : BrickController
{
    public override float InitialHealth => 1f;
    public override int Points => 10;
}
