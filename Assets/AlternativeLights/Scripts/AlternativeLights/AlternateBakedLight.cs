using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternateBakedLight : MonoBehaviour
{
    private MeshRenderer rend;
    [SerializeField]
    public int currentLightState;
    [SerializeField]
    public int[] linkedLightProbes;

    private AlternativeLightsManager manager;

    private void Start()
    {
        manager = FindObjectOfType<AlternativeLightsManager>();
        rend = GetComponent<MeshRenderer>();

        //Set lightprobe and lightmap at start of game
        ChangeLightState(currentLightState);
    }

    //Call the Manager script to change the linked lightprobes and then changes the objects lightmap texture locally
    public void ChangeLightState(int Value)
    {
        //Make sure target lightmap texture is in the valid range
        Value = Mathf.Clamp(Value, 0, manager.maxStatesCount);
        currentLightState = Value;

        //Call the manager to change the selected lightprobes settings
        manager.AssignLightProbesSegment(linkedLightProbes, currentLightState);

        //Switch to a diffrent lightmap texture
        rend.lightmapIndex = currentLightState;
    }

    #region Intraction
    private void OnMouseDown()
    {
        ToggleLights();
    }

    public void ToggleLights()
    {
        currentLightState += 1;
        if (currentLightState > manager.maxStatesCount)
        {
            currentLightState = 0;
        }
        ChangeLightState(currentLightState);
    }
    #endregion Intraction

#if UNITY_EDITOR
    //Used by the editor script for the selection tool
    [HideInInspector]
    public List<int> inRangeProbes;
    [HideInInspector]
    public List<int> tempList;
#endif
}
