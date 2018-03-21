using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonClick : MonoBehaviour {


    public void LoadLevel(string sceneName) {
        Debug.Log("Clicked!");
        SceneManager.LoadScene(sceneName);
    }
}
