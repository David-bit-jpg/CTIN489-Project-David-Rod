using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AlternativeLightsManager : MonoBehaviour
{
    [SerializeField]
    public AlternativeLightingData[] lightStates;

    //Used to clamp object that want to change their lightmaps so they don't go beyond the valid range.
    [HideInInspector]
    public int maxStatesCount;

    //The data that will be applied to the lightprobes of the scene
    [SerializeField]
    public SphericalHarmonicsL2[] probes_Current;

    [HideInInspector]
    public Texture2D[] l_light;
    [HideInInspector]
    public Texture2D[] l_dir;

    //The lightmap that is generated at the start to the game and used to add the additional lightmap textures to the loaded level
    private LightmapData[] LightMapsData;

    //Used by the editor tool to get the lightmaps
#if UNITY_EDITOR
    public List<Texture2D> Final_L_Lights;
    public List<Texture2D> Final_L_Dir;
#endif

    void Start()
    {
        //Calls for the additional lightmaps to be added to the loaded scene
        SetupLightmaps();
    }


    //This is called by objects in the scene to update the lightprobes and set small segments to alternative settings
    public void AssignLightProbesSegment(int[]TargetProbes, int TargetState)
    {
        //Make sure target state is within the valid range
        TargetState = Mathf.Clamp(TargetState, 0, lightStates.Length);

        //Go through the list of light probes index and change the probes_current values
        for (int i = 0; i < TargetProbes.Length; i++)
        {
            int CurrentTarget = TargetProbes[i];
            //Find the lightprobe by it's index number and copys the settings from the stored data to the current scene
            probes_Current[CurrentTarget] = lightStates[TargetState].lightProbesData[CurrentTarget];
        }

        //Apply the changed probe data to the scenes lightmap settings
        LightmapSettings.lightProbes.bakedProbes = probes_Current;
    }

    //Called on the start of the level, it makes a new lightmapData instance to replace the one at load time that we can edit and add additional lightmap textures too.
    public void SetupLightmaps()
    {
        //Make new lightmapData that we can edit at the start of the game
       LightMapsData = new LightmapData[l_light.Length];

        //Add all our lightmaps textures to the temps LightmapData
        for (int i = 0; i < l_light.Length; i++)
        {
            LightMapsData[i] = new LightmapData();
            LightMapsData[i].lightmapColor = l_light[i];

            if (l_dir.Length >= i)
            {
                LightMapsData[i].lightmapDir = l_dir[i];
            }
        }
        maxStatesCount = l_light.Length - 1;

        //Replace the starting lightmap data with our new edited one
        LightmapSettings.lightmaps = LightMapsData;
    }



    //Swaps all lightprobes in the scene to a spesfic state - Note this will only change the light probes used by the editor tool for testings
    public void SwapAllProbes(int TargetState)
    {
        //Make a list of all the scenes lightprobes
        int[] tempList = new int[probes_Current.Length];
        for (int i = 0; i < tempList.Length; i++)
        {
            tempList[i] = i;
        }
        AssignLightProbesSegment(tempList, TargetState);
    }

}
