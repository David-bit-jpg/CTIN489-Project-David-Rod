using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VHSFeatureController : MonoBehaviour
{
    public UniversalRendererData rendererData;
    private ScriptableRendererFeature vhsFeature;

    void Start()
    {
        vhsFeature = rendererData.rendererFeatures.Find(feature => feature.name == "VHS_Material");
        if (vhsFeature != null)
        {
            vhsFeature.SetActive(false);
        }
    }

    public void EnableVHSFeature()
    {
        if (vhsFeature != null)
        {
            vhsFeature.SetActive(true);
        }
    }

    public void DisableVHSFeature()
    {
        if (vhsFeature != null)
        {
            vhsFeature.SetActive(false);
        }
    }
}
