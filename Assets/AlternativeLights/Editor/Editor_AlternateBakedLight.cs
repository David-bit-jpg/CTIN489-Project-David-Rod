using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(AlternateBakedLight))]
public class Editor_AlternateBakedLight : Editor
{
    private AlternateBakedLight myTarget;
    private Vector3[] probes_Pos;
    bool EditProbesMode;

    private void OnEnable()
    {
        myTarget = target as AlternateBakedLight;
        if (myTarget == null) return;


        probes_Pos = LightmapSettings.lightProbes.positions;
        if (myTarget.tempList != null)
        {
            myTarget.tempList.Clear();
        }

        //Get current linked probes in editable list
        if (myTarget.linkedLightProbes.Length != 0)
        {
            for (int i = 0; i < myTarget.linkedLightProbes.Length; i++)
            {
                if (myTarget.tempList != null)
                {
                    myTarget.tempList.Add(myTarget.linkedLightProbes[i]);
                }
            }
        }
    }
    private void OnDisable()
    {
        //I seem to get errors if I don't clear these on exit
        if (myTarget.inRangeProbes != null)
        {
            myTarget.inRangeProbes.Clear();
        }
        if (myTarget.tempList != null)
        {
            myTarget.tempList.Clear();
        }
    }


    public override void OnInspectorGUI()
    {
        myTarget = target as AlternateBakedLight;
        if (myTarget == null) return;

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;

        if (!EditProbesMode)
        {
            //Puts you into edit mode that let's you add the probes you want to link to this object
            if (GUILayout.Button("Edit Linked Probes"))
            {
                EditProbesMode = true;
            }
            //Let's manually trigger a diffrent lightmap state in runtime
            if (GUILayout.Button("Refresh Lightmap(Runtime)"))
            {
                myTarget.ChangeLightState(myTarget.currentLightState);
            }
        }
        else //Edit mode - Let's you add or remove lightprobes linked to this object
        {
            if (GUILayout.Button("Stop Edit Linked Probes"))
            {
                EditProbesMode = false;
            }

            //Edit mode options

            GUILayout.Space(5);
            GUILayout.Label("Add or Remove Probes", style);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Selected"))
            {
                for (int i = 0; i < myTarget.inRangeProbes.Count; i++)
                {
                    if (!myTarget.tempList.Contains(myTarget.inRangeProbes[i]))
                    {
                        myTarget.tempList.Add(myTarget.inRangeProbes[i]);
                    }
                }
                myTarget.linkedLightProbes = myTarget.tempList.ToArray();
            }
            if (GUILayout.Button("Remove Selected"))
            {
                for (int i = 0; i < myTarget.inRangeProbes.Count; i++)
                {

                    myTarget.tempList.Remove(myTarget.inRangeProbes[i]);
                    
                }
                myTarget.linkedLightProbes = myTarget.tempList.ToArray();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Clear All linked probes"))
            {
                myTarget.tempList.Clear();
                myTarget.linkedLightProbes = myTarget.tempList.ToArray();
            }
            GUILayout.Space(5);
            if (GUILayout.Button("Center Selection Tool"))
            {
                positionHandle = SceneView.lastActiveSceneView.pivot;
            }

        }
        base.OnInspectorGUI();
    }

    private Vector3 positionHandle;
    private float selectRange;
    void OnSceneGUI()
    {
        if (EditProbesMode) {
            //A basic tool that let's you select lightprobes in the scene and add them to the objects list of linked probes

            Handles.DrawCamera(new Rect(0, 0, 500, 500), Camera.current);
            positionHandle = Handles.PositionHandle(positionHandle, Quaternion.identity);
            selectRange = Handles.RadiusHandle(Quaternion.identity, positionHandle, selectRange);
            selectRange = Mathf.Clamp(selectRange, .5f, 1000);

            if (myTarget.inRangeProbes != null)
            {
                myTarget.inRangeProbes.Clear();
            }

            for (int i = 0; i < probes_Pos.Length; i++)
            {
                //Is in linked group - Yellow
                if (myTarget.tempList.Contains(i))
                {
                    Handles.color = Color.yellow;
                    // Handles.DrawSphere(0, probes_Pos[i], Quaternion.identity, .6f);
                }
                //Is not in linked group - Red
                else
                {
                    Handles.color = Color.red;
                    // Handles.DrawSphere(0, probes_Pos[i], Quaternion.identity, .55f);
                }
                //Blue In selection Range - Green
                if (Vector3.Distance(probes_Pos[i], positionHandle) < selectRange)
                {
                    Handles.color = Color.green;
                    myTarget.inRangeProbes.Add(i);
                    // Handles.DrawSphere(0, probes_Pos[i], Quaternion.identity, .4f);
                }
            }
        }
        else
        {
            //Display probes linked to object
            if(myTarget.tempList != null)
            {
                Handles.color = Color.yellow;
                for (int i = 0; i < myTarget.tempList.Count; i++)
                {
                    // Handles.DrawSphere(0, probes_Pos[myTarget.tempList[i]], Quaternion.identity, .2f);
                }
            }
        }
    }
}
