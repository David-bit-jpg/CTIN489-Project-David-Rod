using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] Text restartText, endText;
    public bool levelEnded = false;

    private void Awake()
    {
        //make this a singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        restartText.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (levelEnded)
        {
            //Restart Level
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartLevel();
            }
        }
    }

    public void RestartLevel()
    {
        TaskManager.Instance.ClearTasks();
        restartText.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowRestartText()
    {
        restartText.gameObject.SetActive(true);
    }

    public void ShowLevelEnd()
    {
        levelEnded = true;
        endText.gameObject.SetActive(true);

        //NextLevel();
    }

    public void NextLevel()
    {
        int index = SceneManager.GetActiveScene().buildIndex + 1;
        if(index >= SceneManager.sceneCount)
        {
            Debug.LogWarning("Loading beyond the last scene");
            return;
        }
        SceneManager.LoadScene(index);
    }

    
}
