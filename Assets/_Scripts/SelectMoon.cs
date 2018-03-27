using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;

public class SelectMoon : MonoBehaviour {

    public Text moonPhase;
   
   

    public void Clicked() {
    
        Debug.Log("My name is " + gameObject.name);
        moonPhase.text = "" + gameObject.name;
    }

    


}