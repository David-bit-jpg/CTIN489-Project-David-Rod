using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public Light targetLight;
    // public Material targetMaterial;
    public float transitionDuration = 2f;
    [SerializeField] float initialIntensity = 1.2f;

    void Start()
    {
        targetLight.intensity = initialIntensity;
        // SetMaterialEmission(true);
        StartCoroutine(ControlLight());
    }

    IEnumerator ControlLight()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 10f));
            // // emission
            // SetMaterialEmission(false);
            yield return StartCoroutine(ChangeLightIntensity(0));

            yield return new WaitForSeconds(Random.Range(10f, 15f));
            // // emission true
            // SetMaterialEmission(true);
            yield return StartCoroutine(ChangeLightIntensity(initialIntensity));
        }
    }

    IEnumerator ChangeLightIntensity(float targetIntensity)
    {
        float time = 0;
        float startIntensity = targetLight.intensity;
        while (time < transitionDuration)
        {
            targetLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, time / transitionDuration);
            time += Time.deltaTime;
            yield return null;
        }
        targetLight.intensity = targetIntensity;
    }

    // void SetMaterialEmission(bool state)
    // {
    //     if (state)
    //     {
    //         targetMaterial.EnableKeyword("_EMISSION");
    //     }
    //     else
    //     {
    //         targetMaterial.DisableKeyword("_EMISSION");
    //     }
    // }
}
