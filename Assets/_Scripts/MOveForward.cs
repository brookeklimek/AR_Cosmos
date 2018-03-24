using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOveForward : MonoBehaviour {
	private Rigidbody rb;
	public float speed;
	// Update is called once per frame

	void Update () {
		rb = transform.GetComponent<Rigidbody> ();
		rb.velocity = transform.forward * -1 * speed;
	}

}
