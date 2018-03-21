using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour {

    public GameObject explosion;
    public AudioClip explodeNoise;

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
