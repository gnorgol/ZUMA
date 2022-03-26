using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLauncher : MonoBehaviour
{
    public GameObject Ball;
    [SerializeField] private float ballSpeed;
    private GameObject instanceBallForward;
    private GameObject instanceBallBack;
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
        CreateBallForward();
        CreateBallBack();
    }
    private void Update()
    {
        SetBallPostion();
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time > m_NextShootTime)
        {
            Shoot();
            CreateBallForward();
            SwapBall();
            m_NextShootTime = Time.time + m_CoolDownDuration;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            //ChangeColor();
            SwapBall();
        }
        
    }

    private void Shoot()
    {
        shootDir = (shootEndPosition.position - instanceBallForward.transform.position).normalized;
        instanceBallForward.GetComponent<Ball>().Setup(shootDir);

    }
    //Fonction to swap the instanceBallForward and instanceBallBack
    private void SwapBall()
    {
        GameObject temp = instanceBallForward;
        instanceBallForward = instanceBallBack;
        instanceBallBack = temp;
    }
    private void SetBallPostion()
    {
        //Set the instanceBallForward in front of the player
        instanceBallForward.transform.forward = transform.forward;
        instanceBallForward.transform.position = transform.position + transform.right * 2.0f;
        //Set the instanceBallBack in back of the player
        instanceBallBack.transform.forward = transform.forward;
        instanceBallBack.transform.position = transform.position - transform.right * 2.0f;
    }

    private void ChangeColor()
    {
        ballColor = ballColor + 1;
        if (System.Enum.GetNames(typeof(BallColor)).Length == ballColor)
        {
            ballColor = 0;
        }
        SetBallColor(instanceBallForward, ballColor);
        //if instanceBallBack not set , set is ball color
        if (instanceBallBack != null)
        {
            SetBallColor(instanceBallBack, ballColor);
        }
    }
    private void CreateBallForward()
    {
        instanceBallForward = Instantiate(Ball, transform.position, Quaternion.identity);
        instanceBallForward.SetActive(true);
        instanceBallForward.tag = "NewBall";
        SetRandomBallColor(instanceBallForward);
    }
    private void CreateBallBack()
    {
        instanceBallBack = Instantiate(Ball, transform.position, Quaternion.identity);
        instanceBallBack.SetActive(true);
        instanceBallBack.tag = "NewBall";
        SetRandomBallColor(instanceBallBack);
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
