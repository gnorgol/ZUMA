using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

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
    [SerializeField] List<Color> _ListColorsCurve;
    int ballColor;

    #region Event listener
    private void OnEnable()
    {
        SubscribeEvents();
    }
    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    private void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GameLevelChangedEvent>(GameLevelChanged);
        EventManager.Instance.AddListener<AllColorsBallsCurveEvent>(AllColorsBallsCurve);
        EventManager.Instance.AddListener<DestroyInstanceBallEvent>(DestroyInstanceBall);
    }
    private void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GameLevelChangedEvent>(GameLevelChanged);
        EventManager.Instance.RemoveListener<AllColorsBallsCurveEvent>(AllColorsBallsCurve);
        EventManager.Instance.RemoveListener<DestroyInstanceBallEvent>(DestroyInstanceBall);
    }


    public void DestroyInstanceBall(DestroyInstanceBallEvent e)
    {
        Destroy(instanceBallForward);
        Destroy(instanceBallBack);
    }
    private void AllColorsBallsCurve(AllColorsBallsCurveEvent e)
    {
        _ListColorsCurve = e.ListColorsCurve;
    }

    private void GameLevelChanged(GameLevelChangedEvent e)
    {
        //Reset Instance Ball after level change
        ResetInstanceBall();
    }
    #endregion

    private void Start()
    {
        m_NextShootTime = Time.time;
        

    }
    private void Update()
    {
        if (GameManager.Instance.IsPlaying)
        {
            SetBallPostion();
            if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time > m_NextShootTime)
            {
                Shoot();
                EventManager.Instance.Raise(new PlayerShootEvent());
                CreateBallForward();
                SwapBall();
                m_NextShootTime = Time.time + m_CoolDownDuration;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                //ChangeColor();
                SwapBall();
            }
            CheckColorInstanceBall();
        }
    }
    void CheckColorInstanceBall()
    {
        //If Color InstanceBallForward is not in ListColorsCurve then change color
        if (!_ListColorsCurve.Contains(instanceBallForward.GetComponent<Renderer>().material.color))
        {
            ChangeColor(instanceBallForward);
        }
        //If Color InstanceBallBack is not in ListColorsCurve then change color
        if (!_ListColorsCurve.Contains(instanceBallBack.GetComponent<Renderer>().material.color))
        {
            ChangeColor(instanceBallBack);
        }
    }
    void ResetInstanceBall()
    {
        //Destroy instanceBallForward and instanceBallBack
        Destroy(instanceBallForward);
        Destroy(instanceBallBack);
        //Re create instanceBallForward and instanceBallBack
        CreateBallForward();
        CreateBallBack();
        SetBallPostion();
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

    private void ChangeColor(GameObject Ball)
    {
        ballColor = ballColor + 1;
        if (System.Enum.GetNames(typeof(BallColor)).Length == ballColor)
        {
            ballColor = 0;
        }
        SetBallColor(Ball, ballColor);
        //if instanceBallBack not set , set is ball color
        /*        if (instanceBallBack != null)
                {
                    SetBallColor(instanceBallBack, ballColor);
                }*/
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
    private void SetBallColor(GameObject ball, int indexColor)
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
