using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuessMoon : MonoBehaviour {

    

     

    public void Guess() {
        if (SelectMoon.phase == gameObject.name) {
            Debug.Log("correct");
            SelectMoon.correct = true;
            //"gameobject.Name" + is correct!

        }
        else {
            Debug.Log("incorrect");
            SelectMoon.correct = false;
            //"Try again"
        }
    }
}
