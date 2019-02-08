using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using ns3dRudder;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(Controller3dRudder))]
public class Controller3dRudderEditor : Editor
{
    static string PATH_MESH /*= "Assets/3dRudder/Mesh/"*/;
    static public string PATH_EDITOR /*= "Assets/3dRudder/Editor/"*/;
    static public string PATH_ASSET /*= "Assets/3dRudder/Scripts/Params/Data/{0}.asset"*/;
    static public List<string> AxesDefaultParamList = new List<string>() { "Default", "NormalizedLinear", "Angle" };

    public List<string> ModeList = new List<string>();
    Dictionary<string, string> ModeListName = new Dictionary<string, string>();


    string[] RudderIndexes = new [] { "0", "1", "2", "3" };
    public List<string> AxesParamList = new List<string>(AxesDefaultParamList);
    Dictionary<string, string> AxesParamPath = new Dictionary<string, string>();
    int currentAxesParam = 0;
    int currentMode = 0;
    
    AxesParamNormalizedLinear axesParamNormalized;

    GameObject rudderModel;
    MeshFilter rudderMeshFilter;
    MeshRenderer rudderMeshRenderer;
    GameObject crossModel;
    MeshFilter crossMeshFilter;
    MeshRenderer crossMeshRenderer;
    PreviewRenderUtility previewRenderUtility;
    
    SerializedProperty portNumber;
    SerializedProperty canForward;
    SerializedProperty speedForwardBackward;
    SerializedProperty smoothForwardBackward;
    SerializedProperty canStrafe;
    SerializedProperty speedLeftRight;
    SerializedProperty smoothLeftRight;
    SerializedProperty canUpDown;
    SerializedProperty speedUpDown;
    SerializedProperty smoothUpDown;
    SerializedProperty canRotate;
    SerializedProperty speedRotation;
    SerializedProperty smoothRotation;

    private void OnEnable()
    {
        var controller3dRudder = target as Controller3dRudder;
        
        string[] nsRudderScriptGuids = AssetDatabase.FindAssets("ns3dRudder");
        string nsRudderScriptPath = AssetDatabase.GUIDToAssetPath(nsRudderScriptGuids[0]);

        string RudderFolderPath = nsRudderScriptPath.Substring(0, nsRudderScriptPath.Length - 21);

        PATH_MESH = Path.Combine(RudderFolderPath, "Mesh/");
        PATH_EDITOR = Path.Combine(RudderFolderPath, "Editor/");
        PATH_ASSET = Path.Combine(RudderFolderPath, "Scripts/Params/Data/{0}.asset");    

        axesParamNormalized = CreateInstance<AxesParamNormalizedLinear>();
        // Show all locomotion
        RefreshMode(controller3dRudder);
        // Add or delete in the list if new or less config
        RefreshConfig(controller3dRudder);

        rudderModel = (GameObject)AssetDatabase.LoadAssetAtPath(PATH_MESH+"3dR_Low_Poly_Unity2.obj", typeof(GameObject));
        crossModel = (GameObject)AssetDatabase.LoadAssetAtPath(PATH_MESH+"3dR_Low_Poly_Unity_Ref3.obj", typeof(GameObject));
        portNumber = serializedObject.FindProperty("PortNumber");
        canForward = serializedObject.FindProperty("CanForward");
        speedForwardBackward = serializedObject.FindProperty("SpeedForwardBackward");
        smoothForwardBackward = serializedObject.FindProperty("SmoothForwardBackward");
        canStrafe = serializedObject.FindProperty("CanStrafe");
        speedLeftRight = serializedObject.FindProperty("SpeedLeftRight");
        smoothLeftRight = serializedObject.FindProperty("SmoothLeftRight");
        canUpDown = serializedObject.FindProperty("CanUpDown");
        speedUpDown = serializedObject.FindProperty("SpeedUpDown");
        smoothUpDown = serializedObject.FindProperty("SmoothUpDown");
        canRotate = serializedObject.FindProperty("CanRotate");
        speedRotation = serializedObject.FindProperty("SpeedRotation");
        smoothRotation = serializedObject.FindProperty("SmoothRotation");
    }

    #region functions
    public void AddConfig(string file, string path)
    {        
        AxesParamList.Add(file);
        AxesParamPath.Add(file, path);
        currentAxesParam = AxesParamList.FindIndex(x => x == file);
        var controller3dRudder = target as Controller3dRudder;
        controller3dRudder.axesParam = (IAxesParam)AssetDatabase.LoadAssetAtPath(AxesParamPath[file], typeof(IAxesParam));
    }

    public void EditConfig(string oldfile, string newfile, string newpath)
    {
        AxesParamList[AxesParamList.FindIndex(x => x == oldfile)] = newfile;
        AxesParamPath.Remove(oldfile);
        AxesParamPath.Add(newfile, newpath);
    }

    public void RemoveConfig(string file)
    {
        string path = AxesParamPath[file];
        int index = AxesParamList.IndexOf(file);
        AssetDatabase.DeleteAsset(path);
        AxesParamList.Remove(file);
        AxesParamPath.Remove(file);

        if (index <= currentAxesParam)
        {
            var controller3dRudder = target as Controller3dRudder;
            currentAxesParam--;
            controller3dRudder.axesParam = (IAxesParam)AssetDatabase.LoadAssetAtPath(AxesParamPath[AxesParamList[currentAxesParam]], typeof(IAxesParam));
        }
    }

    public IEnumerable<Type> FindSubClassesOf<TBaseType>()
    {
        var baseType = typeof(TBaseType);
        var assembly = baseType.Assembly;

        return assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
    }

    private void RefreshMode(Controller3dRudder controller3dRudder)
    {
        var preprocessors = FindSubClassesOf<ns3dRudder.ILocomotion>();
        ILocomotion locomotion = controller3dRudder.GetComponent<ILocomotion>();
        if (locomotion == null)
        {
            locomotion = controller3dRudder.gameObject.AddComponent<FPSLocomotion>();
        }
        foreach (var t in preprocessors)
        {
            ModeList.Add(t.Name);
            ModeListName.Add(t.Name, t.FullName);
            if (locomotion && t.Name == locomotion.GetType().Name)
                currentMode = ModeList.Count - 1;
        }
    }

    private void RefreshConfig(Controller3dRudder controller3dRudder)
    {        
        string[] guids1 = AssetDatabase.FindAssets("t:IAxesParam", null);
        foreach (string guid1 in guids1)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid1);
            string file = Path.GetFileNameWithoutExtension(path);

            if (!AxesParamList.Contains(file))
                AxesParamList.Add(file);

            if (controller3dRudder.axesParam != null && controller3dRudder.axesParam.name == file)
            {                
                currentAxesParam = AxesParamList.FindIndex(x => x == file);                
            }
            if (!AxesParamPath.ContainsKey(file))
                AxesParamPath.Add(file, path);
        }
        
        if (controller3dRudder.axesParam == null)
        {
            currentAxesParam = AxesParamList.FindIndex(x => x == "Angle");
        }
    }
    #endregion

    public override void OnInspectorGUI()
    {
        var controller3dRudder = target as Controller3dRudder;        
        serializedObject.Update();
        RefreshConfig(controller3dRudder);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            // Port number device
            portNumber.intValue = EditorGUILayout.Popup("Port Number", portNumber.intValue, RudderIndexes);
            EditorGUILayout.LabelField("Locomotion", EditorStyles.boldLabel);
            // Style of movement (FLY, MOVE, FPS, ORBIT, etc)            
            EditorGUI.BeginChangeCheck();
            currentMode = EditorGUILayout.Popup("Mode", currentMode, ModeList.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                ILocomotion locomotion = controller3dRudder.GetComponent<ILocomotion>();
                // same not need to change
                if (locomotion.name != ModeList[currentMode])
                {
                    DestroyImmediate(locomotion);
                    Type t = typeof(ns3dRudder.ILocomotion).Assembly.GetType(ModeListName[ModeList[currentMode]]);
                    controller3dRudder.gameObject.AddComponent(t);
                    if (!EditorApplication.isPlaying)
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    EditorGUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.HelpBox("See FPSLocomotion.cs to know how to implement new locomotion", MessageType.Info, true);
            if (GUILayout.Button("Create Custom Script Mode"))
            {                
                LocomotionScriptTemplate.CreateScriptableObject();
            }
            
            EditorGUILayout.LabelField("Speed Factor", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            // Translation
            EditorGUILayout.BeginHorizontal();
            {
                canForward.boolValue = EditorGUILayout.BeginToggleGroup("Forward / Backward", canForward.boolValue);
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(speedForwardBackward, GUIContent.none);
                    SerializedProperty smooth = smoothForwardBackward.FindPropertyRelative("Enable");
                    smooth.boolValue = EditorGUILayout.ToggleLeft("Smooth", smooth.boolValue, GUILayout.MaxWidth(100));                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndToggleGroup();                
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            {
                canStrafe.boolValue = EditorGUILayout.BeginToggleGroup("Left / Right", canStrafe.boolValue);
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(speedLeftRight, GUIContent.none);
                    SerializedProperty smooth = smoothLeftRight.FindPropertyRelative("Enable");
                    smooth.boolValue = EditorGUILayout.ToggleLeft("Smooth", smooth.boolValue, GUILayout.MaxWidth(100));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndHorizontal();            
            
            // Up down
            EditorGUILayout.BeginHorizontal();
            {
                canUpDown.boolValue = EditorGUILayout.BeginToggleGroup("Up / Down", canUpDown.boolValue);
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(speedUpDown, GUIContent.none);
                    SerializedProperty smooth = smoothUpDown.FindPropertyRelative("Enable");
                    smooth.boolValue = EditorGUILayout.ToggleLeft("Smooth", smooth.boolValue, GUILayout.MaxWidth(100));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndHorizontal();            

            // Rotation
            EditorGUILayout.BeginHorizontal();
            {
                canRotate.boolValue = EditorGUILayout.BeginToggleGroup("Rotation", canRotate.boolValue);
                EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(speedRotation, GUIContent.none);
                    SerializedProperty smooth = smoothRotation.FindPropertyRelative("Enable");
                    smooth.boolValue = EditorGUILayout.ToggleLeft("Smooth", smooth.boolValue, GUILayout.MaxWidth(100));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;            
        }
        EditorGUILayout.EndVertical();
            
        uint index = (uint)portNumber.intValue;
        if (Sdk3dRudder.IsDeviceConnected(index))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // Axes param config
                EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                currentAxesParam = EditorGUILayout.Popup("Select Axes Params", currentAxesParam, AxesParamList.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(controller3dRudder, "change axes param config");
                    string nameParam = AxesParamList[currentAxesParam];
                    if (AxesParamPath.ContainsKey(nameParam))
                        controller3dRudder.axesParam = (IAxesParam)AssetDatabase.LoadAssetAtPath(AxesParamPath[nameParam], typeof(IAxesParam));
                    else
                        controller3dRudder.axesParam = null;
                    if (!EditorApplication.isPlaying)
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }

                // Actions buttons
                EditorGUILayout.BeginHorizontal();
                {
                    // if click on the custom axes param add button
                    if (GUILayout.Button("Create Custom Axes Params"))
                    {
                        AxesParamAddWindow win = CreateInstance<AxesParamAddWindow>();
                        win.Init(this);
                    }
                    if (GUILayout.Button("Edit"))
                    {
                        AxesParamEditWindow win = CreateInstance<AxesParamEditWindow>();
                        win.Init(this);
                    }
                    // if click on the custom axes param del button
                    if (GUILayout.Button("Delete"))
                    {
                        AxesParamDelWindow win = CreateInstance<AxesParamDelWindow>();
                        win.Init(this);
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Custom param             
                if (controller3dRudder.axesParam)
                {
                    controller3dRudder.axesParam.DrawParam();
                    //axesParamNormalized.NonSymmetrical = controller3dRudder.axesParam.NonSymmetrical;
                    axesParamNormalized.Roll2YawCompensation = controller3dRudder.axesParam.Roll2YawCompensation;

                    // CURVES EDITOR
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.LabelField("Curves", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;

                        if (canForward.boolValue)
                        {
                            // Forward / Backward
#if UNITY_5_5_OR_NEWER
                            EditorPrefs.SetBool("showFB", EditorGUILayout.Foldout(EditorPrefs.GetBool("showFB"), "Forward / Backward", true));                            
#else
                            EditorPrefs.SetBool("showFB", EditorGUILayout.Foldout(EditorPrefs.GetBool("showFB"), "Forward / Backward"));
#endif
                            if (EditorPrefs.GetBool("showFB"))
                                controller3dRudder.axesParam.DrawCurve(axesParamNormalized, Axes.ForwardBackward, controller3dRudder.PortNumber);
                        }
                        if (canStrafe.boolValue)
                        {
                            // Left / Right
#if UNITY_5_5_OR_NEWER
                            EditorPrefs.SetBool("showLR", EditorGUILayout.Foldout(EditorPrefs.GetBool("showLR"), "Left / Right", true));
#else
                            EditorPrefs.SetBool("showLR", EditorGUILayout.Foldout(EditorPrefs.GetBool("showLR"), "Left / Right"));
#endif 
                            if (EditorPrefs.GetBool("showLR"))
                                controller3dRudder.axesParam.DrawCurve(axesParamNormalized, Axes.LeftRight, controller3dRudder.PortNumber);
                        }
                        if (canUpDown.boolValue)
                        {
                            // Up / Down
#if UNITY_5_5_OR_NEWER
                            EditorPrefs.SetBool("showUD", EditorGUILayout.Foldout(EditorPrefs.GetBool("showUD"), "Up / Down", true));
#else
                            EditorPrefs.SetBool("showUD", EditorGUILayout.Foldout(EditorPrefs.GetBool("showUD"), "Up / Down"));
#endif
                            if (EditorPrefs.GetBool("showUD"))
                                controller3dRudder.axesParam.DrawCurve(axesParamNormalized, Axes.UpDown, controller3dRudder.PortNumber);
                        }
                        if (canRotate.boolValue)
                        {
                            // Rotation
#if UNITY_5_5_OR_NEWER
                            EditorPrefs.SetBool("showR", EditorGUILayout.Foldout(EditorPrefs.GetBool("showR"), "Rotation Left / Right", true));
#else
                            EditorPrefs.SetBool("showR", EditorGUILayout.Foldout(EditorPrefs.GetBool("showR"), "Rotation Left / Right"));
#endif
                            if (EditorPrefs.GetBool("showR"))
                                controller3dRudder.axesParam.DrawCurve(axesParamNormalized, Axes.Rotation, controller3dRudder.PortNumber);
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            var style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.red;
            style.fontStyle = FontStyle.Bold;
            GUILayout.Label("3dRudder not connected", style);
            ns3dRudder.ErrorCode error = Sdk3dRudder.GetLastError();
            if (error > ns3dRudder.ErrorCode.NotReady)
                Debug.LogErrorFormat("[3dRudder] {0}", Sdk3dRudder.GetErrorText(error));
        }        
        serializedObject.ApplyModifiedProperties();
    }

#region Preview
    private bool ValidateData()
    {
        if (rudderModel != null)
        {
            rudderMeshFilter = rudderModel.GetComponentInChildren<MeshFilter>();
            crossMeshFilter = crossModel.GetComponentInChildren<MeshFilter>();
            rudderMeshRenderer = rudderModel.GetComponentInChildren<MeshRenderer>();
            crossMeshRenderer = crossModel.GetComponentInChildren<MeshRenderer>();
        }
        else
        {
            return false;
        }

        if (previewRenderUtility == null)
        {
            previewRenderUtility = new PreviewRenderUtility();
#if UNITY_2017_1_OR_NEWER
            previewRenderUtility.cameraFieldOfView = 15.0f;            
            previewRenderUtility.camera.transform.position = new Vector3(0, 2, -2.0f);
            
            if (rudderModel != null)
                previewRenderUtility.camera.transform.LookAt(rudderModel.transform);
#else
            previewRenderUtility.m_CameraFieldOfView = 15.0f;
            previewRenderUtility.m_Camera.transform.position = new Vector3(0, 2, -2.0f);

            if (rudderModel != null)
                previewRenderUtility.m_Camera.transform.LookAt(rudderModel.transform);
#endif
        }

        return true;
    }

    public override GUIContent GetPreviewTitle()
    {
        return new GUIContent("3dRudder Live Viewer Input");
    }

    private void OnDisable()
    {
        if (previewRenderUtility != null)
        {
            previewRenderUtility.Cleanup();
        }
    }

    public override bool HasPreviewGUI()
    {
        if (ValidateData())
            return true;
        else
            return false;
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (Event.current.type == EventType.Repaint)
        {
            var controller3dRudder = target as Controller3dRudder;            
            if (Sdk3dRudder.IsDeviceConnected(controller3dRudder.PortNumber))
            {
                previewRenderUtility.BeginPreview(r, background);
                bool oldFog = RenderSettings.fog;
                Unsupported.SetRenderSettingsUseFogNoDirty(false);
#if UNITY_2017_1_OR_NEWER
                previewRenderUtility.lights[0].intensity = 1.4f;
                previewRenderUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
                previewRenderUtility.lights[1].intensity = 0f;                
                //previewRenderUtility.ambientColor = new Color(.1f, .1f, .1f, 0);                
                previewRenderUtility.camera.clearFlags = CameraClearFlags.Nothing;
#else
                previewRenderUtility.m_Light[0].intensity = 1.4f;
                previewRenderUtility.m_Light[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
                previewRenderUtility.m_Light[1].intensity = 0f;
                previewRenderUtility.m_Camera.clearFlags = CameraClearFlags.Nothing;
#endif
                AxesValue axesValues = new AxesValue();
                Sdk3dRudder.GetAxes(controller3dRudder.PortNumber, null, axesValues);
                Status status = Sdk3dRudder.GetStatus(controller3dRudder.PortNumber);

                Vector4 axes = Vector4.zero ;
                Quaternion quatRudder;
                if (status == Status.InUse)
                {                    
                    axes = new Vector4(axesValues.Get(Axes.ForwardBackward), axesValues.Get(Axes.Rotation), axesValues.Get(Axes.LeftRight), axesValues.Get(Axes.UpDown));
                    quatRudder = Quaternion.Euler(axes.x * -1, axes.y + 180.0f, axes.z);
                    // Cross
                    for (int i = 0; i < crossMeshFilter.sharedMesh.subMeshCount; ++i)
                        previewRenderUtility.DrawMesh(crossMeshFilter.sharedMesh, new Vector3(0, -0.01f, 0), Quaternion.identity, crossMeshRenderer.sharedMaterials[i], i);
                }
                else
                {
                    quatRudder = Quaternion.Euler(60, 180, 0);
                }
                // Device
                for(int i = 0; i < rudderMeshFilter.sharedMesh.subMeshCount; ++i)
                    previewRenderUtility.DrawMesh(rudderMeshFilter.sharedMesh, Vector3.zero, quatRudder, rudderMeshRenderer.sharedMaterials[i], i);
#if UNITY_2017_1_OR_NEWER
                previewRenderUtility.Render();                
                previewRenderUtility.EndAndDrawPreview(r);
#else
                previewRenderUtility.m_Camera.Render();
                Texture resultRender = previewRenderUtility.EndPreview();
                GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
#endif
                Unsupported.SetRenderSettingsUseFogNoDirty(oldFog);

                // Display 3dRudder info                                
                float y = r.y + r.height * 0.1f;
                float x = 10;
                int shift = 20;
                GUIStyle style = new GUIStyle();
                style.richText = true;
                style.normal.textColor = Color.white;
                DeviceInformation info = Sdk3dRudder.GetDeviceInformation(controller3dRudder.PortNumber);
                if (info != null)
                {
                    GUI.Label(new Rect(x, y, 200, 30), string.Format("Pitch: <color={1}>{0:0.00}</color>°", axes.x, Mathf.Abs(axes.x) > info.GetUserPitch() ? "red" : "white"), style); y += shift;
                    GUI.Label(new Rect(x, y, 200, 30), string.Format("Roll: <color={1}>{0:0.00}</color>°", axes.z, Mathf.Abs(axes.z) > info.GetUserRoll() ? "red" : "white"), style); y += shift;
                    GUI.Label(new Rect(x, y, 200, 30), string.Format("Yaw: <color={1}>{0:0.00}</color>°", axes.y, Mathf.Abs(axes.y) > info.GetUserYaw() ? "red" : "white"), style); y += shift;
                    GUI.Label(new Rect(x, y, 200, 30), string.Format("UpDown: {0:0.00}%", axes.w), style); y += shift;
                    GUI.Label(new Rect(x, y, 200, 30), string.Format("Status: {0}", Sdk3dRudder.GetStatus(controller3dRudder.PortNumber)), style); y += shift;
                }
            }
        }
    }
#endregion

#region ScriptTemplate
    public static class LocomotionScriptTemplate
    {
        private static string TemplatePath
        {
            get { return Application.dataPath + "/3dRudder/Editor/LocomotionTemplate.cs.txt"; }
        }

        private static MethodInfo CreateScriptAsset
        {
            get
            {
                var projectWindowUtilType = typeof(ProjectWindowUtil);
                return projectWindowUtilType.GetMethod("CreateScriptAsset", BindingFlags.NonPublic | BindingFlags.Static);
            }
        }

        [MenuItem("Assets/Create/3dRudder/Locomotion", priority = 81)] // Create/C# Script has priority 80, so this puts it just below that.
        public static void CreateScriptableObject()
        {
            CreateScriptAsset.Invoke(null, new object[] { TemplatePath, "NewLocomotion.cs" });
        }
    }
#endregion
}
