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
                Debug.Log("LOOK AT ");
                lookTime += Time.deltaTime;
                if (lookTime > 10)
                {
                    playerMovement.killed = true;
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
        Vector3 playerDirection = playerMovement.transform.forward;
        Vector3 toPrefab = (transform.position - playerMovement.transform.position).normalized;
        float angle = Vector3.Angle(playerDirection, toPrefab);
        return angle < 30.0f;
    }
}
