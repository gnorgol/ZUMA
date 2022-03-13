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

            instanceBall.GetComponent<Rigidbody>().velocity = instanceBall.transform.forward * ballSpeed;
            CreateBall();

            m_NextShootTime = Time.time + m_CoolDownDuration;
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ChangeColor();
        }
    }
    private void SetBallPostion()
    {
        instanceBall.transform.forward = transform.forward;
        instanceBall.transform.position = transform.position + transform.forward * transform.localScale.z;
    }

    private void ChangeColor()
    {
        SetBallColor(instanceBall);
    }
    private void CreateBall()
    {
        instanceBall = Instantiate(Ball, transform.position, Quaternion.identity);
        instanceBall.SetActive(true);

        instanceBall.tag = "NewBall";

        SetBallColor(instanceBall);
    }

    public enum BallColor
    {
        red,
        green,
        blue,
        yellow
    }
    private void SetBallColor(GameObject go)
    {
        BallColor randColor = (BallColor)Random.Range(0, 4);

        switch (randColor)
        {
            case BallColor.red:
                go.GetComponent<Renderer>().material.SetColor("_Color", ColorRed.color);
                break;

            case BallColor.green:
                go.GetComponent<Renderer>().material.SetColor("_Color", ColorGreen.color);
                break;

            case BallColor.blue:
                go.GetComponent<Renderer>().material.SetColor("_Color", ColorBlue.color);
                break;

            case BallColor.yellow:
                go.GetComponent<Renderer>().material.SetColor("_Color", ColorYellow.color);
                break;
        }
    }

}
