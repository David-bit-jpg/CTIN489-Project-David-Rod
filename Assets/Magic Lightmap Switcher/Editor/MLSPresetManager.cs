using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace MagicLightmapSwitcher
{
    static class Extensions
    {
        public static IList<T> Clone<T>(this IList<T> listToClone) where T : System.ICloneable
        {
            return listToClone.Select(item => (T) item.Clone()).ToList();
        }
    }

    public class MLSPresetManager : EditorWindow
    {

#if BAKERY_INCLUDED
        public enum BakeryLightType
        {
            Light,
            Skylight,
            CustomMesh
        }
#endif

        public static MagicLightmapSwitcher magicLightmapSwitcher;
        public static MLSPresetManager presetsManagerWindow;
        public static bool initialized;
        public static string targetScene;
        public static int setActivePreset;
        public static string presetName;
        public static bool directEditing;

        private static Vector2 scrollPos;

        public static void Init(bool forceActiveScene = false)
        {
            presetsManagerWindow = (MLSPresetManager) GetWindow(typeof(MLSPresetManager), false, "Preset Manager");
            presetsManagerWindow.name = "Preset Manager";
            presetsManagerWindow.minSize = new Vector2(500 * EditorGUIUtility.pixelsPerPoint, 150 * EditorGUIUtility.pixelsPerPoint);
            presetsManagerWindow.maxSize = new Vector2(500 * EditorGUIUtility.pixelsPerPoint, 1000 * EditorGUIUtility.pixelsPerPoint);
            presetsManagerWindow.Show();

            if (forceActiveScene)
            {
                magicLightmapSwitcher = RuntimeAPI.GetSwitcherInstanceStatic(EditorSceneManager.GetActiveScene().name);
            }
            else
            {
                magicLightmapSwitcher = RuntimeAPI.GetSwitcherInstanceStatic(targetScene);
            }

            SetMLSDataCantrol(true);
            MLSLightmapDataStoring.presetManager = presetsManagerWindow;

            initialized = true;
        }

        private static void SetMLSDataCantrol(bool value)
        {
            MLSLight[] mlsLights = FindObjectsOfType<MLSLight>();

            for (int i = 0; i < mlsLights.Length; i++)
            {
                mlsLights[i].presetManagerActive = value;
            }
        }

        private void OnDestroy()
        {
            SetMLSDataCantrol(false);
        }

        public static void CreateNewPreset(MagicLightmapSwitcher magicLightmapSwitcher, MagicLightmapSwitcher.SceneLightingPreset copyFrom = null)
        {
            MagicLightmapSwitcher.SceneLightingPreset lightingPreset = new MagicLightmapSwitcher.SceneLightingPreset();

            lightingPreset.name = "New Preset" + "_" + magicLightmapSwitcher.lightingPresets.Count.ToString();

            if (magicLightmapSwitcher.lightingPresets.Count > 0)
            {
                if (copyFrom != null)
                {
                    lightingPreset.lightSourcesSettings = copyFrom.lightSourcesSettings.Select(element => new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings() 
                    {
                        mlsLightUID = element.mlsLightUID,
                        mlsLight = element.mlsLight,
                        light = element.light,
                        lightType = element.lightType,
                        position = element.position,
                        rotation = element.rotation,
                        tempRotation = element.tempRotation,
                        color = element.color,
                        colorTemperature = element.colorTemperature,
                        intensity = element.intensity,
                        indirectMultiplier = element.indirectMultiplier,
                        range = element.range,
                        spotOuterAngle = element.spotOuterAngle,
                        areaWidth = element.areaWidth,
                        areaHeight = element.areaHeight,
                        shadowsType = element.shadowsType,
                        bakedShadowsRadius = element.bakedShadowsRadius,
                        directionalBakedShadowAngle = element.directionalBakedShadowAngle,
                        globalFoldoutEnabled = element.globalFoldoutEnabled,
                        transformFoldoutEnabled = element.transformFoldoutEnabled,
                        settingsFoldoutEnabled = element.settingsFoldoutEnabled,                        
                        #if BAKERY_INCLUDED
                        bakeryDirectLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryDirectLightsSettings(),
                        bakeryPointLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryPointLightsSettings(),
                        bakeryLightMeshesSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings()
                        #endif
                    }).ToList();

                    List<MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings> clonedCustomBlendablesSettings = new List<MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings>(copyFrom.customBlendablesSettings.Count);

                    copyFrom.customBlendablesSettings.ForEach((item) =>
                        {
                        clonedCustomBlendablesSettings.Add(new MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings(item));
                    });

                    lightingPreset.customBlendablesSettings = clonedCustomBlendablesSettings;

                    lightingPreset.gameObjectsSettings = copyFrom.gameObjectsSettings.Select(element => new MagicLightmapSwitcher.SceneLightingPreset.GameObjectSettings()
                    {
                        gameObject = element.gameObject,
                        enabled = element.enabled,
                        instanceId = element.instanceId,
                        rotation = element.rotation,
                        position = element.position,
                        transformFoldoutEnabled = element.transformFoldoutEnabled,
                        globalFoldoutEnabled = element.globalFoldoutEnabled
                    }
                    ).ToList();

                    lightingPreset.skyboxSettings = new MagicLightmapSwitcher.SceneLightingPreset.SkyboxSettings(copyFrom.skyboxSettings);
                    lightingPreset.fogSettings = new MagicLightmapSwitcher.SceneLightingPreset.FogSettings(copyFrom.fogSettings);
                    lightingPreset.environmentSettings = new MagicLightmapSwitcher.SceneLightingPreset.EnvironmentSettings(copyFrom.environmentSettings);

#if BAKERY_INCLUDED
                    lightingPreset.bakeryLightMeshesSettings = new List<MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings>(copyFrom.bakeryLightMeshesSettings);
                    #endif
                }
                else
                {
                    copyFrom = magicLightmapSwitcher.lightingPresets[magicLightmapSwitcher.lightingPresets.Count - 1];
                    CreateNewPreset(magicLightmapSwitcher, copyFrom);
                    return;
                }
            }
            else
            {
                if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Tex"))
                {
                    lightingPreset.skyboxSettings.skyboxTexture = RenderSettings.skybox.GetTexture("_Tex") as Cubemap;
                    lightingPreset.skyboxSettings.exposure = RenderSettings.skybox.GetFloat("_Exposure");
                }

                #if BAKERY_INCLUDED
                BakerySkyLight bakerySkyLight = FindObjectOfType<BakerySkyLight>();

                if (bakerySkyLight != null)
                {
                    lightingPreset.skyboxSettings.bakerySkyLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.SkyboxSettings.BakerySkyLightsSettings();
                    lightingPreset.skyboxSettings.bakerySkyLightsSettings.bakerySky = bakerySkyLight;
                }

                lightingPreset.bakeryLightMeshesSettings = new List<MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings>();
                #endif

                lightingPreset.fogSettings.enabled = RenderSettings.fog;
                lightingPreset.fogSettings.fogColor = RenderSettings.fogColor;
                lightingPreset.fogSettings.fogDensity = RenderSettings.fogDensity;

                lightingPreset.environmentSettings.source = RenderSettings.ambientMode;
                lightingPreset.environmentSettings.intensityMultiplier = RenderSettings.ambientIntensity;
                lightingPreset.environmentSettings.ambientColor = RenderSettings.ambientLight;
                lightingPreset.environmentSettings.skyColor = RenderSettings.ambientSkyColor;
                lightingPreset.environmentSettings.equatorColor = RenderSettings.ambientEquatorColor;
                lightingPreset.environmentSettings.groundColor = RenderSettings.ambientGroundColor;
            }

            magicLightmapSwitcher.lightingPresets.Add(lightingPreset);
        }

        public static void RemoveLightingPresetFromQueue(MagicLightmapSwitcher magicLightmapSwitcher, int index)
        {
            magicLightmapSwitcher.lightingPresets.RemoveAt(index);
        }

        private void DuplicatePreset(int pId)
        {
            magicLightmapSwitcher.lightingPresets[pId].UpdatePresetData();

            CreateNewPreset(magicLightmapSwitcher, magicLightmapSwitcher.lightingPresets[pId]);
            magicLightmapSwitcher.lightingPresets[pId + 1].MatchSceneSettings();
            directEditing = true;
            setActivePreset = magicLightmapSwitcher.lightingPresets.Count - 1;
            Init();
        }

        private void AddLightToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, Light[] lights)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] == null)
                {                    
                    continue;
                }

                if (lights[i].lightmapBakeType == LightmapBakeType.Realtime)
                {
                    Debug.LogFormat("<color=cyan>MLS:</color> Light source \"" + lights[i].name + "\" in Realtime mode skipped.");
                    continue;
                }

                AddLightToPreset(presets, lights[i]);
            }
        }

        private void AddLightToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, Light light)
        {
            if (light.lightmapBakeType == LightmapBakeType.Realtime)
            {
                EditorUtility.DisplayDialog("Magic Lightmap Switcher", "Realtime lights cannot be used in baking lightmaps.", "OK");
                return;
            }

            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].lightSourcesSettings.Find(item => item.light == light) != null)
                {
                    return;
                }

                if (magicLightmapSwitcher.workflow == MagicLightmapSwitcher.Workflow.MultiScene)
                {
                    if (light.gameObject.scene != magicLightmapSwitcher.gameObject.scene)
                    {
                        return;
                    }
                }

                MLSLight mlsLight = light.GetComponent<MLSLight>();
                MLSControlledLight mlsControlledLight = light.GetComponent<MLSControlledLight>();

                if (mlsLight == null)
                {
                    if (mlsControlledLight == null)
                    {
                        mlsLight = light.gameObject.AddComponent<MLSLight>();
                        mlsLight.UpdateGUID();
                    }
                }

                MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings lightSourceSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings();

                lightSourceSettings.mlsLightUID = mlsLight.lightGUID;
                lightSourceSettings.mlsLight = mlsLight;
                lightSourceSettings.light = light;

                if (light.transform.parent != null)
                {
                    lightSourceSettings.position = light.transform.localPosition;
                }
                else
                {
                    lightSourceSettings.position = light.transform.position;
                }

                lightSourceSettings.rotation = TransformUtils.GetInspectorRotation(light.transform);
                lightSourceSettings.color = light.color;
                lightSourceSettings.colorTemperature = light.colorTemperature;
                lightSourceSettings.range = light.range;
                lightSourceSettings.spotOuterAngle = light.spotAngle;
                lightSourceSettings.areaWidth = light.areaSize.x;
                lightSourceSettings.areaHeight = light.areaSize.y;
                lightSourceSettings.lightType = light.type;
                lightSourceSettings.intensity = light.intensity;
                lightSourceSettings.indirectMultiplier = light.bounceIntensity;
                lightSourceSettings.shadowsType = light.shadows;
                lightSourceSettings.bakedShadowsRadius = light.shadowRadius;
                lightSourceSettings.directionalBakedShadowAngle = light.shadowAngle;

                presets[i].lightSourcesSettings.Add(lightSourceSettings);

#if BAKERY_INCLUDED
                AddBakeryLightToPreset(presets[i], light);
#endif
            }
        }

#if BAKERY_INCLUDED
        private void AddBakeryLightToPreset<T>(MagicLightmapSwitcher.SceneLightingPreset preset, T light)
        {
            Light currentLight = null;
            BakeryLightMesh bakeryCustomLightMesh = null;
            BakerySkyLight bakerySkyLight = null;

            if (light.GetType() == typeof(Light))
            {
                currentLight = light as Light;
            }
            else if (light.GetType() == typeof(BakeryLightMesh))
            {
                bakeryCustomLightMesh = light as BakeryLightMesh;
            }
            else if (light.GetType() == typeof(BakeryLightMesh[]))
            {
                BakeryLightMesh[] bakeryLightMeshes = light as BakeryLightMesh[];

                for (int i = 0; i < bakeryLightMeshes.Length; i++)
                {
                    if (bakeryLightMeshes[i] == null)
                    {
                        continue;
                    }

                    AddBakeryLightToPreset(preset, bakeryLightMeshes[i]);
                }
            }
            else if (light.GetType() == typeof(BakerySkyLight))
            {
                bakerySkyLight = light as BakerySkyLight;
            }

            if (currentLight != null)
            {
                switch (currentLight.type)
                {
                    case LightType.Directional:
                        BakeryDirectLight bakeryDirectLight = currentLight.gameObject.GetComponent<BakeryDirectLight>();

                        if (bakeryDirectLight != null)
                        {
                            MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryDirectLightsSettings bakeryDirectLightsSettings =
                                preset.lightSourcesSettings.Find(item => item.light == currentLight).bakeryDirectLightsSettings;

                            if (bakeryDirectLightsSettings == null)
                            {
                                bakeryDirectLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryDirectLightsSettings();

                                bakeryDirectLightsSettings.parentGameObject = currentLight.gameObject;
                                bakeryDirectLightsSettings.bakeryDirect = bakeryDirectLight;
                                bakeryDirectLightsSettings.UID = bakeryDirectLight.UID;
                                bakeryDirectLightsSettings.color = bakeryDirectLight.color;
                                bakeryDirectLightsSettings.intensity = bakeryDirectLight.intensity;
                                bakeryDirectLightsSettings.shadowSpread = bakeryDirectLight.shadowSpread;
                                bakeryDirectLightsSettings.samples = bakeryDirectLight.samples;
                                bakeryDirectLightsSettings.bitmask = bakeryDirectLight.bitmask;
                                bakeryDirectLightsSettings.bakeToIndirect = bakeryDirectLight.bakeToIndirect;
                                bakeryDirectLightsSettings.shadowmask = bakeryDirectLight.shadowmask;
                                bakeryDirectLightsSettings.shadowmaskDenoise = bakeryDirectLight.shadowmaskDenoise;
                                bakeryDirectLightsSettings.indirectIntensity = bakeryDirectLight.indirectIntensity;
                                bakeryDirectLightsSettings.cloudShadow = bakeryDirectLight.cloudShadow;
                                bakeryDirectLightsSettings.cloudShadowTilingX = bakeryDirectLight.cloudShadowTilingX;
                                bakeryDirectLightsSettings.cloudShadowTilingY = bakeryDirectLight.cloudShadowTilingY;
                                bakeryDirectLightsSettings.cloudShadowOffsetX = bakeryDirectLight.cloudShadowOffsetX;
                                bakeryDirectLightsSettings.cloudShadowOffsetY = bakeryDirectLight.cloudShadowOffsetY;

                                preset.lightSourcesSettings.Find(item => item.light == currentLight).bakeryDirectLightsSettings = bakeryDirectLightsSettings;
                            }
                        }
                        break;
                    case LightType.Point:
                    case LightType.Spot:
                        BakeryPointLight bakeryPointLight = currentLight.gameObject.GetComponent<BakeryPointLight>();

                        if (bakeryPointLight != null)
                        {
                            MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryPointLightsSettings bakeryPointLightsSettings =
                            preset.lightSourcesSettings.Find(item => item.light == currentLight).bakeryPointLightsSettings;

                            if (bakeryPointLightsSettings == null)
                            {
                                bakeryPointLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryPointLightsSettings();

                                bakeryPointLightsSettings.parentGameObject = currentLight.gameObject;
                                bakeryPointLightsSettings.bakeryPoint = bakeryPointLight;
                                bakeryPointLightsSettings.UID = bakeryPointLight.UID;
                                bakeryPointLightsSettings.color = bakeryPointLight.color;
                                bakeryPointLightsSettings.intensity = bakeryPointLight.intensity;
                                bakeryPointLightsSettings.shadowSpread = bakeryPointLight.shadowSpread;
                                bakeryPointLightsSettings.cutoff = bakeryPointLight.cutoff;
                                bakeryPointLightsSettings.realisticFalloff = bakeryPointLight.realisticFalloff;
                                bakeryPointLightsSettings.samples = bakeryPointLight.samples;
                                bakeryPointLightsSettings.projMode = bakeryPointLight.projMode;
                                bakeryPointLightsSettings.cookie = bakeryPointLight.cookie;
                                bakeryPointLightsSettings.angle = bakeryPointLight.angle;
                                bakeryPointLightsSettings.innerAngle = bakeryPointLight.innerAngle;
                                bakeryPointLightsSettings.cubemap = bakeryPointLight.cubemap;
                                bakeryPointLightsSettings.iesFile = bakeryPointLight.iesFile;
                                bakeryPointLightsSettings.bitmask = bakeryPointLight.bitmask;
                                bakeryPointLightsSettings.bakeToIndirect = bakeryPointLight.bakeToIndirect;
                                bakeryPointLightsSettings.shadowmask = bakeryPointLight.shadowmask;
                                bakeryPointLightsSettings.indirectIntensity = bakeryPointLight.indirectIntensity;
                                bakeryPointLightsSettings.falloffMinRadius = bakeryPointLight.falloffMinRadius;

                                preset.lightSourcesSettings.Find(item => item.light == currentLight).bakeryPointLightsSettings = bakeryPointLightsSettings;
                            }
                        }
                        break;
                    case LightType.Area:
                    case LightType.Disc:
                        BakeryLightMesh bakeryLightMesh = currentLight.gameObject.GetComponent<BakeryLightMesh>();

                        if (bakeryLightMesh != null)
                        {
                            MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings bakeryLightMeshesSettings =
                            preset.lightSourcesSettings.Find(item => item.light == currentLight).bakeryLightMeshesSettings;

                            if (bakeryLightMeshesSettings == null)
                            {
                                bakeryLightMeshesSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings();

                                bakeryLightMeshesSettings.parentGameObject = currentLight.gameObject;
                                bakeryLightMeshesSettings.bakeryLightMesh = bakeryLightMesh;
                                bakeryLightMeshesSettings.UID = bakeryLightMesh.UID;
                                bakeryLightMeshesSettings.color = bakeryLightMesh.color;
                                bakeryLightMeshesSettings.intensity = bakeryLightMesh.intensity;
                                bakeryLightMeshesSettings.texture = bakeryLightMesh.texture;
                                bakeryLightMeshesSettings.cutoff = bakeryLightMesh.cutoff;
                                bakeryLightMeshesSettings.samples = bakeryLightMesh.samples;
                                bakeryLightMeshesSettings.samples2 = bakeryLightMesh.samples2;
                                bakeryLightMeshesSettings.bitmask = bakeryLightMesh.bitmask;
                                bakeryLightMeshesSettings.selfShadow = bakeryLightMesh.selfShadow;
                                bakeryLightMeshesSettings.bakeToIndirect = bakeryLightMesh.bakeToIndirect;
                                bakeryLightMeshesSettings.indirectIntensity = bakeryLightMesh.indirectIntensity;
                                bakeryLightMeshesSettings.lmid = bakeryLightMesh.lmid;

                                preset.lightSourcesSettings.Find(item => item.light == currentLight).bakeryLightMeshesSettings = bakeryLightMeshesSettings;
                            }
                        }
                        break;
                }
            }
            else if (bakeryCustomLightMesh != null)
            {
                BakeryLightMesh bakeryLightMesh = bakeryCustomLightMesh.gameObject.GetComponent<BakeryLightMesh>();

                if (bakeryLightMesh != null)
                {
                    MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings bakeryLightMeshesSettings =
                            preset.bakeryLightMeshesSettings.Find(item => item.bakeryLightMesh == bakeryLightMesh);

                    if (bakeryLightMeshesSettings == null)
                    {
                        bakeryLightMeshesSettings = new MagicLightmapSwitcher.SceneLightingPreset.LightSourceSettings.BakeryLightMeshesSettings();

                        bakeryLightMeshesSettings.parentGameObject = bakeryLightMesh.gameObject;
                        bakeryLightMeshesSettings.bakeryLightMesh = bakeryLightMesh;
                        bakeryLightMeshesSettings.UID = bakeryLightMesh.UID;
                        bakeryLightMeshesSettings.color = bakeryLightMesh.color;
                        bakeryLightMeshesSettings.intensity = bakeryLightMesh.intensity;
                        bakeryLightMeshesSettings.texture = bakeryLightMesh.texture;
                        bakeryLightMeshesSettings.cutoff = bakeryLightMesh.cutoff;
                        bakeryLightMeshesSettings.samples = bakeryLightMesh.samples;
                        bakeryLightMeshesSettings.samples2 = bakeryLightMesh.samples2;
                        bakeryLightMeshesSettings.bitmask = bakeryLightMesh.bitmask;
                        bakeryLightMeshesSettings.selfShadow = bakeryLightMesh.selfShadow;
                        bakeryLightMeshesSettings.bakeToIndirect = bakeryLightMesh.bakeToIndirect;
                        bakeryLightMeshesSettings.indirectIntensity = bakeryLightMesh.indirectIntensity;
                        bakeryLightMeshesSettings.lmid = bakeryLightMesh.lmid;

                        preset.bakeryLightMeshesSettings.Add(bakeryLightMeshesSettings);
                    }
                }
            }
            else if (bakerySkyLight != null)
            {
                if (preset.skyboxSettings.bakerySkyLightsSettings == null)
                {
                    preset.skyboxSettings.bakerySkyLightsSettings = new MagicLightmapSwitcher.SceneLightingPreset.SkyboxSettings.BakerySkyLightsSettings();
                    preset.skyboxSettings.bakerySkyLightsSettings.bakerySky = bakerySkyLight;
                }
            }
        }
#endif
        private void AddCustomBlendableToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, MLSCustomBlendable[] customBlendables)
        {
            for (int i = 0; i < customBlendables.Length; i++)
            {
                if (customBlendables[i] == null)
                {
                    continue;
                }

                AddCustomBlendableToPreset(presets, customBlendables[i]);
            }
        }

        private void AddCustomBlendableToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, MLSCustomBlendable customBlendable)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].customBlendablesSettings.Find(item => item.sourceScriptId == customBlendable.sourceScriptId) != null)
                {
                    return;
                }

                if (magicLightmapSwitcher.workflow == MagicLightmapSwitcher.Workflow.MultiScene)
                {
                    if (customBlendable.gameObject.scene != magicLightmapSwitcher.gameObject.scene)
                    {
                        return;
                    }
                }

                MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings customBlendablesSettings = new MagicLightmapSwitcher.SceneLightingPreset.CustomBlendablesSettings();

                customBlendable.GetSharedParameters();

                customBlendablesSettings.sourceScript = customBlendable;
                customBlendablesSettings.sourceScriptId = customBlendable.sourceScriptId;

                for (int j = 0; j < customBlendable.blendableFloatFields.Count; j++)
                {
                    customBlendablesSettings.blendableFloatParameters.Add(customBlendable.blendableFloatFields[j].Name);
                    customBlendablesSettings.blendableFloatParametersValues.Add((float) customBlendable.blendableFloatFields[j].GetValue(customBlendable));
                }

                for (int j = 0; j < customBlendable.blendableCubemapParameters.Count; j++)
                {
                    customBlendablesSettings.blendableCubemapParameters.Add(customBlendable.blendableCubemapParameters[j].Name);
                    customBlendablesSettings.blendableCubemapParametersValues.Add((Cubemap) customBlendable.blendableCubemapParameters[j].GetValue(customBlendable));
                }

                for (int j = 0; j < customBlendable.blendableColorParameters.Count; j++)
                {
                    customBlendablesSettings.blendableColorParameters.Add(customBlendable.blendableColorParameters[j].Name);
                    customBlendablesSettings.blendableColorParametersValues.Add((Color) customBlendable.blendableColorParameters[j].GetValue(customBlendable));
                }

                presets[i].customBlendablesSettings.Add(customBlendablesSettings);
            }
        }

        private void AddGameObjectToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, GameObject[] gameObjects)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i] == null)
                {
                    continue;
                }

                AddGameObjectToPreset(presets, gameObjects[i]);
            }
        }

        private void AddGameObjectToPreset(List<MagicLightmapSwitcher.SceneLightingPreset> presets, GameObject gameObject)
        {
            for (int i = 0; i < presets.Count; i++)
            {
                if (presets[i].gameObjectsSettings.Find(item => item.gameObject == gameObject) != null)
                {
                    return;
                }

                MagicLightmapSwitcher.SceneLightingPreset.GameObjectSettings gameObjectSettings = new MagicLightmapSwitcher.SceneLightingPreset.GameObjectSettings();

                gameObjectSettings.gameObject = gameObject;
                gameObjectSettings.instanceId = gameObject.GetHashCode();
                gameObjectSettings.position = gameObject.transform.localPosition;
                gameObjectSettings.rotation = gameObject.transform.rotation;
                gameObjectSettings.enabled = gameObject.activeInHierarchy;

                presets[i].gameObjectsSettings.Add(gameObjectSettings);
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (!initialized)
            {
                Init();
            }

            if (magicLightmapSwitcher == null)
            {
                Init(true); 
            }

            if (!MLSEditorUtils.stylesInitialized)
            {
                MLSEditorUtils.InitStyles();
            }     

            if (directEditing)
            {
                directEditing = false;

                for (int i = 0; i < magicLightmapSwitcher.lightingPresets.Count; i++)
                {
                    magicLightmapSwitcher.lightingPresets[i].foldoutEnabled = false;
                }

                magicLightmapSwitcher.lightingPresets[setActivePreset].foldoutEnabled = true;
            }

            GUILayout.Label("Presets For Scene: " + targetScene, MLSEditorUtils.captionStyle);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create New..."))
                {
                    CreateNewPreset(magicLightmapSwitcher);
                }
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                for (int lp = 0; lp < magicLightmapSwitcher.lightingPresets.Count; lp++)
                {
                    using (new GUILayout.VerticalScope(GUI.skin.box))
                    {
                        magicLightmapSwitcher.lightingPresets[lp].foldoutEnabled =
                            EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].foldoutEnabled, "Preset: " + magicLightmapSwitcher.lightingPresets[lp].name, true, MLSEditorUtils.presetMainFoldout);

                        if (magicLightmapSwitcher.lightingPresets[lp].foldoutEnabled)
                        {
                            EditorGUI.BeginChangeCheck();

                            for (int i = 0; i < magicLightmapSwitcher.lightingPresets.Count; i++)
                            {
                                if (magicLightmapSwitcher.lightingPresets[i] != magicLightmapSwitcher.lightingPresets[lp])
                                {
                                    magicLightmapSwitcher.lightingPresets[i].foldoutEnabled = false;
                                }
                            }

                            if ((focusedWindow != null && presetsManagerWindow != null && focusedWindow.titleContent.text == presetsManagerWindow.name) ||
                                EditorGUIUtility.GetObjectPickerObject() != null ||
                                (EditorWindow.mouseOverWindow != null && EditorWindow.mouseOverWindow.GetType().ToString() == "UnityEditor.ColorPicker"))
                            {
                                magicLightmapSwitcher.lightingPresets[lp].MatchSceneSettings();
                            }
                            else
                            {
                                magicLightmapSwitcher.lightingPresets[lp].UpdatePresetData();
                            }

                            using (new GUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button("Duplicate And Edit", GUILayout.MinWidth(150)))
                                {
                                    DuplicatePreset(lp);
                                }

                                if (GUILayout.Button("Remove", GUILayout.MinWidth(150)))
                                {
                                    magicLightmapSwitcher.lightingPresets.RemoveAt(lp);
                                    continue;
                                }
                            }

                            GUILayout.Space(10);

                            GUILayout.Label("General Settings", MLSEditorUtils.captionStyle);

                            magicLightmapSwitcher.lightingPresets[lp].name = EditorGUILayout.TextField("Name: ", magicLightmapSwitcher.lightingPresets[lp].name);
                            magicLightmapSwitcher.lightingPresets[lp].included = EditorGUILayout.Toggle("Included: ", magicLightmapSwitcher.lightingPresets[lp].included);

                            GUILayout.Space(10);

                            EditorGUI.BeginChangeCheck();

                            #region Game Objects
                            GUILayout.Label("Game Objects", MLSEditorUtils.captionStyle);

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();

                                GameObject selectedGameObject = null;
                                GameObject[] selectedGameObjects = null;

                                if (Selection.activeObject != null)
                                {
                                    if (Selection.gameObjects.Length > 1)
                                    {
                                        selectedGameObjects = new GameObject[Selection.gameObjects.Length];

                                        for (int o = 0; o < Selection.gameObjects.Length; o++)
                                        {
                                            selectedGameObjects[o] = Selection.gameObjects[o];
                                        }
                                    }
                                    else
                                    {
                                        if (Selection.activeGameObject != null)
                                        {
                                            selectedGameObject = Selection.activeGameObject;
                                        }
                                    }
                                }

                                if (selectedGameObject == null && selectedGameObjects == null)
                                {
                                    GUI.enabled = false;
                                }

                                if (GUILayout.Button(selectedGameObject != null ? "Add Selected Object..." : selectedGameObjects != null ? "Add Selected Objects..." : "No Selected Objects", GUILayout.MaxWidth(150)))
                                {
                                    if (selectedGameObject != null)
                                    {
                                        AddGameObjectToPreset(magicLightmapSwitcher.lightingPresets, selectedGameObject);
                                    }
                                    else if (selectedGameObjects != null)
                                    {
                                        AddGameObjectToPreset(magicLightmapSwitcher.lightingPresets, selectedGameObjects);
                                    }

                                }

                                GUI.enabled = true;
                            }

                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                if (magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings.Count == 0)
                                {
                                    EditorGUILayout.HelpBox("There are no Game Objects in the preset.", MessageType.Info);
                                }

                                for (int i = 0; i < magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings.Count; i++)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].globalFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].globalFoldoutEnabled,
                                                    magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].gameObject.name, true);

                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Remove"))
                                            {
                                                for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[j].gameObjectsSettings.RemoveAt(i);
                                                }

                                                continue;
                                            }
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].globalFoldoutEnabled)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].transformFoldoutEnabled =
                                                    EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].transformFoldoutEnabled, "Transform", true);

                                            if (magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].transformFoldoutEnabled)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].position =
                                                    EditorGUILayout.Vector3Field("Position", magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].position);

                                                if (magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].justAdded)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].justAdded = false;
                                                    magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation =
                                                        TransformUtils.GetInspectorRotation(magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].gameObject.transform);
                                                }

                                                magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation =
                                                    EditorGUILayout.Vector3Field("Rotation", magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation);

                                                magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].rotation =
                                                    Quaternion.Euler(
                                                        magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation.x,
                                                        magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation.y,
                                                        magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].tempRotation.z);
                                            }

                                            magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].enabled =
                                                EditorGUILayout.Toggle("Enabled", magicLightmapSwitcher.lightingPresets[lp].gameObjectsSettings[i].enabled);
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Lights Settings
                            GUILayout.Label("Light Sources", MLSEditorUtils.captionStyle);

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();

                                Light selectedLight = null;
                                Light[] selectedLights = null;

                                if (Selection.activeObject != null)
                                {                                    
                                    if (Selection.gameObjects.Length > 1)
                                    {
                                        selectedLights = new Light[Selection.gameObjects.Length];

                                        for (int o = 0; o < Selection.gameObjects.Length; o++)
                                        {
                                            selectedLights[o] = Selection.gameObjects[o].GetComponent<Light>();
                                        }
                                    }
                                    else
                                    {
                                        if (Selection.activeGameObject != null)
                                        {
                                            selectedLight = Selection.activeGameObject.GetComponent<Light>();
                                        }
                                    }
                                }                                

                                if (selectedLight == null && selectedLights == null)
                                {
                                    GUI.enabled = false;
                                }                                

                                if (GUILayout.Button(selectedLight != null ? "Add Selected Light..." : selectedLights != null ? "Add Selected Lights..." : "No Selected Lights", GUILayout.MaxWidth(150)))
                                {
                                    if (selectedLight != null)
                                    {
                                        AddLightToPreset(magicLightmapSwitcher.lightingPresets, selectedLight);
                                    }
                                    else if (selectedLights != null)
                                    {
                                        AddLightToPreset(magicLightmapSwitcher.lightingPresets, selectedLights);
                                    }
                                }

                                GUI.enabled = true;
                            }

                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings.Count == 0)
                                {
                                    EditorGUILayout.HelpBox("There are no Lights in the preset.", MessageType.Info);
                                }

                                for (int i = 0; i < magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings.Count; i++)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        string lightName = "_temp";
                                        bool externalConrol = false;

                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.GetComponent<MLSLight>() == null)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.gameObject.AddComponent<MLSLight>();
                                        }

                                        magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.GetComponent<MLSLight>().lastEditedBy = magicLightmapSwitcher.lightingPresets[lp].name;

                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].mlsLight.exludeFromStoring)
                                        {
                                            lightName = magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.name + " (External control - editing locked)";
                                            externalConrol = true;
                                        }
                                        else
                                        {
                                            lightName = magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.name;
                                        }

                                        using (new GUILayout.HorizontalScope())
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].globalFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].globalFoldoutEnabled,
                                                    lightName, true);

                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Select On Scene"))
                                            {
                                                Selection.activeGameObject = magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.gameObject;
                                            }

                                            if (GUILayout.Button("Remove"))
                                            {
                                                for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                                {
                                                    MLSLight mlsLight = magicLightmapSwitcher.lightingPresets[j].lightSourcesSettings[i].light.gameObject.GetComponent<MLSLight>();

                                                    if (mlsLight != null)
                                                    {
                                                        mlsLight.destroyedFromManager = true;
                                                        DestroyImmediate(magicLightmapSwitcher.lightingPresets[j].lightSourcesSettings[i].light.gameObject.GetComponent<MLSLight>());
                                                    }

                                                    magicLightmapSwitcher.lightingPresets[j].lightSourcesSettings.RemoveAt(i);
                                                }
                                                
                                                continue;
                                            }
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].globalFoldoutEnabled)
                                        {
                                            if (externalConrol)
                                            {
                                                GUI.enabled = false;
                                            }

                                            using (new GUILayout.VerticalScope(GUI.skin.box))
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].transformFoldoutEnabled =
                                                    EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].transformFoldoutEnabled, "Transform", true);

                                                if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].transformFoldoutEnabled)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].position =
                                                        EditorGUILayout.Vector3Field("Position", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].position);
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].rotation =
                                                        EditorGUILayout.Vector3Field("Rotation", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].rotation);
                                                }

                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].settingsFoldoutEnabled =
                                                    EditorGUILayout.Foldout(
                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].settingsFoldoutEnabled,
                                                        "General Settings", 
                                                        true);

                                                if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].settingsFoldoutEnabled)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].lightType =
                                                        (LightType) EditorGUILayout.EnumPopup("Type", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].lightType);

                                                    switch (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].lightType)
                                                    {
                                                        case LightType.Directional:
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color =
                                                                EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity, 0f, 100.0f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier = 0;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].shadowsType =
                                                                (LightShadows) EditorGUILayout.EnumPopup("Shadow Type", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].shadowsType);

                                                            switch (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].shadowsType)
                                                            {
                                                                case LightShadows.Soft:
                                                                    break;
                                                                case LightShadows.Hard:
                                                                case LightShadows.None:
                                                                    GUI.enabled = false;
                                                                    break;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].directionalBakedShadowAngle =
                                                                EditorGUILayout.Slider("Baked Shadow Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].directionalBakedShadowAngle, 0f, 90.0f);
                                                            
                                                            GUI.enabled = true;
                                                            break;
                                                        case LightType.Point:
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range < 0.001f)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range = 0.001f;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity, 0f, 100f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier = 0;
                                                            }
                                                            break;
                                                        case LightType.Spot:
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].spotOuterAngle =
                                                                EditorGUILayout.Slider("Spot Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].spotOuterAngle, 1, 179);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity, 0f, 100f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier < 0)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier = 0;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakedShadowsRadius =
                                                                EditorGUILayout.FloatField("Baked Shadows Radius", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakedShadowsRadius);
                                                            break;
                                                        case LightType.Area:
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].areaWidth =
                                                                EditorGUILayout.FloatField("Width", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].areaWidth);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].areaHeight =
                                                                EditorGUILayout.FloatField("Height", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].areaHeight);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity, 0f, 100f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier);
                                                            break;
                                                        case LightType.Disc:
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range =
                                                                EditorGUILayout.FloatField("Range", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].range);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].areaWidth =
                                                                EditorGUILayout.FloatField("Radius", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].areaWidth);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color =
                                                                 EditorGUILayout.ColorField("Color Filter", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color);

                                                            DrawColorTempSliderBackground(5);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature =
                                                                EditorGUILayout.Slider("Color Temperature", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].colorTemperature, 1000f, 20000f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity =
                                                                EditorGUILayout.Slider("Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity, 0f, 100f);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier =
                                                                EditorGUILayout.FloatField("Indirect Multiplier", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].indirectMultiplier);
                                                            break;
                                                    }                                                    
                                                }
#if BAKERY_INCLUDED
                                                switch (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.type)
                                                {
                                                    case LightType.Directional:
                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.GetComponent<BakeryDirectLight>() == null)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings = null;
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[lp], magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light);
                                                        }

                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.bakeryDirectFoldoutEnabled =
                                                            EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.bakeryDirectFoldoutEnabled, "Bakery Settings", true);

                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.bakeryDirectFoldoutEnabled)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.color =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.intensity =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.shadowSpread =
                                                                EditorGUILayout.FloatField("Shadow Spread", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.shadowSpread);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.samples =
                                                                EditorGUILayout.IntField("Shadow Samples", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.samples);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.bitmask =
                                                                EditorGUILayout.IntField("Bitmask", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.bitmask);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.indirectIntensity =
                                                                EditorGUILayout.FloatField("Indirect Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.indirectIntensity);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadow =
                                                                EditorGUILayout.ObjectField("Texture Projection", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadow, typeof(Texture2D), false) as Texture2D;
                                                        
                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadow != null)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadowTilingX =
                                                                    EditorGUILayout.FloatField("Tiling U", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadowTilingX);

                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadowTilingY =
                                                                    EditorGUILayout.FloatField("Tiling V", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadowTilingY);

                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadowOffsetX =
                                                                   EditorGUILayout.FloatField("Offset X", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadowOffsetX);

                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadowOffsetY =
                                                                    EditorGUILayout.FloatField("Offset Y", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryDirectLightsSettings.cloudShadowOffsetY);
                                                            }
                                                        }
                                                        break;
                                                    case LightType.Point:
                                                    case LightType.Spot:
                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.GetComponent<BakeryPointLight>() == null)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings = null;
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[lp], magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light);
                                                        }

                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.bakeryPointFoldoutEnabled =
                                                            EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.bakeryPointFoldoutEnabled, "Bakery Settings", true);

                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.bakeryPointFoldoutEnabled)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.color =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.intensity =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.shadowSpread =
                                                                EditorGUILayout.FloatField("Shadow Spread", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.shadowSpread);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.realisticFalloff =
                                                                EditorGUILayout.Toggle("Physical  Falloff", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.realisticFalloff);

                                                            if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.realisticFalloff)
                                                            {
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.falloffMinRadius =
                                                                    EditorGUILayout.FloatField("Fallof Min Size", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.falloffMinRadius);
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.projMode =
                                                                (BakeryPointLight.ftLightProjectionMode) EditorGUILayout.EnumPopup("Projection Mask", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.projMode);

                                                            switch (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.projMode)
                                                            {
                                                                case BakeryPointLight.ftLightProjectionMode.Omni:
                                                                    break;
                                                                case BakeryPointLight.ftLightProjectionMode.Cookie:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.cookie =
                                                                        EditorGUILayout.ObjectField("Coockie Texture", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.cookie, typeof(Texture2D), false) as Texture2D;
                                                                    break;
                                                                case BakeryPointLight.ftLightProjectionMode.Cubemap:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.cubemap =
                                                                        EditorGUILayout.ObjectField("Projection Cubemap", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.cubemap, typeof(Cubemap), false) as Cubemap;
                                                                    break;
                                                                case BakeryPointLight.ftLightProjectionMode.IES:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.iesFile =
                                                                        EditorGUILayout.ObjectField("IES File", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.iesFile, typeof(Object), false) as Object;
                                                                    break;
                                                                case BakeryPointLight.ftLightProjectionMode.Cone:
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.angle =
                                                                        EditorGUILayout.Slider("Outer Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.angle, 1.0f, 180.0f);
                                                                    magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.innerAngle =
                                                                        EditorGUILayout.Slider("Inner Angle", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.innerAngle, 0.0f, 100.0f);
                                                                    break;
                                                            }

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.samples =
                                                                EditorGUILayout.IntField("Shadow Samples", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.samples);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.bitmask =
                                                                EditorGUILayout.IntField("Bitmask", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.bitmask);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.indirectIntensity =
                                                                EditorGUILayout.FloatField("Indirect Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryPointLightsSettings.indirectIntensity);
                                                        }
                                                        break;
                                                    case LightType.Area:
                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light.GetComponent<BakeryLightMesh>() == null)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings = null;
                                                            continue;
                                                        }
                                                        else
                                                        {
                                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[lp], magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].light);
                                                        }

                                                        magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.bakeryLightMeshFoldoutEnabled =
                                                            EditorGUILayout.Foldout(magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.bakeryLightMeshFoldoutEnabled, "Bakery Settings", true);

                                                        if (magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.bakeryLightMeshFoldoutEnabled)
                                                        {
                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.color =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].color;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.intensity =
                                                                magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].intensity;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.texture =
                                                                        EditorGUILayout.ObjectField(
                                                                            "Texture", 
                                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.texture, typeof(Texture2D), false) as Texture2D;

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.cutoff =
                                                                    EditorGUILayout.FloatField("Cutoff", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.cutoff);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.samples =
                                                                    EditorGUILayout.IntField("Samples Near", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.samples);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.samples2 =
                                                                    EditorGUILayout.IntField("Samples Far", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.samples2);                                                            

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.bitmask =
                                                                EditorGUILayout.IntField("Bitmask", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.bitmask);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.selfShadow =
                                                                EditorGUILayout.Toggle("Self Shadow", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.selfShadow);

                                                            magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.indirectIntensity =
                                                                EditorGUILayout.FloatField("Indirect Intensity", magicLightmapSwitcher.lightingPresets[lp].lightSourcesSettings[i].bakeryLightMeshesSettings.indirectIntensity);
                                                        }
                                                        break;
                                                }
                                                
#endif
                                            }

                                            GUI.enabled = true;
                                        }
                                    }
                                }
                            }
                            #endregion

#if BAKERY_INCLUDED
                            #region Bakery Light Meshes
                            GUILayout.Label("Bakery Light Meshes", MLSEditorUtils.captionStyle);

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();

                                BakeryLightMesh selectedMesh = null;
                                BakeryLightMesh[] selectedMeshes = null;

                                if (Selection.activeObject != null)
                                {
                                    if (Selection.gameObjects.Length > 1)
                                    {
                                        selectedMeshes = new BakeryLightMesh[Selection.gameObjects.Length];

                                        for (int o = 0; o < Selection.gameObjects.Length; o++)
                                        {
                                            BakeryLightMesh lightMesh = Selection.gameObjects[o].GetComponent<BakeryLightMesh>();

                                            if (Selection.gameObjects[o].GetComponent<Light>() == null)
                                            {
                                                selectedMeshes[o] = lightMesh;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Selection.activeGameObject != null)
                                        {
                                            BakeryLightMesh lightMesh = Selection.activeGameObject.GetComponent<BakeryLightMesh>();

                                            if (Selection.activeGameObject.GetComponent<Light>() == null)
                                            {
                                                selectedMesh = Selection.activeGameObject.GetComponent<BakeryLightMesh>();
                                            }
                                        }
                                    }
                                }

                                if (selectedMesh == null && selectedMeshes == null)
                                {
                                    GUI.enabled = false;
                                }

                                if (GUILayout.Button(selectedMesh != null ? "Add Selected Object..." : selectedMeshes != null ? "Add Selected Objects..." : "No Selected Objects", GUILayout.MaxWidth(150)))
                                {
                                    if (selectedMesh != null)
                                    {
                                        for (int p = 0; p < magicLightmapSwitcher.lightingPresets.Count; p++)
                                        {
                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[p], selectedMesh);
                                        }                                        
                                    }
                                    else if (selectedMeshes != null)
                                    {
                                        for (int p = 0; p < magicLightmapSwitcher.lightingPresets.Count; p++)
                                        {
                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[p], selectedMeshes);
                                        }
                                    }
                                }

                                GUI.enabled = true;
                            } 
                            
                            using (new GUILayout.VerticalScope(GUI.skin.box)) 
                            {
                                if (magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings.Count == 0)
                                {
                                    EditorGUILayout.HelpBox("There are no Bakery Light Meshes in the preset.", MessageType.Info);
                                }

                                for (int i = 0; i < magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings.Count; i++)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        if (magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMesh == null)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings.RemoveAt(i);
                                            continue;
                                        }

                                        using (new GUILayout.HorizontalScope())
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMeshFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMeshFoldoutEnabled,
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMesh.name, true);

                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Select On Scene"))
                                            {
                                                Selection.activeGameObject = magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMesh.gameObject;
                                            }

                                            if (GUILayout.Button("Remove"))
                                            {
                                                for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[j].bakeryLightMeshesSettings.RemoveAt(i);
                                                }

                                                continue;
                                            }
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bakeryLightMeshFoldoutEnabled)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].color =
                                                EditorGUILayout.ColorField(
                                                    "Color",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].color);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].intensity =
                                                EditorGUILayout.FloatField(
                                                    "Intensity",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].intensity);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].texture =
                                                EditorGUILayout.ObjectField(
                                                    "Texture",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].texture, typeof(Texture2D), false) as Texture2D;

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].cutoff =
                                                EditorGUILayout.FloatField(
                                                    "Cutoff",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].cutoff);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].samples =
                                                EditorGUILayout.IntField(
                                                    "Samples Near",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].samples);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].samples2 =
                                                EditorGUILayout.IntField(
                                                    "Samples Far",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].samples2);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bitmask =
                                                EditorGUILayout.IntField(
                                                    "Bitmask",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].bitmask);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].selfShadow =
                                                EditorGUILayout.Toggle(
                                                    "Selfshadow",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].selfShadow);

                                            magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].indirectIntensity =
                                                EditorGUILayout.FloatField(
                                                    "Indirect Intensity",
                                                    magicLightmapSwitcher.lightingPresets[lp].bakeryLightMeshesSettings[i].indirectIntensity);
                                        }
                                    }
                                }
                            }
                            #endregion
#endif

                            #region Custom Blendable
                            GUILayout.Label("Custom Blendables", MLSEditorUtils.captionStyle);

                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();

                                MLSCustomBlendable selectedCustomBlendable = null;
                                MLSCustomBlendable[] selectedCustomBlendables = null;

                                if (Selection.activeObject != null)
                                {
                                    if (Selection.gameObjects.Length > 1)
                                    {
                                        selectedCustomBlendables = new MLSCustomBlendable[Selection.gameObjects.Length];

                                        for (int o = 0; o < Selection.gameObjects.Length; o++)
                                        {
                                            selectedCustomBlendables[o] = Selection.gameObjects[o].GetComponent<MLSCustomBlendable>();
                                        }
                                    }
                                    else
                                    {
                                        if (Selection.activeGameObject != null)
                                        {
                                            selectedCustomBlendable = Selection.activeGameObject.GetComponent<MLSCustomBlendable>();
                                        }
                                    }
                                }

                                if (selectedCustomBlendable == null && selectedCustomBlendables == null)
                                {
                                    GUI.enabled = false;
                                }

                                if (GUILayout.Button(selectedCustomBlendable != null ? "Add Selected Object..." : selectedCustomBlendables != null ? "Add Selected Objects..." : "No Selected Objects", GUILayout.MaxWidth(150)))
                                {
                                    if (selectedCustomBlendable != null)
                                    {
                                        AddCustomBlendableToPreset(magicLightmapSwitcher.lightingPresets, selectedCustomBlendable);
                                    }
                                    else if (selectedCustomBlendables != null)
                                    {
                                        AddCustomBlendableToPreset(magicLightmapSwitcher.lightingPresets, selectedCustomBlendables);
                                    }
                                    
                                }

                                GUI.enabled = true;
                            }

                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                if (magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings.Count == 0)
                                {
                                    EditorGUILayout.HelpBox("There are no Custom Blendable objects in the preset.", MessageType.Info);
                                }

                                for (int i = 0; i < magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings.Count; i++)
                                {
                                    if (magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].sourceScript == null)
                                    {
                                        for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                        {
                                            magicLightmapSwitcher.lightingPresets[j].customBlendablesSettings.RemoveAt(i);
                                        }

                                        continue;
                                    }

                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        using (new GUILayout.HorizontalScope())
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].globalFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].globalFoldoutEnabled,
                                                    magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].sourceScript.name, true);

                                            GUILayout.FlexibleSpace();

                                            if (GUILayout.Button("Remove"))
                                            {
                                                for (int j = 0; j < magicLightmapSwitcher.lightingPresets.Count; j++)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[j].customBlendablesSettings.RemoveAt(i);
                                                }

                                                continue;
                                            }
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].globalFoldoutEnabled)
                                        {
                                            for (int j = 0; j < magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableFloatParameters.Count; j++)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableFloatParametersValues[j] =
                                                    EditorGUILayout.FloatField(
                                                        magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableFloatParameters[j],
                                                        magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableFloatParametersValues[j]);
                                            }

                                            for (int j = 0; j < magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableColorParameters.Count; j++)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableColorParametersValues[j] =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent(magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableColorParameters[j]),
                                                        magicLightmapSwitcher.lightingPresets[lp].customBlendablesSettings[i].blendableColorParametersValues[j],
                                                        false, false, true);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Lighting Settings
                            GUILayout.Label("General Lighting", MLSEditorUtils.captionStyle);

                            using (new GUILayout.VerticalScope(GUI.skin.box))
                            {
                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.globalFoldoutEnabled =
                                        EditorGUILayout.Foldout(
                                            magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.globalFoldoutEnabled,
                                            "Skybox Settings", true);

                                if (magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.globalFoldoutEnabled)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.skyboxTexture =
                                            EditorGUILayout.ObjectField("Skybox Texture", magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.skyboxTexture, typeof(Cubemap), false) as Cubemap;
                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.exposure =
                                            EditorGUILayout.FloatField("Exposure", magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.exposure);
                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.tintColor =
                                            EditorGUILayout.ColorField("Tint", magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.tintColor);

#if BAKERY_INCLUDED
                                        BakerySkyLight bakerySkyLight = FindObjectOfType<BakerySkyLight>();

                                        if (bakerySkyLight == null)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings = null;
                                            continue;
                                        }
                                        else
                                        {
                                            AddBakeryLightToPreset(magicLightmapSwitcher.lightingPresets[lp], bakerySkyLight);
                                        }

                                        if (magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings != null)
                                        {
                                            magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bakerySkylightFoldoutEnabled =
                                                EditorGUILayout.Foldout(
                                                    magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bakerySkylightFoldoutEnabled,
                                                    "Bakery Sky Settings", true);

                                            if (magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bakerySkylightFoldoutEnabled)
                                            {
                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.color =
                                                    EditorGUILayout.ColorField(
                                                        "Color",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.color);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.intensity =
                                                    EditorGUILayout.FloatField(
                                                        "Intensity",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.intensity);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.cubemap =
                                                    EditorGUILayout.ObjectField(
                                                        "Skybox Texture", 
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.cubemap, typeof(Cubemap), false) as Cubemap;

                                                if (magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.cubemap != null)
                                                {
                                                    magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.correctRotation =
                                                        EditorGUILayout.Toggle(
                                                            "Correct Rotation",
                                                            magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.correctRotation);
                                                }

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.samples =
                                                    EditorGUILayout.IntField(
                                                        "Samples",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.samples);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.hemispherical =
                                                    EditorGUILayout.Toggle(
                                                        "Hemispherical",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.hemispherical);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bitmask =
                                                    EditorGUILayout.IntField(
                                                        "Bitmask",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.bitmask);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.indirectIntensity =
                                                    EditorGUILayout.FloatField(
                                                        "Indirect Intensty",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.indirectIntensity);

                                                magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.tangentSH =
                                                    EditorGUILayout.Toggle(
                                                        "Tangent SH",
                                                        magicLightmapSwitcher.lightingPresets[lp].skyboxSettings.bakerySkyLightsSettings.tangentSH);
                                            }
                                        }
#endif
                                    }
                                }

                                magicLightmapSwitcher.lightingPresets[lp].fogSettings.globalFoldoutEnabled =
                                        EditorGUILayout.Foldout(
                                            magicLightmapSwitcher.lightingPresets[lp].fogSettings.globalFoldoutEnabled,
                                            "Fog Settings", true);

                                if (magicLightmapSwitcher.lightingPresets[lp].fogSettings.globalFoldoutEnabled)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        magicLightmapSwitcher.lightingPresets[lp].fogSettings.enabled =
                                            EditorGUILayout.Toggle("Fog Enabled", magicLightmapSwitcher.lightingPresets[lp].fogSettings.enabled);
                                        magicLightmapSwitcher.lightingPresets[lp].fogSettings.fogColor =
                                            EditorGUILayout.ColorField("Color", magicLightmapSwitcher.lightingPresets[lp].fogSettings.fogColor);
                                        magicLightmapSwitcher.lightingPresets[lp].fogSettings.fogDensity =
                                            EditorGUILayout.FloatField("Density", magicLightmapSwitcher.lightingPresets[lp].fogSettings.fogDensity);
                                    }
                                }

                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.globalFoldoutEnabled =
                                        EditorGUILayout.Foldout(
                                            magicLightmapSwitcher.lightingPresets[lp].environmentSettings.globalFoldoutEnabled,
                                            "Environment Settings", true);

                                if (magicLightmapSwitcher.lightingPresets[lp].environmentSettings.globalFoldoutEnabled)
                                {
                                    using (new GUILayout.VerticalScope(GUI.skin.box))
                                    {
                                        magicLightmapSwitcher.lightingPresets[lp].environmentSettings.source =
                                            (AmbientMode) EditorGUILayout.EnumPopup("Source", magicLightmapSwitcher.lightingPresets[lp].environmentSettings.source);

                                        switch (magicLightmapSwitcher.lightingPresets[lp].environmentSettings.source)
                                        {
                                            case AmbientMode.Trilight:
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.skyColor =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent("Sky Color"), magicLightmapSwitcher.lightingPresets[lp].environmentSettings.skyColor, true, false, true);
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.equatorColor =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent("Equator Color"), magicLightmapSwitcher.lightingPresets[lp].environmentSettings.equatorColor, true, false, true);
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.groundColor =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent("Ground Color"), magicLightmapSwitcher.lightingPresets[lp].environmentSettings.groundColor, true, false, true);
                                                break;
                                            case AmbientMode.Flat:
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.skyColor =
                                                    EditorGUILayout.ColorField(
                                                        new GUIContent("Ambient Color"), magicLightmapSwitcher.lightingPresets[lp].environmentSettings.skyColor, true, false, true);
                                                break;
                                            case AmbientMode.Skybox:
                                                magicLightmapSwitcher.lightingPresets[lp].environmentSettings.intensityMultiplier =
                                                    EditorGUILayout.Slider("Intensity Multiplier", magicLightmapSwitcher.lightingPresets[lp].environmentSettings.intensityMultiplier, 0f, 8f);
                                                break;
                                        }
                                    }
                                }

                                //magicLightmapSwitcher.lightingPresets[lp].lightmapParameters =
                                //    EditorGUILayout.ObjectField("Lightmap Parameters", magicLightmapSwitcher.lightingPresets[lp].lightmapParameters, typeof(LightmapParameters), false) as LightmapParameters;
                            }
                            #endregion

                            if (EditorGUI.EndChangeCheck())
                            {
                                magicLightmapSwitcher.lightingPresets[lp].MatchSceneSettings();
                            }
                        }

                        GUILayout.Space(10);

                        using (new GUILayout.HorizontalScope())
                        {
                            if (!magicLightmapSwitcher.lightingPresets[lp].foldoutEnabled)
                            {
                                GUILayout.FlexibleSpace();

                                if (GUILayout.Button("Load", GUILayout.MaxWidth(80)))
                                {
                                    magicLightmapSwitcher.lightingPresets[lp].MatchSceneSettings();
                                }

                                if (GUILayout.Button("Duplicate", GUILayout.MaxWidth(80)))
                                {
                                    DuplicatePreset(lp);
                                }

                                if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
                                {
                                    magicLightmapSwitcher.lightingPresets.RemoveAt(lp);
                                }
                            }
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        static void DrawColorTempSliderBackground(int sizeCorrection)
        {
            Rect test = new Rect(GUILayoutUtility.GetLastRect());
            GUI.Box(new Rect(test.x + 59 + sizeCorrection, test.y + 20, test.width - 20, test.height),
                CreateKelvinGradientTexture("cTemp", 367 - sizeCorrection, (int) test.height, 1000f, 20000f, 2));
        }

        static Texture2D CreateKelvinGradientTexture(string name, int width, int height, float minKelvin, float maxKelvin, float current)
        {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false, true)
            {
                name = name,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[width * height];

            float mappedMax = Mathf.Pow(maxKelvin, 1f / current);
            float mappedMin = Mathf.Pow(minKelvin, 1f / current);

            for (int i = 0; i < width; i++)
            {
                float pixelfrac = i / (float)(width - 1);
                float mappedValue = (mappedMax - mappedMin) * pixelfrac + mappedMin;
                float kelvin = Mathf.Pow(mappedValue, current);
                Color kelvinColor = Mathf.CorrelatedColorTemperatureToRGB(kelvin);
                for (int j = 0; j < height; j++)
                    pixels[j * width + i] = kelvinColor.gamma;
            }

            texture.SetPixels32(pixels);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }
    }
}