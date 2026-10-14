using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Estilo visual Neon Arcade 80s para as telas (UI) do jogo.
/// IMPORTANTE: só altera cores, materiais, sprites e efeitos. Nunca altera posição/tamanho
/// (RectTransform) dos elementos já existentes — o layout montado no Canvas é preservado.
/// A única coisa adicionada é um filho "NeonFrame" (moldura brilhante) esticado ao redor de botões/campos.
/// </summary>
public static class NeonUIStyle
{
    public enum Glow { Soft, Strong }

    // Paleta (mesma usada na gameplay)
    public static readonly Color DeepSpace = new Color(0.043f, 0.035f, 0.11f, 1f); // #0B091C
    public static readonly Color Cyan      = new Color(0f, 0.9f, 1f);              // #00E5FF
    public static readonly Color Magenta   = new Color(0.92f, 0.15f, 1f);          // #EA26FF
    public static readonly Color Pink      = new Color(1f, 0.2f, 0.5f);            // #FF3380
    public static readonly Color Green     = new Color(0.1f, 1f, 0.5f);            // #1AFF80
    public static readonly Color Yellow    = new Color(1f, 0.92f, 0.1f);           // #FFEB1A
    public static readonly Color Lavender  = new Color(0.78f, 0.74f, 1f);          // texto secundário

    public const string FrameName = "NeonFrame";

    // Geometria dos sprites procedurais (em pixels; 100 PPU => 1px = 1 unidade de UI)
    public const int FrameTexSize = 64;
    public const int FrameGlowPad = 12;   // halo externo da moldura
    public const int FrameBorder  = 28;   // 9-slice da moldura
    public const int FillTexSize  = 40;
    public const int FillBorder   = 16;   // 9-slice do fundo
    private const float LineThickness = 3f;
    private const float LineRadius    = 12f;

    // ---------------------------------------------------------------------------------------------
    // Papéis (por nome do objeto) -> cor neon
    // ---------------------------------------------------------------------------------------------

    public static Color ButtonColorFor(string objName)
    {
        switch (objName)
        {
            case "Btn_Jogar":
            case "Btn_Continuar":
            case "Btn_Confirmar":  return Green;
            case "Btn_Recomecar":
            case "Btn_Reiniciar":  return Cyan;
            case "Btn_Menu":
            case "Btn_Voltar":     return Magenta;
            case "Btn_Sair":       return Pink;
            default:               return Cyan;
        }
    }

    public static Color PanelAccentFor(string objName, out float backgroundAlpha)
    {
        switch (objName)
        {
            case "PausePanel":     backgroundAlpha = 0.82f; return Cyan;
            case "VictoryPanel":   backgroundAlpha = 0.90f; return Green;
            case "GameOverPanel":  backgroundAlpha = 0.90f; return Pink;
            case "NameInputPanel": backgroundAlpha = 0.96f; return Cyan;
            default:               backgroundAlpha = 0.90f; return Cyan;
        }
    }

    public static void GetTextRole(string objName, out Color color, out Glow glow, out bool pulse)
    {
        pulse = false;
        glow = Glow.Strong;
        switch (objName)
        {
            case "Titulo":             color = Magenta; pulse = true; return;
            case "Text_TituloNome":    color = Cyan;                  return;
            case "Text_PauseTitle":    color = Cyan;    pulse = true; return;
            case "Text_Victory":       color = Green;   pulse = true; return;
            case "Text_GameOver":      color = Pink;    pulse = true; return;
            case "LevelStartText":     color = Cyan;    pulse = true; return;
            case "Text_VictoryScore":  color = Yellow;                return;
            case "Text_VictoryPlayer": color = Cyan;    glow = Glow.Soft; return;
            default:                   color = Lavender; glow = Glow.Soft; return;
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Aplicação de estilo
    // ---------------------------------------------------------------------------------------------

    /// <summary>Texto neon: núcleo claro + contorno e brilho na cor neon.</summary>
    public static void StyleText(TMP_Text text, Color neon, Glow glow, NeonAssetProvider assets, bool pulse = false)
    {
        if (text == null) return;
        if (assets == null) assets = NeonAssetProvider.Runtime;

        float alpha = text.color.a;
        Color c = Color.Lerp(neon, Color.white, glow == Glow.Strong ? 0.4f : 0.2f);
        c.a = alpha <= 0.01f ? 1f : alpha;
        text.color = c;

        Material mat = assets.GetTextMaterial(text.font, neon, glow);
        if (mat != null) text.fontSharedMaterial = mat;

        NeonPulse existing = text.GetComponent<NeonPulse>();
        if (pulse && existing == null) text.gameObject.AddComponent<NeonPulse>();
        else if (!pulse && existing != null) DestroySafe(existing);
    }

    /// <summary>Aplica o estilo do papel do texto (definido pelo nome do objeto).</summary>
    public static void StyleTextByRole(TMP_Text text, NeonAssetProvider assets)
    {
        if (text == null) return;
        GetTextRole(text.name, out Color color, out Glow glow, out bool pulse);
        StyleText(text, color, glow, assets, pulse);
    }

    /// <summary>Botão neon: fundo escuro arredondado, moldura brilhante, texto neon e hover animado.</summary>
    public static void StyleButton(Button button, Color neon, NeonAssetProvider assets)
    {
        if (button == null) return;
        if (assets == null) assets = NeonAssetProvider.Runtime;

        RectTransform rt = (RectTransform)button.transform;
        float mult = MultiplierFor(rt);

        Image bg = button.targetGraphic as Image;
        if (bg == null) button.TryGetComponent(out bg);
        if (bg != null)
        {
            bg.sprite = assets.FillSprite;
            bg.type = Image.Type.Sliced;
            bg.fillCenter = true;
            bg.pixelsPerUnitMultiplier = mult;
            bg.color = Color.white; // a cor real vem do ColorBlock (permite hover)
            button.targetGraphic = bg;
        }

        button.transition = Selectable.Transition.ColorTint;
        button.colors = MakeColorBlock(neon, 0.92f);

        Image frame = EnsureFrame(rt, neon, assets, mult);

        if (button.name == "Btn_Pause")
        {
            // Símbolo universal de pause: 2 barras verticais neon centralizadas
            Transform iconTrans = button.transform.Find("Icon_Pause");
            Image iconImg;
            if (iconTrans == null)
            {
                GameObject iconObj = new GameObject("Icon_Pause");
                iconObj.transform.SetParent(button.transform, false);
                RectTransform irt = iconObj.AddComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = Vector2.zero;
                irt.sizeDelta = new Vector2(16f, 16f);
                iconImg = iconObj.AddComponent<Image>();
            }
            else
            {
                iconImg = iconTrans.GetComponent<Image>();
                if (iconImg != null && iconImg.rectTransform != null)
                {
                    iconImg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    iconImg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    iconImg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    iconImg.rectTransform.anchoredPosition = Vector2.zero;
                    iconImg.rectTransform.sizeDelta = new Vector2(16f, 16f);
                }
            }

            if (iconImg != null)
            {
                iconImg.sprite = assets.PauseIconSprite;
                iconImg.color = neon;
                iconImg.raycastTarget = false;
            }

            // Desativa textos para exibir exclusivamente as 2 barras
            foreach (TMP_Text label in button.GetComponentsInChildren<TMP_Text>(true))
                label.gameObject.SetActive(false);
            foreach (Text legacy in button.GetComponentsInChildren<Text>(true))
                legacy.gameObject.SetActive(false);
        }
        else
        {
            foreach (TMP_Text label in button.GetComponentsInChildren<TMP_Text>(true))
                StyleText(label, neon, Glow.Soft, assets);

            bool hasTmp = button.GetComponentInChildren<TMP_Text>(true) != null;
            foreach (Text legacy in button.GetComponentsInChildren<Text>(true))
            {
                if (hasTmp) legacy.gameObject.SetActive(false);
                else legacy.color = Color.Lerp(neon, Color.white, 0.2f);
            }
        }

        if (!button.TryGetComponent(out NeonButtonFx fx)) fx = button.gameObject.AddComponent<NeonButtonFx>();
        fx.Setup(frame, neon, true);
    }

    /// <summary>Campo de texto neon (ex.: nome do jogador).</summary>
    public static void StyleInputField(TMP_InputField field, Color neon, NeonAssetProvider assets)
    {
        if (field == null) return;
        if (assets == null) assets = NeonAssetProvider.Runtime;

        RectTransform rt = (RectTransform)field.transform;
        float mult = MultiplierFor(rt);

        Image bg = field.targetGraphic as Image;
        if (bg == null) field.TryGetComponent(out bg);
        if (bg != null)
        {
            bg.sprite = assets.FillSprite;
            bg.type = Image.Type.Sliced;
            bg.fillCenter = true;
            bg.pixelsPerUnitMultiplier = mult;
            bg.color = Color.white;
            field.targetGraphic = bg;
        }

        field.transition = Selectable.Transition.ColorTint;
        ColorBlock cb = MakeColorBlock(neon, 0.95f);
        cb.selectedColor = Color.Lerp(DarkFill(0.95f), neon, 0.14f); // destaque enquanto digita
        field.colors = cb;

        Image frame = EnsureFrame(rt, neon, assets, mult);

        if (field.textComponent != null)
            StyleText(field.textComponent, neon, Glow.Soft, assets);

        if (field.placeholder is TMP_Text ph)
        {
            if (ph.font != null && ph.font.material != null) ph.fontSharedMaterial = ph.font.material;
            ph.color = new Color(Lavender.r, Lavender.g, Lavender.b, 0.45f);
        }

        field.customCaretColor = true;
        field.caretColor = neon;
        field.caretWidth = 2;
        field.selectionColor = new Color(neon.r, neon.g, neon.b, 0.35f);

        if (!field.TryGetComponent(out NeonButtonFx fx)) fx = field.gameObject.AddComponent<NeonButtonFx>();
        fx.Setup(frame, neon, false);
    }

    /// <summary>Fundo de painel no tom espacial do jogo. Painéis que não ocupam a tela toda ganham moldura neon.</summary>
    public static void StylePanel(Image panel, Color accent, float alpha, NeonAssetProvider assets)
    {
        if (panel == null) return;
        if (assets == null) assets = NeonAssetProvider.Runtime;

        panel.color = new Color(DeepSpace.r, DeepSpace.g, DeepSpace.b, alpha);

        if (!IsFullScreen(panel.rectTransform))
        {
            panel.sprite = assets.FillSprite;
            panel.type = Image.Type.Sliced;
            panel.fillCenter = true;
            panel.pixelsPerUnitMultiplier = 1f;
            EnsureFrame(panel.rectTransform, accent, assets, 1f);
        }
    }

    /// <summary>Estiliza uma tela inteira (painel + todos os botões, campos e textos dentro dele).</summary>
    public static void StyleScreen(Transform root, NeonAssetProvider assets)
    {
        if (root == null) return;
        if (assets == null) assets = NeonAssetProvider.Runtime;

        if (root.name.EndsWith("Panel") && root.TryGetComponent(out Image panelImg))
        {
            Color accent = PanelAccentFor(root.name, out float alpha);
            StylePanel(panelImg, accent, alpha, assets);
        }

        foreach (Button b in root.GetComponentsInChildren<Button>(true))
            StyleButton(b, ButtonColorFor(b.name), assets);

        foreach (TMP_InputField f in root.GetComponentsInChildren<TMP_InputField>(true))
        {
            Transform panel = FindPanelAncestor(f.transform);
            Color accent = panel != null ? PanelAccentFor(panel.name, out _) : Cyan;
            StyleInputField(f, accent, assets);
        }

        foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (t.GetComponentInParent<Selectable>(true) != null) continue; // já estilizado com o botão/campo
            if (t.GetComponentInParent<TMP_InputField>(true) != null) continue;
            StyleTextByRole(t, assets);
        }
    }

    // ---------------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------------

    public static Image EnsureFrame(RectTransform target, Color neon, NeonAssetProvider assets, float mult)
    {
        Transform existing = target.Find(FrameName);
        GameObject go;
        if (existing != null)
        {
            go = existing.gameObject;
        }
        else
        {
            go = new GameObject(FrameName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = target.gameObject.layer;
            go.transform.SetParent(target, false);
        }
        go.transform.SetAsFirstSibling(); // atrás do texto, na frente do fundo do botão

        RectTransform frt = (RectTransform)go.transform;
        float pad = FrameGlowPad / mult;
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.pivot = new Vector2(0.5f, 0.5f);
        frt.offsetMin = new Vector2(-pad, -pad);
        frt.offsetMax = new Vector2(pad, pad);
        frt.localScale = Vector3.one;

        Image img = go.GetComponent<Image>();
        img.sprite = assets.FrameSprite;
        img.type = Image.Type.Sliced;
        img.fillCenter = true;
        img.pixelsPerUnitMultiplier = mult;
        img.color = neon;
        img.raycastTarget = false;
        return img;
    }

    private static ColorBlock MakeColorBlock(Color neon, float alpha)
    {
        Color dark = DarkFill(alpha);
        ColorBlock cb = ColorBlock.defaultColorBlock;
        cb.normalColor      = dark;
        cb.highlightedColor = Color.Lerp(dark, neon, 0.28f);
        cb.pressedColor     = Color.Lerp(dark, neon, 0.55f);
        cb.selectedColor    = dark;
        cb.disabledColor    = new Color(0.12f, 0.12f, 0.16f, 0.5f);
        cb.colorMultiplier  = 1f;
        cb.fadeDuration     = 0.08f;
        return cb;
    }

    private static Color DarkFill(float alpha) => new Color(0.05f, 0.04f, 0.13f, alpha);

    private static float MultiplierFor(RectTransform rt)
    {
        float h = rt.rect.height;
        return (h > 0f && h < 40f) ? 2f : 1f; // botões pequenos (ex.: PAUSE) usam moldura mais fina
    }

    private static bool IsFullScreen(RectTransform rt)
    {
        return rt.anchorMin.sqrMagnitude < 0.0001f
            && (rt.anchorMax - Vector2.one).sqrMagnitude < 0.0001f
            && rt.offsetMin.sqrMagnitude < 1f
            && rt.offsetMax.sqrMagnitude < 1f;
    }

    private static Transform FindPanelAncestor(Transform t)
    {
        for (Transform p = t.parent; p != null; p = p.parent)
            if (p.name.EndsWith("Panel")) return p;
        return null;
    }

    private static void DestroySafe(Object o)
    {
        if (Application.isPlaying) Object.Destroy(o);
        else Object.DestroyImmediate(o);
    }

    // ---------------------------------------------------------------------------------------------
    // Geração procedural de assets (usada em runtime e pelo script de Editor, que salva em disco)
    // ---------------------------------------------------------------------------------------------

    public static string MaterialName(TMP_FontAsset font, Color neon, Glow glow)
    {
        return $"{font.name} - Neon {ColorUtility.ToHtmlStringRGB(neon)} {glow}";
    }

    public static Material CreateTextMaterial(TMP_FontAsset font, Color neon, Glow glow)
    {
        if (font == null || font.material == null) return null;

        Material mat = new Material(font.material);
        Shader sdf = Shader.Find("TextMeshPro/Distance Field"); // suporta Glow (o Mobile não)
        if (sdf != null) mat.shader = sdf;

        bool strong = glow == Glow.Strong;
        mat.SetColor("_FaceColor", Color.white);

        if (mat.HasProperty("_OutlineWidth"))
        {
            mat.SetFloat("_OutlineWidth", strong ? 0.14f : 0.1f);
            mat.SetColor("_OutlineColor", neon);
        }

        if (mat.HasProperty("_GlowColor"))
        {
            mat.EnableKeyword("GLOW_ON");
            mat.SetColor("_GlowColor", new Color(neon.r, neon.g, neon.b, strong ? 0.85f : 0.6f));
            mat.SetFloat("_GlowOffset", 0f);
            mat.SetFloat("_GlowInner", strong ? 0.08f : 0.05f);
            mat.SetFloat("_GlowOuter", strong ? 0.5f : 0.3f);
            mat.SetFloat("_GlowPower", strong ? 0.55f : 0.75f);
        }
        else if (mat.HasProperty("_UnderlayColor"))
        {
            // Fallback (shader Mobile): underlay suave centralizado imita o brilho
            mat.EnableKeyword("UNDERLAY_ON");
            mat.SetColor("_UnderlayColor", new Color(neon.r, neon.g, neon.b, strong ? 0.8f : 0.55f));
            mat.SetFloat("_UnderlayOffsetX", 0f);
            mat.SetFloat("_UnderlayOffsetY", 0f);
            mat.SetFloat("_UnderlayDilate", strong ? 0.6f : 0.35f);
            mat.SetFloat("_UnderlaySoftness", strong ? 0.9f : 0.7f);
        }

        ShaderUtilities.UpdateShaderRatios(mat);
        mat.name = MaterialName(font, neon, glow);
        return mat;
    }

    /// <summary>Moldura: linha fina arredondada + halo suave (branca; a cor vem do Image.color).</summary>
    public static Texture2D GenerateFrameTexture()
    {
        int s = FrameTexSize;
        float half = s * 0.5f;
        float h = half - FrameGlowPad - LineThickness * 0.5f;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
        {
            name = "NeonFrame", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
        };
        var px = new Color32[s * s];
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            Vector2 p = new Vector2(x + 0.5f - half, y + 0.5f - half);
            float sd = RoundedRectSD(p, new Vector2(h, h), LineRadius);
            float d = Mathf.Abs(sd) - LineThickness * 0.5f;          // distância até a borda da linha
            float core = Mathf.Clamp01(0.5f - d);                     // linha com anti-aliasing
            float glow = (sd > 0f ? 0.55f : 0.3f) * Mathf.Exp(-Mathf.Max(0f, d) / 3.2f);
            float a = Mathf.Max(core, glow);
            px[y * s + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f));
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    /// <summary>Fundo arredondado com o mesmo raio da moldura.</summary>
    public static Texture2D GenerateFillTexture()
    {
        int s = FillTexSize;
        float half = s * 0.5f;
        float radius = LineRadius + LineThickness * 0.5f;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false)
        {
            name = "NeonFill", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
        };
        var px = new Color32[s * s];
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            Vector2 p = new Vector2(x + 0.5f - half, y + 0.5f - half);
            float a = Mathf.Clamp01(0.5f - RoundedRectSD(p, new Vector2(half, half), radius));
            px[y * s + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    public static Sprite CreateSlicedSprite(Texture2D tex, int border)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0,
                             SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    /// <summary>Coração neon nítido: contorno definido, preenchimento interno translúcido e brilho neon suave.</summary>
    public static Texture2D GenerateHeartTexture(int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "NeonHeart", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
        };
        var px = new Color32[size * size];
        float scale = size * (30f / 64f);
        float centerX = (size * 0.5f) - 0.5f;
        float bottomTipY = size * (17f / 64f);

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            // No Unity Texture2D: y=0 é a base, y=size-1 é o topo
            float iqX = (x - centerX) / scale;
            float iqY = (y - bottomTipY) / scale;

            float sd = IqHeartSD(new Vector2(iqX, iqY));
            float pixelDist = sd * scale;

            // Contorno neon nítido
            float lineDist = Mathf.Abs(pixelDist) - 1.2f;
            float lineCore = Mathf.Clamp01(0.5f - lineDist);

            // Preenchimento interno sutil
            float fill = pixelDist < 0f ? 0.4f : 0f;

            // Halo suave externo
            float glow = pixelDist > 0f ? 0.5f * Mathf.Exp(-pixelDist / 2.5f) : 0f;

            float alpha = Mathf.Clamp01(Mathf.Max(lineCore, Mathf.Max(fill, glow)));
            float whiteness = Mathf.Clamp01(1f - Mathf.Abs(pixelDist) / 1.0f);
            byte col = (byte)Mathf.RoundToInt(Mathf.Lerp(220f, 255f, whiteness));
            byte a = (byte)Mathf.RoundToInt(alpha * 255f);

            px[y * size + x] = new Color32(col, col, col, a);
        }
        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Exact SDF for a heart (Inigo Quilez standard formulation).
    /// </summary>
    private static float IqHeartSD(Vector2 p)
    {
        p.x = Mathf.Abs(p.x);

        if (p.y + p.x > 1.0f)
        {
            return (p - new Vector2(0.25f, 0.75f)).magnitude - Mathf.Sqrt(2f) / 4.0f;
        }

        Vector2 q1 = p - new Vector2(0.0f, 1.0f);
        Vector2 q2 = p - 0.5f * Mathf.Max(p.x + p.y, 0.0f) * Vector2.one;
        return Mathf.Sqrt(Mathf.Min(q1.sqrMagnitude, q2.sqrMagnitude)) * Mathf.Sign(p.x - p.y);
    }

    private static float RoundedRectSD(Vector2 p, Vector2 halfSize, float r)
    {
        Vector2 q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - halfSize + new Vector2(r, r);
        return new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - r;
    }

    /// <summary>Símbolo universal de pause: duas barras verticais paralelas com contorno, preenchimento e halo neon.</summary>
    public static Texture2D GeneratePauseIconTexture(int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "NeonPause", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
        };
        var px = new Color32[size * size];

        float halfW = 3.5f;
        float halfH = 13.0f;
        float radius = 1.8f;
        float spacing = 8.5f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float pxCoord = x - 31.5f;
            float pyCoord = y - 31.5f;

            // Barra esquerda
            float pLeftX = Mathf.Abs(pxCoord - (-spacing));
            float pLeftY = Mathf.Abs(pyCoord);
            float qx = Mathf.Max(0f, pLeftX - (halfW - radius));
            float qy = Mathf.Max(0f, pLeftY - (halfH - radius));
            float sdLeft = Mathf.Sqrt(qx * qx + qy * qy) - radius;
            if (pLeftX <= (halfW - radius) && pLeftY <= (halfH - radius))
                sdLeft = Mathf.Max(pLeftX - halfW, pLeftY - halfH);

            // Barra direita
            float pRightX = Mathf.Abs(pxCoord - spacing);
            float pRightY = Mathf.Abs(pyCoord);
            float rx = Mathf.Max(0f, pRightX - (halfW - radius));
            float ry = Mathf.Max(0f, pRightY - (halfH - radius));
            float sdRight = Mathf.Sqrt(rx * rx + ry * ry) - radius;
            if (pRightX <= (halfW - radius) && pRightY <= (halfH - radius))
                sdRight = Mathf.Max(pRightX - halfW, pRightY - halfH);

            float sd = Mathf.Min(sdLeft, sdRight);

            // Anti-aliasing e glow
            float lineDist = Mathf.Abs(sd) - 1.0f;
            float lineCore = Mathf.Clamp01(0.5f - lineDist);
            float fill = sd < 0f ? 0.85f : 0f;
            float glow = sd > 0f ? 0.6f * Mathf.Exp(-sd / 2.2f) : 0f;

            float alpha = Mathf.Clamp01(Mathf.Max(lineCore, Mathf.Max(fill, glow)));
            float whiteness = Mathf.Clamp01(1f - Mathf.Abs(sd) / 1.0f);
            byte col = (byte)Mathf.RoundToInt(Mathf.Lerp(220f, 255f, whiteness));
            byte a = (byte)Mathf.RoundToInt(alpha * 255f);

            px[y * size + x] = new Color32(col, col, col, a);
        }

        tex.SetPixels32(px);
        tex.Apply();
        return tex;
    }
}

/// <summary>
/// Fornece sprites e materiais do estilo neon. A versão padrão (Runtime) gera tudo em memória;
/// o script de Editor usa uma subclasse que salva os assets no projeto (para ficarem serializados nas cenas).
/// </summary>
public class NeonAssetProvider
{
    public static readonly NeonAssetProvider Runtime = new NeonAssetProvider();

    private Sprite _frame;
    private Sprite _fill;
    private Sprite _heart;
    private Sprite _pauseIcon;
    private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();

    public virtual Sprite FrameSprite
    {
        get
        {
            if (_frame == null) _frame = NeonUIStyle.CreateSlicedSprite(NeonUIStyle.GenerateFrameTexture(), NeonUIStyle.FrameBorder);
            return _frame;
        }
    }

    public virtual Sprite FillSprite
    {
        get
        {
            if (_fill == null) _fill = NeonUIStyle.CreateSlicedSprite(NeonUIStyle.GenerateFillTexture(), NeonUIStyle.FillBorder);
            return _fill;
        }
    }

    public virtual Sprite HeartSprite
    {
        get
        {
            if (_heart == null)
            {
                Texture2D tex = NeonUIStyle.GenerateHeartTexture(64);
                _heart = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            return _heart;
        }
    }

    public virtual Sprite PauseIconSprite
    {
        get
        {
            if (_pauseIcon == null)
            {
                Texture2D tex = NeonUIStyle.GeneratePauseIconTexture(64);
                _pauseIcon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
            return _pauseIcon;
        }
    }

    public virtual Material GetTextMaterial(TMP_FontAsset font, Color neon, NeonUIStyle.Glow glow)
    {
        if (font == null) return null;
        string key = NeonUIStyle.MaterialName(font, neon, glow);
        if (!_materials.TryGetValue(key, out Material mat) || mat == null)
        {
            mat = NeonUIStyle.CreateTextMaterial(font, neon, glow);
            _materials[key] = mat;
        }
        return mat;
    }
}
