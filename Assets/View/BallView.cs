using System.Collections;
using UnityEngine;
using TMPro;//1º

public class BallView : MonoBehaviour
{
    private BallController _ballController;
    private BallModel _ballModel;


    void Start()
    {
        _ballController =  GetComponent<BallController>();
        _ballModel      =  GetComponent<BallModel>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private float _lastBrickHitTime = -1f;

    public void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision, isEnter: true);
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        // Se a bola ficar presa ou em contato contínuo com obstáculo/tijolos, resolve imediatamente
        HandleCollision(collision, isEnter: false);
    }

    private void HandleCollision(Collision2D collision, bool isEnter)
    {
        bool isBrick = collision.gameObject.TryGetComponent(out BrickView brickView);

        if (isBrick)
        {
            // Causa dano no impacto inicial ou a cada intervalo mínimo se estiver em contato
            if (isEnter || (Time.fixedTime - _lastBrickHitTime >= 0.08f))
            {
                AudioController.Instance.PlayBrickHit();
                _lastBrickHitTime = Time.fixedTime;
                brickView.PerformTakeDamage(1f);
            }
        }

        if (!collision.gameObject.TryGetComponent<PlayerController>(out _))
        {
            if (!isBrick && isEnter) AudioController.Instance.PlayBallHit();
            _ballController.PerfectAngleReflect(collision);
        }
        else
        {
            if (isEnter)
            {
                AudioController.Instance.PlayPlayerHit();
                _ballController.CalcBallAngleReflect(collision);
                if (GameManager.Instance != null) GameManager.Instance.AddScore(-5);
            }
        }
    }
}
