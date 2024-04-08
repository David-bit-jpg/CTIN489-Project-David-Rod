using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    int currentScene;

    void Update()
    {
        //Gets the currently active scene.
        currentScene = SceneManager.GetActiveScene().buildIndex;

        //Reloads the scene when the user presses the L key.
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(currentScene);
        }
    }
}
