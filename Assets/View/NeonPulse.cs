using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Faz um texto/gráfico neon pulsar suavemente, com uma "piscada" ocasional de letreiro neon.
/// Usa tempo não escalado (funciona nas telas de pause/vitória/game over com Time.timeScale = 0).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Graphic))]
public class NeonPulse : MonoBehaviour
{
    [SerializeField, Range(0f, 0.5f)] private float amplitude = 0.12f;
    [SerializeField] private float speed = 2.2f;
    [SerializeField] private bool flicker = true;

    private Graphic _graphic;
    private float _baseAlpha = 1f;
    private float _flickerUntil;
    private float _nextFlicker;

    private void Awake()
    {
        _graphic = GetComponent<Graphic>();
    }

    private void OnEnable()
    {
        if (_graphic == null) _graphic = GetComponent<Graphic>();
        _baseAlpha = _graphic.color.a;
        _nextFlicker = Time.unscaledTime + Random.Range(2.5f, 6f);
        _flickerUntil = 0f;
    }

    private void OnDisable()
    {
        if (_graphic == null) return;
        Color c = _graphic.color;
        c.a = _baseAlpha;
        _graphic.color = c;
    }

    private void Update()
    {
        float now = Time.unscaledTime;
        float k = 1f - amplitude * (0.5f + 0.5f * Mathf.Sin(now * speed));

        if (flicker)
        {
            if (now >= _nextFlicker)
            {
                _flickerUntil = now + Random.Range(0.06f, 0.14f);
                _nextFlicker = now + Random.Range(3f, 7f);
            }
            if (now < _flickerUntil) k *= 0.55f;
        }

        Color c = _graphic.color;
        c.a = _baseAlpha * k;
        _graphic.color = c;
    }
}
