using UnityEngine;

public class BallModel : MonoBehaviour
{

    [SerializeField] private float _power;
    [SerializeField] private float _speed;
    [SerializeField] private Vector2 _direction;
    [SerializeField] private int _pontuacao;

    public float power { get => _power; set => _power = value; }
    public float speed { get => _speed; set => _speed = value; }
    public Vector2 direction { get => _direction; set => _direction = value; }
    public int pontuacao { get => _pontuacao; set => _pontuacao = value;}

}
