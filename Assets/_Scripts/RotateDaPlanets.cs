using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateDaPlanets : MonoBehaviour {

    public int speed = -10;


	void Update () {
        transform.Rotate(Vector3.up * Time.deltaTime * speed);
	}
}
