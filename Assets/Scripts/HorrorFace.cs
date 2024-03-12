using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class HorrorFace : MonoBehaviour
{
    public float lifetime;
    private bool shouldRotate = true;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement =  FindObjectOfType<PlayerMovement>();
        StartCoroutine(DestroyAfterTime(lifetime));
    }

    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    void Update()
    {
        if(playerMovement != null && shouldRotate)
        {
            Vector3 direction = playerMovement.transform.position - transform.position;
            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        shouldRotate = false;
    }
}
