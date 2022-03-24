using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLauncher : MonoBehaviour
{
    public GameObject Ball;
    [SerializeField] private float ballSpeed;
    public GameObject instanceBall;
    [SerializeField] private Material ColorYellow;
    [SerializeField] private Material ColorRed;
    [SerializeField] private Material ColorGreen;
    [SerializeField] private Material ColorBlue;
    [SerializeField] float m_CoolDownDuration;
    float m_NextShootTime;
    [SerializeField] Transform shootEndPosition;
    Vector3 shootDir;
    int ballColor;
    private void Start()
    {
        m_NextShootTime = Time.time;
        CreateBall();
    }
    private void Update()
    {
        SetBallPostion();
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time > m_NextShootTime)
        {
            Shoot();
            CreateBall();
            m_NextShootTime = Time.time + m_CoolDownDuration;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ChangeColor();
        }
        
    }

    private void Shoot()
    {
        shootDir = (shootEndPosition.position - instanceBall.transform.position).normalized;
        instanceBall.GetComponent<Ball>().Setup(shootDir);

    }

    private void SetBallPostion()
    {
        instanceBall.transform.forward = transform.forward;
        instanceBall.transform.position = transform.position + transform.right * 2.0f;
    }

    private void ChangeColor()
    {
        ballColor = ballColor + 1;
        if (System.Enum.GetNames(typeof(BallColor)).Length == ballColor)
        {
            ballColor = 0;
        }

        SetBallColor(instanceBall, ballColor);
    }
    private void CreateBall()
    {
        instanceBall = Instantiate(Ball, transform.position, Quaternion.identity);
        instanceBall.SetActive(true);

        instanceBall.tag = "NewBall";

        SetRandomBallColor(instanceBall);
    }

    public enum BallColor
    {
        red,
        green,
        blue,
        yellow
    }
    private void SetRandomBallColor(GameObject ball)
    {
        ballColor = Random.Range(0, 4);
        SetBallColor(ball, ballColor);
    }
    private void SetBallColor(GameObject ball,int indexColor)
    {
        BallColor randomColor = (BallColor)indexColor;
        switch (randomColor)
        {
            case BallColor.red:
                ball.GetComponent<Renderer>().material.SetColor("_Color", ColorRed.color);
                break;

            case BallColor.green:
                ball.GetComponent<Renderer>().material.SetColor("_Color", ColorGreen.color);
                break;

            case BallColor.blue:
                ball.GetComponent<Renderer>().material.SetColor("_Color", ColorBlue.color);
                break;

            case BallColor.yellow:
                ball.GetComponent<Renderer>().material.SetColor("_Color", ColorYellow.color);
                break;
        }
    }

}
