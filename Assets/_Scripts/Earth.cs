using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class Earth : MonoBehaviour {

   
    public GameObject asteroid;
    public GameObject secondEarth;
   
    public Text helperText;

    private void OnEnable() {
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable() {
        LeanTouch.OnFingerDown -= OnFingerTap;
    }

    void Start() {
       // secondEarth.SetActive(false);
    }



    void OnFingerTap(LeanFinger finger) {
        if (finger.IsOverGui == true) {
            return;
        }

        //  Instantiate(asteroid);
        secondEarth.SetActive(true);
       Destroy(gameObject);

       

    }


}
