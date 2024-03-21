using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] public Material vhsMaterial;
    public static LevelManager Instance;
    [SerializeField] Text restartText, endText;
    public bool levelEnded = false;
    PlayerMovement player;
    BalloonSpawnerGood balloonSpawnerGood;
    public Volume postVolume;
    public Vignette thisVignette;

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
        postVolume.profile.TryGet(out thisVignette);
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
        UpdateVHSParameters();
        TaskManager.Instance.ClearTasks();
        /*restartText.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);*/
        TaskManager.Instance.InstantiateTasks();
        TaskManager.Instance.InitTaskText();
        //player.gameObject.transform.position = new Vector3(3.70000005f, 0.200000003f, -33.5999985f);
        balloonSpawnerGood = FindAnyObjectByType<BalloonSpawnerGood>();
        balloonSpawnerGood.setMimicMovement();
        LightmapSwitcher.Instance.SwitchToDay();
        postVolume = FindObjectOfType<Volume>();
        postVolume.sharedProfile.TryGet(out thisVignette);
    }
    private void UpdateVHSParameters()
    {
        if (vhsMaterial != null)
        {
            float strength = 0.0f;
            float strip = 0.0f;
            float pixelOffset = 0.0f;
            float shake = 0.0f;
            float speed = 0.0f;
            vhsMaterial.SetFloat("_Strength", strength);
            vhsMaterial.SetFloat("_StripSize", strip);
            vhsMaterial.SetFloat("_PixelOffset", pixelOffset);
            vhsMaterial.SetFloat("_Shake", shake);
            vhsMaterial.SetFloat("_Speed", speed);
        }
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
        if (index >= SceneManager.sceneCount)
        {
            Debug.LogWarning("Loading beyond the last scene");
            return;
        }
        SceneManager.LoadScene(index);
    }


}
