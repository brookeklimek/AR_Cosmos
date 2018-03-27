using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuessMoon : MonoBehaviour {
    public Transform correctPrefab;
    public Transform incorrectPrefab;
   // public Text resultText;
    
    public void Guess() {
        if (SelectMoon.phase == gameObject.name) {
            Debug.Log("correct");
            SelectMoon.correct = true;

            Instantiate(correctPrefab);
            Destroy(correctPrefab, 3.0f);
            //"gameobject.Name" + is correct!

        }
        else {
            Debug.Log("incorrect");
            SelectMoon.correct = false;

            Instantiate(incorrectPrefab);
            Destroy(incorrectPrefab, 2.0f);

            //"Try again"
        }
    }
}
