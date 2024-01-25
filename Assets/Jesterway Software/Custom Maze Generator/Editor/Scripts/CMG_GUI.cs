using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


namespace JesterwaySoftware.CustomMazeGenerator
{
    public class CMG_GUI : EditorWindow
    {
        public static Button reset_button;
        
        public static IntegerField mazeX_intField;
        public static IntegerField mazeY_intField;
        public static SliderInt loopChance_intSlider;
        public static Button calculate_button;

        public static Vector3Field mazePosition_vector3;
        public static Vector3Field mazeRotation_vector3;
        public static FloatField tileSize_floatField;
        public static FloatField wallThickness_floatField;
        public static FloatField wallHeight_floatField;
        public static Toggle unify_toggle;
        public static Toggle entranceNorth_toggle;
        public static Toggle entranceEast_toggle;
        public static Toggle entranceSouth_toggle;
        public static Toggle entranceWest_toggle;
        public static ObjectField wallMaterial_objectField;
        public static ObjectField floorMaterial_objectField;
        public static Toggle rotateUVs_toggle;
        public static Toggle scaleUVs_toggle;
        public static Toggle zFighting_toggle;
        public static Toggle inactiveWalls_toggle;
        public static Toggle innerWalls_toggle;
        public static Button display_button;


        [MenuItem("Tools/Custom Maze Generator")]
        public static void ShowWindow()
        {
            CMG_GUI window = GetWindow<CMG_GUI>();
            window.titleContent = new GUIContent("Maze Settings");
            window.minSize = new Vector2(480, 1140);
        }


        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Jesterway Software/Custom Maze Generator/Editor/Scripts/CMG_GUI.uxml");
            VisualElement UXML = visualTree.Instantiate();

            root.Add(UXML);

            reset_button = root.Query<Button>("reset_button").First();

            mazeX_intField = root.Query<IntegerField>("mazeX_intField").First();
            mazeY_intField = root.Query<IntegerField>("mazeY_intField").First();
            loopChance_intSlider = root.Query<SliderInt>("loopChance_intSlider").First();
            calculate_button = root.Query<Button>("calculate_button").First();

            mazePosition_vector3 = root.Query<Vector3Field>("mazePosition_vector3").First();
            mazeRotation_vector3 = root.Query<Vector3Field>("mazeRotation_vector3").First();
            entranceNorth_toggle = root.Query<Toggle>("entranceNorth_toggle").First();
            entranceEast_toggle = root.Query<Toggle>("entranceEast_toggle").First();
            entranceSouth_toggle = root.Query<Toggle>("entranceSouth_toggle").First();
            entranceWest_toggle = root.Query<Toggle>("entranceWest_toggle").First();
            unify_toggle = root.Query<Toggle>("unify_toggle").First();
            tileSize_floatField = root.Query<FloatField>("tileSize_floatField").First();
            wallThickness_floatField = root.Query<FloatField>("wallThickness_floatField").First();
            wallHeight_floatField = root.Query<FloatField>("wallHeight_floatField").First();
            wallMaterial_objectField = root.Query<ObjectField>("wallMaterial_objectField").First();
            floorMaterial_objectField = root.Query<ObjectField>("floorMaterial_objectField").First();
            rotateUVs_toggle = root.Query<Toggle>("rotateUVs_toggle").First();
            scaleUVs_toggle = root.Query<Toggle>("scaleUVs_toggle").First();
            zFighting_toggle = root.Query<Toggle>("zFighting_toggle").First();
            inactiveWalls_toggle = root.Query<Toggle>("inactiveWalls_toggle").First();
            innerWalls_toggle = root.Query<Toggle>("innerWalls_toggle").First();
            display_button = root.Query<Button>("display_button").First();

            if (reset_button != null)
            {
                reset_button.clicked += () => { Helper.ResetDefaults(); };
            }

            if (calculate_button != null)
            {
                calculate_button.clicked += () => { Calculate.CalculateMazeFile(mazeX_intField.value, mazeY_intField.value, loopChance_intSlider.value); };
            }

            if (display_button != null)
            {
                display_button.clicked += () => { Display.DisplayMazeFile(); };
            }
        }
    }
}