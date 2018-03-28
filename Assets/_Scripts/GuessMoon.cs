using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuessMoon : MonoBehaviour {
    public Text answer;
    
    public void Guess() {
         
        if (RandomMoonName.phase == gameObject.name) {
            Debug.Log("correct");
            RandomMoonName.correct = true;


           answer.text = "Correct!";

        }
        else {
            Debug.Log("incorrect");
            RandomMoonName.correct = false;



           answer.text = "Try again...";
        }
    }
}
