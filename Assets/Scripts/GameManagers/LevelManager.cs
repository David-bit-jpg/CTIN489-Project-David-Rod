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
    PlayerMovement player;
    BalloonSpawnerGood balloonSpawnerGood;

    private void Awake()
    {
        //make this a singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        restartText.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);
        player = FindObjectOfType<PlayerMovement>();
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
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TaskManager.Instance.ClearTasks();
        /*restartText.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);*/
        TaskManager.Instance.InstantiateTasks();
        TaskManager.Instance.InitTaskText();
        //player.gameObject.transform.position = new Vector3(3.70000005f, 0.200000003f, -33.5999985f);
        balloonSpawnerGood = FindAnyObjectByType<BalloonSpawnerGood>();
        balloonSpawnerGood.setMimicMovement();
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
