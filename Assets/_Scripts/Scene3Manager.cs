using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class Scene3Manager  : MonoBehaviour {

    public GameObject toScaleScene;
    public GameObject observableScene;
    public Text helperText;

    private int view = 0;

    private void OnEnable() {
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable() {
        LeanTouch.OnFingerTap -= OnFingerTap;
    }

    void Start() {
        toScaleScene.SetActive(false);
    }

    void OnFingerTap(LeanFinger finger) {

        if (finger.IsOverGui == true) {
            return;
        }

        switch(view) {
            case 0:
                observableScene.SetActive(false);
                toScaleScene.SetActive(true);
                view = 1;
                break;
            case 1:
                toScaleScene.SetActive(false);
                observableScene.SetActive(true);
                view = 0;
                break;
        }
          helperText.text = "Fun fact"; //"Earth is in the outskirts of the galaxy. An obscure locale on the edge of a distant spiral arm.";
        // Add helper text to tell user they can scale

    }


}
