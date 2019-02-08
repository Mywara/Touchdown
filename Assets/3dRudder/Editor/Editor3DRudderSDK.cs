using UnityEngine;
using UnityEditor;
using ns3dRudder;

public class Editor3dRudderSDK : EditorWindow
{
    private bool[] show = new bool[Sdk3dRudder.MAX_DEVICE];
    private static Texture logo;

    const int sizeSensor = 100;
    private readonly Vector2 sliderAxes = new Vector2(-1f, 1f);
    static AxesParamNormalizedLinear AxesParam;

    [MenuItem("Help/3dRudder SDK")]
    static void Help()
    {
        Application.OpenURL("https://3drudder-dev.com/docs/3drudder-documentations/3drudder-sdk-unity/");
    }

    [MenuItem("3dRudder/Controllers")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(Editor3dRudderSDK), false);
        window.minSize = new Vector2(500, 300);
        if (logo == null)
            logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3dRudder/Editor/3dRudderIcon.png");
        GUIContent titleContent = new GUIContent("3dRudder", logo);
        window.titleContent = titleContent;
        if (AxesParam == null)
            AxesParam = CreateInstance<AxesParamNormalizedLinear>();
        AxesParam.NonSymmetrical = true;
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField(string.Format("Unity Package Version {0:X4}", Sdk3dRudder._3DRUDDER_PACKAGE_VERSION), EditorStyles.boldLabel);

        // The actual window code goes here
        for (uint i = 0; i < Sdk3dRudder.MAX_DEVICE; ++i)
            DisplayRudder(i);
    }

    void DisplayRudder(uint i)
    {        
        Status status = Sdk3dRudder.GetStatus(i);
        ns3dRudder.ErrorCode nstatus = Sdk3dRudder.GetLastError();        
        string info;
        if (status == Status.IsNotConnected)
            info = status.ToString();
        else if (status == Status.Error || status == Status.NoStatus)
            info = Sdk3dRudder.GetErrorText(nstatus);
        else
            info = "Connected FW : " + Sdk3dRudder.GetVersion(i).ToString("X4");

        show[i] = EditorGUILayout.Foldout(show[i], "3dRudder " + i + " (" + info + ")");

        if (show[i])
        {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Axis", EditorStyles.boldLabel);
            
            AxesValue axes = new AxesValue();
            if (AxesParam == null)
                AxesParam = CreateInstance<AxesParamNormalizedLinear>();
            Sdk3dRudder.GetAxes(i, AxesParam, axes);

            EditorGUILayout.Slider("Left Right", ParsePrecision(axes.Get(Axes.LeftRight)), sliderAxes.x, sliderAxes.y);
            EditorGUILayout.Slider("Forward Backward", ParsePrecision(axes.Get(Axes.ForwardBackward)), sliderAxes.x, sliderAxes.y);
            EditorGUILayout.Slider("Up Down", ParsePrecision(axes.Get(Axes.UpDown)), sliderAxes.x, sliderAxes.y);
            EditorGUILayout.Slider("Rotation", ParsePrecision(axes.Get(Axes.Rotation)), sliderAxes.x, sliderAxes.y);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
            var style = new GUIStyle(GUI.skin.label);
            if (status == Status.IsNotConnected)
                style.normal.textColor = new Color(0.75f, 0.75f, 0);
            else if (status == Status.Error)
                style.normal.textColor = new Color(0.75f, 0, 0);
            else
                style.normal.textColor = status > Status.StayStill ? new Color(0, 0.75f, 0) : new Color(0, 0.25f, 0.75f);
            EditorGUILayout.BeginHorizontal();
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40;

            if (status == Status.Error)
            {
                ns3dRudder.ErrorCode nError = Sdk3dRudder.GetLastError();
                string sStatus = status.ToString() + " : " + Sdk3dRudder.GetErrorText(nError);
                EditorGUILayout.LabelField("Status", sStatus , style);
            }
            else
                EditorGUILayout.LabelField("Status", status.ToString(), style);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test sound"))
                Sdk3dRudder.PlaySnd(i, 4400, 100);
            /*if (GUILayout.Button("Test hide"))
                Sdk3dRudder.HideSystemDevice(i, true);*/
            bool frozen = Sdk3dRudder.IsFrozen(i);
            if (GUILayout.Button(frozen ? "Unfreeze" : "Freeze"))
                Sdk3dRudder.SetFreeze(i, !frozen);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Sensors", EditorStyles.boldLabel);
            DisplaySensor(i);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }

    void DisplaySensor(uint index)
    {
        EditorGUIUtility.labelWidth = 20;
        for (uint i = 0; i < 6; ++i)
            EditorGUILayout.FloatField(i.ToString(), Sdk3dRudder.GetSensor(index, i), GUILayout.MaxWidth(sizeSensor));
    }

    float ParsePrecision(float value)
    {
        return float.Parse(value.ToString("0.00"));
    }

    public void OnInspectorUpdate()
    {
        // Needed to repaint the window editor
        Repaint();
    }
}
