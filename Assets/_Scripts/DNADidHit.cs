using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DNADidHit : MonoBehaviour {
	//Material hitMaterial;

	float timer;
	bool timerFlip;

	public Text extinct;
	public Text youLive;
	public Text badMutes;
	public Text goodMutes;

	public Button menuBtn;
	public Button playAgain;

	private int badMutesCount;
	private int goodMutesCount;

	private float newScale;

	// Use this for initialization
	void Start () {
		badMutesCount = 5;
		goodMutesCount = 0;

		//hitMaterial = GetComponent<Renderer>().material;

		newScale = 4.0f;

		extinct.text = "";
		youLive.text = "";
	}
	
	// Update is called once per frame
	void Update () {
		if (timerFlip) {
			timer -= Time.deltaTime;

			if (timer < 0.1f) {
				//hitMaterial.color = Color.white;
				timerFlip = !timerFlip;
			}
		}

		badMutes.text = "Bad Mutations Left: " + badMutesCount;
		goodMutes.text = "Good Mutations Gained: " + goodMutesCount;

	}

	void OnTriggerEnter (Collider disc) {
		if (disc.gameObject.tag == "DNA_Green") {
			//hitMaterial.color = Color.green;

			goodMutesCount++;
			if (goodMutesCount > 5) {
				youLive.text = "You lived to the next generation";
				Time.timeScale = 0;
				playAgain.gameObject.SetActive(true);
			}

		} else if (disc.gameObject.tag == "DNA_Red") {
			//hitMaterial.color = Color.red;

			newScale -= 0.75f; 

			gameObject.transform.localScale = new Vector3 (newScale, newScale, newScale);

			badMutesCount--;
			if (badMutesCount == 0) {
				extinct.text = "You're Extinct...";
				Time.timeScale = 0;
				menuBtn.gameObject.SetActive(true);
			}
		}

		timer = 0.5f;
		timerFlip = true;
	}

	public void LoadScene (string scene) {
		SceneManager.LoadScene (scene);
	}

}
