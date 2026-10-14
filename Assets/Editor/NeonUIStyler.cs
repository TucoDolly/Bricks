using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Aplica o visual Neon Arcade 80s nas telas do jogo (Início, Nome do Jogador, Pause, Vitória e Game Over).
/// Preserva TODO o posicionamento montado no Canvas: só altera cores, sprites, materiais e efeitos.
/// </summary>
public static class NeonUIStyler
{
    private static readonly string[] AllScenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/SampleScene.unity",
        "Assets/Scenes/Fase2.unity",
        "Assets/Scenes/Fase3.unity",
    };

    private static readonly string[] GameplayScreens = { "PausePanel", "VictoryPanel", "GameOverPanel" };

    [MenuItem("Ferramentas/Aplicar Visual Neon nas Telas (UI)")]
    public static void ApplyFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string previous = SceneManager.GetActiveScene().path;
        ApplyToAllScenes();

        if (!string.IsNullOrEmpty(previous) && File.Exists(previous))
            EditorSceneManager.OpenScene(previous);
    }

    [MenuItem("Ferramentas/Aplicar Visual Neon nas Telas (Cena Atual)")]
    public static void ApplyToCurrentScene()
    {
        StyleOpenScene(new NeonEditorAssetProvider());
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[NeonUIStyler] Visual neon aplicado na cena atual (salve a cena para manter).");
    }

    public static void ApplyToAllScenes()
    {
        var assets = new NeonEditorAssetProvider();
        foreach (string path in AllScenes)
        {
            if (!File.Exists(path)) continue;
            Scene scene = EditorSceneManager.OpenScene(path);
            StyleOpenScene(assets);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[NeonUIStyler] Telas neon aplicadas em: " + path);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[NeonUIStyler] Visual neon aplicado em todas as telas (layout preservado).");
    }

    private static void StyleOpenScene(NeonAssetProvider assets)
    {
        Dictionary<string, Transform> byName = IndexSceneByName();

        if (byName.ContainsKey("NameInputPanel") || Object.FindAnyObjectByType<MainMenuController>(FindObjectsInactive.Include) != null)
        {
            // Tela de início: estiliza o Canvas inteiro (título, botões e painel de nome)
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
                if (canvas.isRootCanvas) NeonUIStyle.StyleScreen(canvas.transform, assets);

            if (byName.TryGetValue("NameInputPanel", out Transform namePanel))
                NeonUIStyle.StyleScreen(namePanel, assets); // garante o fundo do painel

            ApplyCameraBackground();
            MarkAllDirty();
            return;
        }

        // Gameplay: telas de Pause, Vitória e Game Over
        foreach (string screen in GameplayScreens)
            if (byName.TryGetValue(screen, out Transform panel))
                NeonUIStyle.StyleScreen(panel, assets);

        // Botão de pause da TopBar
        if (byName.TryGetValue("Btn_Pause", out Transform pauseBtn) && pauseBtn.TryGetComponent(out Button btn))
            NeonUIStyle.StyleButton(btn, NeonUIStyle.Cyan, assets);

        StyleTopBarColors(byName, assets);
        MarkAllDirty();
    }

    private static void MarkAllDirty()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Component c in root.GetComponentsInChildren<Component>(true))
            {
                if (c != null) EditorUtility.SetDirty(c);
            }
        }
    }

    /// <summary>Só cores da HUD superior (sem mexer em posição).</summary>
    private static void StyleTopBarColors(Dictionary<string, Transform> byName, NeonAssetProvider assets)
    {
        if (byName.TryGetValue("TopBar", out Transform topBar) && topBar.TryGetComponent(out Image bar))
            bar.color = new Color(0.04f, 0.035f, 0.1f, 0.95f);
        if (byName.TryGetValue("BorderLine", out Transform line) && line.TryGetComponent(out Image lineImg))
            lineImg.color = NeonUIStyle.Cyan;
        if (byName.TryGetValue("TextoPontos", out Transform score) && score.TryGetComponent(out TMP_Text scoreTmp))
            scoreTmp.color = new Color(0f, 0.95f, 1f, 1f);
        if (byName.TryGetValue("TextoVidas", out Transform lives) && lives.TryGetComponent(out TMP_Text livesTmp))
            livesTmp.color = NeonUIStyle.Pink;
        if (byName.TryGetValue("LivesContainer", out Transform container))
        {
            foreach (Image heart in container.GetComponentsInChildren<Image>(true))
            {
                if (heart.gameObject != container.gameObject)
                {
                    heart.sprite = assets.HeartSprite;
                    heart.color = NeonUIStyle.Pink;
                }
            }
        }
    }

    private static void ApplyCameraBackground()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = NeonUIStyle.DeepSpace;
    }

    private static Dictionary<string, Transform> IndexSceneByName()
    {
        var map = new Dictionary<string, Transform>();
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (!map.ContainsKey(t.name)) map.Add(t.name, t);
        return map;
    }

    /// <summary>Versão de Editor do provedor: salva sprites (PNG) e materiais (.mat) no projeto.</summary>
    public class NeonEditorAssetProvider : NeonAssetProvider
    {
        private const string SpriteFolder = "Assets/Sprites/NeonUI";
        private const string MaterialFolder = "Assets/Settings/NeonUI";

        private Sprite _frame;
        private Sprite _fill;
        private Sprite _heart;
        private Sprite _pauseIcon;
        private readonly Dictionary<string, Material> _cache = new Dictionary<string, Material>();

        public override Sprite FrameSprite
        {
            get
            {
                if (_frame == null) _frame = LoadOrCreateSprite("NeonFrame.png", NeonUIStyle.GenerateFrameTexture, NeonUIStyle.FrameBorder);
                return _frame;
            }
        }

        public override Sprite FillSprite
        {
            get
            {
                if (_fill == null) _fill = LoadOrCreateSprite("NeonFill.png", NeonUIStyle.GenerateFillTexture, NeonUIStyle.FillBorder);
                return _fill;
            }
        }

        public override Sprite HeartSprite
        {
            get
            {
                if (_heart == null) _heart = LoadOrCreateSprite("NeonHeart.png", () => NeonUIStyle.GenerateHeartTexture(64), 0);
                return _heart;
            }
        }

        public override Sprite PauseIconSprite
        {
            get
            {
                if (_pauseIcon == null) _pauseIcon = LoadOrCreateSprite("NeonPause.png", () => NeonUIStyle.GeneratePauseIconTexture(64), 0);
                return _pauseIcon;
            }
        }

        public override Material GetTextMaterial(TMP_FontAsset font, Color neon, NeonUIStyle.Glow glow)
        {
            if (font == null) return null;
            string name = NeonUIStyle.MaterialName(font, neon, glow);
            if (_cache.TryGetValue(name, out Material cached) && cached != null) return cached;

            EnsureFolder(MaterialFolder);
            string path = $"{MaterialFolder}/{name}.mat";

            Material fresh = NeonUIStyle.CreateTextMaterial(font, neon, glow);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (fresh == null) return mat;

            if (mat == null)
            {
                AssetDatabase.CreateAsset(fresh, path);
                mat = fresh;
            }
            else
            {
                mat.shader = fresh.shader;
                mat.CopyPropertiesFromMaterial(fresh);
                mat.shaderKeywords = fresh.shaderKeywords;
                EditorUtility.SetDirty(mat);
                Object.DestroyImmediate(fresh);
            }

            _cache[name] = mat;
            return mat;
        }

        private static Sprite LoadOrCreateSprite(string file, System.Func<Texture2D> generator, int border)
        {
            EnsureFolder(SpriteFolder);
            string path = $"{SpriteFolder}/{file}";

            if (!File.Exists(path))
            {
                Texture2D tex = generator();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.spriteBorder = new Vector4(border, border, border, border);
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect; // necessário para 9-slice
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
