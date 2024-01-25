using UnityEngine;


namespace JesterwaySoftware.CustomMazeGenerator
{
    public static class Helper
    {
        public readonly static string version = "v1.0.3";

        public static void ResetDefaults()
        {
            CMG_GUI.mazeX_intField.value = 20;
            CMG_GUI.mazeY_intField.value = 20;
            CMG_GUI.loopChance_intSlider.value = 0;
            CMG_GUI.mazePosition_vector3.value = new Vector3(0f, 0f, 0f);
            CMG_GUI.mazeRotation_vector3.value = new Vector3(0f, 0f, 0f);
            CMG_GUI.tileSize_floatField.value = 2.8f;
            CMG_GUI.wallThickness_floatField.value = 0.8f;
            CMG_GUI.wallHeight_floatField.value = 3f;
            CMG_GUI.unify_toggle.value = true;
            CMG_GUI.entranceNorth_toggle.value = false;
            CMG_GUI.entranceEast_toggle.value = false;
            CMG_GUI.entranceSouth_toggle.value = true;
            CMG_GUI.entranceWest_toggle.value = false;
            CMG_GUI.wallMaterial_objectField.value = null;
            CMG_GUI.floorMaterial_objectField.value = null;
            CMG_GUI.rotateUVs_toggle.value = true;
            CMG_GUI.scaleUVs_toggle.value = true;
            CMG_GUI.zFighting_toggle.value = true;
            CMG_GUI.inactiveWalls_toggle.value = false;
            CMG_GUI.innerWalls_toggle.value = false;
        }
    }
}