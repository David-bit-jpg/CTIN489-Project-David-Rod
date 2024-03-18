using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using MimicSpace;

public class BalloonSpawnerGood : MonoBehaviour
{
    [Header("Balloons")]
    [SerializeField] private GameObject redBalloon;
    [SerializeField] private GameObject pinkBalloon;
    [SerializeField] private GameObject yellowBalloon;
    [SerializeField] public float xMin = -27.0f, xMax = 27.0f;
    [SerializeField] public float zMin = -27.0f, zMax = 27.0f;
    [SerializeField] public int spawnNum = 7;
    [SerializeField] public string TaskDescription;
    [SerializeField] int levelIndex = 0;
    public int balloonCount;
    public float safeDistance = 0.4f;
    PlayerMovement mPlayer;
    Task balloonTask;
    Movement mimicMovement;


    private List<GameObject> spawnedBalloons = new List<GameObject>();
    void Start()
    {
        balloonCount = spawnNum;

        //spawn balloons
        for(int i = 0; i < spawnNum;)
        {
            if (SpawnRandomBalloon())
            {
                i++;
            }
        }
        setMimicMovement();

    }

    public void setMimicMovement()
    {
        mimicMovement = FindObjectOfType<Movement>(true);
    }
    void Update()
    {
        if(spawnNum == 0)
        {
            Debug.LogWarning("Balloon Spawner has zero spawned ballons");
        }

        if (CheckForBreakedBalloons())
        {
            //yet to be done
        }

        if (balloonCount == 0 && !mimicMovement.gameObject.activeInHierarchy)
        {
            //remove this task
            TaskManager.Instance.RemoveTaskByType(TaskType.BalloonTask);
            
            switch (levelIndex)
            {
                case 0:
                    Task captureTask = new Task("", TaskType.CaptuerTask);
                    TaskManager.Instance.AddTask(captureTask);
                    mimicMovement.gameObject.SetActive(true);
                    break;
            }
                
        }
    }

    private bool SpawnRandomBalloon()
    {
        GameObject[] balloons = new GameObject[] { redBalloon, pinkBalloon, yellowBalloon };
        int index = Random.Range(0, balloons.Length);
        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(xMin, xMax);
            float randomZ = Random.Range(zMin, zMax);
            spawnPosition = new Vector3(randomX, 1, randomZ);
            Collider[] colliders = Physics.OverlapSphere(spawnPosition, safeDistance);

            if (colliders.Length == 0)
            {
                validPositionFound = true;
                break;
            }
        }

        if (validPositionFound)
        {
            GameObject spawnedBalloon = Instantiate(balloons[index], spawnPosition, Quaternion.identity);
            spawnedBalloons.Add(spawnedBalloon);
        }
        else
        {
            Debug.Log("Failed to find a valid spawn position for the balloon.");
            return false;
        }
        return true;
    }
    bool CheckForBreakedBalloons()
    {
        foreach (GameObject b in new List<GameObject>(spawnedBalloons))
        {
            if(b!=null)
            {
                Break_Ghost balloonScript = b.GetComponent<Break_Ghost>();
                if (balloonScript != null && balloonScript.Is_Breaked)
                {
                    spawnedBalloons.Remove(b);
                    return true;
                }
            }
        }
        return false;
    }
}
