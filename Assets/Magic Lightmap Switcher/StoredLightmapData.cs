using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLightmapSwitcher
{
    [System.Serializable]
    public class StoredLightmapData : ScriptableObject
    {
        [System.Serializable]
        public class SphericalHarmonics
        {
            public float[] coefficients = new float[27];
            [SerializeField]
            public Vector3 position;
        }

        [System.Serializable]
        public class ReflectionProbes
        {
            [SerializeField]
            public string[] name;
            [SerializeField]
            public Cubemap[] cubeReflectionTexture;
        }

        [System.Serializable]
        public class RendererData
        {
            [SerializeField]
            public MLSObject mlsObject;
            [SerializeField]
            public string objectId;
            [SerializeField]
            public int lightmapIndex;
            [SerializeField]
            public Quaternion rotation;
            [SerializeField]
            public Vector3 position;
            [SerializeField]
            public Vector4 lightmapScaleOffset;  
        }

        [System.Serializable]
        public class TerrainData
        {
            [SerializeField]
            public string objectId;
            [SerializeField]
            public int lightmapIndex;
            [SerializeField]
            public Vector4 lightmapOffsetScale;
        }

        [System.Serializable]
        public class LightSourceData
        {
            [SerializeField]
            public string name;
            [SerializeField]
            public string instanceID;
            [SerializeField]
            public string lightUID;
            [SerializeField]
            public Vector3 position;
            [SerializeField]
            public Quaternion rotation;
            [SerializeField]
            public float intensity;
            [SerializeField]
            public Color color;
            [SerializeField]
            public float temperature;
            [SerializeField]
            public float range;
            [SerializeField]
            public float spotAngle;
            [SerializeField]
            public int shadowType;
            [SerializeField]
            public Vector2 areaSize;
            [SerializeField]
            public float enableTime;
            [SerializeField]
            public float disableTime;
            [SerializeField]
            public MLSControlledLight.ControlType controlType;
        }

        [System.Serializable]
        public class FogSettings
        {
            [SerializeField]
            public bool enabled;
            [SerializeField]
            public Color fogColor;
            [SerializeField]
            public float fogDensity; 
        }

        [System.Serializable]
        public class EnvironmentSettings
        {
            [SerializeField]
            public AmbientMode source;
            [SerializeField]
            public Color ambientColor;
            [SerializeField]
            public Color skyColor;
            [SerializeField]
            public Color equatorColor;
            [SerializeField]
            public Color groundColor;
            [SerializeField]
            public float intensityMultiplier;
        }

        [System.Serializable]
        public class SkyboxSettings
        {
            [SerializeField]
            public bool enabled;
            [SerializeField]
            public Color tintColor;
            [SerializeField]
            public Cubemap skyboxTexture;
            [SerializeField]
            public float exposure;
        }

        [System.Serializable]
        public class CustomBlendableData
        {
            [System.Serializable]
            public class BlendableFloatFieldData
            {                
                [SerializeField]
                public FieldInfo sourceField;
                [SerializeField]
                public string fieldName;
                [SerializeField]
                public float fieldValue;
                [SerializeField]
                public bool foldoutEnabled;
            }

            [System.Serializable]
            public class BlendableCubemapFieldData
            {                
                [SerializeField]
                public FieldInfo sourceField;
                [SerializeField]
                public string fieldName;
                [SerializeField]
                public Cubemap fieldValue;                
                [SerializeField]
                public bool foldoutEnabled;
            }

            [System.Serializable]
            public class BlendableColorFieldData
            {                
                [SerializeField]
                public FieldInfo sourceField;
                [SerializeField]
                public string fieldName;
                [SerializeField]
                public Color fieldValue;                
                [SerializeField]
                public bool foldoutEnabled;
            }

            [SerializeField]
            public string sourceScriptName;
            [SerializeField]
            public string lightmapName;
            [SerializeField]
            public string sourceScriptId;
            [SerializeField]
            public BlendableFloatFieldData[] blendableFloatFieldsDatas;
            [SerializeField]
            public BlendableCubemapFieldData[] blendableCubemapFieldsDatas;
            [SerializeField]
            public BlendableColorFieldData[] blendableColorFieldsDatas;
            [SerializeField]
            public bool foldoutEnabled;
        }

        [System.Serializable]
        public class SceneLightingData
        {
            [SerializeField]
            public string lightmapName;
            [SerializeField]
            public RendererData[] rendererDatas;            
            [SerializeField]
            public TerrainData[] terrainDatas;
            [SerializeField]
            public LightSourceData[] lightSourceDatas;
            [SerializeField]
            public CustomBlendableData[] customBlendableDatas;
            [SerializeField]
            public Texture2D[] lightmapsLight;
            [SerializeField]
            public Texture2D[] lightmapsDirectional;
            [SerializeField]
            public Texture2D[] lightmapsShadowmask;
            [SerializeField]
            public SkyboxSettings skyboxSettings;
#if BAKERY_INCLUDED
            [SerializeField]
            public Texture2D[] lightmapsBakerySH;
            [SerializeField]
            public Texture2D[] lightmapsBakeryRNM0;
            [SerializeField]
            public Texture2D[] lightmapsBakeryRNM1;
            [SerializeField]
            public Texture2D[] lightmapsBakeryRNM2;
#endif
            [SerializeField]
            public ReflectionProbes reflectionProbes;
            [SerializeField]
            public Cubemap[] skyboxReflectionTexture;
            [SerializeField]
            public SphericalHarmonics[] lightProbes;
            [SerializeField]
            public float[] lightProbes1D;
            [SerializeField]
            public int initialLightProbesArrayPosition = 0;
            [SerializeField]
            public FogSettings fogSettings;
            [SerializeField]
            public EnvironmentSettings environmentSettings;
        }

        public string sceneGUID;
        public string dataPrefix;
        public string dataName;
        public int blendingIndex;
        public Vector2 blendingRange;
        public float startValue;
        public SceneLightingData sceneLightingData;
        
        [HideInInspector]
        public MagicLightmapSwitcher.Workflow workflow;
        [HideInInspector]
        public int prevBlendIndex;
        [HideInInspector]
        public Dictionary<string, Cubemap> storedReflectionProbesData;
        [HideInInspector]
        public Dictionary<string, RendererData> rendererDatasDeserialized;
        [HideInInspector]
        public Dictionary<string, TerrainData> terrainDatasDeserialized;
        [HideInInspector]
        public Dictionary<string, LightSourceData> lightSourceDataDeserialized;
        public bool removed;
    }
}
