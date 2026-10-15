using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Micro-animação neon dos botões: a moldura "respira" levemente e acende ao passar o mouse,
/// com um pequeno zoom. Usa tempo não escalado, pois as telas aparecem com Time.timeScale = 0.
/// Agora inclui efeitos sonoros táteis ao passar o mouse e ao clicar.
/// </summary>
[DisallowMultipleComponent]
public class NeonButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image frame;
    [SerializeField] private Color neonColor = new Color(0f, 0.9f, 1f);
    [SerializeField] private bool scaleOnHover = true;
    [SerializeField, Range(0f, 1f)] private float idleAlpha = 0.7f;
    [SerializeField] private float hoverScale = 1.05f;

    private Selectable _selectable;
    private Vector3 _baseScale = Vector3.one;
    private bool _hovered;
    private float _t;

    public void Setup(Image frameImage, Color color, bool scale)
    {
        frame = frameImage;
        neonColor = color;
        scaleOnHover = scale;
    }

    private void Awake()
    {
        _baseScale = transform.localScale;
        TryGetComponent(out _selectable);
    }

    private void OnEnable()
    {
        _hovered = false;
        _t = 0f;
        Apply(0f);
    }

    private void OnDisable()
    {
        _hovered = false;
        _t = 0f;
        transform.localScale = _baseScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        if (_selectable == null || _selectable.IsInteractable())
        {
            AudioController.Instance?.PlayButtonHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData) => _hovered = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData != null && eventData.button != PointerEventData.InputButton.Left) return;
        if (_selectable == null || _selectable.IsInteractable())
        {
            AudioController.Instance?.PlayButtonClick();
        }
    }

    private void Update()
    {
        bool active = _hovered && (_selectable == null || _selectable.IsInteractable());
        _t = Mathf.MoveTowards(_t, active ? 1f : 0f, Time.unscaledDeltaTime * 8f);
        Apply(Mathf.SmoothStep(0f, 1f, _t));
    }

    private void Apply(float k)
    {
        if (frame != null)
        {
            float breathe = 0.9f + 0.1f * Mathf.Sin(Time.unscaledTime * 2.4f);
            Color c = neonColor;
            c.a = Mathf.Lerp(idleAlpha * breathe, 1f, k);
            frame.color = c;
        }

        if (scaleOnHover)
            transform.localScale = _baseScale * Mathf.Lerp(1f, hoverScale, k);
    }
}
