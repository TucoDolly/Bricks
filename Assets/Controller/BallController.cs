using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class BallController : MonoBehaviour
{

    private BallModel _ballModel;
    private Rigidbody2D _rigidBody2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ballModel   = GetComponent<BallModel>();
        _rigidBody2d = GetComponent<Rigidbody2D>();

        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }



    public void PerfectAngleReflect(Collision2D collision)
    {
        _ballModel.direction = Vector2.Reflect(_ballModel.direction, collision.contacts[0].normal);
        AngleChange(_ballModel.direction);

    }

    public void AngleChange(Vector2 dir)
    {
        _ballModel.direction = dir;
        _rigidBody2d.linearVelocity = _ballModel.direction * _ballModel.speed;
    }


    public void CalcBallAngleReflect(Collision2D playerCol)
    {
        // Quantidade de pixels do player
        float playerPixels = 120f;
        //Nº de pixels do player, divido por dois, dividido por 100 para enquadrar na escal da Unity
        float unityScaledHalfPlayerPixels = playerPixels / 2f / 100f;
        //Fator de conversão de 180 graus para 120 pixels
        float scaleFactorToPutIn1dot8Range = 1.5f;
        //Angulo calculado em Degree, na escla da unit, e transformado
        //para a escala de 0 a 180
        float angleDegUnityScale = (playerCol.transform.position.x - transform.position.x 
                                  + unityScaledHalfPlayerPixels) * scaleFactorToPutIn1dot8Range;
        //Transformei o valor da escala da unit(Que era dividido por 100), para angulo em Deg convencional
        float angleDeg = angleDegUnityScale * 100f;
        if (angleDeg < 25f)
        {
            angleDeg = 25f;
        }else if (angleDeg >155f)
        {
            angleDeg = 155f;
        }
        //Conversão matemática do angulo em Degree para Radian
        float angleRad = angleDeg * Mathf.PI / 180f;
        _ballModel.direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        AngleChange(_ballModel.direction);
    }

}
