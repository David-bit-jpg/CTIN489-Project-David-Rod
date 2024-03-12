#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    public class StoreRenderersData
    {
        public IEnumerator Execute(StoredLightmapData lightmapData, MagicLightmapSwitcher mainComponent)
        {
            MLSProgressBarHelper.StartNewStage("Storing Renderers Data...");

            yield return null;

            Object[] renderers = Object.FindObjectsOfType(typeof(MeshRenderer));

            List<StoredLightmapData.RendererData> rendererDatasTemp = new List<StoredLightmapData.RendererData>();

            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.GetComponent<MLSStaticRenderer>() == null)
                {
                    if (renderer.scaleInLightmap == 0 || !renderer.enabled || renderer.receiveGI == ReceiveGI.LightProbes)
                    {
                        continue;
                    }
                }

                if (renderer.GetComponent<MeshRenderer>() != null)
                {
#if BAKERY_INCLUDED
                    if (renderer.gameObject.GetComponent<BakeryLightMesh>() != null)
                    {
                        continue;
                    }
#endif

                    MLSStaticRenderer staticRenderer = null;
                    MLSDynamicRenderer dynamicRenderer = null;

                    if (CheckIfContributeGI(renderer.gameObject))
                    {
                        if (renderer.receiveGI == ReceiveGI.Lightmaps)
                        {
                            if (renderer.gameObject.GetComponent<MLSDynamicRenderer>() != null)
                            {
                                GameObject.DestroyImmediate(renderer.gameObject.GetComponent<MLSDynamicRenderer>());
                            }

                            if (renderer.gameObject.GetComponent<MLSStaticRenderer>() == null)
                            {
#if BAKERY_INCLUDED
                                if (renderer.gameObject.GetComponent<BakeryLightMesh>() == null)
                                {
                                    staticRenderer = renderer.gameObject.AddComponent<MLSStaticRenderer>();
                                    staticRenderer.UpdateGUID();
                                }
#else
                                staticRenderer = renderer.gameObject.AddComponent<MLSStaticRenderer>();
                                staticRenderer.UpdateGUID();
#endif
                            }
                            else
                            {
                                staticRenderer = renderer.gameObject.GetComponent<MLSStaticRenderer>();
                            }
                        }
                    }
                    else
                    {
                        if (renderer.gameObject.GetComponent<MLSDynamicRenderer>() == null)
                        {
                            if (renderer.gameObject.GetComponent<MLSStaticRenderer>() != null)
                            {
                                GameObject.DestroyImmediate(renderer.gameObject.GetComponent<MLSStaticRenderer>());
                            }
#if BAKERY_INCLUDED
                            if (renderer.gameObject.GetComponent<BakeryLightMesh>() == null)
                            {
                                dynamicRenderer = renderer.gameObject.AddComponent<MLSDynamicRenderer>();
                                dynamicRenderer.UpdateGUID();
                            }
#else
                            dynamicRenderer = renderer.gameObject.AddComponent<MLSDynamicRenderer>();
                            dynamicRenderer.UpdateGUID();
#endif
                        }
                        else
                        {
                            dynamicRenderer = renderer.gameObject.GetComponent<MLSDynamicRenderer>();
                        }
                    }

                    if (staticRenderer != null)
                    {
                        if (rendererDatasTemp.Find(item => item.objectId == staticRenderer.scriptId) != null)
                        {
                            staticRenderer.UpdateGUID();
                        }
                        
                        StoredLightmapData.RendererData rendererData = new StoredLightmapData.RendererData();

                        rendererData.mlsObject = staticRenderer;
                        rendererData.objectId = staticRenderer.scriptId;
                        rendererData.lightmapIndex = renderer.lightmapIndex;
                        rendererData.lightmapScaleOffset = renderer.lightmapScaleOffset;
                        rendererData.rotation = renderer.gameObject.transform.rotation;
                        rendererData.position = renderer.gameObject.transform.position;

                        rendererDatasTemp.Add(rendererData);
                    }
                }

                if (UnityEditorInternal.InternalEditorUtility.isApplicationActive)
                {
                    if (MLSProgressBarHelper.UpdateProgress(renderers.Length, 0))
                    {
                        yield return null;
                    }
                }
            }

            lightmapData.sceneLightingData.rendererDatas = rendererDatasTemp.ToArray();

            MLSLightmapDataStoring.stageExecuting = false;
        }

        public bool CheckIfContributeGI(GameObject gameObject)
        {
            bool isStatic = false;
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(gameObject);

#if UNITY_2019_2_OR_NEWER
            if ((flags & StaticEditorFlags.ContributeGI) != 0)
            {
                isStatic = true;
            }
#else
            if ((flags & StaticEditorFlags.LightmapStatic) != 0)
            {
                isStatic = true;
            }
#endif

            return isStatic;
        }
    }
}
#endif