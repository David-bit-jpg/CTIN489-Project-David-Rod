using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class FaceSpawner : MonoBehaviour
{
    public GameObject horrorFacePrefab; 
    public float minDistance; 
    public float maxDistance; 
    public float spawnInterval;
    public int spawnNum;

    PlayerMovement mPlayer;
    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        mPlayer = FindObjectOfType<PlayerMovement>();
        StartCoroutine(SpawnHorrorFaceRoutine());
    }

    IEnumerator SpawnHorrorFaceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            spawnedPositions.Clear();
            for (int i = 0; i < spawnNum; i++)
            {
                SpawnHorrorFaceNearPlayer();
                yield return null;
            }
        }
    }

    void SpawnHorrorFaceNearPlayer()
    {
        Vector3 playerPosition = mPlayer.transform.position;
        Camera mainCamera = Camera.main;
        Vector3 spawnPosition = Vector3.zero;
        bool positionInCameraView = true;
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        int attempts = 0;
        while (positionInCameraView && attempts < 100)
        {
            attempts++;
            Vector3 randomDirection = Random.insideUnitSphere.normalized;
            randomDirection.y = 0;
            float distance = Random.Range(minDistance, maxDistance);
            Vector3 potentialSpawnPosition = playerPosition + randomDirection * distance;
            potentialSpawnPosition.y = Random.Range(1.5f, 2.5f);
            bool tooClose = false;
            float minFaceDist = 10f;
            foreach (Vector3 pos in spawnedPositions)
            {
                if (Vector3.Distance(new Vector3(potentialSpawnPosition.x, 0, potentialSpawnPosition.z), new Vector3(pos.x, 0, pos.z)) < minFaceDist)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose) continue; 

            Bounds spawnBounds = new Bounds(potentialSpawnPosition, Vector3.one * 0.5f);
            positionInCameraView = GeometryUtility.TestPlanesAABB(cameraPlanes, spawnBounds);

            if (!positionInCameraView)
            {
                GameObject horrorFaceInstance = Instantiate(horrorFacePrefab, potentialSpawnPosition, Quaternion.identity);
                AdjustPositionToNearestWall(horrorFaceInstance);
                spawnedPositions.Add(potentialSpawnPosition);
                break;
            }
        }
    }
    void AdjustPositionToNearestWall(GameObject horrorFace)
    {
        float minDistanceFromPlayer = 3.0f;
        RaycastHit[] hits = Physics.RaycastAll(horrorFace.transform.position, Vector3.down, Mathf.Infinity, LayerMask.GetMask("Wall"));
        Vector3 playerPosition = mPlayer.transform.position;
        playerPosition.y = 0;
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    Vector3 hitPosition = hit.point;
                    hitPosition.y = 0; 
                    float distanceFromPlayer = Vector3.Distance(hitPosition,playerPosition);

                    if (distanceFromPlayer >= minDistanceFromPlayer)
                    {
                        horrorFace.transform.position = hit.point + hit.normal * 0.1f; 
                        horrorFace.transform.rotation = Quaternion.LookRotation(hit.normal); 
                        return; 
                    }
                }
            }
        }
    }
}