using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class AllowTranslate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			RaycastHit hit = new RaycastHit();

			if(Input.GetMouseButton(0)) {
				if(Physics.Raycast(ray, out hit)) {
					//gameObject.GetComponent<LeanTranslate> ().enabled = true;
					print (gameObject.name);
					gameObject.GetComponent<LeanSelectable> ().Select();
				}
			}
		
	}
}
