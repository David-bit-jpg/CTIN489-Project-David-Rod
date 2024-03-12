#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class StoreReflectionProbesData
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            MLSProgressBarHelper.StartNewStage("Storing Reflection Probes...");

            TextureImporterSettings textureImporterSettings = new TextureImporterSettings();

            textureImporterSettings.npotScale = TextureImporterNPOTScale.ToNearest;
            textureImporterSettings.textureShape = TextureImporterShape.TextureCube;
            textureImporterSettings.alphaSource = TextureImporterAlphaSource.FromInput;
            textureImporterSettings.filterMode = FilterMode.Trilinear;
            textureImporterSettings.mipmapEnabled = true;
            textureImporterSettings.cubemapConvolution = TextureImporterCubemapConvolution.Specular;
            textureImporterSettings.sRGBTexture = true;

            yield return null;

            ReflectionProbe[] sceneReflectionProbes = Object.FindObjectsOfType(typeof(ReflectionProbe)) as ReflectionProbe[];
            lightmapData.sceneLightingData.reflectionProbes = new StoredLightmapData.ReflectionProbes();
            lightmapData.sceneLightingData.reflectionProbes.name = new string[sceneReflectionProbes.Length];
            lightmapData.sceneLightingData.reflectionProbes.cubeReflectionTexture = new Cubemap[sceneReflectionProbes.Length];

            EditorUtility.SetDirty(lightmapData);

            string fullStorePath = "";

            for (int i = 0; i < sceneReflectionProbes.Length; i++)
            {
                fullStorePath = FileUtil.GetProjectRelativePath(Application.dataPath + "/Resources/" + mainComponent.systemProperties.storePath + "/" +
                    EditorSceneManager.GetActiveScene().name +
                    "/ReflectionProbe-" + lightmapData.sceneLightingData.lightmapName + "_" + i + ".exr");

                //EditorUtility.SetDirty(lightmapData);

                if (AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(sceneReflectionProbes[i].bakedTexture), fullStorePath))
                {
                    if (mainComponent.systemProperties.highDefinitionRPActive)
                    {
                        TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(fullStorePath);

                        importer.SetTextureSettings(textureImporterSettings);
                        importer.SaveAndReimport();
                    }

                    if (MLSManager.clearDefaultDataFolder)
                    {
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(sceneReflectionProbes[i].bakedTexture));
                    }
                }

                //EditorUtility.SetDirty(lightmapData);

                if (!sceneReflectionProbes[i].gameObject.name.Contains("MLS"))
                {
                    sceneReflectionProbes[i].gameObject.name = "MLS_" + sceneReflectionProbes[i].gameObject.name + "_" + i;
                }

                lightmapData.sceneLightingData.reflectionProbes.name[i] = sceneReflectionProbes[i].name;
                lightmapData.sceneLightingData.reflectionProbes.cubeReflectionTexture[i] = AssetDatabase.LoadAssetAtPath(fullStorePath, typeof(Cubemap)) as Cubemap;

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(sceneReflectionProbes.Length, 0))
                    {
                        yield return null;
                    }
                }
            }

            AssetDatabase.SaveAssets();

            fullStorePath = FileUtil.GetProjectRelativePath(Application.dataPath + "/Resources/" + mainComponent.systemProperties.storePath + "/" +
                EditorSceneManager.GetActiveScene().name +
                "/SkyboxReflectionProbe-" + lightmapData.sceneLightingData.lightmapName + ".exr");

            GameObject tmpGameObject = new GameObject();
            ReflectionProbe tmpReflection = tmpGameObject.AddComponent<ReflectionProbe>();
            tmpReflection.resolution = RenderSettings.defaultReflectionResolution;
            tmpReflection.clearFlags = UnityEngine.Rendering.ReflectionProbeClearFlags.Skybox;
            tmpReflection.cullingMask = 0;
            tmpReflection.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;

            if (Lightmapping.BakeReflectionProbe(tmpReflection, fullStorePath))
            {
                if (mainComponent.systemProperties.highDefinitionRPActive)
                {
                    TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(fullStorePath);

                    importer.SetTextureSettings(textureImporterSettings);
                    importer.SaveAndReimport();
                }
            }

            lightmapData.sceneLightingData.skyboxReflectionTexture = new Cubemap[1];
            lightmapData.sceneLightingData.skyboxReflectionTexture[0] = tmpReflection.customBakedTexture as Cubemap;

            GameObject.DestroyImmediate(tmpGameObject);

            //AssetDatabase.Refresh();
            //EditorUtility.SetDirty(lightmapData);
            //AssetDatabase.Refresh();

            MLSLightmapDataStoring.stageExecuting = false;
        }
    }
}
#endif