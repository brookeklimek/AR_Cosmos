using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class Mover : MonoBehaviour {

    public float speed;
    //public GameObject launch;
    public GameObject fire;

    public AudioClip liftOff;
    public AudioClip launchNoise;

    private void OnEnable() {
        LeanTouch.OnFingerTap += OnFingerTap;
    }

    private void OnDisable() {
        LeanTouch.OnFingerTap -= OnFingerTap;
    }


    
    void OnFingerTap(LeanFinger finger) {

        
        StartCoroutine(Example());



    }


    IEnumerator Example() {
         fire.SetActive(true);
        SoundManager.instance.PlaySingle(liftOff);

        yield return new WaitForSeconds(3);

       //SoundManager.instance.PlaySingle(launchNoise);

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = transform.forward * speed;

        yield return new WaitForSeconds(12);

        Spaceshuttle.notGenerated = true;

        Destroy(gameObject);
        }


    }
