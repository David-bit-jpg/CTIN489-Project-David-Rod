using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HorrorFace : MonoBehaviour
{
    public float lifetime;
    public LayerMask visibilityLayerMask;
    private bool shouldRotate = true;
    private PlayerMovement playerMovement;
    private bool isVisibleToPlayer = false;
    private float visibleTimer = 0f;
    private Camera playerCamera;

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerCamera = Camera.main;
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
        }

        CheckVisibility();
        if (isVisibleToPlayer)
        {
            visibleTimer += Time.deltaTime;
            if (visibleTimer >= 6f)
            {
                playerMovement.killed = true;
            }
        }
        else
        {
            visibleTimer = 0f;
        }
    }

    void CheckVisibility()
    {
        Vector3 directionToPlayer = playerCamera.transform.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, Mathf.Infinity, visibilityLayerMask))
        {
            if (hit.collider.gameObject.GetComponent<PlayerMovement>() != null)
            {
                isVisibleToPlayer = true;
            }
            else
            {
                isVisibleToPlayer = false;
            }
        }
    }
}
