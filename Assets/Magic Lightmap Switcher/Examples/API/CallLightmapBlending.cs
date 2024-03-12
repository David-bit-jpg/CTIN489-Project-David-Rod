//Import Magic Lightmap Switcher class
using MagicLightmapSwitcher;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class CallLightmapBlending : MonoBehaviour
    {
        public StoredLightingScenario lightingScenario;
        public float blendingLength;

        private RuntimeAPI runtimeAPI;

        // Start is called before the first frame update
        void Start()
        {
            runtimeAPI = new RuntimeAPI();
        }

        // Update is called once per frame
        void Update()
        {
            runtimeAPI.BlendLightmapsCyclic(blendingLength, lightingScenario);
        }
    }
}
