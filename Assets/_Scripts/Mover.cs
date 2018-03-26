using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class Mover : MonoBehaviour {

    public float speed;
    //public GameObject launch;
    public GameObject fire;

    private void OnEnable() {
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable() {
        LeanTouch.OnFingerTap -= OnFingerTap;
    }


    void Start() {
        
    }

    

    void OnFingerTap(LeanFinger finger) {

        //Rigidbody rigidbody = GetComponent<Rigidbody>();
        //rigidbody.velocity = transform.forward * speed;

        StartCoroutine(Example());



    }


    IEnumerator Example() {
          print(Time.time);
            fire.SetActive(true);
            yield return new WaitForSeconds(3);
            print(Time.time);

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = transform.forward * speed;

        yield return new WaitForSeconds(8);

        Debug.Log("wait for 5 seconds");
        Spaceshuttle.notGenerated = true;


       Destroy(gameObject);
        }


    }
