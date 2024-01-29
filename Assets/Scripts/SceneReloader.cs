using MimicSpace;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    ThirdPersonController player;
    Movement mimicMove;

    void Start()
    {
        player = FindObjectOfType<ThirdPersonController>();
        mimicMove = FindObjectOfType<Movement>();
    }
    void Update()
    {
        // Check for a key press or any other condition to trigger the scene reload
        if (Input.GetKeyDown(KeyCode.R) && (player.isDead || mimicMove.isDead))
        {
            ReloadScene();
        }
    }

    void ReloadScene()
    {
        // Get the current scene's index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Reload the current scene
        SceneManager.LoadScene(currentSceneIndex);
    }
}
