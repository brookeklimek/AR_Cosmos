using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetDrop : MonoBehaviour {
	Material correctMaterial;

	// Use this for initialization
	void Start () {
		Debug.Log ("It intiiated");
		correctMaterial = GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter (Collider planet) {
		if (planet.gameObject.tag == gameObject.tag) {
			Debug.Log ("Planet" + gameObject.tag + "Touched");
			QuizManager.AddPlanetCorrect ();
			correctMaterial.color = Color.green;
		}	
	}
}
