using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
public class BalloonSpawner : MonoBehaviour
{
    [Header("Balloons")]
    [SerializeField] private GameObject redBalloon;
    [SerializeField] private GameObject pinkBalloon;
    [SerializeField] private GameObject yellowBalloon;
    [SerializeField] public float xMin = -27.0f, xMax = 27.0f;
    [SerializeField] public float zMin = -27.0f, zMax = 27.0f;
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Text warningText;
    [SerializeField] private int spawnNum = 6;
    public float raycastDistance = 50.0f;
    public float safeDistance = 0.4f;
    PlayerMovement mPlayer;

    private List<GameObject> spawnedBalloons = new List<GameObject>();
    void Start()
    {
        warningText.gameObject.SetActive(false);
        mPlayer = FindObjectOfType<PlayerMovement>();
        StartCoroutine(SpawnBalloonWithInterval());
    }
    void Update()
    {
        if (CheckForBreakedBalloons())
        {
            SpawnGhostNearPlayer();
        }
    }
    void SpawnGhostNearPlayer()
    {
        TriggerTextAnimation();
        Vector3 playerForward = mPlayer.transform.forward;
        Vector3 playerRight = mPlayer.transform.right;

        float angle = Random.Range(-180, 180);
        Vector3 randomDirection = Quaternion.Euler(0, angle, 0) * playerForward * Random.Range(0.5f, 6.0f);

        if (angle > -45 && angle < 45)
        {
            randomDirection += playerRight * 6.0f;
        }

        Vector3 targetPosition = mPlayer.transform.position + randomDirection;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 6.0f, NavMesh.AllAreas))
        {
            Instantiate(ghostPrefab, hit.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Failed to find a valid position for the ghost on NavMesh.");
        }
    }

    private IEnumerator SpawnBalloonWithInterval()
    {
        yield return new WaitForSeconds(Random.Range(15f, 25f));
        if (spawnedBalloons.Count <= spawnNum)
        {
            SpawnRandomBalloon();
        }
        StartCoroutine(SpawnBalloonWithInterval());
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
    public void TriggerTextAnimation()
    {
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float duration = 2.0f;
        float holdTime = 1.0f;
        float startSize = 0f;
        float endSize = 60f;
        warningText.gameObject.SetActive(true);
        for (float timer = 0; timer < duration; timer += Time.deltaTime)
        {
            float progress = timer / duration;
            warningText.fontSize = (int)Mathf.Lerp(startSize, endSize, progress);
            yield return null;
        }

        warningText.fontSize = (int)endSize;

        yield return new WaitForSeconds(holdTime);

        warningText.fontSize = (int)startSize;
        warningText.text = "";
        warningText.gameObject.SetActive(false);
    }
}
