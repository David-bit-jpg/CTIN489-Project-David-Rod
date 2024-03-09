using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class FaceSpawner : MonoBehaviour
{
    public GameObject horrorFacePrefab; 
    public float minDistance = 10f; 
    public float maxDistance = 20f; 
    public float spawnInterval = 20f;

    PlayerMovement mPlayer;

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
            SpawnHorrorFaceNearPlayer();
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
            float distance = Random.Range(minDistance, maxDistance);
            spawnPosition = playerPosition + randomDirection * distance;
            spawnPosition.y = Random.Range(1f, 2.5f);
            Bounds spawnBounds = new Bounds(spawnPosition, Vector3.one * 0.5f);
            positionInCameraView = GeometryUtility.TestPlanesAABB(cameraPlanes, spawnBounds);

            if (!positionInCameraView)
            {
                GameObject horrorFaceInstance = Instantiate(horrorFacePrefab, spawnPosition, Quaternion.identity);
                AdjustPositionToNearestWall(horrorFaceInstance);
                Debug.Log($"Spawned horror face at {spawnPosition}, distance: {distance}");
                break;
            }
        }

        if (attempts >= 100) {
            Debug.Log("Failed to find a position outside the camera view after 100 attempts.");
        }
    }
    void AdjustPositionToNearestWall(GameObject horrorFace)
    {
        float minDistanceFromPlayer = 5.0f;

        RaycastHit[] hits = Physics.RaycastAll(horrorFace.transform.position, Vector3.down, Mathf.Infinity, LayerMask.GetMask("Wall"));
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Wall"))
                {
                    float distanceFromPlayer = Vector3.Distance(hit.point, mPlayer.transform.position);

                    if (distanceFromPlayer >= minDistanceFromPlayer)
                    {
                        horrorFace.transform.position = hit.point + hit.normal * 0.1f; 
                        horrorFace.transform.rotation = Quaternion.LookRotation(hit.normal); 
                        return; 
                    }
                }
            }

            Debug.Log("All walls are too close to the player.");
        }
        else
        {
            Debug.Log("No walls detected.");
        }
    }


}
