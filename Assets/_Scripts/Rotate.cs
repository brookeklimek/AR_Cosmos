using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

    public int speed = 2;

    void Update() {


        // ...also rotate around the World's Y axis
        transform.Rotate(0, -speed * 0.3f, 0);
    }
}
