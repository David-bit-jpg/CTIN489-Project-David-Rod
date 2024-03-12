using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MagicLightmapSwitcher
{
    public class RuntimeAPI
    {
        #region Blending
        private float currentLerpTime;
        private StoredLightingScenario currentScenario;
        private MagicLightmapSwitcher currentSwitcherSource;

        public MagicLightmapSwitcher GetSwitcherSource(string targetScene)
        {
            if (currentSwitcherSource == null)
            {
                currentSwitcherSource = GetSwitcherInstanceStatic(targetScene);
            }

            return currentSwitcherSource;
        }

        public static MagicLightmapSwitcher GetSwitcherInstanceStatic(string targetScene)
        {
            MagicLightmapSwitcher resultSource = null;
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene currentTestingScene = SceneManager.GetSceneAt(i);

                if (currentTestingScene.name == targetScene)
                {
                    GameObject[] gameObjects = currentTestingScene.GetRootGameObjects();

                    for (int j = 0; j < gameObjects.Length; j++)
                    {
                        if (gameObjects[j].name == "Magic Tools")
                        {
                            for (int o = 0; o < gameObjects[j].transform.childCount; o++)
                            {
                                MagicLightmapSwitcher switcherObject = gameObjects[j].transform.GetChild(o).GetComponent<MagicLightmapSwitcher>();

                                if (switcherObject != null)
                                {
                                    if (switcherObject.gameObject.scene.name == currentTestingScene.name)
                                    {
                                        resultSource = switcherObject;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (resultSource == null)
            {
                Debug.LogFormat("<color=cyan>MLS:</color>The system was unable to determine the instance of the core component. " +
                    "Make sure you provide the correct Lightmap Scenario.");            
            }

            return resultSource;
        }

        /// <summary>
        /// Runs a blending cycle for the selected lighting scenario.
        /// </summary>
        /// <param name="cycleLength">Blend cycle time.</param>
        /// <param name="scenario">Lighting Scenario Asset.</param>
        public void BlendLightmapsCyclic(float cycleLength, StoredLightingScenario scenario)
        {
            if (scenario == null)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> Wrong lighting scenario asset.");
                return;
            }
            else if (scenario.blendableLightmaps.Count < 2)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> Insufficient data to blending. You must add at least two sets of lighting data to the scenario.");
                return;
            }

            GetSwitcherSource(scenario.targetScene);

            if (currentSwitcherSource == null || !currentSwitcherSource.storedDataUpdated)
            {
                return;
            }

            scenario.SetActive(currentSwitcherSource);

            currentLerpTime += Time.deltaTime;

            if (currentLerpTime > cycleLength)
            {
                currentLerpTime = 0;
            }

            float blendingPercent = currentLerpTime / cycleLength;

            scenario.globalBlendFactor = Mathf.Lerp(0, 1, blendingPercent);
            Blending.Blend(currentSwitcherSource, scenario.globalBlendFactor, scenario, scenario.targetScene);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="speed">Blending length.</param>
        /// <param name="scenario">Lighting Scenario Asset.</param>
        public void BlendLightmapsPingPong(float speed, StoredLightingScenario scenario)
        {
            if (scenario == null)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> Wrong lighting scenario asset.");
                return;
            }
            else if (scenario.blendableLightmaps.Count < 2)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> Insufficient data to blending. You must add at least two sets of lighting data to the scenario.");
                return;
            }

            GetSwitcherSource(scenario.targetScene);

            if (currentSwitcherSource == null || !currentSwitcherSource.storedDataUpdated)
            {
                return;
            }

            scenario.SetActive(currentSwitcherSource);

            scenario.globalBlendFactor = Mathf.PingPong(Time.time * speed, 1);
            Blending.Blend(currentSwitcherSource, scenario.globalBlendFactor, scenario, scenario.targetScene);
        }

#if ENVIRO_LW || ENVIRO_HD
        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public void EnviroControlled(StoredLightingScenario scenario)
        {
            if (scenario == null)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> Wrong lighting scenario asset.");
                return;
            }
            else if (scenario.blendableLightmaps.Count < 2)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> Insufficient data to blending. You must add at least two sets of lighting data to the scenario.");
                return;
            }

            GetSwitcherSource(scenario.targetScene);

            if (currentSwitcherSource == null || !currentSwitcherSource.storedDataUpdated)
            {
                return;
            }

            scenario.SetActive(currentSwitcherSource);

            scenario.globalBlendFactor = Remap(EnviroSkyMgr.instance.GetTimeOfDay(), 0.0f, 24.0f, 0.0f, 1.0f);
            Blending.Blend(currentSwitcherSource, scenario.globalBlendFactor, scenario, scenario.targetScene);
        }
#endif
#endregion

        #region Switching
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lightmapIndex">Lightmap index to switch to.</param>
        /// <param name="scenario">A Lighting Scenario containing the desired set of lightmaps.</param>
        public void SwitchLightmap(int lightmapIndex, StoredLightingScenario scenario)
        {
            if (scenario == null)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> Wrong lighting scenario asset.");
                return;
            }
            else if (lightmapIndex > scenario.blendableLightmaps.Count)
            {
                Debug.LogFormat("<color=cyan>MLS:</color> The lightmap index you specified is greater than the number of lightmaps in the scenario.");
                return;
            }

            GetSwitcherSource(scenario.targetScene);

            if (currentSwitcherSource == null || !currentSwitcherSource.storedDataUpdated)
            {
                return;
            }

            currentSwitcherSource.lightingDataSwitching = true;
            scenario.SetActive(currentSwitcherSource);

            if (currentSwitcherSource.systemProperties.useSwitchingOnly)
            {
                Switching.LoadLightingData(currentSwitcherSource, lightmapIndex, Switching.LoadMode.Asynchronously);
            }
            else
            {
                Blending.Blend(currentSwitcherSource, scenario.blendableLightmaps[lightmapIndex].startValue, scenario, scenario.targetScene);
            }

            currentSwitcherSource.OnLoadedLightmapChanged[scenario.eventsListId].Invoke(scenario, lightmapIndex);
        }        
        #endregion
    }
}
