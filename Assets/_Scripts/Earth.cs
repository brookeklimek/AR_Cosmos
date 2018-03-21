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
        LeanTouch.OnFingerDown += OnFingerDown;
    }

    private void OnDisable() {
        LeanTouch.OnFingerDown -= OnFingerDown;
    }

    void Start() {
       // secondEarth.SetActive(false);
    }



    void OnFingerDown(LeanFinger finger) {


        //  Instantiate(asteroid);
       // secondEarth.SetActive(true);
       // Destroy(gameObject);

       

    }


}
