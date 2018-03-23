using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class Earth : MonoBehaviour {

   
    public GameObject asteroid;
   
    public Text helperText;

    private void OnEnable() {
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable() {
        LeanTouch.OnFingerTap -= OnFingerTap;
    }

    void Start() {
       // secondEarth.SetActive(false);
    }



    void OnFingerTap(LeanFinger finger) {
        if (finger.IsOverGui == true) {
            return;
        }

        //  move asteroid to earth

       

       

    }


}
