using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour {
	public static int correctPlanets;
	public GameObject correct;
	static float startingX;

	public List<GameObject> planets = new List<GameObject>();

	// Use this for initialization
	void Start () {
		correctPlanets = 0;
		//startingX = -0.5f;
		startingX = -4.14f;

		List<GameObject> selectedPlanets = new List<GameObject>();

		while ( selectedPlanets.Count < 8 ) {
			int randomIndex = Random.Range( 0, planets.Count );
			if ( !selectedPlanets.Contains( planets[randomIndex] ) )
				selectedPlanets.Add( planets[randomIndex] );
		}

		selectedPlanets.ForEach (PopulatePlanets);
	}
	
	// Update is called once per frame
	void Update () {
		if (correctPlanets == 8) {
			correct.SetActive(true);
		}
	}

	public static void AddPlanetCorrect () {
		correctPlanets++;	
	}

	public static void PopulatePlanets (GameObject planet) {
		

		planet.transform.position = new Vector3 (startingX, 0.05f, 0);
		Debug.Log (startingX);
		Instantiate (planet);

		//startingX += 0.15f;
		startingX += 1.24f;
	} 
}
