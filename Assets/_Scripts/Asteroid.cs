using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

public class Asteroid : MonoBehaviour {

    public GameObject explosion;
    public AudioClip explodeNoise;
    public float speed;

    private Rigidbody rb;
    private GameObject target;
    private bool sendAsteroid;
    private LeanSelect selectAsteroid;


    //private void OnEnable() {
    //    LeanTouch.OnFingerTap += OnFingerTap;
    //}

    //private void OnDisable() {
    //    LeanTouch.OnFingerTap -= OnFingerTap;
    //}

    void Start() {
        rb = GetComponent<Rigidbody>();
        sendAsteroid = true;
    }

     void Update() {
        target = GameObject.FindGameObjectWithTag("MiddleEarth");

        if(sendAsteroid) {
            //  move asteroid to earth
            Debug.Log("moving asteroid");
            transform.LookAt(target.transform);
            rb.velocity = transform.forward * speed;
            sendAsteroid = false;
        }
    }



    //void OnFingerTap(LeanFinger finger) {
    //    if (finger.IsOverGui == true) {
    //        return;
    //    }

        

       

       


  //}

    //  public void SendAsteroid () {
    //    sendAsteroid = true;
    //}

    void OnTriggerEnter(Collider other) {
        
            if (explosion != null) {
                GameObject newExplosion = (GameObject)Instantiate(explosion);
                newExplosion.transform.position = this.transform.position;
                Object.Destroy(newExplosion, 0.5f);
            }

            if (explodeNoise != null) {
                AudioSource.PlayClipAtPoint(explodeNoise, transform.position, 1.0f);
            }

            Destroy(gameObject);
        
    }
}
