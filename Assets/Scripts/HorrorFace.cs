using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HorrorFace : MonoBehaviour
{
    public float lifetime;
    private bool shouldRotate = true;
    private PlayerMovement playerMovement;
    private Transform cameraTransform;
    private float lookTime = 0;
    public float sphereRadius = 1.0f;

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        StartCoroutine(DestroyAfterTime(lifetime));
        cameraTransform = Camera.main.transform;
    }

    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    void Update()
    {
        if (playerMovement != null && shouldRotate)
        {
            Vector3 direction = playerMovement.transform.position - transform.position;
            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;

            if (IsPlayerLookingAtPrefab() && !CheckForWall())
            {
                lookTime += Time.deltaTime;
                if (lookTime > 6.0f)
                {
                    playerMovement.killed = true;
                    playerMovement.SetCanMove(false);
                }
            }
            else
            {
                lookTime = 0;
            }
        }
    }

    bool IsPlayerLookingAtPrefab()
    {
        Vector3 playerPositionFlat = new Vector3(cameraTransform.position.x, 0, cameraTransform.position.z);
        Vector3 prefabPositionFlat = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerDirection = cameraTransform.forward;
        Vector3 toPrefabFlat = (prefabPositionFlat - playerPositionFlat).normalized;

        float angle = Vector3.Angle(playerDirection, toPrefabFlat);
        return angle < 30;
    }
    bool CheckForWall()
    {
        RaycastHit hit;

        Vector3 toPrefab = (transform.position - cameraTransform.position);

        Vector3 rayStart = cameraTransform.position;
        Vector3 rayDirection = toPrefab;

        float rayLength = Vector3.Distance(transform.position, cameraTransform.position);

        Debug.DrawRay(rayStart, rayDirection * rayLength, Color.red, 2.0f);

        if (Physics.SphereCast(rayStart, sphereRadius, rayDirection, out hit, rayLength))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Door"))
            {
                // Debug.Log("Wall detected between player and HorrorFace.");
                return true;
            }
        }
        // Debug.Log("No wall detected between player and HorrorFace.");
        return false;
    }

}