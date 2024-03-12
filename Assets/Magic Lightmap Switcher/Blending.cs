using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MagicLightmapSwitcher
{
    public class Blending
    {
        private static bool propertyIdsInitialized = false;

        public static int _MLS_ENABLE_LIGHTMAPS_BLENDING;
        public static int _MLS_ENABLE_REFLECTIONS_BLENDING;
        public static int _MLS_ENABLE_SKY_CUBEMAPS_BLENDING;
        public static int _MLS_REFLECTIONS_FLAG;
        public static int _MLS_Lightmap_Color_Blend_From;
        public static int _MLS_Lightmap_Color_Blend_To;
        public static int _MLS_Lightmap_Directional_Blend_From;
        public static int _MLS_Lightmap_Directional_Blend_To;
        public static int _MLS_Lightmap_ShadowMask_Blend_From;
        public static int _MLS_Lightmap_ShadowMask_Blend_To;
        public static int _MLS_Reflection_Blend_From_0;
        public static int _MLS_Reflection_Blend_To_0;
        public static int _MLS_Reflection_Blend_From_1;
        public static int _MLS_Reflection_Blend_To_1;
        public static int _MLS_Reflection_Probe_0_Box;
        public static int _MLS_Reflection_Probe_1_Box;
        public static int _MLS_Reflection_Probe_0_Pos;
        public static int _MLS_Reflection_Probe_1_Pos;
        public static int _MLS_Lightmaps_Blend_Factor;
        public static int _MLS_Reflections_Blend_Factor;
        public static int _MLS_Sky_Cubemap_Blend_Factor;
        public static int _MLS_Sky_Cubemap_Blend_From;
        public static int _MLS_Sky_Cubemap_Blend_To;
        public static int _MLS_Sky_Blend_From_Exposure;
        public static int _MLS_Sky_Blend_To_Exposure;
        public static int _MLS_Sky_Blend_From_Tint;
        public static int _MLS_Sky_Blend_To_Tint;
        public static int _MLS_SkyReflection_Blend_From;
        public static int _MLS_SkyReflection_Blend_To;

#if BAKERY_INCLUDED
        public static int _MLS_BakeryRNM0_From;
        public static int _MLS_BakeryRNM0_To;
        public static int _MLS_BakeryRNM1_From;
        public static int _MLS_BakeryRNM1_To;
        public static int _MLS_BakeryRNM2_From;
        public static int _MLS_BakeryRNM2_To;
#endif

        public static Dictionary<string, BlendingOperationalData> blendingOperationalDatas = new Dictionary<string, BlendingOperationalData>();
        private static List<MagicLightmapSwitcher.AffectedObject> resultStaticAffectedObjects;
        private static List<MagicLightmapSwitcher.AffectedObject> resultDynamicAffectedObjects;
        private static List<MLSLight> resultAffectedLights;
        private static bool lightProbesArrayProcessing;
        private static Queue<BlendProbesThreadData> blendProbesThreadsQueue = new Queue<BlendProbesThreadData>();
        private static Queue<ProbesReplacingThreadData> probesReplacingThreadsQueue = new Queue<ProbesReplacingThreadData>();        
        private static ProbesReplacingThreadData lastReplacedProbesData = new ProbesReplacingThreadData();

        public class BlendingOperationalData
        {
            public string sceneName;
            public int loadIndex;
            public int lightProbesArrayIndex;
        }

        public class BlendProbesThreadData
        {
            public MagicLightmapSwitcher switcherInstance;
            public int lightProbesArrayPosition;
            public float[] blendFromArray;
            public float[] blendToArray;
            public SphericalHarmonicsL2[] sphericalHarmonicsArray;
            public float blendFactor;
        }

        public class ProbesReplacingThreadData
        {
            public MagicLightmapSwitcher switcherInstance;
            public SphericalHarmonicsL2[] sphericalHarmonicsArray;
            public BlendProbesThreadData lastProbesData;
        }

        public static void InitiShaderProperties()
        {
            if (!propertyIdsInitialized)
            {
                _MLS_ENABLE_LIGHTMAPS_BLENDING = Shader.PropertyToID("_MLS_ENABLE_LIGHTMAPS_BLENDING");
                _MLS_ENABLE_REFLECTIONS_BLENDING = Shader.PropertyToID("_MLS_ENABLE_REFLECTIONS_BLENDING");
                _MLS_ENABLE_SKY_CUBEMAPS_BLENDING = Shader.PropertyToID("_MLS_ENABLE_SKY_CUBEMAPS_BLENDING");
                _MLS_REFLECTIONS_FLAG = Shader.PropertyToID("_MLS_ReflectionsFlag");
                _MLS_Lightmap_Color_Blend_From = Shader.PropertyToID("_MLS_Lightmap_Color_Blend_From");
                _MLS_Lightmap_Color_Blend_To = Shader.PropertyToID("_MLS_Lightmap_Color_Blend_To");
                _MLS_Lightmap_Directional_Blend_From = Shader.PropertyToID("_MLS_Lightmap_Dir_Blend_From");
                _MLS_Lightmap_Directional_Blend_To = Shader.PropertyToID("_MLS_Lightmap_Dir_Blend_To");
                _MLS_Lightmap_ShadowMask_Blend_From = Shader.PropertyToID("_MLS_Lightmap_ShadowMask_Blend_From");
                _MLS_Lightmap_ShadowMask_Blend_To = Shader.PropertyToID("_MLS_Lightmap_ShadowMask_Blend_To");
                _MLS_Reflection_Blend_From_0 = Shader.PropertyToID("_MLS_Reflection_Blend_From_0");
                _MLS_Reflection_Blend_To_0 = Shader.PropertyToID("_MLS_Reflection_Blend_To_0");
                _MLS_Reflection_Blend_From_1 = Shader.PropertyToID("_MLS_Reflection_Blend_From_1");
                _MLS_Reflection_Blend_To_1 = Shader.PropertyToID("_MLS_Reflection_Blend_To_1");                
                _MLS_Lightmaps_Blend_Factor = Shader.PropertyToID("_MLS_Lightmaps_Blend_Factor");
                _MLS_Reflections_Blend_Factor = Shader.PropertyToID("_MLS_Reflections_Blend_Factor");
                _MLS_Sky_Cubemap_Blend_Factor = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_Factor");
                _MLS_Sky_Cubemap_Blend_From = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_From");
                _MLS_Sky_Cubemap_Blend_To = Shader.PropertyToID("_MLS_Sky_Cubemap_Blend_To");
                _MLS_Sky_Blend_From_Exposure = Shader.PropertyToID("_MLS_Sky_Blend_From_Exposure");
                _MLS_Sky_Blend_To_Exposure = Shader.PropertyToID("_MLS_Sky_Blend_To_Exposure");
                _MLS_Sky_Blend_From_Tint = Shader.PropertyToID("_MLS_Sky_Blend_From_Tint");
                _MLS_Sky_Blend_To_Tint = Shader.PropertyToID("_MLS_Sky_Blend_To_Tint");
                _MLS_SkyReflection_Blend_From = Shader.PropertyToID("_MLS_SkyReflection_Blend_From");
                _MLS_SkyReflection_Blend_To = Shader.PropertyToID("_MLS_SkyReflection_Blend_To");

#if BAKERY_INCLUDED
                _MLS_BakeryRNM0_From = Shader.PropertyToID("_MLS_BakeryRNM0_From");
                _MLS_BakeryRNM0_To = Shader.PropertyToID("_MLS_BakeryRNM0_To");
                _MLS_BakeryRNM1_From = Shader.PropertyToID("_MLS_BakeryRNM1_From");
                _MLS_BakeryRNM1_To = Shader.PropertyToID("_MLS_BakeryRNM1_To");
                _MLS_BakeryRNM2_From = Shader.PropertyToID("_MLS_BakeryRNM2_From");
                _MLS_BakeryRNM2_To = Shader.PropertyToID("_MLS_BakeryRNM2_To");
#endif

        propertyIdsInitialized = true;
            }
        }

        public static void UpdateBlendingOperationalData(string targetScene)
        {
#if UNITY_2020_1_OR_NEWER  
            MagicLightmapSwitcher[] magicLightmapSwitchers = GameObject.FindObjectsOfType<MagicLightmapSwitcher>();
            int totalProbesOnScene = 0;

            for (int i = 0; i < magicLightmapSwitchers.Length; i++)
            {
                if (magicLightmapSwitchers[i].availableScenarios.Count > 0)
                {
                    totalProbesOnScene += magicLightmapSwitchers[i].availableScenarios[0].blendableLightmaps[0].lightingData.sceneLightingData.initialLightProbesArrayPosition;
                    magicLightmapSwitchers[i].availableScenarios[0].lightProbesArrayPosition = totalProbesOnScene - magicLightmapSwitchers[i].availableScenarios[0].blendableLightmaps[0].lightingData.sceneLightingData.initialLightProbesArrayPosition;
                }
            }
#else
            if (!blendingOperationalDatas.ContainsKey(targetScene))
            {
                BlendingOperationalData blendingOperationalData = new BlendingOperationalData();

                blendingOperationalData.sceneName = targetScene;
                blendingOperationalData.loadIndex = blendingOperationalDatas.Count;
                blendingOperationalData.lightProbesArrayIndex = 0;

                blendingOperationalDatas.Add(targetScene, blendingOperationalData);
            }
#endif
        }

        public static void Blend(MagicLightmapSwitcher switcherInstance, float blendFactor, StoredLightingScenario storedLightmapScenario, string targetScene)
        {
            if (!storedLightmapScenario.selfTestCompleted && !storedLightmapScenario.SelfTest())
            {
                Debug.LogErrorFormat("<color=cyan>MLS:</color> An error was detected in the stored lightmap data. " +
                    "Try reassigning the data in the blending queue. Scenario: " + storedLightmapScenario.name);
                return;
            }
            else
            {
                if (!switcherInstance.storedDataUpdated)
                {
                    switcherInstance.StartCoroutine(switcherInstance.UpdateStoredArray(SceneManager.GetActiveScene().name, true));
                }

                if (!storedLightmapScenario.selfTestSuccess)
                {
                    return;
                }
            }

            //if (Camera.main == null)
            //{
            //    Debug.LogErrorFormat("<color=cyan>MLS:</color> You have not installed the main camera. Tag the main camera with \"MainCamera\".");
            //    return;
            //}

            if (switcherInstance.workflow == MagicLightmapSwitcher.Workflow.MultiScene)
            {
                switcherInstance.staticAffectedObjects.TryGetValue(targetScene, out resultStaticAffectedObjects);
                switcherInstance.dynamicAffectedObjects.TryGetValue(targetScene, out resultDynamicAffectedObjects);
                switcherInstance.storedLights.TryGetValue(targetScene, out resultAffectedLights);
            }
            else
            {
                resultStaticAffectedObjects = switcherInstance.sceneStaticAffectedObjects;
                resultDynamicAffectedObjects = switcherInstance.sceneDynamicAffectedObjects;
                resultAffectedLights = switcherInstance.sceneAffectedLightSources;
            }

            for (int i = 0; i < storedLightmapScenario.blendableLightmaps.Count; i++)
            {  
                if (storedLightmapScenario.targetScene != storedLightmapScenario.blendableLightmaps[i].lightingData.dataPrefix)
                {
                    Debug.LogErrorFormat("<color=cyan>MLS:</color>The \"Blendable Lightmaps Queue\"" +
                        "contains invalid data. Make sure the queue contains the data stored for the current scene.");
                    return;
                }

                if (i < storedLightmapScenario.blendableLightmaps.Count - 2)
                {
                    if (blendFactor >= storedLightmapScenario.blendableLightmaps[i].startValue && blendFactor <= storedLightmapScenario.blendableLightmaps[i + 1].startValue)
                    {
                        storedLightmapScenario.lightingDataFromIndex =
                            storedLightmapScenario.blendableLightmaps[i].blendingIndex;
                        storedLightmapScenario.lightingDataToIndex =
                            storedLightmapScenario.blendableLightmaps[i + 1].blendingIndex;

                        storedLightmapScenario.localBlendFactor =
                            Mathf.Clamp((blendFactor - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataFromIndex].startValue) /
                            (storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].startValue - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataFromIndex].startValue), 0, 1);

                        break;
                    }
                }
                else
                {
                    if (blendFactor >= storedLightmapScenario.blendableLightmaps[i].startValue)
                    {
                        storedLightmapScenario.lightingDataFromIndex = storedLightmapScenario.blendableLightmaps[i].blendingIndex;
                        storedLightmapScenario.lightingDataToIndex = storedLightmapScenario.blendableLightmaps.Count - 1;

                        storedLightmapScenario.localBlendFactor =
                            Mathf.Clamp((blendFactor - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataFromIndex].startValue) /
                            (1 - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataFromIndex].startValue), 0, 1);

                        break;
                    }
                }
            }         

            float reflectionsRangedBlend =
                    Mathf.Clamp((storedLightmapScenario.localBlendFactor - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].reflectionsBlendingRange.x) /
                    (storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].reflectionsBlendingRange.y - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].reflectionsBlendingRange.x), 0, 1);

            float lightmapsRangedBlend =
                    Mathf.Clamp((storedLightmapScenario.localBlendFactor - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].lightmapBlendingRange.x) /
                    (storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].lightmapBlendingRange.y - storedLightmapScenario.blendableLightmaps[storedLightmapScenario.lightingDataToIndex].lightmapBlendingRange.x), 0, 1);

            BlendLightmapsData(switcherInstance, reflectionsRangedBlend, lightmapsRangedBlend, storedLightmapScenario.blendableLightmaps, storedLightmapScenario.lightingDataFromIndex, storedLightmapScenario.lightingDataToIndex);
            BlendCustomData(storedLightmapScenario.localBlendFactor, blendFactor, reflectionsRangedBlend, lightmapsRangedBlend, storedLightmapScenario, storedLightmapScenario.lightingDataFromIndex, storedLightmapScenario.lightingDataToIndex);
            BlendLightProbesData(switcherInstance, storedLightmapScenario, storedLightmapScenario.lightingDataFromIndex, storedLightmapScenario.lightingDataToIndex, lightmapsRangedBlend);
            BlendLightSourcesData(storedLightmapScenario.localBlendFactor, blendFactor, storedLightmapScenario.blendableLightmaps, storedLightmapScenario.lightingDataFromIndex, storedLightmapScenario.lightingDataToIndex);
            BlendGameObjectsData(storedLightmapScenario.localBlendFactor, blendFactor, storedLightmapScenario.blendableLightmaps, storedLightmapScenario.lightingDataFromIndex, storedLightmapScenario.lightingDataToIndex);
            BlendCommonLightingSettings(lightmapsRangedBlend, storedLightmapScenario.blendableLightmaps, storedLightmapScenario.lightingDataFromIndex, storedLightmapScenario.lightingDataToIndex);

            switcherInstance.lastLightmapScenario = storedLightmapScenario;
            switcherInstance.OnBlendingValueChanged[storedLightmapScenario.eventsListId].Invoke(storedLightmapScenario, blendFactor, reflectionsRangedBlend, lightmapsRangedBlend);
        }

        private static void SetReflectionsBlendingState(MagicLightmapSwitcher.AffectedObject targetObject, int val)
        {
            targetObject.SetShaderFloat(_MLS_ENABLE_REFLECTIONS_BLENDING, val);
        }

        private static void BlendReflectionProbes(
            MagicLightmapSwitcher.AffectedObject targetObject,
            List<StoredLightingScenario.LightmapData> storedLightmapDatas,
            List<ReflectionProbeBlendInfo> closestReflectionProbes,
            int fromIndex,
            int toIndex)
        {
            Cubemap blendFrom_0;
            Cubemap blendFrom_1;
            Cubemap blendTo_0;
            Cubemap blendTo_1;

            if (closestReflectionProbes[0].probe == null || closestReflectionProbes[0].probe.mode == ReflectionProbeMode.Realtime)
            {
                return;
            }

            string firstProbe;

            firstProbe = closestReflectionProbes[0].probe.name;

            storedLightmapDatas[fromIndex].lightingData.storedReflectionProbesData.TryGetValue(firstProbe, out blendFrom_0);
            storedLightmapDatas[toIndex].lightingData.storedReflectionProbesData.TryGetValue(firstProbe, out blendTo_0);

            if (blendFrom_0 == null || blendTo_0 == null)
            {
                SetReflectionsBlendingState(targetObject, 0);
            }
            else
            {
                SetReflectionsBlendingState(targetObject, 1);

                targetObject.SetShaderTexture(_MLS_Reflection_Blend_From_0, blendFrom_0);
                targetObject.SetShaderTexture(_MLS_Reflection_Blend_To_0, blendTo_0);                
            }

            if (closestReflectionProbes.Count > 1)
            {
                if (closestReflectionProbes[0].probe == null || closestReflectionProbes[1].probe.mode == ReflectionProbeMode.Realtime)
                {
                    return;
                }

                string secondProbe;

                secondProbe = closestReflectionProbes[1].probe.name;

                storedLightmapDatas[fromIndex].lightingData.storedReflectionProbesData.TryGetValue(secondProbe, out blendFrom_1);
                storedLightmapDatas[toIndex].lightingData.storedReflectionProbesData.TryGetValue(secondProbe, out blendTo_1);

                if (blendFrom_0 == null || blendFrom_1 == null || blendTo_0 == null || blendTo_1 == null)
                {
                    SetReflectionsBlendingState(targetObject, 0);
                }
                else
                {
                    SetReflectionsBlendingState(targetObject, 1);

                    targetObject.SetShaderTexture(_MLS_Reflection_Blend_From_1, blendFrom_1);
                    targetObject.SetShaderTexture(_MLS_Reflection_Blend_To_1, blendTo_1);                    
                }
            }
        }

        private static void BlendSkyboxReflectionProbes(
            MagicLightmapSwitcher.AffectedObject targetObject,
            List<StoredLightingScenario.LightmapData> storedLightmapDatas,
            int fromIndex,
            int toIndex)

        {
            targetObject.SetShaderTexture(_MLS_SkyReflection_Blend_From, storedLightmapDatas[fromIndex].lightingData.sceneLightingData.skyboxReflectionTexture[0]);
            targetObject.SetShaderTexture(_MLS_SkyReflection_Blend_To, storedLightmapDatas[fromIndex].lightingData.sceneLightingData.skyboxReflectionTexture[0]);
            targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 0);
        }

        private static void ProcessReflectionProbes(
            ReflectionProbeUsage reflectionProbeUsage,
            MagicLightmapSwitcher.AffectedObject targetObject,
            List<StoredLightingScenario.LightmapData> storedLightmapDatas,
            int fromIndex,
            int toIndex)
        {
            targetObject.SetShaderInt(_MLS_ENABLE_REFLECTIONS_BLENDING, 1);            

            if (targetObject.meshRenderer != null)
            {
                targetObject.meshRenderer.GetClosestReflectionProbes(targetObject.reflectionProbeBlendInfo);
            }
            else if (targetObject.terrain != null)
            {
                targetObject.terrain.GetClosestReflectionProbes(targetObject.reflectionProbeBlendInfo);
            }

            switch (reflectionProbeUsage)
            {
                case ReflectionProbeUsage.Off:
                    BlendSkyboxReflectionProbes(
                        targetObject,
                        storedLightmapDatas,
                        fromIndex,
                        toIndex);

                    targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 0);
                    break;
                case ReflectionProbeUsage.BlendProbes:
                case ReflectionProbeUsage.Simple:
                    if (targetObject.reflectionProbeBlendInfo.Count > 0)
                    {
                        BlendReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            targetObject.reflectionProbeBlendInfo,
                            fromIndex,
                            toIndex);

                        targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 1);
                    }
                    else
                    {
                        BlendSkyboxReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            fromIndex,
                            toIndex);

                        targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 0);
                    }
                    break;
                case ReflectionProbeUsage.BlendProbesAndSkybox:
                    if (targetObject.reflectionProbeBlendInfo.Count > 0)
                    {
                        BlendReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            targetObject.reflectionProbeBlendInfo,
                            fromIndex,
                            toIndex);

                        BlendSkyboxReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            fromIndex,
                            toIndex);

                        targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 2);
                    }
                    else
                    {
                        BlendSkyboxReflectionProbes(
                            targetObject,
                            storedLightmapDatas,
                            fromIndex,
                            toIndex);

                        targetObject.SetShaderInt(_MLS_REFLECTIONS_FLAG, 0);
                    }
                    break;
            }
        }

        private static void BlendLightmapsData(
            MagicLightmapSwitcher switcherInstance, 
            float reflectionsBlendFactor, 
            float lightmapsBlendFactor, 
            List<StoredLightingScenario.LightmapData> storedLightmapDatas, 
            int fromIndex, int toIndex)
        {
            InitiShaderProperties();

            for (int i = 0; i < resultDynamicAffectedObjects.Count; i++)
            {
                if (resultDynamicAffectedObjects[i].meshRenderer != null)
                {
                    resultDynamicAffectedObjects[i].InitPropertyBlock();
                }
                else
                {
                    resultDynamicAffectedObjects.RemoveAt(i);
                    return;
                }

                #region The functionality is temporarily disabled, as there are suspicions that this causes flickering of objects
#if !UNITY_EDITOR
                    //if (resultStaticAffectedObjects[i].meshRenderer.transform.MLS_IsVisibleFrom(Camera.main))
#endif
                #endregion
                {
                    if (storedLightmapDatas.Count < 3 || (resultDynamicAffectedObjects[i].lastFromIndex != fromIndex ||
                        switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario))
                    {
                        ProcessReflectionProbes(
                            resultDynamicAffectedObjects[i].meshRenderer.reflectionProbeUsage,
                            resultDynamicAffectedObjects[i],
                            storedLightmapDatas,
                            fromIndex,
                            toIndex);
                    }

                    resultDynamicAffectedObjects[i].SetShaderFloat(_MLS_Reflections_Blend_Factor, reflectionsBlendFactor);
                    resultDynamicAffectedObjects[i].SetShaderFloat(_MLS_Reflections_Blend_Factor, reflectionsBlendFactor);
                    resultDynamicAffectedObjects[i].ApplyPropertyBlock();
                }

                resultDynamicAffectedObjects[i].lastFromIndex = fromIndex;
            }

            for (int i = 0; i < resultStaticAffectedObjects.Count; i++)
            {
                if (resultStaticAffectedObjects[i].meshRenderer != null || resultStaticAffectedObjects[i].terrain != null)
                {
                    resultStaticAffectedObjects[i].InitPropertyBlock();
                }
                else
                {
                    resultStaticAffectedObjects.RemoveAt(i);
                    return;
                }

                if (resultStaticAffectedObjects[i].terrain == null)
                {
                    #region The functionality is temporarily disabled, as there are suspicions that this causes flickering of objects
#if !UNITY_EDITOR
                    //if (resultStaticAffectedObjects[i].meshRenderer.transform.MLS_IsVisibleFrom(Camera.main))
#endif
                    #endregion
                    {
                        if (storedLightmapDatas.Count < 3 || (resultStaticAffectedObjects[i].lastFromIndex != fromIndex ||
                            switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario))
                        {
                            ProcessReflectionProbes(
                                resultStaticAffectedObjects[i].meshRenderer.reflectionProbeUsage,
                                resultStaticAffectedObjects[i],
                                storedLightmapDatas,
                                fromIndex,
                                toIndex);
                        }

                        StoredLightmapData.RendererData rendererData;
                        storedLightmapDatas[fromIndex].lightingData.rendererDatasDeserialized.TryGetValue(resultStaticAffectedObjects[i].objectId, out rendererData);

                        if (rendererData == null)
                        {
                            resultStaticAffectedObjects.RemoveAt(i);
                            Debug.LogWarningFormat("<color=cyan>MLS:</color> " +
                                "The object \"" + resultStaticAffectedObjects[i].meshRenderer.name + "\" " +
                                "is not present in the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" lighting data, it is automatically isolated " +
                                "and will not participate in blending or switching lightmaps. \r\n" +
                                "Why did this happen? \r\n" +
                                "The object was active and marked as static during baking of the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" preset, " +
                                "but was deactivated or marked as dynamic in the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" preset. " +
                                "Object \"" + resultStaticAffectedObjects[i].meshRenderer.name + "\" might be getting deactivated by some other script.");
                            return;
                        }

                        if (rendererData.lightmapIndex > -1)
                        {
                            resultStaticAffectedObjects[i].SetShaderInt(_MLS_ENABLE_LIGHTMAPS_BLENDING, 1);

                            if (storedLightmapDatas.Count < 3 || (resultStaticAffectedObjects[i].lastFromIndex != fromIndex ||
                                switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario))
                            {
                                resultStaticAffectedObjects[i].SetShaderTexture(
                                    _MLS_Lightmap_Color_Blend_From,
                                    storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsLight[rendererData.lightmapIndex]);

                                resultStaticAffectedObjects[i].SetShaderTexture(
                                    _MLS_Lightmap_Color_Blend_To,
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsLight[rendererData.lightmapIndex]);

                                if (storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsDirectional.Length > 0 &&
                                    storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsDirectional[rendererData.lightmapIndex] != null &&
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsDirectional.Length > 0 &&
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsDirectional[rendererData.lightmapIndex] != null)
                                {
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_Lightmap_Directional_Blend_From,
                                        storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsDirectional[rendererData.lightmapIndex]);

                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_Lightmap_Directional_Blend_To,
                                        storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsDirectional[rendererData.lightmapIndex]);
                                }

                                if (storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsShadowmask.Length > 0 &&
                                    storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsShadowmask[rendererData.lightmapIndex] != null &&
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsShadowmask.Length > 0 &&
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsShadowmask[rendererData.lightmapIndex] != null)
                                {
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_Lightmap_ShadowMask_Blend_From,
                                        storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsShadowmask[rendererData.lightmapIndex]);

                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_Lightmap_ShadowMask_Blend_To,
                                        storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsShadowmask[rendererData.lightmapIndex]);
                                }

#if BAKERY_INCLUDED
                                if (storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsBakeryRNM0.Length > 0)
                                {
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_BakeryRNM0_From,
                                        storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsBakeryRNM0[rendererData.lightmapIndex]);
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_BakeryRNM0_To,
                                        storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsBakeryRNM0[rendererData.lightmapIndex]);
                                }

                                if (storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsBakeryRNM1.Length > 0)
                                {
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                    _MLS_BakeryRNM1_From,
                                    storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsBakeryRNM1[rendererData.lightmapIndex]);
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_BakeryRNM1_To,
                                        storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsBakeryRNM1[rendererData.lightmapIndex]);
                                }

                                if (storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsBakeryRNM2.Length > 0)
                                {
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                    _MLS_BakeryRNM2_From,
                                    storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsBakeryRNM2[rendererData.lightmapIndex]);
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_BakeryRNM2_To,
                                        storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsBakeryRNM2[rendererData.lightmapIndex]);
                                }
#endif
                            }
                        }
                    }
                }
                else
                {
                    if (resultStaticAffectedObjects[i].terrain.isActiveAndEnabled)
                    {
                        if (storedLightmapDatas.Count < 3 || (resultStaticAffectedObjects[i].lastFromIndex != fromIndex ||
                            switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario))
                        {
                            ProcessReflectionProbes(
                            resultStaticAffectedObjects[i].terrain.reflectionProbeUsage,
                            resultStaticAffectedObjects[i],
                            storedLightmapDatas,
                            fromIndex,
                            toIndex);
                        }

                        StoredLightmapData.TerrainData terrainData;
                        storedLightmapDatas[fromIndex].lightingData.terrainDatasDeserialized.TryGetValue(resultStaticAffectedObjects[i].objectId, out terrainData);

                        if (terrainData == null)
                        {
                            resultStaticAffectedObjects.RemoveAt(i);
                            Debug.LogWarningFormat("<color=cyan>MLS:</color> " +
                                "The object \"" + resultStaticAffectedObjects[i].meshRenderer.name + "\" " +
                                "is not present in the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" lighting data, it is automatically isolated " +
                                "and will not participate in blending or switching lightmaps. \r\n" +
                                "Why did this happen? \r\n" +
                                "The object was active and marked as static during baking of the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" preset, " +
                                "but was deactivated or marked as dynamic in the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" preset. " +
                                "Object \"" + resultStaticAffectedObjects[i].meshRenderer.name + "\" might be getting deactivated by some other script.");
                            return;
                        }

                        if (terrainData.lightmapIndex > -1)
                        {
                            if (storedLightmapDatas.Count < 3 || (resultStaticAffectedObjects[i].lastFromIndex != fromIndex ||
                            switcherInstance.lastLightmapScenario != switcherInstance.currentLightmapScenario))
                            {
                                resultStaticAffectedObjects[i].SetShaderTexture(
                                    _MLS_Lightmap_Color_Blend_From,
                                    storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsLight[terrainData.lightmapIndex]);
                                resultStaticAffectedObjects[i].SetShaderTexture(
                                    _MLS_Lightmap_Color_Blend_To,
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsLight[terrainData.lightmapIndex]);

                                if (storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsDirectional.Length > 0 &&
                                    storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsDirectional[terrainData.lightmapIndex] != null &&
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsDirectional.Length > 0 &&
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsDirectional[terrainData.lightmapIndex] != null)
                                {
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_Lightmap_Directional_Blend_From,
                                        storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsDirectional[terrainData.lightmapIndex]);
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_Lightmap_Directional_Blend_To,
                                        storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsDirectional[terrainData.lightmapIndex]);
                                }

                                if (storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsShadowmask.Length > 0 &&
                                    storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsShadowmask[terrainData.lightmapIndex] != null &&
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsShadowmask.Length > 0 &&
                                    storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsShadowmask[terrainData.lightmapIndex] != null)
                                {
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_Lightmap_ShadowMask_Blend_From,
                                        storedLightmapDatas[fromIndex].lightingData.sceneLightingData.lightmapsShadowmask[terrainData.lightmapIndex]);
                                    resultStaticAffectedObjects[i].SetShaderTexture(
                                        _MLS_Lightmap_ShadowMask_Blend_To,
                                        storedLightmapDatas[toIndex].lightingData.sceneLightingData.lightmapsShadowmask[terrainData.lightmapIndex]);
                                }
                            }
                        }
                    }
                }

                resultStaticAffectedObjects[i].lastFromIndex = fromIndex;
                resultStaticAffectedObjects[i].SetShaderFloat(_MLS_Reflections_Blend_Factor, reflectionsBlendFactor);
                resultStaticAffectedObjects[i].SetShaderFloat(_MLS_Lightmaps_Blend_Factor, lightmapsBlendFactor);
                resultStaticAffectedObjects[i].ApplyPropertyBlock();
            }

            Shader.SetGlobalInt(_MLS_ENABLE_SKY_CUBEMAPS_BLENDING, 1);
            Shader.SetGlobalFloat(_MLS_Sky_Cubemap_Blend_Factor, reflectionsBlendFactor);

            Shader.SetGlobalFloat(
                _MLS_Sky_Blend_From_Exposure,
                QualitySettings.activeColorSpace ==
                ColorSpace.Gamma ? storedLightmapDatas[fromIndex].lightingData.sceneLightingData.skyboxSettings.exposure :
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.skyboxSettings.exposure / 4);
            Shader.SetGlobalFloat(
                _MLS_Sky_Blend_To_Exposure,
                QualitySettings.activeColorSpace ==
                ColorSpace.Gamma ? storedLightmapDatas[toIndex].lightingData.sceneLightingData.skyboxSettings.exposure :
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.skyboxSettings.exposure / 4);

            Shader.SetGlobalColor(_MLS_Sky_Blend_From_Tint, storedLightmapDatas[fromIndex].lightingData.sceneLightingData.skyboxSettings.tintColor);
            Shader.SetGlobalColor(_MLS_Sky_Blend_To_Tint, storedLightmapDatas[toIndex].lightingData.sceneLightingData.skyboxSettings.tintColor);
            Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_From, storedLightmapDatas[fromIndex].lightingData.sceneLightingData.skyboxSettings.skyboxTexture);
            Shader.SetGlobalTexture(_MLS_Sky_Cubemap_Blend_To, storedLightmapDatas[toIndex].lightingData.sceneLightingData.skyboxSettings.skyboxTexture);
        }

        private static void BlendCustomData(float localBlendFactor, float globalBlendFactor, float reflectionsBlendFactor, float lightmapsBlendFactor, StoredLightingScenario storedLightmapScenario, int fromIndex, int toIndex)
        {
            if (storedLightmapScenario.collectedCustomBlendableDatas.Count > 0)
            {
                if (storedLightmapScenario.collectedCustomBlendableDatas.Find(item => item.sourceScript == null) != null)
                {
                    storedLightmapScenario.SynchronizeCustomBlendableData();
                }
                else
                {
                    for (int i = 0; i < storedLightmapScenario.collectedCustomBlendableDatas.Count; i++)
                    {
                        if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableFloatFieldsDatas.Count > 0)
                        {
                            if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableFloatFieldsDatas.Find(item => item.sourceField == null) != null)
                            {
                                storedLightmapScenario.SynchronizeCustomBlendableData();
                            }
                        }

                        if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableColorFieldsDatas.Count > 0)
                        {
                            if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableColorFieldsDatas.Find(item => item.sourceField == null) != null)
                            {
                                storedLightmapScenario.SynchronizeCustomBlendableData();
                            }
                        }

                        if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas.Count > 0)
                        {
                            if (storedLightmapScenario.collectedCustomBlendableDatas[i].blendableCubemapFieldsDatas.Find(item => item.sourceField == null) != null)
                            {
                                storedLightmapScenario.SynchronizeCustomBlendableData();
                            }
                        }
                    }   
                }

                storedLightmapScenario.UpdateCustomBlendableData(localBlendFactor, globalBlendFactor, reflectionsBlendFactor, lightmapsBlendFactor, fromIndex, toIndex, 0);
            }
        }

        private static void BlendLightSourcesData(float localBlendFactor, float blendFactor, List<StoredLightingScenario.LightmapData> storedLightmapDatas, int fromIndex, int toIndex)
        {
            for (int i = 0; i < resultAffectedLights.Count; i++)
            {
                if (!resultAffectedLights[i].enabled)
                {
                    continue;
                }

                StoredLightmapData.LightSourceData lightFrom;
                StoredLightmapData.LightSourceData lightTo;

                storedLightmapDatas[fromIndex].lightingData.lightSourceDataDeserialized.TryGetValue(resultAffectedLights[i].lightGUID, out lightFrom);
                storedLightmapDatas[toIndex].lightingData.lightSourceDataDeserialized.TryGetValue(resultAffectedLights[i].lightGUID, out lightTo);

                if (lightFrom == null || lightTo == null)
                {
                    continue;
                }

                resultAffectedLights[i].sourceLight.transform.position = Vector3.Lerp(
                    lightFrom.position,
                    lightTo.position,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.transform.rotation = Quaternion.Lerp(
                    lightFrom.rotation,
                    lightTo.rotation,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.intensity = Mathf.Lerp(
                    lightFrom.intensity,
                    lightTo.intensity,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.color = Color.Lerp(
                    lightFrom.color,
                    lightTo.color,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.colorTemperature = Mathf.Lerp(
                    lightFrom.temperature,
                    lightTo.temperature,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.range = Mathf.Lerp(
                    lightFrom.range,
                    lightTo.range,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.spotAngle = Mathf.Lerp(
                    lightFrom.spotAngle,
                    lightTo.spotAngle,
                    localBlendFactor);
                resultAffectedLights[i].sourceLight.shadows = 
                    localBlendFactor > resultAffectedLights[i].shadowTypeSwitchValue ? (LightShadows) lightTo.shadowType : (LightShadows) lightFrom.shadowType;
            }
        }

        private static void BlendGameObjectsData(float localBlendFactor, float blendFactor, List<StoredLightingScenario.LightmapData> storedLightmapDatas, int fromIndex, int toIndex)
        {
            for (int i = 0; i < resultStaticAffectedObjects.Count; i++)
            {
                if (resultStaticAffectedObjects[i].terrain != null)
                {
                    continue;
                }

                StoredLightmapData.RendererData rendererDataFrom;
                StoredLightmapData.RendererData rendererDataTo;
                
                storedLightmapDatas[fromIndex].lightingData.rendererDatasDeserialized.TryGetValue(resultStaticAffectedObjects[i].objectId, out rendererDataFrom);
                storedLightmapDatas[toIndex].lightingData.rendererDatasDeserialized.TryGetValue(resultStaticAffectedObjects[i].objectId, out rendererDataTo);

                if (rendererDataFrom == null || rendererDataTo == null)
                {
                    resultStaticAffectedObjects.RemoveAt(i);
                    Debug.LogWarningFormat("<color=cyan>MLS:</color> " +
                        "The object \"" + resultStaticAffectedObjects[i].meshRenderer.name + "\" " +
                        "is not present in the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" lighting data, it is automatically isolated " +
                        "and will not participate in blending or switching lightmaps. \r\n" +
                        "Why did this happen? \r\n" +
                        "The object was active and marked as static during baking of the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" preset, " +
                        "but was deactivated or marked as dynamic in the \"" + storedLightmapDatas[fromIndex].lightingData.name + "\" preset. " +
                        "Object \"" + resultStaticAffectedObjects[i].meshRenderer.name + "\" might be getting deactivated by some other script.");
                    return;
                }

                if (rendererDataFrom.position != rendererDataTo.position)
                {
                    resultStaticAffectedObjects[i].meshRenderer.gameObject.transform.position = Vector3.Lerp(
                        rendererDataFrom.position,
                        rendererDataTo.position,
                        localBlendFactor);
                }

                if (rendererDataFrom.rotation != rendererDataTo.rotation)
                {
                    resultStaticAffectedObjects[i].meshRenderer.gameObject.transform.rotation = Quaternion.Lerp(
                        rendererDataFrom.rotation,
                        rendererDataTo.rotation,
                        localBlendFactor);
                }
            }
        }

        private static void BlendCommonLightingSettings(float blendFactor, List<StoredLightingScenario.LightmapData> storedLightmapDatas, int fromIndex, int toIndex)
        {
            RenderSettings.fogColor = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.fogSettings.fogColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.fogSettings.fogColor,
                blendFactor);
            RenderSettings.fogDensity = Mathf.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.fogSettings.fogDensity,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.fogSettings.fogDensity,
                blendFactor);
            RenderSettings.ambientMode = storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.source;
            RenderSettings.ambientIntensity = Mathf.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.intensityMultiplier,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.intensityMultiplier,
                blendFactor);
            RenderSettings.ambientLight = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.ambientColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.ambientColor,
                blendFactor);
            RenderSettings.ambientSkyColor = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.skyColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.skyColor,
                blendFactor);
            RenderSettings.ambientEquatorColor = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.equatorColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.equatorColor,
                blendFactor);
            RenderSettings.ambientGroundColor = Color.Lerp(
                storedLightmapDatas[fromIndex].lightingData.sceneLightingData.environmentSettings.groundColor,
                storedLightmapDatas[toIndex].lightingData.sceneLightingData.environmentSettings.groundColor,
                blendFactor);
        }

        private static void BlendLightProbesThread(object data)
        {
            BlendProbesThreadData threadData = data as BlendProbesThreadData;

            int counter = 0;
            float[] exit = new float[threadData.blendFromArray.Length];
            float[][] combinedTemp = new float[Mathf.RoundToInt(exit.Length / 27)][];

            Parallel.For(0, threadData.blendFromArray.Length, (i =>
            {
                exit[i] = Mathf.Lerp(
                threadData.blendFromArray[i],
                threadData.blendToArray[i],
                threadData.blendFactor);
            }));

            for (int i = 0; i < exit.Length; i += 27)
            {
                float[] temp = new float[27];
                System.Array.Copy(exit, i, temp, 0, 27);
                combinedTemp[counter] = temp;
                counter++;
            }

            Parallel.For(0, combinedTemp.Length, (i, state) =>
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        threadData.sphericalHarmonicsArray[i][j, k] = combinedTemp[i][j * 9 + k];
                    }
                }
            });

            blendProbesThreadsQueue.Enqueue(threadData);
            //System.GC.Collect(1, GCCollectionMode.Optimized);
        }

        private static void LightProbesReplacingThread(object data)
        {
            ProbesReplacingThreadData threadData = data as ProbesReplacingThreadData;
            SphericalHarmonicsL2[] finalArray = threadData.sphericalHarmonicsArray;

            Array.Copy(
                threadData.lastProbesData.sphericalHarmonicsArray,
                0,
                finalArray,
                threadData.lastProbesData.lightProbesArrayPosition,
                threadData.lastProbesData.sphericalHarmonicsArray.Length);

            probesReplacingThreadsQueue.Enqueue(threadData);
            //System.GC.Collect(1, GCCollectionMode.Optimized);
        }

        private static void BlendLightProbesData(MagicLightmapSwitcher switcherInstance, StoredLightingScenario storedLightmapScenario, int from, int to, float blendFactor)
        {
            if (switcherInstance.stopProbesBlending)
            {
                return;
            }

            if (blendProbesThreadsQueue.Count > 3 ||
                probesReplacingThreadsQueue.Count > 3 ||
                LightmapSettings.lightProbes == null)
            {
                blendProbesThreadsQueue.Clear();
                probesReplacingThreadsQueue.Clear();
                lightProbesArrayProcessing = false;
                return;
            }

            if (probesReplacingThreadsQueue.Count > 0)
            {
                lastReplacedProbesData = probesReplacingThreadsQueue.Dequeue();

                if (lastReplacedProbesData != null && lastReplacedProbesData.sphericalHarmonicsArray != null)
                {
                    if (LightmapSettings.lightProbes.bakedProbes.Length == lastReplacedProbesData.sphericalHarmonicsArray.Length)
                    {
                        LightmapSettings.lightProbes.bakedProbes = lastReplacedProbesData.sphericalHarmonicsArray;
                    }
                }
            }

            if (!lightProbesArrayProcessing)
            {
                lightProbesArrayProcessing = true;

                if (blendProbesThreadsQueue.Count > 0)
                {
                    BlendProbesThreadData lastProbesData = blendProbesThreadsQueue.Dequeue();

                    if (lastProbesData != null)
                    {
                        ProbesReplacingThreadData probesReplacingThreadData = new ProbesReplacingThreadData();

                        probesReplacingThreadData.switcherInstance = switcherInstance;
                        probesReplacingThreadData.lastProbesData = lastProbesData;
                        probesReplacingThreadData.sphericalHarmonicsArray = LightmapSettings.lightProbes.bakedProbes;

                        ThreadPool.QueueUserWorkItem(LightProbesReplacingThread, probesReplacingThreadData);
                    }
                }

                lightProbesArrayProcessing = false;
            }

            BlendProbesThreadData blendProbesThreadData = new BlendProbesThreadData();

            blendProbesThreadData.switcherInstance = switcherInstance;
            blendProbesThreadData.lightProbesArrayPosition = storedLightmapScenario.lightProbesArrayPosition;
            blendProbesThreadData.blendFromArray = storedLightmapScenario.blendableLightmaps[from].lightingData.sceneLightingData.lightProbes1D;
            blendProbesThreadData.blendToArray = storedLightmapScenario.blendableLightmaps[to].lightingData.sceneLightingData.lightProbes1D;
            blendProbesThreadData.sphericalHarmonicsArray = new SphericalHarmonicsL2[storedLightmapScenario.blendableLightmaps[to].lightingData.sceneLightingData.lightProbes.Length];
            blendProbesThreadData.blendFactor = blendFactor;

            ThreadPool.QueueUserWorkItem(BlendLightProbesThread, blendProbesThreadData);

            if (switcherInstance.lightingDataSwitching)
            {
                switcherInstance.lightingDataSwitching = false;
                switcherInstance.StartCoroutine(_DoLightprobesBlendQueue(switcherInstance));
            }
        }
    
        private static IEnumerator _DoLightprobesBlendQueue(MagicLightmapSwitcher switcherInstance)
        {
            while (blendProbesThreadsQueue.Count == 0)
            {
                yield return null;
            }

            BlendProbesThreadData lastProbesData = blendProbesThreadsQueue.Dequeue();

            if (lastProbesData != null)
            {
                ProbesReplacingThreadData probesReplacingThreadData = new ProbesReplacingThreadData();

                probesReplacingThreadData.switcherInstance = switcherInstance;
                probesReplacingThreadData.lastProbesData = lastProbesData;
                probesReplacingThreadData.sphericalHarmonicsArray = LightmapSettings.lightProbes.bakedProbes;

                ThreadPool.QueueUserWorkItem(LightProbesReplacingThread, probesReplacingThreadData);
            }

            while (probesReplacingThreadsQueue.Count == 0)
            {
                yield return null;
            }

            while (probesReplacingThreadsQueue.Count > 0)
            {                
                lastReplacedProbesData = probesReplacingThreadsQueue.Dequeue();

                if (lastReplacedProbesData != null && lastReplacedProbesData.sphericalHarmonicsArray != null)
                {
                    if (LightmapSettings.lightProbes.bakedProbes.Length == lastReplacedProbesData.sphericalHarmonicsArray.Length)
                    {
                        LightmapSettings.lightProbes.bakedProbes = lastReplacedProbesData.sphericalHarmonicsArray;
                    }
                }

                yield return null;
            }
        }
    }
}