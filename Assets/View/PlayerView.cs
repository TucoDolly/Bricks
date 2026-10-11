using UnityEngine;

public class PlayerView : MonoBehaviour
{

    private PlayerController _playerController;
    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && (sr.color == Color.white || sr.color == Color.clear))
        {
            sr.color = new Color(0.1f, 1f, 0.5f, 1f); // Verde Limão Neon (#1AFF80)
        }
    }

    void Start()
    {
        _playerController = GetComponent<PlayerController>(); 
    }

    // A leitura de input e o movimento agora são feitos no PlayerController (Input System + Rigidbody2D).
}
