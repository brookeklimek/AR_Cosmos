using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour {

    public float TTL;

	
	void Start () {
        Destroy(gameObject, TTL);
	}
	
	
}
