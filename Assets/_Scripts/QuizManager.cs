using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour {
	public static int correctPlanets;
	public GameObject correct;

	// Use this for initialization
	void Start () {
		correctPlanets = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if (correctPlanets == 8) {
			Debug.Log ("Correct!");
			correct.SetActive(true);
		}
	}

	public static void AddPlanetCorrect () {
		correctPlanets++;	
	}
}
