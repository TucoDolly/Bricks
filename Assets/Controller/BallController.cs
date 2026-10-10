using System;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class BallController : MonoBehaviour
{

    private BallModel _ballModel;
    private Rigidbody2D _rigidBody2d;
    private Transform _playerTransform;
    private Vector2 _offsetFromPlayer;
    private bool _launched;

    // Ângulo máximo de desvio da vertical no lançamento (graus)
    [SerializeField] private float _launchAngleRange = 30f;

    void Start()
    {
        _ballModel   = GetComponent<BallModel>();
        _rigidBody2d = GetComponent<Rigidbody2D>();

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            _playerTransform  = player.transform;
            _offsetFromPlayer = transform.position - _playerTransform.position;
        }
        else
        {
            // Sem raquete na cena: lança imediatamente
            Launch();
            return;
        }

        _rigidBody2d.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (_launched)
        {
            // Bola caiu abaixo da tela: volta para a raquete
            Camera cam = Camera.main;
            if (cam != null && _playerTransform != null &&
                transform.position.y < cam.transform.position.y - cam.orthographicSize)
            {
                if (GameManager.Instance != null) GameManager.Instance.LoseLife();
                ResetBall();
            }
            return;
        }

        bool pressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                       (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
        if (pressed) Launch();
    }

    public void ResetBall()
    {
        _launched = false;
        _rigidBody2d.linearVelocity = Vector2.zero;
        _rigidBody2d.position = (Vector2)_playerTransform.position + _offsetFromPlayer;
    }

    void FixedUpdate()
    {
        // Antes do lançamento, a bola acompanha a raquete
        if (!_launched && _playerTransform != null)
        {
            _rigidBody2d.position = (Vector2)_playerTransform.position + _offsetFromPlayer;
            _rigidBody2d.linearVelocity = Vector2.zero;
        }
    }

    public void Launch()
    {
        if (_launched) return;
        _launched = true;

        // A cena serializou speed = 0; usa um valor padrão para a bola poder se mover
        if (_ballModel.speed <= 0f) _ballModel.speed = 8f;

        float angle = UnityEngine.Random.Range(-_launchAngleRange, _launchAngleRange) * Mathf.Deg2Rad;
        _ballModel.direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }



    public void PerfectAngleReflect(Collision2D collision)
    {
        _ballModel.direction = Vector2.Reflect(_ballModel.direction, collision.contacts[0].normal).normalized;
        
        // Evitar que a bola fique pingando apenas na horizontal infinitamente
        if (Mathf.Abs(_ballModel.direction.y) < 0.2f)
        {
            // Cópia local para alterar o struct
            Vector2 newDir = _ballModel.direction;
            float sign = newDir.y >= 0f ? 1f : -1f;
            newDir.y = 0.2f * sign;
            _ballModel.direction = newDir.normalized;
        }

        // Evitar que a bola fique pingando apenas na vertical infinitamente (grudada na parede)
        if (Mathf.Abs(_ballModel.direction.x) < 0.2f)
        {
            Vector2 newDir = _ballModel.direction;
            float sign = newDir.x >= 0f ? 1f : -1f;
            if (sign == 0f) sign = 1f;
            newDir.x = 0.2f * sign;
            _ballModel.direction = newDir.normalized;
        }

        AngleChange(_ballModel.direction);
    }

    public void AngleChange(Vector2 dir)
    {
        _ballModel.direction = dir;
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }


    // Ângulo mínimo em relação à horizontal (evita bola quase paralela à raquete)
    [SerializeField] private float _minPaddleAngle = 25f;

    public void CalcBallAngleReflect(Collision2D playerCol)
    {
        Bounds b = playerCol.collider.bounds;
        // -1 = ponta esquerda, 0 = centro, 1 = ponta direita
        float hit = Mathf.Clamp((transform.position.x - b.center.x) / b.extents.x, -1f, 1f);
        // Ângulo medido a partir da vertical: centro = reto, pontas = inclinado
        float maxFromVertical = 90f - _minPaddleAngle;
        float angleRad = hit * maxFromVertical * Mathf.Deg2Rad;
        _ballModel.direction = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad));
        AngleChange(_ballModel.direction);
    }

}
