using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class Galaxy : MonoBehaviour {

   // public GameObject galaxy;
    public GameObject earlyEarth;
    //public GameObject asteroids;
    public Material startMaterial;
    public Material tappedMaterial;
    public Text helperText;

    private void OnEnable() {
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable() {
        LeanTouch.OnFingerTap -= OnFingerTap;
    }

    void Start () {
        earlyEarth.SetActive(false);
      // home.GetComponent<Renderer>().material = startMaterial;
	}
	
	

   // public void ChangeColor () {
    //    home.GetComponent<Renderer>().material = tappedMaterial;


  //  }

    void OnFingerTap(LeanFinger finger) {

        if (finger.IsOverGui == true) {
            return;
        }
        // home.GetComponent<Renderer>().material = tappedMaterial;

        earlyEarth.SetActive(true);
        Destroy(gameObject);
        

        helperText.text = "Fun fact"; //"Earth is in the outskirts of the galaxy. An obscure locale on the edge of a distant spiral arm.";
        // Add helper text to tell user they can scale

    }

    
}
