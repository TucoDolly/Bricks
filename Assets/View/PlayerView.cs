using UnityEngine;

public class PlayerView : MonoBehaviour
{

    private PlayerController _playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerController = GetComponent<PlayerController>(); 
    }

    // A leitura de input e o movimento agora são feitos no PlayerController (Input System + Rigidbody2D).
}
