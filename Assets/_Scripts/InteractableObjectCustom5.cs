using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class InteractableObjectCustom5 : MonoBehaviour {

	private GameObject lean5;
	private GameObject sphere5;

	void Start () {
		lean5 = GameObject.Find ("LeanTouch5");
		sphere5 = GameObject.Find ("5");
	}

	void LateUpdate () {
		CheckForTouch ();
	}

	void CheckForTouch () {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hit = new RaycastHit();

		if(Input.GetMouseButton(0)) {
			if(Physics.Raycast(ray, out hit)) {
				lean5.GetComponent<LeanTouch> ().enabled = true;
				sphere5.GetComponent<LeanTranslate> ().enabled = true;
			} else {
				lean5.GetComponent<LeanTouch> ().enabled = false;
				sphere5.GetComponent<LeanTranslate> ().enabled = false;
			}
		}
	}
}
