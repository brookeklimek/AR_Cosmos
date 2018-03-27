using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoonGameButton : MonoBehaviour {

    public GameObject startScene;
    public GameObject testingScene;

    private bool practiceScene = true;
    private bool testScene = false;
    private Text buttonText;

    private void Start() {
        buttonText = GetComponent<Text>();
    }

    public void Clicked() {
        if(practiceScene) {
            startScene.SetActive(false);
            practiceScene = false;

            testingScene.SetActive(true);
            testScene = true;
            buttonText.text = "Go back";
         }

        else {
            testingScene.SetActive(false);
            testScene = false;

            startScene.SetActive(true);
            practiceScene = true;
            buttonText.text = "Test your knowledge";
        }
    }
}
