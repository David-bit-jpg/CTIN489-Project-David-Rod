using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static Vector3 shakeOffset = Vector3.zero;
    
    public IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            shakeOffset = new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return null;
        }

        shakeOffset = Vector3.zero;
    }
}
