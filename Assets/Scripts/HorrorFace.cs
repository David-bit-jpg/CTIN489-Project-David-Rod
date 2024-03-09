using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class HorrorFace : MonoBehaviour
{
    public float lifetime = 10f;

    void Start()
    {
        StartCoroutine(DestroyAfterTime(lifetime));
    }

    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
