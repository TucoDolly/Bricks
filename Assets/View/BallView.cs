using System.Collections;
using UnityEngine;
using TMPro;//1º

public class BallView : MonoBehaviour
{
    private BallController _ballController;
    private BallModel _ballModel;

    public TextMeshProUGUI TextoUIPontos;//2º

    void Start()
    {
        _ballController =  GetComponent<BallController>();
        _ballModel      =  GetComponent<BallModel>();
    }

    
    public void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Enemy")
        {
            BrickView _brickView = collision.gameObject.GetComponent<BrickView>(); 
            _brickView.PerformTakeDamage(1f);
            //Startar contador de pontuação
            _ballModel.pontuacao = _ballModel.pontuacao + 10;//3º
            //Atualizar em Tela
            TextoUIPontos.text = "Pontuação: " + _ballModel.pontuacao;//4º
        }

        if (collision.gameObject.tag != "Player")
        {
            _ballController.PerfectAngleReflect(collision);
        }
        else
        {
            _ballController.CalcBallAngleReflect(collision);
            //Startar contador de pontuação
            _ballModel.pontuacao = _ballModel.pontuacao - 2;//5º
            //Atualizar em Tela
            TextoUIPontos.text = "Pontuação: " + _ballModel.pontuacao;//6º
        }
    }
}
