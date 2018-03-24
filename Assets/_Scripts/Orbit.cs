using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {

	public float speed;
	public float yPosition;
	private Vector3 pivot;

	void Update () {
		pivot = new Vector3 (0, yPosition, 0);
		transform.RotateAround(pivot, Vector3.up, speed * Time.deltaTime);
	}

}