using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using Random = UnityEngine.Random;

public class RobotGameState : GameState
{
	public float SpawnRadius = .5f;
	public float MinRadius = .1f;
	public float SpawnHeight = 0.05f;
	public float HeightRange = .001f;
	public int Reward = 1;
	 
	public int Penalty = -1;
	public int FrameLimit = 5000;  // Number of frames before ending game. -1 means no games ever end.
	
	public GameObject Robot;
	public GameObject GBall;
	public GameObject BBall;
	public RobotControl RobotController;
	public BoxCollider SuckerCollider;

	private int _frameCount;
	
	// Use this for initialization
	void Start ()
	{
		// for(int i = 0; i < 1000; i++){SpawnBall(GBall);}
		SpawnBall(GBall);
		Score = -1000;
	}
	
	// Update is called once per frame
	void Update () {
		var good =  GameObject.FindGameObjectsWithTag("GoodBall");
		
		// The game ends if the number of good balls is 0
		IsOver = good.Length == 0 || (_frameCount >= FrameLimit && FrameLimit != -1);
		if (IsOver)
		{
			Debug.Log("Ending Game. Frames: " + FrameLimit + "Good balls: " + good.Length);
		}
		else
		{
			// Since the game isn't over, then calculate the score, where score is distance to the ball
			var ballPosLocal = transform.InverseTransformPoint(good[0].transform.position);
			var distFromGood = Vector3.Distance(SuckerCollider.center, ballPosLocal);
			Score = (int) (-distFromGood * 1000);
			Debug.Log("Score!" + Score);
		}

		_frameCount += 1;
	}

	public override void ResetGame()
	{
		RobotController.ResetState();
		ClearBalls();
		SpawnBall(GBall);
		Score = 0;
		_frameCount = 0;
	}
	
	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.name.Contains(GBall.name))
		{
			Debug.Log("GOT A GOOD BALL!");
			Destroy(col.gameObject);
			Score += Reward;
		}
		if (col.gameObject.name.Contains(BBall.name))
		{
			Debug.Log("GOT A BAD BALL!");
			Score += Penalty;
			Destroy(col.gameObject);
		}
	}

	void SpawnBall(GameObject ball)
	{
		// Spawn a ball a certain distance from the robots base
		var location = Robot.transform.position;
		var randomCircle = Random.insideUnitCircle;
		var shiftX = SpawnRadius * randomCircle.x;
		var shiftZ = SpawnRadius * randomCircle.y;
		// var shiftY = Robot.transform.position.y; // SpawnHeight + Random.RandomRange(-HeightRange / 2, HeightRange / 2);
		var shiftY = SpawnHeight + Random.RandomRange(-HeightRange / 2, HeightRange / 2);
		
		// Set boundaries
		if (Math.Pow(Math.Pow(shiftX, 2) + Math.Pow(shiftZ, 2), .5) < MinRadius)
		{
			//If it's not within the desired boundaries, retry
			SpawnBall(ball);
			return;
		}
	
		// The robots rotat-able space is set to -90 to 90. Limit the ability for balls to spawn behind robot.
		if (shiftZ <= 0)
		{
			SpawnBall(ball);
			return;
		}
		location.x += shiftX;
		location.z += shiftZ;
		location.y += shiftY;
		
		Instantiate(ball, location, Quaternion.identity);
	}

	void ClearBalls()
	{
		var good =  GameObject.FindGameObjectsWithTag("GoodBall");
		var objects = good.Concat(GameObject.FindGameObjectsWithTag("BadBall"));

		foreach(GameObject obj in objects)
			Destroy(obj);
		
	}
}
