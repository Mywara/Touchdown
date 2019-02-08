using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif
using UnityEngine;

/**
* \namespace    ns3dRudder
*
*/
namespace ns3dRudder
{
    /**
    * \class    AxesParamCustom
    *
    * \brief    The axes parameter custom.
    */
    //[CreateAssetMenu(fileName = "Custom", menuName = "3dRudder/AxesParamCustom", order = 1)]
    public class AxesParamCustom : IAxesParam
    {        
        // Custom curves
        public Curve[] CustomCurves;
        
        /**
        *
        * \brief    Constructor
        *
        *
        * ¤compatible_plateforme Win, PS4¤
        * ¤compatible_firm From x.4.x.x¤
        * ¤compatible_sdk From 2.00¤
        * 
        */
        public AxesParamCustom() : base()
        {
            CustomCurves = AxesParamDefault.GetDefaultNonNormalizedCurves();
            NonSymmetrical = true;            
        }     
       
        /**
        *
        * \brief    Update curves data
        *
        * ¤compatible_plateforme Win, PS4¤
        * ¤compatible_firm From x.4.x.x¤
        * ¤compatible_sdk From 2.00¤
        *
        * \param    nPortNumber The port number.
        *                       .
        * \return   The last error. The possible error code returned by this method. ErrorCode can also be a success. \n \n \n <b> Possible error codes : </b>
        *
        * \possibleret #Success
        * \possibleret #NotInitialized
        * \possibleret #FirmwareNeedToBeUpdated
        * \possibleret #DeviceNotSupported
        * \possibleret #ValidationError
        * \possibleret #DeviceInitError
        * \possibleret #NotConnected
        * \possibleret #NotReady
        * \possibleret #ValidationTimeOut
        * \possibleret #IncorrectCommand
        *
        */
        public override ErrorCode UpdateParam(uint nPortNumber)
        {
            ErrorCode nError = ErrorCode.Success;
            DeviceInformation Info = Sdk3dRudder.GetDeviceInformation(nPortNumber);
            if (Info != null)
            {
                float XSatLR = Info.GetUserRoll() * CustomCurves[(int)Axes.LeftRight].XSat / Info.GetMaxRoll();
                SetCurve(Axes.LeftRight, new Curve(XSatLR, CustomCurves[(int)Axes.LeftRight].DeadZone * XSatLR, CustomCurves[(int)Axes.LeftRight].Exp));

                float XSatFB = Info.GetUserPitch() * CustomCurves[(int)Axes.ForwardBackward].XSat / Info.GetMaxPitch();
                SetCurve(Axes.ForwardBackward, new Curve(XSatFB, CustomCurves[(int)Axes.ForwardBackward].DeadZone * XSatFB, CustomCurves[(int)Axes.ForwardBackward].Exp));

                float XSatR = Info.GetUserYaw() * CustomCurves[(int)Axes.Rotation].XSat / Info.GetMaxYaw();
                SetCurve(Axes.Rotation, new Curve(XSatR, CustomCurves[(int)Axes.Rotation].DeadZone * XSatR, CustomCurves[(int)Axes.Rotation].Exp));
            }
            else
                nError = Sdk3dRudder.GetLastError();

            SetCurve(Axes.UpDown, new Curve(CustomCurves[(int)Axes.UpDown].XSat, CustomCurves[(int)Axes.UpDown].DeadZone * CustomCurves[(int)Axes.UpDown].XSat, CustomCurves[(int)Axes.UpDown].Exp));
            return nError;
        }

#if UNITY_EDITOR
        public override void DrawParam()
        {
            EditorGUI.BeginChangeCheck();
            bool nonSymmetrical = EditorGUILayout.Toggle("Non Symmetrical", NonSymmetrical);
            float roll2YawCompensation = EditorGUILayout.Slider("Roll to Yaw compensation", Roll2YawCompensation, 0.0f, 1.0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "change axes param");
                NonSymmetrical = nonSymmetrical;
                Roll2YawCompensation = roll2YawCompensation;
                if (!EditorApplication.isPlaying)
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorUtility.SetDirty(this);
            }

        }

        public override void DrawCurve(IAxesParam axesParamNormalized, Axes axe, uint portNumber, float maxRange)
        {                          
            base.DrawCurve(axesParamNormalized, axe, portNumber);

            EditorGUI.BeginChangeCheck();
            //float xsat = Mathf.Max(0.11f, (float)System.Math.Round(GetCurve(axe).XSat, 2));            
            float DeadZone = EditorGUILayout.Slider("Deadzone", CustomCurves[(int)axe].DeadZone, 0.0f, 0.99f);          
            float XSat = EditorGUILayout.Slider("Sensitivity", CustomCurves[(int)axe].XSat, 0.1f, 2.0f);
            float Exp = EditorGUILayout.Slider("Shape", CustomCurves[(int)axe].Exp, 0.0f, 5.0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "change axes param curve");
                CustomCurves[(int)axe].DeadZone = DeadZone;
                CustomCurves[(int)axe].XSat = XSat;
                CustomCurves[(int)axe].Exp = Exp;
                if (!EditorApplication.isPlaying)
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}