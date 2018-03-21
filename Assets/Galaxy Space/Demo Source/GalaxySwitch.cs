using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxySwitch : MonoBehaviour {

    public GameObject[] Galaxies;
    private int CurrentID = 0;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Comma))
        {

            if (CurrentID > 0)
            {

                CurrentID--;
                Galaxies[CurrentID + 1].SetActive(false);
                Galaxies[CurrentID].SetActive(true);

            }

        }
        if (Input.GetKeyDown(KeyCode.Period))
        {

            if (CurrentID < Galaxies.Length-1)
            {

                CurrentID++;
                Galaxies[CurrentID - 1].SetActive(false);
                Galaxies[CurrentID].SetActive(true);

            }

        }
    }

}
