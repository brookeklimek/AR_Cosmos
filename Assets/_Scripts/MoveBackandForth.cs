using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBackandForth : MonoBehaviour {
	float X;
	bool flip;

	void Start () {
		X = transform.position.x;
		flip = true;

	}

 	void Update () {
		if (X < -3 || X > 3) {
			flip = !flip;
		}

		MoveSpawn (flip);
	}

	void MoveSpawn (bool flop) {
		
		if (flop) {
			X -= 0.05f;
		} else {
			X += 0.05f;
		}

		transform.position = new Vector3 (X, 0, 6);

	}
}
