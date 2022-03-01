using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLauncher : MonoBehaviour
{
	public GameObject Ball;
	public float ballSpeed = 10;
	public GameObject instanceBall;


	private void Start()
	{
		CreateBall();
	}
	private void Update()
	{
		SetBallPostion();

		//shoot ball
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			instanceBall.GetComponent<Rigidbody>().AddForce(instanceBall.transform.forward * ballSpeed);
			CreateBall();
		}

	}

	private void SetBallPostion()
	{
		instanceBall.transform.forward = transform.forward;
		instanceBall.transform.position = transform.position + transform.forward * transform.localScale.z;
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
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                break;

            case BallColor.green:
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                break;

            case BallColor.blue:
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
                break;

            case BallColor.yellow:
                go.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
                break;
        }
    }

}
