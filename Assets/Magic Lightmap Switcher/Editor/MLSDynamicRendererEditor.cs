using UnityEditor;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [CustomEditor(typeof(MLSDynamicRenderer))]
    public class MLSDynamicRendererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MLSDynamicRenderer dynamicRenderer = (MLSDynamicRenderer)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Script GUID");
                GUILayout.Label(dynamicRenderer.scriptId);
            }

            if (GUILayout.Button("Update GUID"))
            {
                dynamicRenderer.UpdateGUID();
            }
        }
    }
}
