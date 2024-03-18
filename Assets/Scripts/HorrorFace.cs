using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HorrorFace : MonoBehaviour
{
    public float lifetime;
    private bool shouldRotate = true;
    private PlayerMovement playerMovement;
    private float lookTime = 0;

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        StartCoroutine(DestroyAfterTime(lifetime));
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

            if (IsPlayerLookingAtPrefab())
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
        Vector3 playerPositionFlat = new Vector3(playerMovement.transform.position.x, 0, playerMovement.transform.position.z);
        Vector3 prefabPositionFlat = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerDirection = playerMovement.transform.forward;
        Vector3 toPrefabFlat = (prefabPositionFlat - playerPositionFlat).normalized;

        float angle = Vector3.Angle(playerDirection, toPrefabFlat);
        Debug.Log($"Player to HorrorFace Angle: {angle}");
        return angle < 30;
    }

}