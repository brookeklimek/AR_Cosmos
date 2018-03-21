using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussianRandom : MonoBehaviour
{

    public static float Value
    {
        get
        {
            float u, v, S;

            do
            {
                u = 2.0f * Random.value - 1.0f;
                v = 2.0f * Random.value - 1.0f;
                S = Mathf.Pow(u, 2) + Mathf.Pow(v, 2);
            }
            while (S >= 1.0);

            float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
            return u * fac;
        }
    }

}
