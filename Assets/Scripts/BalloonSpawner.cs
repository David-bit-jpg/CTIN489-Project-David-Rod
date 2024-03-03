using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
public class BalloonSpawner : MonoBehaviour
{
    [SerializeField] public GameObject objectToSpawn;
    [SerializeField] public int objectsToSpawn = 5;
    [SerializeField] public float xMin = -28.0f, xMax = 28.0f;
    [SerializeField] public float zMin = -28.0f, zMax = 28.0f;
    public float raycastDistance = 50.0f;
    public float safeDistance = 0.4f;
    void Start()
    {
        SpawnObjectsNearWall();
    }

    void SpawnObjectsNearWall()
    {
        int spawnedObjects = 0;

        while (spawnedObjects < objectsToSpawn)
        {
            float randomX = Random.Range(xMin, xMax);
            float randomZ = Random.Range(zMin, zMax);
            Vector3 spawnPosition = new Vector3(randomX, 2, randomZ);

            Collider[] colliders = Physics.OverlapSphere(spawnPosition, safeDistance);
            if (colliders.Length == 0)
            {
                Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
                spawnedObjects++;
            }
        }
    }
}
