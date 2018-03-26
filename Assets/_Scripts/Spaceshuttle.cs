using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spaceshuttle : MonoBehaviour {

    public GameObject prefabToGenerate;
    public GameObject prefabToGenerate2;
    public float speed;
    public GameObject launch;

    public static bool notGenerated;
   

    private bool found = false;
    private float lastLaunch;
    private float waitTime = 10;

     void Found() {
        found = true;
    }

    void Lost() {
        found = false;
    }

    

    void Start() {
        notGenerated = true;
        Object.Instantiate(prefabToGenerate2, transform);

    }

    void Update() {
        if (!found) {
            return;
        }

       


       

            if(notGenerated) {
                
                Object.Instantiate(prefabToGenerate, transform);
                notGenerated = false;
            }
        
            
        }

        
    
}
