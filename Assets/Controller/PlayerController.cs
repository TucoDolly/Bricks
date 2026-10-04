using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{

    private PlayerModel _playerModel;
    private Transform _playerTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerModel       = GetComponent<PlayerModel>();
        _playerTransform   = GetComponent<Transform>();
    }

    public void Move(float h)
    {
        if ((_playerTransform.position.x > -1.530f && h < 0f ) ||
            (_playerTransform.position.x < +1.530f && h > 0f ))  { 
        _playerTransform.Translate(h * _playerModel.speed, 0f, 0f);
        }
    }
}
