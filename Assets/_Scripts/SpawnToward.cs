using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnToward : MonoBehaviour {
	public GameObject mutation;
	public Transform spawnPoint;

	public float timer = 3.0f;
	List<float> intervals = new List<float>();

	void Start () {
		intervals.Add (0.4f);
		intervals.Add (1.0f);
		intervals.Add (1.7f);
		intervals.Add (2.4f);
		intervals.Add (3.5f);
	}

	void Update () {

		timer -= Time.deltaTime;
		int randomIndex = Random.Range( 0, intervals.Count );

		if (timer <= 0.0f) {
			Instantiate (mutation, spawnPoint);
			timer = intervals[randomIndex];
		}

	}

}
