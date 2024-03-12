#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class StoreLightmapTextures
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            MLSProgressBarHelper.StartNewStage("Storing Lightmap Textures...");

            lightmapData.sceneLightingData.lightmapsLight = new Texture2D[LightmapSettings.lightmaps.Length];
            lightmapData.sceneLightingData.lightmapsDirectional = new Texture2D[LightmapSettings.lightmaps.Length];
            lightmapData.sceneLightingData.lightmapsShadowmask = new Texture2D[LightmapSettings.lightmaps.Length];
            lightmapData.sceneLightingData.fogSettings = new StoredLightmapData.FogSettings();

#if BAKERY_INCLUDED
            ftLightmapsStorage ftLightmaps = ftRenderLightmap.FindRenderSettingsStorage();

            lightmapData.sceneLightingData.lightmapsBakeryRNM0 = new Texture2D[ftLightmaps.rnmMaps0.Count];
            lightmapData.sceneLightingData.lightmapsBakeryRNM1 = new Texture2D[ftLightmaps.rnmMaps0.Count];
            lightmapData.sceneLightingData.lightmapsBakeryRNM2 = new Texture2D[ftLightmaps.rnmMaps0.Count];

            for (int i = 0; i < lightmapData.sceneLightingData.lightmapsBakeryRNM0.Length; i++)
            {
                lightmapData.sceneLightingData.lightmapsBakeryRNM0[i] = SaveTexture(i, MLSManager.LightmapType.BakeryRNM0, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);
            }

            for (int i = 0; i < lightmapData.sceneLightingData.lightmapsBakeryRNM1.Length; i++)
            {
                lightmapData.sceneLightingData.lightmapsBakeryRNM1[i] = SaveTexture(i, MLSManager.LightmapType.BakeryRNM1, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);
            }

            for (int i = 0; i < lightmapData.sceneLightingData.lightmapsBakeryRNM2.Length; i++)
            {
                lightmapData.sceneLightingData.lightmapsBakeryRNM2[i] = SaveTexture(i, MLSManager.LightmapType.BakeryRNM2, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);
            }
#endif

            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {                
                lightmapData.sceneLightingData.lightmapsLight[i] = SaveTexture(i, MLSManager.LightmapType.Color, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);
                lightmapData.sceneLightingData.lightmapsDirectional[i] = SaveTexture(i, MLSManager.LightmapType.Directional, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);
                lightmapData.sceneLightingData.lightmapsShadowmask[i] = SaveTexture(i, MLSManager.LightmapType.Shadowmask, lightmapData.sceneLightingData.lightmapName, lightmapData, mainComponent);

                if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Tex"))
                {
                    StoredLightmapData.SkyboxSettings skyboxSettings = new StoredLightmapData.SkyboxSettings();

                    skyboxSettings.skyboxTexture = RenderSettings.skybox.GetTexture("_Tex") as Cubemap;
                    skyboxSettings.exposure = RenderSettings.skybox.GetFloat("_Exposure");
                    skyboxSettings.tintColor = RenderSettings.skybox.GetColor("_Tint");

                    lightmapData.sceneLightingData.skyboxSettings = skyboxSettings;
                }

                lightmapData.sceneLightingData.fogSettings.enabled = RenderSettings.fog;
                lightmapData.sceneLightingData.fogSettings.fogColor = RenderSettings.fogColor;
                lightmapData.sceneLightingData.fogSettings.fogDensity = RenderSettings.fogDensity;

                lightmapData.sceneLightingData.environmentSettings.source = RenderSettings.ambientMode;
                lightmapData.sceneLightingData.environmentSettings.intensityMultiplier = RenderSettings.ambientIntensity;
                lightmapData.sceneLightingData.environmentSettings.ambientColor = RenderSettings.ambientLight;
                lightmapData.sceneLightingData.environmentSettings.skyColor = RenderSettings.ambientSkyColor;
                lightmapData.sceneLightingData.environmentSettings.equatorColor = RenderSettings.ambientEquatorColor;
                lightmapData.sceneLightingData.environmentSettings.groundColor = RenderSettings.ambientGroundColor;

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(LightmapSettings.lightmaps.Length, 0))
                    {
                        yield return null;
                    }
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.SetDirty(lightmapData);
            AssetDatabase.Refresh();

            MLSLightmapDataStoring.stageExecuting = false;
        }

        private Texture2D SaveTexture(int lightmapIndex, MLSManager.LightmapType lightmapType, string lightmapName, StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            string fullStorePath = "";

#if BAKERY_INCLUDED
            ftLightmapsStorage ftLightmaps = ftRenderLightmap.FindRenderSettingsStorage();
#endif

            switch (lightmapType)
            {
                case MLSManager.LightmapType.Color:
                    fullStorePath = FileUtil.GetProjectRelativePath(Application.dataPath + "/Resources/" + mainComponent.systemProperties.storePath + "/" +
                        EditorSceneManager.GetSceneAt(MLSManager.selectedScene).name +
                        "/LightmapLight_" + lightmapName + "_" + lightmapIndex + ".exr");

                    EditorUtility.SetDirty(lightmapData);

                    if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].lightmapColor), fullStorePath))
                    {
                        mainComponent.storedAssetsCount++;
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].lightmapColor));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.Directional:
                    if (LightmapSettings.lightmaps[lightmapIndex].lightmapDir != null)
                    {
                        fullStorePath = FileUtil.GetProjectRelativePath(Application.dataPath + "/Resources/" + mainComponent.systemProperties.storePath + "/" +
                            EditorSceneManager.GetSceneAt(MLSManager.selectedScene).name +
                            "/LightmapDirectional_" + lightmapName + "_" + lightmapIndex + ".png");

                        EditorUtility.SetDirty(lightmapData);

                        if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].lightmapDir), fullStorePath))
                        {
                            mainComponent.storedAssetsCount++;
                        }

                        if (MLSManager.clearDefaultDataFolder)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].lightmapDir));
                        }

                        EditorUtility.SetDirty(lightmapData);
                    }
                    else
                    {
                        Debug.LogFormat("<color=cyan>MLS:</color> No directional maps found.");
                    }
                    break;
                case MLSManager.LightmapType.Shadowmask:
                    if (LightmapSettings.lightmaps[lightmapIndex].shadowMask != null)
                    {
                        fullStorePath = FileUtil.GetProjectRelativePath(Application.dataPath + "/Resources/" + mainComponent.systemProperties.storePath + "/" +
                            EditorSceneManager.GetSceneAt(MLSManager.selectedScene).name +
                            "/LightmapShadowmask_" + lightmapName + "_" + lightmapIndex + ".png");

                        EditorUtility.SetDirty(lightmapData);

                        if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].shadowMask), fullStorePath))
                        {
                            mainComponent.storedAssetsCount++;
                        }

                        if (MLSManager.clearDefaultDataFolder)
                        {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(LightmapSettings.lightmaps[lightmapIndex].shadowMask));
                        }

                        EditorUtility.SetDirty(lightmapData);
                    }
                    else
                    {
                        Debug.LogFormat("<color=cyan>MLS:</color> No shadowmask maps found.");
                    }
                    break;
#if BAKERY_INCLUDED
                case MLSManager.LightmapType.BakerySH:
                    
                    break;
                case MLSManager.LightmapType.BakeryRNM0:
                    fullStorePath = FileUtil.GetProjectRelativePath(Application.dataPath + "/Resources/" + mainComponent.systemProperties.storePath + "/" +
                        EditorSceneManager.GetSceneAt(MLSManager.selectedScene).name +
                        "/LightmapBakeryRNM0_" + lightmapName + "_" + lightmapIndex + ".hdr");

                    EditorUtility.SetDirty(lightmapData);

                    if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps0[lightmapIndex]), fullStorePath))
                    {
                        mainComponent.storedAssetsCount++;
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps0[lightmapIndex]));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.BakeryRNM1:
                    fullStorePath = FileUtil.GetProjectRelativePath(Application.dataPath + "/Resources/" + mainComponent.systemProperties.storePath + "/" +
                        EditorSceneManager.GetSceneAt(MLSManager.selectedScene).name +
                        "/LightmapBakeryRNM1_" + lightmapName + "_" + lightmapIndex + ".hdr");

                    EditorUtility.SetDirty(lightmapData);

                    if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps1[lightmapIndex]), fullStorePath))
                    {
                        mainComponent.storedAssetsCount++;
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps1[lightmapIndex]));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
                case MLSManager.LightmapType.BakeryRNM2:
                    fullStorePath = FileUtil.GetProjectRelativePath(Application.dataPath + "/Resources/" + mainComponent.systemProperties.storePath + "/" +
                        EditorSceneManager.GetSceneAt(MLSManager.selectedScene).name +
                        "/LightmapBakeryRNM2_" + lightmapName + "_" + lightmapIndex + ".hdr");

                    EditorUtility.SetDirty(lightmapData);

                    if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps2[lightmapIndex]), fullStorePath))
                    {
                        mainComponent.storedAssetsCount++;
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(ftLightmaps.rnmMaps2[lightmapIndex]));
                    }

                    EditorUtility.SetDirty(lightmapData);
                    break;
#endif
            }

            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(lightmapData);

            return AssetDatabase.LoadAssetAtPath(fullStorePath, typeof(Texture2D)) as Texture2D;
        }
    }
}
#endif