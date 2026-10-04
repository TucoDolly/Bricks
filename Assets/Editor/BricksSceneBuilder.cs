using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Automatiza a montagem da cena do jogo Bricks pelo menu "Ferramentas".
/// </summary>
public static class BricksSceneBuilder
{
    [MenuItem("Ferramentas/Montar Cena Bricks")]
    public static void BuildScene()
    {
        CreateBall();
        CreatePlayer();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[BricksSceneBuilder] Cena Bricks montada com sucesso.");
    }

    [MenuItem("Ferramentas/Ajustar Física e Posições")]
    public static void AdjustPhysicsAndPositions()
    {
        GameObject ball = GameObject.Find("Ball");
        GameObject player = GameObject.Find("Player");

        if (ball == null || player == null)
        {
            Debug.LogError("[BricksSceneBuilder] 'Ball' e/ou 'Player' não encontrados na cena ativa. Use primeiro 'Ferramentas/Montar Cena Bricks'.");
            return;
        }

        Undo.SetCurrentGroupName("Ajustar Física e Posições");
        int undoGroup = Undo.GetCurrentGroup();

        // Posições
        Undo.RecordObject(player.transform, "Posicionar Player");
        player.transform.position = new Vector3(0f, -4f, 0f);
        Undo.RecordObject(ball.transform, "Posicionar Ball");
        ball.transform.position = new Vector3(0f, -3.5f, 0f);

        // Gravidade da Ball
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            Undo.RecordObject(ballRb, "Zerar gravidade da Ball");
            ballRb.gravityScale = 0f;
        }
        else
        {
            Debug.LogWarning("[BricksSceneBuilder] Ball não possui Rigidbody2D.");
        }

        // Colisores ajustados ao sprite
        FitBoxColliderToSprite(player);
        FitCircleColliderToSprite(ball);

        // Tijolos: ajusta o BoxCollider2D de cada filho de 'BricksContainer'.
        GameObject bricksContainer = GameObject.Find("BricksContainer");
        if (bricksContainer != null)
        {
            foreach (Transform child in bricksContainer.transform)
            {
                FitBoxColliderToSprite(child.gameObject);
            }
        }


        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[BricksSceneBuilder] Física e posições ajustadas.");
    }

    [MenuItem("Ferramentas/Gerar Paredes")]
    public static void GenerateWalls()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            Debug.LogError("[BricksSceneBuilder] É necessária uma Camera.main ortográfica na cena.");
            return;
        }

        const float thickness = 1f;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // Pai centrado na câmara; filhos usam posições locais relativas ao centro da vista.
        GameObject borders = new GameObject("Borders");
        Undo.RegisterCreatedObjectUndo(borders, "Criar Borders");
        Vector3 camPos = cam.transform.position;
        borders.transform.position = new Vector3(camPos.x, camPos.y, 0f);

        // As paredes ficam do lado de fora da área visível, encostadas nas bordas.
        // Top/Bottom cobrem também os cantos (largura + 2 * espessura).
        float fullWidth = halfW * 2f + thickness * 2f;
        float fullHeight = halfH * 2f;

        CreateWall(borders, "TopWall", new Vector2(0f, halfH + thickness * 0.5f), new Vector2(fullWidth, thickness), false);
        CreateWall(borders, "BottomWall", new Vector2(0f, -halfH - thickness * 0.5f), new Vector2(fullWidth, thickness), true);
        CreateWall(borders, "LeftWall", new Vector2(-halfW - thickness * 0.5f, 0f), new Vector2(thickness, fullHeight), false);
        CreateWall(borders, "RightWall", new Vector2(halfW + thickness * 0.5f, 0f), new Vector2(thickness, fullHeight), false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = borders;
        Debug.Log("[BricksSceneBuilder] Paredes geradas.");
    }

    private static void CreateWall(GameObject parent, string name, Vector2 localPosition, Vector2 size, bool isTrigger)
    {
        GameObject wall = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(wall, "Criar " + name);
        Undo.SetTransformParent(wall.transform, parent.transform, "Parentear " + name);
        wall.transform.localPosition = localPosition;

        BoxCollider2D col = Undo.AddComponent<BoxCollider2D>(wall);
        col.size = size;
        col.isTrigger = isTrigger;
    }

    private static bool TryGetLocalBounds(GameObject go, out Vector2 localSize, out Vector2 localCenter)
    {
        localSize = Vector2.zero;
        localCenter = Vector2.zero;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning($"[BricksSceneBuilder] '{go.name}' não possui SpriteRenderer com sprite atribuído; collider não ajustado.");
            return false;
        }

        // bounds está em espaço de mundo; convertemos para espaço local do objeto.
        Bounds b = sr.bounds;
        Vector3 scale = go.transform.lossyScale;
        if (Mathf.Approximately(scale.x, 0f) || Mathf.Approximately(scale.y, 0f))
        {
            return false;
        }

        localSize = new Vector2(b.size.x / Mathf.Abs(scale.x), b.size.y / Mathf.Abs(scale.y));
        localCenter = go.transform.InverseTransformPoint(b.center);
        return true;
    }

    private static void FitBoxColliderToSprite(GameObject go)
    {
        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            Debug.LogWarning($"[BricksSceneBuilder] '{go.name}' não possui BoxCollider2D.");
            return;
        }
        if (!TryGetLocalBounds(go, out Vector2 size, out Vector2 center)) return;

        Undo.RecordObject(col, "Ajustar BoxCollider2D");
        col.size = size;
        col.offset = center;
    }

    [MenuItem("Ferramentas/Gerar Tijolos")]
    public static void GenerateBricks()
    {
        // Evita grades duplicadas.
        if (GameObject.Find("BricksContainer") != null)
        {
            Debug.LogWarning("[BricksSceneBuilder] 'BricksContainer' já existe na cena. Remova-o antes de gerar novamente.");
            return;
        }

        // Parâmetros configuráveis da grade.
        const int columns = 6;
        const int rows = 4;
        const float startX = -2.5f;   // origem X (primeira coluna)
        const float startY = 4f;      // origem Y (primeira linha, no topo)
        const float spacingX = 1f;    // espaçamento entre colunas
        const float spacingY = 0.5f;  // espaçamento entre linhas

        GameObject container = new GameObject("BricksContainer");
        Undo.RegisterCreatedObjectUndo(container, "Gerar Tijolos");

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject brick = new GameObject("Brick");
                // Registrado dentro do mesmo grupo; desfazer o container remove também os filhos.
                brick.transform.SetParent(container.transform, false);
                brick.transform.position = new Vector3(startX + col * spacingX, startY - row * spacingY, 0f);

                brick.AddComponent<SpriteRenderer>();
                brick.AddComponent<BoxCollider2D>();
                brick.AddComponent<BrickModel>();
                brick.AddComponent<BrickView>();
                brick.AddComponent<BrickController>();
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = container;
        Debug.Log($"[BricksSceneBuilder] {columns * rows} tijolos gerados.");
    }

    private static void FitCircleColliderToSprite(GameObject go)
    {
        CircleCollider2D col = go.GetComponent<CircleCollider2D>();
        if (col == null)
        {
            Debug.LogWarning($"[BricksSceneBuilder] '{go.name}' não possui CircleCollider2D.");
            return;
        }
        if (!TryGetLocalBounds(go, out Vector2 size, out Vector2 center)) return;

        Undo.RecordObject(col, "Ajustar CircleCollider2D");
        col.radius = Mathf.Max(size.x, size.y) * 0.5f;
        col.offset = center;
    }

    private static void CreateBall()
    {
        GameObject ball = new GameObject("Ball");
        Undo.RegisterCreatedObjectUndo(ball, "Criar Ball");

        Undo.AddComponent<SpriteRenderer>(ball);

        Rigidbody2D rb = Undo.AddComponent<Rigidbody2D>(ball);
        rb.bodyType = RigidbodyType2D.Dynamic;

        Undo.AddComponent<CircleCollider2D>(ball);

        Undo.AddComponent<BallModel>(ball);
        Undo.AddComponent<BallView>(ball);
        Undo.AddComponent<BallController>(ball);
    }

    private static void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        Undo.RegisterCreatedObjectUndo(player, "Criar Player");

        Undo.AddComponent<SpriteRenderer>(player);

        Rigidbody2D rb = Undo.AddComponent<Rigidbody2D>(player);
        rb.bodyType = RigidbodyType2D.Kinematic;

        Undo.AddComponent<BoxCollider2D>(player);

        Undo.AddComponent<PlayerModel>(player);
        Undo.AddComponent<PlayerView>(player);
        Undo.AddComponent<PlayerController>(player);
    }
}
