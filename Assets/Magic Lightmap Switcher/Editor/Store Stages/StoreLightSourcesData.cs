#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class StoreLightSourcesData
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            if (mainComponent == null)
            {
                mainComponent = RuntimeAPI.GetSwitcherInstanceStatic(EditorSceneManager.GetActiveScene().name);
            }
            
            MLSProgressBarHelper.StartNewStage("Storing Lights Data...");

            yield return null;

            Object[] lightSources = Object.FindObjectsOfType(typeof(Light));

            List< StoredLightmapData.LightSourceData> tempLightsList = new List<StoredLightmapData.LightSourceData>();            

            int counter = 0;

            foreach (Light light in lightSources)
            {
                MLSLight mlsLight = light.GetComponent<MLSLight>();

                if (mlsLight != null && !mlsLight.exludeFromStoring)
                {
                    if (mainComponent.lightingPresets[0].lightSourcesSettings.Find(item => item.mlsLightUID == light.GetComponent<MLSLight>().lightGUID) != null)
                    {
                        if (light.lightmapBakeType == LightmapBakeType.Baked || light.lightmapBakeType == LightmapBakeType.Mixed)
                        {
                            tempLightsList.Add(new StoredLightmapData.LightSourceData());

                            if (tempLightsList.Find(item => item.lightUID == light.GetComponent<MLSLight>().lightGUID) != null) 
                            {
                                light.GetComponent<MLSLight>().UpdateGUID();
                            }

                            tempLightsList[counter].name = light.name;
                            tempLightsList[counter].instanceID = light.GetInstanceID().ToString();
                            tempLightsList[counter].lightUID = light.GetComponent<MLSLight>().lightGUID;
                            tempLightsList[counter].position = light.transform.position;
                            tempLightsList[counter].rotation = light.transform.rotation;
                            tempLightsList[counter].intensity = light.intensity;
                            tempLightsList[counter].color = light.color;
                            tempLightsList[counter].temperature = light.colorTemperature;
                            tempLightsList[counter].range = light.range;
                            tempLightsList[counter].spotAngle = light.spotAngle;
                            tempLightsList[counter].shadowType = (int)light.shadows;
                            tempLightsList[counter].areaSize = light.areaSize;

                            counter++;
                        }
                    }
                    else
                    {
                        if (!MLSLightmapDataStoring.bakingLightingDataQueue)
                        {
                            GameObject.DestroyImmediate(mlsLight);
                        }
                    }
                }

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(lightSources.Length, 0))
                    {
                        yield return null;
                    }
                }
            }

            lightmapData.sceneLightingData.lightSourceDatas = tempLightsList.ToArray();
            MLSLightmapDataStoring.stageExecuting = false;
        }
    }
}
#endif
