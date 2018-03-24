using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNADidHit : MonoBehaviour {
	Material hitMaterial;

	float timer;
	bool timerFlip;

	// Use this for initialization
	void Start () {
		hitMaterial = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		if (timerFlip) {
			timer -= Time.deltaTime;

			if (timer < 0.1f) {
				hitMaterial.color = Color.white;
				timerFlip = !timerFlip;
			}
		}


	}

	void OnTriggerEnter (Collider disc) {
		if (disc.gameObject.tag == "DNA") {
			Debug.Log ("HittedNA");
			hitMaterial.color = Color.red;

			timer = 0.5f;
			timerFlip = true;
		}	
	}

}
