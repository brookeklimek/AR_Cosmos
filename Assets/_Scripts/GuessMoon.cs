using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuessMoon : MonoBehaviour {
    
    
    public void Guess() {
         
        if (RandomMoonName.phase == gameObject.name) {
            Debug.Log("correct");
            RandomMoonName.correct = true;

          
            //"gameobject.Name" + is correct!

        }
        else {
            Debug.Log("incorrect");
            RandomMoonName.correct = false;

            

            //"Try again"
        }
    }
}
