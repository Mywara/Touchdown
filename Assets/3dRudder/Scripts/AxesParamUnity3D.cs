#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif
using UnityEngine;

namespace ns3dRudder
{
    public class U3D_AnimationCurve : ns3dRudder.Curve
    {
        public AnimationCurve Curve = null;

        public U3D_AnimationCurve(float fxSat, AnimationCurve curve) : base(fxSat,0.0f,1.0f)
        {
            Curve = curve;
        }

        public override float CalcCurveValue(float fValue)
        {
            return  Curve.Evaluate(fValue)* XSat;
        }
    }

    //[CreateAssetMenu(fileName = "Custom", menuName = "3dRudder/AxesParamCustomUnity", order = 1)]
    public class AxesParamUnity3D : IAxesParam
    {
        public AnimationCurve[] animationCurves;
        public AxesParamUnity3D() : base()
        {
            NonSymmetrical = true;
            animationCurves = new AnimationCurve[4];
            for(int i = 0; i < animationCurves.Length; ++i)
                animationCurves[i] = new AnimationCurve(new Keyframe(-1, -1), new Keyframe(-0.25f, 0), new Keyframe(0, 0), new Keyframe(0.25f, 0), new Keyframe(1, 1));
        }

        public override ErrorCode UpdateParam(uint nPortNumber)
        {
            ErrorCode nError = ErrorCode.Success;
            DeviceInformation Info = Sdk3dRudder.GetDeviceInformation(nPortNumber);
            if (Info != null)
            {
                SetCurve(Axes.LeftRight, new U3D_AnimationCurve(Info.GetUserRoll() / Info.GetMaxRoll(), animationCurves[(int)Axes.LeftRight]));
                SetCurve(Axes.ForwardBackward, new U3D_AnimationCurve(Info.GetUserPitch() / Info.GetUserPitch(), animationCurves[(int)Axes.ForwardBackward]));
                SetCurve(Axes.UpDown, new U3D_AnimationCurve(0.6f, animationCurves[(int)Axes.UpDown]));
                SetCurve(Axes.Rotation, new U3D_AnimationCurve(Info.GetUserYaw() / Info.GetMaxYaw(), animationCurves[(int)Axes.Rotation]));
            }
            else
                nError = Sdk3dRudder.GetLastError();
            
            Roll2YawCompensation = Info.GetDefaultRoll2YawCompensation();
            return nError;
        }

#if UNITY_EDITOR
        public override void DrawParam()
        {
            NonSymmetrical = EditorGUILayout.Toggle("Non Symmetrical", NonSymmetrical);
            Roll2YawCompensation = EditorGUILayout.Slider("Roll to Yaw compensation", Roll2YawCompensation, 0.0f, 1.0f);
        }

        float m_Value;

        public override void DrawCurve(IAxesParam axesParamNormalized, Axes axe, uint portNumber, float maxRange)
        {
            //base.DrawCurve(axe, portNumber);

            EditorGUI.BeginChangeCheck();
            /*
            //animationCurve = EditorGUILayout.CurveField(new GUIContent("Curve"), animationCurve, GUILayout.MinHeight(100));

            //This is the Label for the Slider
            GUI.Label(new Rect(0, 300, 100, 30), "Rectangle Width");
            //This is the Slider that changes the size of the Rectangle drawn
            m_Value = GUI.HorizontalSlider(new Rect(100, 300, 100, 30), m_Value, 1.0f, 250.0f);
            */
            var shader = Shader.Find("Hidden/Internal-Colored");
            Material mat = new Material(shader);            

            // curves representations
            Rect rect = GUILayoutUtility.GetRect(0, 0, 100, 100);         
            EditorGUI.CurveField(rect, animationCurves[(int)axe]);

            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(rect);
                GL.PushMatrix();

                //GL.Clear(true, false, Color.black);
                mat.SetPass(0);

                AxesValue axes = new AxesValue();
                ErrorCode error = Sdk3dRudder.GetAxes(portNumber, axesParamNormalized, axes);
                if (error != ErrorCode.Success)
                    Debug.LogFormat("[3dRudder] Error GetAxes: {0}", Sdk3dRudder.GetErrorText(error));

                GL.Begin(GL.LINES);
                GL.Color(new Color(0.8f, 0.0f, 0.0f));

                float xRudder = MapRangeValue(axes.Get(axe), -1.0f, 1.0f, 15.0f, rect.width);

                GL.Vertex3(xRudder, 0, 0);
                GL.Vertex3(xRudder, rect.height, 0);
                GL.End();

                GL.PopMatrix();
                GUI.EndClip();
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (!EditorApplication.isPlaying)
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorUtility.SetDirty(this);
            }
        }
#endif
    };

    /*[System.Serializable]
    public class AxesParamCurveUnity3D : IAxesParam
    {
        AnimationCurve[] curves;

        public AxesParamCurveUnity3D(AnimationCurve[] animCurve, bool nonSymmetrical, float roll2YawCompensation) : base()
        {
            curves = animCurve;
            NonSymmetrical = nonSymmetrical;
            Roll2YawCompensation = roll2YawCompensation;
        }

        public override ErrorCode UpdateParam(uint nPortNumber)
        { 
            ErrorCode nError = ErrorCode.Success;
            DeviceInformation Info = Sdk3dRudder.GetDeviceInformation(nPortNumber);
            if (Info != null)
            {
                SetCurve(Axes.LeftRight, new U3D_AnimationCurve(Info.GetUserRoll() / Info.GetMaxRoll(), curves[(int)Axes.LeftRight]));
                SetCurve(Axes.ForwardBackward, new U3D_AnimationCurve(Info.GetUserPitch() / Info.GetUserPitch(), curves[(int)Axes.ForwardBackward]));
                SetCurve(Axes.UpDown, new U3D_AnimationCurve(0.6f, curves[(int)Axes.UpDown]));
                SetCurve(Axes.Rotation, new U3D_AnimationCurve(Info.GetUserYaw() / Info.GetMaxYaw(), curves[(int)Axes.Rotation]));
            }
            else
                nError = Sdk3dRudder.GetLastError();   
            return nError;
        }
    };*/
}