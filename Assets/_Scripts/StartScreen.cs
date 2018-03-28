using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreen : MonoBehaviour {
	public Text title;
	public GameObject quote;
	private float timer;

	private float newScale;

	// Use this for initialization
	void Start () {
		timer = 7.0f;
		newScale = 2.0f;
	}
	
	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;


		if (title.transform.localScale.x > 1.05f) {
			newScale -= 0.01f;
			title.transform.localScale = new Vector3 (newScale, newScale, newScale);
		} 

		if (timer < 5.0f) {
			quote.gameObject.SetActive (true);
		}

		if (timer < 3.0f) {
			SceneManager.LoadScene ("Menu");
		}
	}
}
