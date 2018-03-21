using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class InteractableObjectCustom4 : MonoBehaviour {

	private GameObject lean4;
	private GameObject sphere4;

	void Start () {
		lean4 = GameObject.Find ("LeanTouch4");
		sphere4 = GameObject.Find ("4");
	}

	void LateUpdate () {
		CheckForTouch ();
	}

	void CheckForTouch () {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hit = new RaycastHit();

		if(Input.GetMouseButton(0)) {
			if(Physics.Raycast(ray, out hit)) {
				lean4.GetComponent<LeanTouch> ().enabled = true;
				sphere4.GetComponent<LeanTranslate> ().enabled = true;
			}
		}
	}
}
