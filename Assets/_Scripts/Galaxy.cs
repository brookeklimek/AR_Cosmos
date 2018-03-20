using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class Galaxy : MonoBehaviour {

    public GameObject home;
    public Material startMaterial;
    public Material tappedMaterial;
    public Text helperText;

    private void OnEnable() {
        LeanTouch.OnFingerDown += OnFingerDown;
    }

    private void OnDisable() {
        LeanTouch.OnFingerDown -= OnFingerDown;
    }

    void Start () {
        home.SetActive(false);
      // home.GetComponent<Renderer>().material = startMaterial;
	}
	
	

   // public void ChangeColor () {
    //    home.GetComponent<Renderer>().material = tappedMaterial;


  //  }

    void OnFingerDown(LeanFinger finger) {
        // home.GetComponent<Renderer>().material = tappedMaterial;
        home.SetActive(true);
        Destroy(home.gameObject, 5.0f);

        helperText.text = "Fun fact"; //"Earth is in the outskirts of the galaxy. An obscure locale on the edge of a distant spiral arm.";
        // Add helper text to tell user they can scale

    }

    
}
