using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
public class BalloonSpawnerGood : MonoBehaviour
{
    [Header("Balloons")]
    [SerializeField] private GameObject redBalloon;
    [SerializeField] private GameObject pinkBalloon;
    [SerializeField] private GameObject yellowBalloon;
    [SerializeField] public float xMin = -27.0f, xMax = 27.0f;
    [SerializeField] public float zMin = -27.0f, zMax = 27.0f;
    [SerializeField] public int spawnNum = 7;
    public float safeDistance = 0.4f;
    PlayerMovement mPlayer;

    private List<GameObject> spawnedBalloons = new List<GameObject>();
    void Start()
    {
        StartCoroutine(SpawnBalloonWithInterval());
    }
    void Update()
    {
        if (CheckForBreakedBalloons())
        {
            //yet to be done
        }
    }

    private IEnumerator SpawnBalloonWithInterval()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        if (spawnedBalloons.Count <= spawnNum)
        {
            SpawnRandomBalloon();
            StartCoroutine(SpawnBalloonWithInterval());
        }
    }
    private void SpawnRandomBalloon()
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
        }
    }
    bool CheckForBreakedBalloons()
    {
        foreach (GameObject b in new List<GameObject>(spawnedBalloons))
        {
            Break_Ghost balloonScript = b.GetComponent<Break_Ghost>();
            if (balloonScript != null && balloonScript.Is_Breaked)
            {
                spawnedBalloons.Remove(b);
                return true;
            }
        }
        return false;
    }
}
