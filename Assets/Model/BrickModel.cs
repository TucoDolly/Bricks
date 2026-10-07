using UnityEngine;

public class BrickModel : MonoBehaviour
{

    [SerializeField] private float _health;
    [SerializeField] private int _points;

    public float Health { get => _health; set => _health = value; }
    public int Points { get => _points; set => _points = value; }


}
