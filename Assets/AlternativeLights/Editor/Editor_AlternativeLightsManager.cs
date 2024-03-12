using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AlternativeLightsManager))]

public class Editor_AlternativeLightsManager : Editor
{
    private AlternativeLightsManager manager;
    private int selectedGroup;

    private void OnEnable()
    {
        manager = target as AlternativeLightsManager;
        if (manager == null) return;
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.ObjectField("lightingDataAsset", Lightmapping.lightingDataAsset, typeof(object));

        if (GUILayout.Button("Sync Scene Data"))
        {
            manager.probes_Current = LightmapSettings.lightProbes.bakedProbes;
            GetStoredLightmaps();
            manager.SetupLightmaps();
        }

        //The below button let's you check the light probes of diffrent states in the editor, note changes using this do not save and will reset to defult when the scene is reloaded
        selectedGroup = EditorGUILayout.IntField("Test Probe Groups:", selectedGroup);
        if (GUILayout.Button("View Light Probes For Target Group"))
        {
            selectedGroup = Mathf.Clamp(selectedGroup, 0, manager.lightStates.Length - 1);
            manager.SwapAllProbes(selectedGroup);
            SceneView.RepaintAll();

        }

        base.OnInspectorGUI();
    }

    void GetStoredLightmaps()
    {
        //Clear the old temp maps
        if (manager.Final_L_Lights != null)
        {
            manager.Final_L_Lights.Clear();
        }
        if (manager.Final_L_Dir != null)
        {
            manager.Final_L_Dir.Clear();
        }

        //Update the temp maps with the maps in the stored data
        for (int i = 0; i < manager.lightStates.Length; i++)
        {
            for (int l = 0; l < manager.lightStates[i].l_Light.Length; l++)
            {
                manager.Final_L_Lights.Add(manager.lightStates[i].l_Light[l]);
            }
            for (int l = 0; l < manager.lightStates[i].l_Dir.Length; l++)
            {
                manager.Final_L_Dir.Add(manager.lightStates[i].l_Dir[l]);
            }
        }

        //Apply the new list to the permanent in scene list
        manager.l_light = manager.Final_L_Lights.ToArray();
        manager.l_dir = manager.Final_L_Dir.ToArray();
        
    }
}
