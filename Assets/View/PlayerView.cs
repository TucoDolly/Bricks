using UnityEngine;

public class PlayerView : MonoBehaviour
{

    private PlayerController _playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerController = GetComponent<PlayerController>(); 
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        _playerController.Move(h);
    }
}
