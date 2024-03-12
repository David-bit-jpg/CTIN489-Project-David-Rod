using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEngine;

namespace MagicLightmapSwitcher
{
    [ExecuteInEditMode]
    public class MLSShaderCodeModifier : EditorWindow
    {
        private struct CmdProcessArguments
        {
            public enum CommandType
            {
                Copy,
                Delete
            }

            public CmdProcessArguments(CmdProcessArguments data)
            {
                commandType = data.commandType;
                deleteFrom = data.deleteFrom;
                copyFrom = data.copyFrom;
                copyTo = data.copyTo;
            }

            public CommandType commandType;
            public string deleteFrom;
            public string copyFrom;
            public string copyTo;
        }

        private static SystemProperties systemProperties;
        
        private static bool manual;
        public static bool waitingForAssetImporting;

        private static bool standard_RP_Patched;

        private static bool initialized;
        private static MLSShaderCodeModifier shaderCodeModifierWindow;

        #region Standard Render Pipeline
        private static string standard_RP_SourcesPath;
        private static string standard_RP_ModifySourcesPath;

        // Standard RP Default Source Files
        private static string STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE = "UnityImageBasedLighting.cginc";
        private static string STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE = "UnityPBSLighting.cginc";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE = "UnityShadowLibrary.cginc";
        private static string STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE = "UnityGlobalIllumination.cginc";
        private static string STANDARD__CORE__DEFAULT_SOURCE_FILE = "UnityStandardCore.cginc";

        // Standard RP Additional Includes
        private static string STANDARD__CORE__COMMON__INCLUDE_FILE = "MLS_Standard_Common.cginc";

        // Standard RP Search Fragments
        private static string STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__SEARCH_STRING =
            "half3 Unity_GlossyEnvironment (UNITY_ARGS_TEXCUBE(tex), half4 hdr, Unity_GlossyEnvironmentData glossIn)";
        private static string STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD_GI__SEARCH_STRING =
            "gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__SEARCH_STRING =
            "fixed UnitySampleBakedOcclusion (float2 lightmapUV, float3 worldPos)";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__SEARCH_STRING =
            "fixed4 UnityGetRawBakedOcclusions(float2 lightmapUV, float3 worldPos)";
        private static string STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__SEARCH_STRING =
            "#include \"UnityImageBasedLighting.cginc\"";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__SEARCH_STRING =
            "half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, data.lightmapUV.xy);" +
            "fixed4 bakedDirTex = UNITY_SAMPLE_TEX2D_SAMPLER (unity_LightmapInd, unity_Lightmap, data.lightmapUV.xy);";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__SEARCH_STRING =
            "half3 env0 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], glossIn);" +
            "half3 env1 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0), data.probeHDR[1], glossIn);";

        // Standard RP Patch Source Files
        private static string STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__MODYFI_SOURCE_FILE = "Unity_Image_Based_Lighting_Glossy_Environment.txt";
        private static string STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD__MODYFI_SOURCE_FILE = "Unity_PBS_Lighting_Standard_Lighting_Additions.txt";
        private static string STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__MODYFI_SOURCE_FILE = "Global_Illumination_Include_Section_Additions.txt";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__MODYFI_SOURCE_FILE = "Global_Illumination_UnityGI_IndirectSpecular_Additions.txt";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__MODYFI_SOURCE_FILE = "Global_Illumination_Unity_GI_Base_Additions.txt";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__MODYFI_SOURCE_FILE = "Unity_Shadow_Library_Unity_Sample_Baked_Occlusion_Additions.txt";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__MODYFI_SOURCE_FILE = "Unity_Shadow_Library_Unity_Get_Raw_Baked_Occlusions_Additions.txt";

        // Standard RP Patch Lines
        private List<string> standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines = new List<string>();
        private List<string> standard__Unity_PBS_Lighting__Lighting_Standard_Lines = new List<string>();
        private List<string> standard__Global_Illumination__Include_Section_Lines = new List<string>();
        private List<string> standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines = new List<string>();
        private List<string> standard__Global_Illumination__Unity_GI_Base_Lines = new List<string>();
        private List<string> standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines = new List<string>();
        private List<string> standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines = new List<string>();
        private List<string> includeSectionAdditions = new List<string>();
        private List<string> fragmentGIBlendReflectionProbes = new List<string>();

        // Standard MLS Patched Signatures
        private static string STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__SIGNATURE = "//<MLS_IMAGE_BASED_LIGHTING_ENVIRONMENT_CUSTOM>";
        private static string STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD__SIGNATURE = "//<MLS_UNITY_PBS_LIGHTING_LIGHTING_STANDARD_ADDITIONS>";
        private static string STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__SIGNATURE = "//<MLS_GLOBAL_ILLUMINATION_INCLUDE_SECTION>";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__SIGNATURE = "//<MLS_GLOBAL_ILLUMINATION_UNITY_GI_INDIRECT_SPECULAR_ADDITIONS>";
        private static string STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__SIGNATURE = "//<MLS_GLOBAL_ILLUMINATION_UNITY_GI_BASE_ADDITIONS>";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__SIGNATURE = "//<MLS_UNITY_SHADOW_LIBRARY_UNITY_SAMPLE_BAKED_OCCLUSION_ADDITIONS>";
        private static string STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__SIGNATURE = "//<MLS_UNITY_SHADOW_LIBRARY_UNITY_GET_RAW_BAKED_OCCLUSIONS_ADDITIONS>";
        #endregion

        [MenuItem("Tools/Magic Tools/Magic Lightmap Switcher/Prepare Shaders...", priority = 1)]
        static void OpenWindow()
        {
            shaderCodeModifierWindow = (MLSShaderCodeModifier) GetWindow(typeof(MLSShaderCodeModifier), true, "Magic Lightmap Switcher - Patch Shaders");
            shaderCodeModifierWindow.maxSize = new Vector2(250 * EditorGUIUtility.pixelsPerPoint, 100 * EditorGUIUtility.pixelsPerPoint);
            shaderCodeModifierWindow.minSize = shaderCodeModifierWindow.maxSize;
            shaderCodeModifierWindow.position = new Rect(
                Screen.width + shaderCodeModifierWindow.minSize.x * 0.5f,
                Screen.height - shaderCodeModifierWindow.minSize.y * 0.5f,
                shaderCodeModifierWindow.minSize.x,
                shaderCodeModifierWindow.minSize.y);

            shaderCodeModifierWindow.Show();
        }

        public static void Initialize()
        {
            string managedDir = "";

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                managedDir = EditorApplication.applicationPath.Split(new string[] { "Unity.exe" }, StringSplitOptions.None)[0];
            }
            else
            {
                System.Reflection.Assembly entryAssembly = new System.Diagnostics.StackTrace().GetFrames().Last().GetMethod().Module.Assembly;
                managedDir = System.IO.Path.GetDirectoryName(entryAssembly.Location);
            }

            standard_RP_SourcesPath = managedDir + "Data/CGIncludes/";

            if (!Directory.Exists(standard_RP_SourcesPath))
            {
                standard_RP_SourcesPath = managedDir + "/../../CGIncludes/";
            }

            if (!Directory.Exists(standard_RP_SourcesPath))
            {
                UnityEngine.Debug.LogError("Can't find directory: " + standard_RP_SourcesPath);
                return;
            }

            string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);
            string projectRelativePath = Application.dataPath + directories[0].Split(new string[] { "Magic Lightmap Switcher" }, StringSplitOptions.None)[1];            
            standard_RP_ModifySourcesPath = directories[0] + "/Editor/Dependent Resources/Shader Sources/Standard/";

            systemProperties = AssetDatabase.LoadAssetAtPath(FileUtil.GetProjectRelativePath(projectRelativePath + "/Magic Lightmap Switcher/Editor/SystemProperties.asset"), typeof(SystemProperties)) as SystemProperties;

            if (systemProperties == null)
            {
                systemProperties = ScriptableObject.CreateInstance<SystemProperties>();

                AssetDatabase.CreateAsset(systemProperties, FileUtil.GetProjectRelativePath(projectRelativePath + "/Magic Lightmap Switcher/Editor/SystemProperties.asset"));
                AssetDatabase.SaveAssets();
            }

            standard_RP_Patched = CheckForStandardRPPatched(false);
            systemProperties.standardRPPatched = standard_RP_Patched;

            initialized = true;

            EditorUtility.SetDirty(systemProperties);            
        }

        private static void PrintPatchReport(List<string> patchErrors = null)
        {
            if (patchErrors != null)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < patchErrors.Count; i++)
                {
                    if (i < patchErrors.Count - 1)
                    {
                        sb.Append(patchErrors[i] + ", ");
                    }
                    else
                    {
                        sb.Append(patchErrors[i]);
                    }
                }

                Debug.LogErrorFormat("" +
                    "<color=cyan>MLS:</color> Patch error of [" + sb.ToString() + "] " + "Unity version: " + Application.unityVersion.ToString() + ". " + "Render Pipeline: Standard" +
                    "\r\n" +
                    "Show the text of this error to the developer.");
            }
            else
            {
                Debug.LogFormat("<color=cyan>MLS:</color> Standard Shaders patched successfully.");
            }
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (!initialized)
            {
                Initialize();
            }

            GetSystemProperties();

            MLSEditorUtils.InitStyles();

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("These changes will be applied to the base Unity shaders.", MLSEditorUtils.captionStyle);

            GUILayout.Label(
                "Changes do not affect the basic functionality of the shaders, " +
                "they only contain some extras to support real-time lightmap blending.", MLSEditorUtils.labelCenteredStyle);

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.Label("Standard Shaders", MLSEditorUtils.caption_1_Style);

            if (standard_RP_Patched)
            {
                systemProperties.standardRPPatched = true;

                if (GUILayout.Button("Restore Standard RP Shaders", MLSEditorUtils.bigButtonStyle))
                {
                    RestoreStandardRPShaders();
                    systemProperties.prevTimeSinceStartup = 0;
                    standard_RP_Patched = CheckForStandardRPPatched(false);
                }
            }
            else
            {
                systemProperties.standardRPPatched = false;

                if (GUILayout.Button("Patch Standard RP Shaders", MLSEditorUtils.bigButtonStyle))
                {
                    PatchStandardRPShaders();
                    systemProperties.prevTimeSinceStartup = 0;
                    standard_RP_Patched = CheckForStandardRPPatched(true);
                }
            }

            systemProperties.standardRPActive = true;

            EditorUtility.SetDirty(systemProperties);

            GUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
            {
                Rect templateEnd = GUILayoutUtility.GetLastRect();

                if (shaderCodeModifierWindow != null)
                {
                    shaderCodeModifierWindow.minSize = new Vector2(shaderCodeModifierWindow.minSize.x, templateEnd.position.y + templateEnd.height);
                }
            }
        }

        public static void GetSystemProperties()
        {
            if (systemProperties == null)
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);
                string projectRelativePath = Application.dataPath + directories[0].Split(new string[] { "Magic Lightmap Switcher" }, StringSplitOptions.None)[1];

                systemProperties = AssetDatabase.LoadAssetAtPath(FileUtil.GetProjectRelativePath(projectRelativePath + "/Magic Lightmap Switcher/Editor/SystemProperties.asset"), typeof(SystemProperties)) as SystemProperties;
            }

            if (systemProperties == null)
            {
                string[] directories = Directory.GetDirectories(Application.dataPath, "Magic Lightmap Switcher", SearchOption.AllDirectories);
                string projectRelativePath = Application.dataPath + directories[0].Split(new string[] { "Magic Lightmap Switcher" }, StringSplitOptions.None)[1];

                systemProperties = ScriptableObject.CreateInstance<SystemProperties>();

                AssetDatabase.CreateAsset(systemProperties, FileUtil.GetProjectRelativePath(projectRelativePath + "/Magic Lightmap Switcher/Editor/SystemProperties.asset"));
                AssetDatabase.SaveAssets();
            }
        }

        public static bool CheckForStandardRPPatched(bool printReport)
        {            
            int patchedFragments = 0;
            int filePatchedFragments = 0;
            List<string> patchErrors = new List<string>();

            try
            {
                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__SIGNATURE == line.Trim() ||
                            STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__SIGNATURE == line.Trim() ||
                            STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                            filePatchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 3)
                    {
                        patchErrors.Add("GlobalIllumination.cginc");
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("ImageBasedLighting.cginc");
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("PBSLighting.cginc");
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("UnityShadowLibrary.cginc (Deferred Section)");
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    filePatchedFragments = 0;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) &&
                            (STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__SIGNATURE == line.Trim()))
                        {
                            patchedFragments++;
                        }
                    }

                    if (filePatchedFragments != 1)
                    {
                        patchErrors.Add("UnityShadowLibrary.cginc (Forward Section)");
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("The file could not be read: " + e.Message);
                return false;
            }

            if (patchedFragments == 10)
            {
                if (printReport)
                {
                    PrintPatchReport();
                }

                return true;
            }
            else
            {
                if (printReport)
                {
                    PrintPatchReport(patchErrors);
                }

                return false;
            }
        }

        private static void RunAsAdmin(List<CmdProcessArguments> argumentsList)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(@"/c ");

            for (int i = 0; i < argumentsList.Count; i++)
            {
                switch (argumentsList[i].commandType)
                {
                    case CmdProcessArguments.CommandType.Copy:
                        stringBuilder.Append("copy /y \"" + argumentsList[i].copyFrom.Replace("/", @"\") + "\" \"" + argumentsList[i].copyTo.Replace("/", @"\") + "\" && ");
                        break;
                    case CmdProcessArguments.CommandType.Delete:
                        stringBuilder.Append("del /q \"" + argumentsList[i].deleteFrom.Replace("/", @"\") + "\" && ");
                        break;
                }                
            }

            stringBuilder.Append("pause");

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.FileName = "cmd.exe";
            startInfo.Verb = "runas";
            startInfo.UseShellExecute = true;
            startInfo.Arguments = stringBuilder.ToString();

            try
            {
                System.Diagnostics.Process process =  new System.Diagnostics.Process();

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void PatchStandardRPShaders()
        {
            CmdProcessArguments arguments = new CmdProcessArguments();
            List<CmdProcessArguments> processArguments = new List<CmdProcessArguments>();

            standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines.Clear();
            standard__Unity_PBS_Lighting__Lighting_Standard_Lines.Clear();
            standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines.Clear();
            standard__Global_Illumination__Include_Section_Lines.Clear();
            standard__Global_Illumination__Unity_GI_Base_Lines.Clear();
            standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines.Clear();
            standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines.Clear();
            includeSectionAdditions.Clear();
            fragmentGIBlendReflectionProbes.Clear();

            #region Read Modified Sources

            try
            {
                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Global_Illumination__Include_Section_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Unity_PBS_Lighting__Lighting_Standard_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Global_Illumination__Unity_GI_Base_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines.Add(reader.ReadLine());
                    }
                }

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__MODYFI_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines.Add(reader.ReadLine());
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogErrorFormat("<color=cyan>MLS:</color> The file could not be read: " + e.Message);
            }

            #endregion

            #region Buckup Original Files

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_ModifySourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE, standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE);
                }
            }

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original";

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE, standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original");
                }
            }

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original";

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE, standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original");
                }             
            }

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original";

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE, standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original");
                }                
            }

            if (!File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original";

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE, standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original");
                }
            }

            RunAsAdmin(processArguments);

            #endregion

            #region Patch Shader Files

            List<string> fileLines = new List<string>();
            processArguments.Clear();            

            try
            {
                #region Global Illumination

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) && STANDARD__GLOBAL_ILLUMINATION__INCLUDE_SECTION__SEARCH_STRING.Contains(line.Trim()))
                        {
                            for (int i = 0; i < standard__Global_Illumination__Include_Section_Lines.Count; i++)
                            {
                                fileLines.Add(standard__Global_Illumination__Include_Section_Lines[i]);
                            }

                            fileLines.Add("");
                            fileLines.Add(line);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_INDIRECT_SPECULAR__SEARCH_STRING.Contains(line.Trim()))
                        {
                            if (line.Trim().Contains("env0"))
                            {
                                fileLines.Add("");

                                for (int i = 0; i < 28; i++)
                                {
                                    fileLines.Add(standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines[i]);
                                }

                                fileLines.Add("");
                            }

                            if (line.Trim().Contains("env1"))
                            {
                                for (int i = 29; i < 56; i++)
                                {
                                    fileLines.Add(standard__Global_Illumination__Unity_GI_Indirect_Specular_Lines[i]);
                                }

                                fileLines.Add("");
                            }
                            continue;
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__GLOBAL_ILLUMINATION__UNITY_GI_BASE__SEARCH_STRING.Contains(line.Trim()))
                        {
                            if (line.Trim().Contains("half4 bakedColorTex"))
                            {
                                fileLines.Add("");

                                for (int i = 0; i < 6; i++)
                                {
                                    fileLines.Add(standard__Global_Illumination__Unity_GI_Base_Lines[i]);
                                }

                                fileLines.Add("");
                            }

                            if (line.Trim().Contains("fixed4 bakedDirTex"))
                            {
                                for (int i = 7; i < 15; i++)
                                {
                                    fileLines.Add(standard__Global_Illumination__Unity_GI_Base_Lines[i]);
                                }

                                fileLines.Add("");
                            }
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                if (!File.Exists(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    File.Create(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE).Close(); 
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(
                        standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE, 
                        standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE);
                }

                #endregion

                #region PBS Lightnig

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    bool skipLine = false;

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (skipLine)
                        {
                            skipLine = false;
                            continue;
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__UNITY_PBS_LIGHTING__LIGHTING_STANDARD_GI__SEARCH_STRING.Contains(line.Trim()))
                        {
                            skipLine = true;

                            fileLines.Add(line);
                            fileLines.Add("#endif");
                            fileLines.Add("");

                            for (int i = 0; i < standard__Unity_PBS_Lighting__Lighting_Standard_Lines.Count; i++)
                            {
                                fileLines.Add(standard__Unity_PBS_Lighting__Lighting_Standard_Lines[i]);
                            }

                            fileLines.Add("");
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                if (!File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    File.Create(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE).Close();
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(
                        standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE,
                        standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE);
                }

                #endregion

                #region Image Based Lighting

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!string.IsNullOrEmpty(line) && STANDARD__UNITY_IMAGE_BASED_LIGHTING__UNITY_GLOSSY_ENVIRONMENT__SEARCH_STRING == line.Trim())
                        {
                            fileLines.Add("");

                            for (int i = 0; i < standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines.Count; i++)
                            {
                                fileLines.Add(standard__Unity_Image_Based_Lighting__Glossy_Environment_Lines[i]);
                            }

                            fileLines.Add("");
                            fileLines.Add(line);
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                if (!File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    File.Create(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE).Close();
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(
                        standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE,
                        standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE);
                }

                #endregion

                #region Shadow Library

                using (StreamReader reader = new StreamReader(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    bool skipLine = false;
                    bool fragmentFound = false;
                    bool replaceStartLine = false;
                    bool replaceEndLine = false;
                    List<string> tempLines = new List<string>();

                    while (!reader.EndOfStream)
                    {
                        if (skipLine)
                        {
                            skipLine = false;
                            continue;
                        }

                        string line = reader.ReadLine();

                        if (fragmentFound)
                        {
                            if (!replaceStartLine)
                            {
                                if (!string.IsNullOrEmpty(line) && line.Trim() == "#if defined (SHADOWS_SHADOWMASK)")
                                {
                                    replaceStartLine = true;

                                    for (int i = 0; i < standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines.Count; i++)
                                    {
                                        fileLines.Add(standard__Unity_Shadow_Library__Unity_Sample_Baked_Occlusion_Lines[i]);
                                    }
                                }
                                else
                                {
                                    fileLines.Add(line);
                                    continue;
                                }
                            }

                            if (!replaceEndLine)
                            {
                                if (!string.IsNullOrEmpty(line) && line.Trim() == "return saturate(dot(rawOcclusionMask, unity_OcclusionMaskSelector));")
                                {
                                    fragmentFound = false;
                                    replaceEndLine = true;
                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__UNITY_SHADOW_LIBRARY__UNITY_SAMPLE_BAKED_OCCLUSION__SEARCH_STRING == line.Trim())
                        {
                            fileLines.Add(line);
                            fragmentFound = true;
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                if (!File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    File.Create(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE).Close();
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                using (StreamReader reader = new StreamReader(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    bool skipLine = false;
                    bool fragmentFound = false;
                    bool replaceStartLine = false;
                    bool replaceEndLine = false;
                    bool isNGSSModified = false;

                    string target = "";

                    while (!reader.EndOfStream)
                    { 
                        if (skipLine)
                        {
                            skipLine = false;
                            continue;
                        }

                        string line = reader.ReadLine();

                        if (!isNGSSModified)
                        {
                            if (!string.IsNullOrEmpty(line) && line.Trim() == "//NGSS SUPPORT")
                            {
                                isNGSSModified = true;
                                target = "return UNITY_SAMPLE_TEX2D_SAMPLER(unity_ShadowMask, unity_Lightmap, lightmapUV.xy);//Unity 2017 and below";
                            }
                            else
                            {
                                target = "return UNITY_SAMPLE_TEX2D(unity_ShadowMask, lightmapUV.xy);";
                            }
                        }

                        if (fragmentFound)
                        {
                            if (!replaceStartLine)
                            {
                                if (!string.IsNullOrEmpty(line) && line.Trim() == target)
                                {
                                    replaceStartLine = true;

                                    for (int i = 0; i < standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines.Count; i++)
                                    {
                                        fileLines.Add(standard__Unity_Shadow_Library__Unity_Get_Raw_Baked_Occlusions_Lines[i]);
                                    }
                                }
                                else
                                {
                                    fileLines.Add(line);
                                    continue;
                                }
                            }

                            if (!replaceEndLine)
                            {
                                if (!string.IsNullOrEmpty(line) && line.Trim() == "#else")
                                {
                                    fileLines.Add(line);
                                    fragmentFound = false;
                                    replaceEndLine = true;
                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(line) && STANDARD__UNITY_SHADOW_LIBRARY__UNITY_GET_RAW_BAKED_OCCLUSIONS__SEARCH_STRING == line.Trim())
                        {
                            fileLines.Add(line);
                            fragmentFound = true;
                            continue;
                        }

                        fileLines.Add(line);
                    }
                }

                using (StreamWriter writer = new StreamWriter(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    for (int i = 0; i < fileLines.Count; i++)
                    {
                        writer.WriteLine(fileLines[i]);
                    }
                }

                fileLines.Clear();

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;

                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Copy(
                        standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE,
                        standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE);
                }

                #endregion

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    RunAsAdmin(processArguments);
                }

                if (File.Exists(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE))
                {
                    File.Delete(standard_RP_ModifySourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE);
                }

                if (File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    File.Delete(standard_RP_ModifySourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE);
                }

                if (File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE))
                {
                    File.Delete(standard_RP_ModifySourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE);
                }

                if (File.Exists(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE))
                {
                    File.Delete(standard_RP_ModifySourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("The file could not be read: " + e.Message);
            }
            #endregion

            if (EditorUtility.DisplayDialog(
                "Magic Lightmap Switcher", 
                "You must restart the editor for the changes to take effect. Restart now?", "Yes", "No, I will do it later."))
            {
                systemProperties.useSwitchingOnly = false;
                systemProperties.editorRestarted = true;
                EditorApplication.OpenProject(Directory.GetCurrentDirectory());
            }
            else
            {
                systemProperties.useSwitchingOnly = false;
                systemProperties.editorRestarted = false;
                Debug.LogFormat("<color=cyan>MLS:</color> You won't be able to use lightmap blending and switching until the editor is restarted.");
            }
        }

        private void RestoreStandardRPShaders()
        {
            CmdProcessArguments arguments;
            List<CmdProcessArguments> processArguments = new List<CmdProcessArguments>();

            if (File.Exists(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;  
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__GLOBAL_ILLUMINATION__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_PBS_LIGHTING__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_IMAGE_BASED_LIGHTING__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__CORE__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original"))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Copy;
                    arguments.copyFrom = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original";
                    arguments.copyTo = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));

                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original";
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE);
                    File.Copy(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original", standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE);
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original");
                    File.Delete(standard_RP_SourcesPath + STANDARD__UNITY_SHADOW_LIBRARY__DEFAULT_SOURCE_FILE + "_original.meta");
                }
            }

            if (File.Exists(standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE))
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    arguments = new CmdProcessArguments();

                    arguments.commandType = CmdProcessArguments.CommandType.Delete;
                    arguments.deleteFrom = standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE;
                    processArguments.Add(new CmdProcessArguments(arguments));
                }
                else
                {
                    File.Delete(standard_RP_SourcesPath + STANDARD__CORE__COMMON__INCLUDE_FILE);
                }
            }

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                RunAsAdmin(processArguments);
            }
        }
    }
}