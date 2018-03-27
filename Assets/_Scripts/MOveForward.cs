using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOveForward : MonoBehaviour {
	private Rigidbody rb;
	public float speed;
	// Update is called once per frame
	private GameObject imgTarget;

	public float timer = 5.0f;

	void Update () {
		timer -= Time.deltaTime;

		rb = transform.GetComponent<Rigidbody> ();
		rb.velocity = new Vector3(0, 0, speed * -1);//transform.forward * -1 * speed;

		if (timer < 0.1f) {
			Destroy (gameObject);
		}
	}

}
