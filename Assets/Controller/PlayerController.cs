using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    private PlayerModel _playerModel;
    private Transform _playerTransform;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private float _inputX;

    void Awake()
    {
        _playerModel       = GetComponent<PlayerModel>();
        _playerTransform   = GetComponent<Transform>();
        _rb                = GetComponent<Rigidbody2D>();
        _spriteRenderer    = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        _inputX = 0f;
        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            _inputX -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            _inputX += 1f;
    }

    void FixedUpdate()
    {
        // Unity 6: Rigidbody2D.velocity foi renomeado para linearVelocity
        _rb.linearVelocity = new Vector2(_inputX * _playerModel.moveSpeed, 0f);
    }

    void LateUpdate()
    {
        if (Time.timeScale == 0f) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth  = halfHeight * cam.aspect;
        float halfPaddle = _spriteRenderer.bounds.extents.x;
        float camX       = cam.transform.position.x;

        float minX = camX - halfWidth + halfPaddle;
        float maxX = camX + halfWidth - halfPaddle;

        Vector3 pos = _playerTransform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        _playerTransform.position = pos;
    }

    public void Move(float h)
    {
        if ((_playerTransform.position.x > -1.530f && h < 0f ) ||
            (_playerTransform.position.x < +1.530f && h > 0f ))  { 
        _playerTransform.Translate(h * _playerModel.speed, 0f, 0f);
        }
    }
}
