using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;

public class ManageScene : MonoBehaviour {

   public GameObject galaxy;
    public GameObject bigBang;
    public Text helperText;
    public AudioClip bang;

    private void OnEnable() {
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable() {
        LeanTouch.OnFingerTap -= OnFingerTap;
    }

    void Start () {
        bigBang.SetActive(false);
      
	}
	
	

  

    void OnFingerTap(LeanFinger finger) {

        if (finger.IsOverGui == true) {
            return;
        }
       

        bigBang.SetActive(true);
        
        SoundManager.instance.PlaySingle(bang);
        StartCoroutine(Example());
        StopCoroutine(Example());
        

       

    }

    IEnumerator Example() {
        
       // SoundManager.instance.PlaySingle(liftOff);

        yield return new WaitForSeconds(5);

        //SoundManager.instance.PlaySingle(launchNoise);

        galaxy.SetActive(true);

        helperText.text = "Fun fact"; //"Earth is in the outskirts of the galaxy. An obscure locale on the edge of a distant spiral arm.";
        // Add helper text to tell user they can scale
    }


}
