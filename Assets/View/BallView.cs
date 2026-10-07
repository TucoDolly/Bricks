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

    private float _lastHitFixedTime = -1f;


    public void OnCollisionEnter2D(Collision2D collision)
    {
        // Evita quebrar 2 tijolos de uma vez
        if (Mathf.Approximately(_lastHitFixedTime, Time.fixedTime)) return;
        _lastHitFixedTime = Time.fixedTime;

        if (collision.gameObject.TryGetComponent(out BrickView _brickView))
        {
            AudioController.Instance.PlayBrickHit();
            // A pontuação agora é gerenciada dentro do BrickController para não dar conflito no Restart
            _brickView.PerformTakeDamage(1f);
        }

        if (!collision.gameObject.TryGetComponent<PlayerController>(out _))
        {
            if (_brickView == null) AudioController.Instance.PlayBallHit();
            _ballController.PerfectAngleReflect(collision);
        }
        else
        {
            AudioController.Instance.PlayPlayerHit();
            _ballController.CalcBallAngleReflect(collision);
            if (GameManager.Instance != null) GameManager.Instance.AddScore(-2);
        }
    }
}
