using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnToward : MonoBehaviour {
	public GameObject goodMutation;
	public GameObject badMutation;
	private GameObject currentMutation;

	public List<Transform> spawnPoints;
	private Transform spawnNow;

	private bool switchMutate;

	public float timer = 3.0f;
	List<float> intervals = new List<float>();

	void Start () {
		intervals.Add (0.4f);
		intervals.Add (1.0f);
		intervals.Add (1.7f);
		intervals.Add (2.4f);
		intervals.Add (3.5f);

		//switchMutate = true;
	}

	void Update () {

		timer -= Time.deltaTime;


		if (switchMutate) {
			currentMutation = goodMutation;
		} else {
			currentMutation = badMutation;
		}

		if (timer <= 0.0f) {
			int randomSpawnIndex = Random.Range( 0, spawnPoints.Count );
			int randomTimeIndex = Random.Range( 0, intervals.Count ); 

			spawnNow = spawnPoints[randomSpawnIndex];
			Instantiate (currentMutation, spawnNow);

			timer = intervals[randomTimeIndex];

			switchMutate = !switchMutate;
		}

	}

}
