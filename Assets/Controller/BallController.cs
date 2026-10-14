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

    private Vector2 _lastStuckCheckPos;
    private float _stuckTimer = 0f;

    void Start()
    {
        _ballModel   = GetComponent<BallModel>();
        _rigidBody2d = GetComponent<Rigidbody2D>();

        // Material 2D sem atrito (friction = 0) e elástico (bounciness = 1)
        // Impede que a física do Unity faça a bola desacelerar ou prender ao roçar em quinas/tijolos
        PhysicsMaterial2D frictionless = new PhysicsMaterial2D("BallFrictionlessMaterial")
        {
            friction = 0f,
            bounciness = 1f
        };
        if (_rigidBody2d != null) _rigidBody2d.sharedMaterial = frictionless;
        var circleCol = GetComponent<CircleCollider2D>();
        if (circleCol != null) circleCol.sharedMaterial = frictionless;

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

        bool pressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (pressed) Launch();
    }

    public void ResetBall()
    {
        _launched = false;
        _stuckTimer = 0f;
        _rigidBody2d.linearVelocity = Vector2.zero;
        if (_playerTransform != null)
        {
            _rigidBody2d.position = (Vector2)_playerTransform.position + _offsetFromPlayer;
        }
    }

    void FixedUpdate()
    {
        // Antes do lançamento, a bola acompanha a raquete
        if (!_launched)
        {
            if (_playerTransform != null)
            {
                _rigidBody2d.position = (Vector2)_playerTransform.position + _offsetFromPlayer;
                _rigidBody2d.linearVelocity = Vector2.zero;
            }
            return;
        }

        if (Time.timeScale == 0f) return;

        // Mantém a velocidade constante cravada (evita que atrito ou colisão dupla parem a bola)
        float targetSpeed = (_ballModel != null && _ballModel.speed > 0f) ? _ballModel.speed : 8f;
        float currentSpeed = _rigidBody2d.linearVelocity.magnitude;

        if (currentSpeed < targetSpeed * 0.7f || _rigidBody2d.linearVelocity.sqrMagnitude < 0.01f)
        {
            Vector2 dir = (_ballModel != null && _ballModel.direction.sqrMagnitude > 0.01f) ? _ballModel.direction : Vector2.down;
            _rigidBody2d.linearVelocity = dir.normalized * targetSpeed;
        }
        else
        {
            _rigidBody2d.linearVelocity = _rigidBody2d.linearVelocity.normalized * targetSpeed;
        }

        if (_ballModel != null)
        {
            _ballModel.direction = _rigidBody2d.linearVelocity.normalized;
        }

        // Watchdog anti-travamento: detecta se a bola ficou confinada por mais de 0.35s
        CheckAntiStuck();
    }

    public void Launch()
    {
        if (_launched) return;
        _launched = true;
        _stuckTimer = 0f;
        _lastStuckCheckPos = _rigidBody2d.position;

        // A cena serializou speed = 0; usa um valor padrão para a bola poder se mover
        if (_ballModel.speed <= 0f) _ballModel.speed = 8f;

        float angle = UnityEngine.Random.Range(-_launchAngleRange, _launchAngleRange) * Mathf.Deg2Rad;
        _ballModel.direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)).normalized;
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }

    private void CheckAntiStuck()
    {
        // Só verifica travamento se a bola estiver realmente quase parada (velocidade linear muito baixa)
        // ou se estiver presa colidindo continuamente com tijolos.
        // Se a bola está voando livre pelo ar (velocidade normal), NUNCA deve alterar a direção!
        if (_rigidBody2d.linearVelocity.magnitude < 1f)
        {
            _stuckTimer += Time.fixedDeltaTime;
            if (_stuckTimer >= 0.5f)
            {
                UnstuckBall();
                _stuckTimer = 0f;
            }
        }
        else
        {
            _stuckTimer = 0f;
        }
    }

    public void UnstuckBall()
    {
        // Só atua se houver tijolos bloqueando a bola
        Collider2D[] colliders = Physics2D.OverlapCircleAll(_rigidBody2d.position, 0.4f);
        Vector2 escapeDir = Vector2.zero;
        int bricksFound = 0;

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject || col.isTrigger) continue;

            if (col.TryGetComponent<BrickView>(out var brickView))
            {
                brickView.PerformTakeDamage(1f);
                Vector2 away = ((Vector2)transform.position - (Vector2)col.bounds.center);
                if (away.sqrMagnitude > 0.001f)
                {
                    escapeDir += away.normalized;
                    bricksFound++;
                }
            }
        }

        if (bricksFound > 0)
        {
            escapeDir = escapeDir.sqrMagnitude > 0.01f ? escapeDir.normalized : Vector2.down;
            escapeDir = EnsureMinimumAngle(escapeDir);
            _rigidBody2d.position += escapeDir * 0.1f;
            AngleChange(escapeDir);
        }
        else
        {
            // Se não está perto de nenhum tijolo, apenas restaura a direção e velocidade corretas
            Vector2 dir = _ballModel != null && _ballModel.direction.sqrMagnitude > 0.01f ? _ballModel.direction : Vector2.down;
            AngleChange(dir);
        }
    }

    public void PerfectAngleReflect(Collision2D collision)
    {
        Vector2 normal = Vector2.zero;
        int count = collision.contactCount;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                normal += collision.GetContact(i).normal;
            }
            normal.Normalize();
        }

        if (normal == Vector2.zero)
        {
            normal = ((Vector2)transform.position - (Vector2)collision.transform.position).normalized;
            if (normal == Vector2.zero) normal = Vector2.down;
        }

        string objName = collision.gameObject.name;

        // Paredes laterais: a colisão é estritamente horizontal!
        // Inverte apenas o eixo X e NUNCA inverte o eixo Y (se estava descendo, continua descendo!).
        if (objName == "LeftWall" || objName == "RightWall" || (Mathf.Abs(normal.x) > 0.7f && Mathf.Abs(normal.y) < 0.3f))
        {
            float targetSignX = objName == "LeftWall" ? 1f : (objName == "RightWall" ? -1f : (normal.x >= 0 ? 1f : -1f));
            Vector2 dir = _ballModel.direction;
            dir.x = Mathf.Abs(dir.x) * targetSignX;
            _ballModel.direction = dir.normalized;
        }
        // Parede superior (ou impacto puramente vertical vindo de cima)
        else if (objName == "TopWall" || (Mathf.Abs(normal.y) > 0.7f && Mathf.Abs(normal.x) < 0.3f))
        {
            float targetSignY = normal.y >= 0 ? 1f : -1f;
            Vector2 dir = _ballModel.direction;
            dir.y = Mathf.Abs(dir.y) * targetSignY;
            _ballModel.direction = dir.normalized;
        }
        else
        {
            // Tijolos ou quinas: reflexão padrão usando a normal de contato
            if (Vector2.Dot(_ballModel.direction, normal) < 0f)
            {
                _ballModel.direction = Vector2.Reflect(_ballModel.direction, normal).normalized;
            }
            else
            {
                // Se já estiver se afastando, apenas garante que a velocidade vá no sentido para fora
                _ballModel.direction = Vector2.Reflect(-_ballModel.direction, normal).normalized;
            }
        }

        _ballModel.direction = EnsureMinimumAngle(_ballModel.direction);
        AngleChange(_ballModel.direction);
    }

    private Vector2 EnsureMinimumAngle(Vector2 dir)
    {
        // Garante que o sinal de Y e X originais seja rigorosamente preservado ao aplicar o ângulo mínimo!
        if (Mathf.Abs(dir.y) < 0.25f)
        {
            float signY = dir.y >= 0f ? 1f : -1f;
            dir.y = 0.25f * signY;
            dir = dir.normalized;
        }

        if (Mathf.Abs(dir.x) < 0.25f)
        {
            float signX = dir.x >= 0f ? 1f : -1f;
            dir.x = 0.25f * signX;
            dir = dir.normalized;
        }

        return dir;
    }

    public void AngleChange(Vector2 dir)
    {
        if (_ballModel != null)
        {
            _ballModel.direction = dir.normalized;
            float speed = _ballModel.speed > 0f ? _ballModel.speed : 8f;
            _rigidBody2d.linearVelocity = _ballModel.direction * speed;
        }
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
