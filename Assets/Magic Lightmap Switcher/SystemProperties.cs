using UnityEngine;

namespace MagicLightmapSwitcher
{    
    [System.Serializable]
    public class SystemProperties : ScriptableObject
    {
        [SerializeField]
        public bool standardRPActive;
        [SerializeField]
        public bool universalRPActive;
        [SerializeField]
        public bool highDefinitionRPActive;
        [SerializeField]
        public bool standardRPPatched;
        [SerializeField]
        public bool universalRPPatched;
        [SerializeField]
        public bool highDefinitionRPPatched;
        [SerializeField]
        public bool clearOriginalLightingData;
        [SerializeField]
        public bool batchByLightmapIndex;
        [SerializeField]
        public bool editorRestarted;
        [SerializeField]
        public double prevTimeSinceStartup;
        [SerializeField]
        public string storePath = "/MLS_DATA";
        [SerializeField]
        public bool deferredWarningConfirmed;
        [SerializeField]
        public bool useSwitchingOnly;
    }
}
