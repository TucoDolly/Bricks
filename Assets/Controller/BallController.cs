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
        if (_launched)
        {
            // Bola caiu abaixo da tela: volta para a raquete
            Camera cam = Camera.main;
            if (cam != null && _playerTransform != null &&
                transform.position.y < cam.transform.position.y - cam.orthographicSize)
            {
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
        _ballModel.direction = Vector2.Reflect(_ballModel.direction, collision.contacts[0].normal);
        AngleChange(_ballModel.direction);

    }

    public void AngleChange(Vector2 dir)
    {
        _ballModel.direction = dir;
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }


    public void CalcBallAngleReflect(Collision2D playerCol)
    {
        // Quantidade de pixels do player
        float playerPixels = 120f;
        //Nº de pixels do player, divido por dois, dividido por 100 para enquadrar na escal da Unity
        float unityScaledHalfPlayerPixels = playerPixels / 2f / 100f;
        //Fator de conversão de 180 graus para 120 pixels
        float scaleFactorToPutIn1dot8Range = 1.5f;
        //Angulo calculado em Degree, na escla da unit, e transformado
        //para a escala de 0 a 180
        float angleDegUnityScale = (playerCol.transform.position.x - transform.position.x 
                                  + unityScaledHalfPlayerPixels) * scaleFactorToPutIn1dot8Range;
        //Transformei o valor da escala da unit(Que era dividido por 100), para angulo em Deg convencional
        float angleDeg = angleDegUnityScale * 100f;
        if (angleDeg < 25f)
        {
            angleDeg = 25f;
        }else if (angleDeg >155f)
        {
            angleDeg = 155f;
        }
        //Conversão matemática do angulo em Degree para Radian
        float angleRad = angleDeg * Mathf.PI / 180f;
        _ballModel.direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        AngleChange(_ballModel.direction);
    }

}
