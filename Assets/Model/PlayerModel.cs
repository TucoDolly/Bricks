using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private int   _life;
    [SerializeField] private float _scale;
    public float moveSpeed = 10f;

    public float speed { get => _speed; set => _speed = value; }    
    public int life { get => _life; set => _life = value; }
    public float scale { get => _scale; set => _scale = value; }

}
