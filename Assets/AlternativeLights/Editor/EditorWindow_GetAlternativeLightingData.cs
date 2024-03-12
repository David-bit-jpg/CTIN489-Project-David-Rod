using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class EditorWindow_GetAlternativeLightingData : EditorWindow
{
    private AlternativeLightingData currentData;

    [MenuItem("Swapping Lightmaps/Get Alternative Lighting Data")]
    static void ShowEditor()
    {
        EditorWindow_GetAlternativeLightingData editor = EditorWindow.GetWindow<EditorWindow_GetAlternativeLightingData>();
    }

    void OnGUI()
    {
        //If no target available don't show any options
        if (currentData == null)
        {
            GUILayout.BeginArea(new Rect(position.width/4, position.height/4, position.width/2, 400));
           
            //Display settings
            EditorStyles.label.wordWrap = true;
            EditorStyles.label.alignment = TextAnchor.LowerCenter;

            EditorGUILayout.LabelField("Copy current scene lightmap probe data into a target 'AlternativeLightingData' - Please select a target", GUILayout.Height(122), GUILayout.Width(position.width / 2));
            currentData = EditorGUILayout.ObjectField(currentData, typeof(AlternativeLightingData), true) as AlternativeLightingData;

            GUILayout.EndArea();
            return;
        }
        else
        {
            //Display settings
            int margin = 50;
            GUIStyle boxStyles = new GUIStyle(GUI.skin.box);
            boxStyles.normal.textColor = Color.white;

            GUILayout.BeginArea(new Rect(margin, margin, position.width - (margin + margin ), 150 ) , EditorStyles.helpBox);

            currentData = EditorGUILayout.ObjectField(currentData, typeof(AlternativeLightingData), true) as AlternativeLightingData;

            #region GetLightProbesData

            if (GUILayout.Button("Get Scene Data", GUILayout.Width(position.width - (margin * 2 + 10))))
            {
                GetProbeDataFromScene();
            }

            //Once there is data to save show options to store data
            if(currentSceneProbes != null)
            {
                EditorGUILayout.LabelField(captureTime + " - Captured Scene Data - " + currentSceneProbes.Length + " Light Probes Found", GUILayout.Height(25), GUILayout.Width(position.width - (margin * 2 )));
                if (GUILayout.Button("Sava scene Data to target?", GUILayout.Width(position.width - (margin * 2 + 10))))
                {
                    currentData.lightProbesData = currentSceneProbes;
                    lastSave = System.DateTime.UtcNow.ToString("HH:mm");
                }
                if (currentData.lightProbesData != null)
                {
                    EditorGUILayout.LabelField(lastSave + " - Stored target Data - " + currentData.lightProbesData.Length + " Light Probes saved", GUILayout.Height(25), GUILayout.Width(position.width - (margin * 2)));
                }
                else
                {
                    EditorGUILayout.LabelField("No light probe Data in target", GUILayout.Height(25), GUILayout.Width(position.width - (margin * 2)));

                }
            }
            #endregion GetLightProbesData

            GUILayout.EndArea();


            #region GetLightmapTextures
            GUILayout.BeginArea(new Rect(margin, 175 + (margin ), position.width - (margin + margin - 3), (position.height - 175) - (margin *2)), EditorStyles.helpBox);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Get Scene Textures"))
            {
                GetLightmapTextures();
            }
            if (GUILayout.Button("Save Scene Textures"))
            {
                SaveLightmapTextures();
            }
            GUILayout.EndHorizontal();

            #region Display_LightmapTextures

            #region Display_CurrentScenedLightmaps
            EditorStyles.objectField.alignment = TextAnchor.MiddleCenter;
            float windowHeight = (position.height * .75f) - (margin * 4);
            GUILayout.BeginArea(new Rect(margin*1.75f, margin, position.width / 2 - (margin * 3), (position.height * .75f) - (margin * 4)), EditorStyles.helpBox);

            if (currentData.l_LightTemp != null)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < currentData.l_LightTemp.Count; i++)
                {
                    Texture2D myTexture = (Texture2D)EditorGUILayout.ObjectField("", currentData.l_LightTemp[i], typeof(Texture2D), false , GUILayout.Height(windowHeight * .475f));
                }
                EditorGUILayout.EndHorizontal();
            }
            if (currentData.l_DirTemp != null)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < currentData.l_DirTemp.Count; i++)
                {
                    Texture2D myTexture = (Texture2D)EditorGUILayout.ObjectField("", currentData.l_DirTemp[i], typeof(Texture2D), false, GUILayout.Height((windowHeight * .475f) ) );
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
            #endregion Display_CurrentScenedLightmaps
            #region Display_SavedLightmaps
            GUILayout.BeginArea(new Rect(position.width / 2 - (margin/2), margin, position.width / 2 - (margin * 3), (position.height * .75f) - (margin * 4)), EditorStyles.helpBox);

            if (currentData.l_Light != null)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < currentData.l_Light.Length; i++)
                {
                    Texture2D myTexture = (Texture2D)EditorGUILayout.ObjectField("", currentData.l_Light[i], typeof(Texture2D), false, GUILayout.Height(windowHeight * .475f));
                }
                EditorGUILayout.EndHorizontal();
            }
            if (currentData.l_Dir != null)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < currentData.l_Dir.Length; i++)
                {
                    Texture2D myTexture = (Texture2D)EditorGUILayout.ObjectField("", currentData.l_Dir[i], typeof(Texture2D), false, GUILayout.Height((windowHeight * .475f)));
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
            #endregion Display_SavedLightmaps


            #endregion Display_LightmapTextures

            GUILayout.EndArea();
            #endregion GetLightmapTextures
        }
    }

    private SphericalHarmonicsL2[] currentSceneProbes;
    private LightmapData[] lightmapData;

    private string captureTime;
    private string lastSave;

    void GetProbeDataFromScene()
    {
        if (LightmapSettings.lightProbes.bakedProbes == null)
        {
            Debug.LogError("No Lightprobe Data found - Do you need to genrate the current scenes lightmaps data?");
        }
        //Copy light probes lighting and strength
        currentSceneProbes = LightmapSettings.lightProbes.bakedProbes;

        captureTime = System.DateTime.UtcNow.ToString("HH:mm");
    }

    LightmapData[] refLightmapData;
    void GetLightmapTextures()
    {
        if (LightmapSettings.lightmaps != null)
        {
            refLightmapData = LightmapSettings.lightmaps;
        }
        if (refLightmapData != null)
        {
            //Clear the texture list before finding the current scenes textures again
            if (currentData.l_DirTemp.Count != null)
            {
                currentData.l_DirTemp.Clear();
            }
            if (currentData.l_LightTemp.Count != null)
            {
                currentData.l_LightTemp.Clear();
            }

            //Find all lightmap textures in the current scene
            for (int i = 0; i < refLightmapData.Length; i ++)
            {
                if (refLightmapData[i].lightmapDir != null)
                {
                    Debug.Log("Found Lightmap Dir at Index:" + i.ToString());
                    currentData.l_DirTemp.Add(refLightmapData[i].lightmapDir);
                }
                if (refLightmapData[i].lightmapColor != null)
                {
                    Debug.Log("Found Lightmap Color at Index:" + i.ToString());
                    currentData.l_LightTemp.Add(refLightmapData[i].lightmapColor);
                }
            }
        }
        else
        {
            Debug.LogError("No lightmap data found in current scene! Please bake lightmap");
        }
    }

    void SaveLightmapTextures()
    {
        if (currentData.l_DirTemp.Count != null)
        {
            currentData.l_Dir = currentData.l_DirTemp.ToArray();
        }
        if (currentData.l_DirTemp.Count != null)
        {
            currentData.l_Light = currentData.l_LightTemp.ToArray();
        }
    }
}
