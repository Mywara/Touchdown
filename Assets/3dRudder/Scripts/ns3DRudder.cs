/**
* \file ns3dRudder.cs.
*
* \brief    The C# 3dRudder SDK
*/
#define DEBUG_3DRUDDER

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Runtime.Remoting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/**
* \namespace    ns3dRudder
*
* \brief    Namespace used for the 3dRudder SDK Unity
*
*/
namespace ns3dRudder
{  
    /// Values that represent axes returned by GetAxes.
    public enum Axes
    {
        /// Left / Right Axis resulting from the physical action on the Roll angle of the 3dRudder. 
        LeftRight = 0,              
        /// Forward / Backward Axis resulting from the physical action on the Pitch angle of the 3dRudder.
        ForwardBackward,            
        /// Up / Down Axis resulting on the pressure sensor action on the 3dRudder.
        UpDown,                     
        /// Horizontal Rotation Axis resulting from the physical action on the Yaw Angle of the 3dRudder.
        Rotation,                   
        /// The Maximum possible axes
        MaxAxes,        
    }

    /// Values that represent status.
    public enum Status
    {
        /// While the 3dRudder initializes.
        NoStatus,

        /// Puts the 3dRudder on the floor, curved side below, without putting your feet on the device. The user waits for approx. 5 seconds for the 3dRudder to boot up until 3 short beeps are heard.
        NoFootStayStill,

        /// The 3dRudder initialize for about 2 seconds. Once done a long beep will be heard from the device. The 3dRudder is then operational.
        Initialization,

        /// Put your first feet on the 3dRudder.
        PutYourFeet,

        /// Put your second Foot on the 3dRudder.
        PutSecondFoot,

        /// The user must wait still for half a second for user calibration until a last short beep is heard from the device. The 3dRudder is then ready to be used.
        StayStill,

        /// The 3dRudder is in use.
        InUse,

        /// The 3dRudder is frozen.
        Frozen = 253,

        /// The 3dRudder is not connected.
        IsNotConnected = 254,

        /// Call GetLastError function to get the error code
        Error = 255,
    }

    /// Values that represent error codes.
    public enum ErrorCode
    {
        /// The command had been successful
        Success = 0,                
        /// The 3dRudder is not connected
        NotConnected,               
        /// The device fail to execute the command
        Fail,                       
        /// Incorrect intern command
        IncorrectCommand,           
        /// Timeout communication with the 3dRudder
        Timeout,                    
        /// Device not supported by the SDK
        DeviceNotSupported,         
        /// The new connected 3dRudder did an error at the Initialization
        DeviceInitError,            
        /// The security of the 3dRudder had not been validated.
        ValidationError,            
        /// The security of the 3dRudder did a timeout : it could append when you stop the thread when debugging. 
        ValidationTimeOut,          
        /// The 3dRudder isn't ready
        NotReady,                   
        /// Indicated that the Firmware must be updated
        FirmwareNeedToBeUpdated,    
        /// The 3dRudder's SDK isn't initialized
        NotInitialized,             
        /// This command is not supported in this version  of the SDK (or plateform).
        NotSupported,
        /// The dashboard is not installed
        DashboardInstallError,
        /// The dashboard need to be updated
        DashboardUpdateError,
        /// Other Errors.
        Other = 0xFF
    }

    
    /// delegate for calculate curve value
    public delegate float CalcCurveValue_t(IntPtr pCurve, float value);
    /// delegate for callback event
    public delegate void CallbackTypeConnect(UInt32 value);

    /**
     * \class   Sdk3dRudder
     *
     * \brief   This SDK allows you to integrate the 3dRudder in your Game or experience.
     *
     * All the SDK is defined in the class Sdk3dRudder.
     *
     * With this SDK it's possible to manage up to four 3dRudder MAX_DEVICE defines the max ports number (from 0 to 3).
     */
    public class Sdk3dRudder
    {
        public static readonly int MAX_DEVICE = 4;
        public static readonly uint _3DRUDDER_SDK_VERSION = 0x0203;
        public static readonly uint _3DRUDDER_PACKAGE_VERSION = 0x0206;

#if UNITY_EDITOR
        static Sdk3dRudder()
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.quitting += EditorApplication_quitting;
#endif
            EditorApplication.update += Update;
        }

        private static bool initialized = false;
        static void Update()
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }
            //EditorApplication.update -= RunOnce;
            EventData eventData = GetEventMessage();
            if (eventData != null)
            {
                switch (eventData.typeEvent)
                {
                    case EventData.TypeEVent.OnConnect:
                        EventRudder.OnConnect(eventData.portNumber);
                        break;
                    case EventData.TypeEVent.OnDisconnect:
                        EventRudder.OnDisconnect(eventData.portNumber);
                        break;
                    case EventData.TypeEVent.OnEndSound:
                        EventRudder.OnEndSound(eventData.portNumber);
                        break;
                }
            }
        }

        private static void EditorApplication_quitting()
        {
            // Stop SDK
            Stop();
            initialized = false;
        }
#endif

        /**
         *
         * \brief   Initializes Sdk3dRudder with SetEvent() and Init()
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤ 
         *                      .
         */
        public static void Initialize()
        {
#if DEBUG_3DRUDDER            
            EventRudder.OnConnectEvent += (portNumber) => Debug.LogFormat("[3dRudder]  {0} connected, firmware :{1:X4}", portNumber, GetVersion(portNumber));
            EventRudder.OnDisconnectEvent += (portNumber) => Debug.LogFormat("[3dRudder]  {0} disconnected, firmware : {1:X4}", portNumber, GetVersion(portNumber));
            EventRudder.OnEndSoundEvent += (portNumber) => Debug.LogFormat("[3dRudder]  {0} EndSound Event", portNumber);
#endif

#if UNITY_EDITOR
            Import.InitQueueEvent();
#else
            SetEvent(new CallbackTypeConnect(EventRudder.OnConnect), new CallbackTypeConnect(EventRudder.OnDisconnect), new CallbackTypeConnect(EventRudder.OnEndSound));
#endif
            // Init SDK
            Init();

            // Show info
            Debug.LogFormat("[3dRudder] SDK Version : {0:X4} / Package Version {1:X4}", GetSDKVersion(), _3DRUDDER_PACKAGE_VERSION);            
        }

        #region functions
        /**
         *
         * \brief   Initializes this SDK
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \return  The possible error code returned by this method. ErrorCode can also be a success. \n \n \n <b> Possible error codes : </b>
         *
         * \possibleret #Success
         * \possibleret #Timeout
         * \possibleret #NotReady
         * \possibleret #IncorrectCommand    
         *                      .
         */
        public static ErrorCode Init()
        {
            ErrorCode init = (ErrorCode)Import.Init();
#if DEBUG_3DRUDDER
            Debug.LogFormat("[3dRudder] SDK Init : {0}[{1}]", init, GetErrorText(init));
#endif
#if UNITY_EDITOR
            if (init == ErrorCode.DashboardInstallError || init == ErrorCode.DashboardUpdateError || init == ErrorCode.ValidationError)
            {
                EditorUtility.DisplayDialog("Warning: 3dRudder SDK failing to load", "Please, " + GetErrorText(init), "Ok");
                Application.OpenURL("https://www.3drudder.com/start/");
            }
            else if (GetSDKVersion() > _3DRUDDER_SDK_VERSION)
            {
                EditorUtility.DisplayDialog("Warning: 3dRudder unity package no up to date", "Please, you have to update the 3dRudder unity package", "Ok");                
            }
            else if (GetSDKVersion() < _3DRUDDER_SDK_VERSION)
            {
                EditorUtility.DisplayDialog("Warning: 3dRudder SDK no up to date", "Please, you have to update the 3dRudder dashboard", "Ok");
            }
#endif
            return init;
        }

        /**
         *
         * \brief   Gets the From SDK version.
         *
         * Return the From Firmware version of the library. The returned value is coded in BCD (binary coded decimal), so 0x0200 means version 2.0
         * 
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \attention  It's not necessary to use Init() function before.
         * 
         * \return  The From Firmware version. The version The returned value is coded in BCD (binary coded decimal), so 0x1410 means version 1.4.1.0
         *          .
         */
        public static ushort GetSDKVersion()
        {
            return Import.GetSDKVersion();
        }

        /**
         *
         * \brief   Gets number of connected device. Maximum 4
         *
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \return  The number of connected device.
         *          .
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
         *          .
         */
        public static UInt32 GetNumberOfConnectedDevice()
        {
            return Import.GetNumberOfConnectedDevice();
        }

        /**
         *
         * \brief   Query if 'nPortNumber' is device connected
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]
         *
         * \return  True if device connected, false or error if not. Check GetLastError for possible error.  \n \n \n <b> Possible error codes : </b>
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
         *          .
         */
        public static bool IsDeviceConnected(UInt32 nPortNumber)
        {
            return Import.IsDeviceConnected(nPortNumber);
        }

        /**
         *
         * \brief   Gets the firmware's version.
         *
         * \brief   Return version number of the firmware of the 3dRudder connected to the nPortNumber port.
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]
         *
         * \return  The version. 
         * \possibleret The version is a fixed point unsigned short in hexadecimal: 0x1318 means version 1.3.1.8. 
         * \possibleret Return 0xFFFF in case of error. 
         * \possibleret Return 0 if the 3dRudder isn't Ready. \n \n \n <b> Possible error codes returned by this method : </b>
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
         *          .
         */
        public static ushort GetVersion(UInt32 nPortNumber)
        {
            return Import.GetFWVersion(nPortNumber);
        }

        /**
         *
         * \brief   Hides the system device.
         *
         * By default the 3dRudder is seen by the system as a Directinput device, a mouse or a keyboard (this can be changed thanks to the dashboard). 
         *
         * The function HideSystemDevice allows to hide the 3dRudder from the system, so your game will not see it as a DirectInput device. 
         *
         * **Please think to put it back in standard mode when you exit your game !**
         *
         *
         * ¤compatible_plateforme Win¤
         * ¤compatible_firm From 1.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]
         * \param   bHide       True to hide, False to show.
         *
         * \return  The possible error code returned by this method. ErrorCode can also be a success. \n \n \n <b> Possible error codes under Windows :</b>
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
         * \possibleret #Fail
         * \possibleret #Timeout
         * 
         */
        public static ErrorCode HideSystemDevice(UInt32 nPortNumber, bool bHide)
        {            
            return Import.HideSystemDevice(nPortNumber, bHide);
        }

        /**
         *
         * \brief   Query if 'nPortNumber' is system device hidden
         *
         *
         *    ¤compatible_plateforme Win¤
         *    ¤compatible_firm From 1.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]
         *
         * \return  True if system device hidden, false or error if not. Check GetLastError for possible error.  \n \n \n <b> Possible error codes : </b>
         *          .
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
         * \possibleret #Fail
         * \possibleret #Timeout
         * 
         */
        public static bool IsSystemDeviceHidden(UInt32 nPortNumber)
        {
            return Import.IsSystemDeviceHidden(nPortNumber);
        }

        /**
         *
         * \brief   Play sound
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]
         * \param   nFrequency  The frequency of the sound in Hz (440 is a A).
         * \param   nDuration   The duration.
         *
         * \return  The possible error code returned by this method. ErrorCode can also be a success. \n \n \n <b> Possible error codes : </b>
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
         * \possibleret #Fail
         * \possibleret #Timeout
         *          .
         */
        public static ErrorCode PlaySnd(UInt32 nPortNumber, ushort nFrequency, ushort nDuration)
        {
            return Import.PlaySnd(nPortNumber, nFrequency, nDuration);
        }

        /**
         *
         * \brief   Play sound list in Tone.
         * 
         * Play a sequence of sound on a 3dRudder connected to the nPortNumber port defined by sTones array with the size nSize.
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.0.8¤
         *    ¤compatible_sdk From 2.00¤
         * 
         * \attention Usable after the firmware version X.4.0.8. 
         * 
         * \param       nPortNumber         The port number of the 3dRudder [0,3]
         * \param       nSize               The size.
         * \param [in]  sTones              The tones list.
         * \param       bAddToPlayedList    (Optional) Add to played List (true: default) or Replace the played list.
         *
         * \return  The possible error code returned by this method. ErrorCode can also be a success. \n \n \n <b> Possible error codes : </b>
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
         * \possibleret #Fail
         * \possibleret #Timeout
         *      .
         */
        public static ErrorCode PlaySndEx(UInt32 nPortNumber, UInt32 nSize, Tone sTones, bool bAddToPlayedList = true)
        {
            GCHandle pTones = GCHandle.Alloc(sTones, GCHandleType.Pinned);
            ErrorCode error = Import.PlaySndEx1(nPortNumber, nSize, pTones.AddrOfPinnedObject(), bAddToPlayedList);
            pTones.Free();
            return error;
        }

        /**
         * 
         * \brief   Play sound list in text, using a string
         *
         *    ¤sample Example_PlaySndEx_string¤
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.0.8¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \attention Usable only after the firmware version X.4.0.8. 
         * \attention **Be careful ! PlaySndEx(UInt32 nPortNumber, UInt32 nSize, Tone pTones, bool bAddToPlayedList = true) ; & PlaySndEx(UInt32 nPortNumber, string sTones, bool bAddToPlayedList = true); ARE NOT THE SAME !** One uses a pointer, and the other one a String as arguments.
         *
         * \param       nPortNumber         The port number of the 3dRudder [0,3]
         * \param [in]  sTones              The tones list in text.
         * \param       bAddToPlayedList    (Optional) Add to played List (true: default) or Replace the played list.
         *
         * \return  The possible error code returned by this method. ErrorCode can also be a success. \n \n \n <b> Possible error codes : </b>
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
         * \possibleret #Fail
         * \possibleret #Timeout
         *.
         */
        public static ErrorCode PlaySndEx(UInt32 nPortNumber, string sTones, bool bAddToPlayedList = true)
        {
            System.IntPtr pTones = Marshal.StringToHGlobalAnsi(sTones);
            ErrorCode error = Import.PlaySndEx2(nPortNumber, pTones, bAddToPlayedList);
            Marshal.FreeHGlobal(pTones);
            return error;
        }

        /**
         *
         * \brief   Gets user offset
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \param           nPortNumber The port number of the 3dRudder [0,3]
         * \param [in,out]  sAxis       If non-null, the axis.
         *
         * \return  The user offset.
         *          .
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
         * \possibleret #Fail
         * \possibleret #Timeout
         *
         */
        public static ErrorCode GetUserOffset(UInt32 nPortNumber, AxesValue sAxis)
        {
            GCHandle pAxis = GCHandle.Alloc(sAxis, GCHandleType.Pinned);
            ErrorCode error = Import.GetUserOffset(nPortNumber, pAxis.AddrOfPinnedObject());
            pAxis.Free();
            return error;
        }

        /*private struct AutoPinner : IDisposable
        {
            GCHandle _pinnedArray;
            bool m_bPinned;
            public AutoPinner(System.Object obj, GCHandleType nType)
            {
                m_bPinned = false;
                if (nType == GCHandleType.Pinned)
                    m_bPinned = true;
                _pinnedArray = GCHandle.Alloc(obj, nType);
            }
            public AutoPinner(System.Object obj)
            {
                m_bPinned = false;
                _pinnedArray = GCHandle.Alloc(obj);
            }
            public static implicit operator IntPtr(AutoPinner ap)
            {
                if (ap.m_bPinned)
                    return ap._pinnedArray.AddrOfPinnedObject();
                else
                    return GCHandle.ToIntPtr(ap._pinnedArray);

            }
            public void Dispose()
            {
                _pinnedArray.Free();
            }
        }*/

        /**
         *
         * \brief   Get the current value for each axis define by AxesValue. Axe's angle is given in degree.
         *
         * Values are valid only if the status is "InUse".
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \param           nPortNumber The port number of the 3dRudder [0,3]
         * \param           sAxesParam  The axes parameter.
         * \param [in,out]  sAxes       If non-null, the axes.
         *
         * \return  or the axes. If sAxesParams is null, GetAxes return directly the angle (Degree) of each axis. \n \n \n <b> Possible error codes : </b>
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
         * \possibleret #Fail
         * \possibleret #Timeout
         *
         */
        public static ErrorCode GetAxes(UInt32 nPortNumber, IAxesParam sAxesParam, AxesValue sAxes)
        {
            ErrorCode error;
            GCHandle pAxis = GCHandle.Alloc(sAxes, GCHandleType.Pinned);
            if (sAxesParam == null)
            {
                error = Import.GetAxes(nPortNumber, pAxis.AddrOfPinnedObject(), IntPtr.Zero, null);
                pAxis.Free();
                return error;
            }

            error = sAxesParam.UpdateParam(nPortNumber);
            //if (error != ErrorCode.Success) return error;

            InternalAxesParam iAxesParam = new InternalAxesParam();
            GCHandle handle1 = default(GCHandle);
            Curve sCurCurve1 = sAxesParam.GetCurve((Axes)0);
            if (sCurCurve1 == null)
                iAxesParam.m_bValidatedAxes1 = 0;
            else
            {
                iAxesParam.m_fDeadZone1 = sCurCurve1.DeadZone;
                iAxesParam.m_fxSat1 = sCurCurve1.XSat;
                iAxesParam.m_fExp1 = sCurCurve1.Exp;
                iAxesParam.m_bValidatedAxes1 = 1;
                handle1 = GCHandle.Alloc(sCurCurve1);
                iAxesParam.m_handleCurveObject1 = (ulong)GCHandle.ToIntPtr(handle1);
            }
            GCHandle handle2 = default(GCHandle);
            Curve sCurCurve2 = sAxesParam.GetCurve((Axes)1);
            if (sCurCurve2 == null)
                iAxesParam.m_bValidatedAxes2 = 0;
            else
            {
                iAxesParam.m_fDeadZone2 = sCurCurve2.DeadZone;
                iAxesParam.m_fxSat2 = sCurCurve2.XSat;
                iAxesParam.m_fExp2 = sCurCurve2.Exp;
                iAxesParam.m_bValidatedAxes2 = 1;
                handle2 = GCHandle.Alloc(sCurCurve2);
                iAxesParam.m_handleCurveObject2 = (ulong)GCHandle.ToIntPtr(handle2);
            }
            GCHandle handle3 = default(GCHandle);
            Curve sCurCurve3 = sAxesParam.GetCurve((Axes)2);
            if (sCurCurve3 == null)
                iAxesParam.m_bValidatedAxes3 = 0;
            else
            {
                iAxesParam.m_fDeadZone3 = sCurCurve3.DeadZone;
                iAxesParam.m_fxSat3 = sCurCurve3.XSat;
                iAxesParam.m_fExp3 = sCurCurve3.Exp;
                iAxesParam.m_bValidatedAxes3 = 1;
                handle3 = GCHandle.Alloc(sCurCurve3);
                iAxesParam.m_handleCurveObject3 = (ulong)GCHandle.ToIntPtr(handle3);
            }
            GCHandle handle4 = default(GCHandle);
            Curve sCurCurve4 = sAxesParam.GetCurve((Axes)3);
            if (sCurCurve4 == null)
                iAxesParam.m_bValidatedAxes4 = 0;
            else
            {
                iAxesParam.m_fDeadZone4 = sCurCurve4.DeadZone;
                iAxesParam.m_fxSat4 = sCurCurve4.XSat;
                iAxesParam.m_fExp4 = sCurCurve4.Exp;
                iAxesParam.m_bValidatedAxes4 = 1;
                handle4 = GCHandle.Alloc(sCurCurve4);
                iAxesParam.m_handleCurveObject4 = (ulong)GCHandle.ToIntPtr(handle4);
            }
                
            iAxesParam.m_fRoll2YawCompensation = sAxesParam.Roll2YawCompensation;
            iAxesParam.m_bNonSymmetrical = sAxesParam.NonSymmetrical == false ? (uint)0 : (uint)1;

            GCHandle pCurves = GCHandle.Alloc(iAxesParam, GCHandleType.Pinned);

            //CalcCurveValue_t calc = new CalcCurveValue_t(IAxesParam.CalcCurveValueCb);
            error = Import.GetAxes(nPortNumber, pAxis.AddrOfPinnedObject(), pCurves.AddrOfPinnedObject(), IAxesParam.CalcCurveValueCb);
            pAxis.Free();
            pCurves.Free();

            if (handle1.IsAllocated) handle1.Free();
            if (handle2.IsAllocated) handle2.Free();
            if (handle3.IsAllocated) handle3.Free();
            if (handle4.IsAllocated) handle4.Free();

            return error;
        }

        /**
         *
         * \brief   Gets the status.
         * This function read the current status of the 3dRudder connected to nPortNumber
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]
         *
         * \return  The status.
         *          .
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
         * \possibleret #Fail
         * \possibleret #Timeout
         *
         */
        public static Status GetStatus(UInt32 nPortNumber)
        {
            return Import.GetStatus(nPortNumber);
        }

        /**
         *
         * \brief   Gets a sensor, and the values of the force sensors. The value is in grams.
         *
         * This function reads the values of the 6 force sensors indexed by nIndex of the 3dRudder connected on nPortNumber.
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]
         * \param   nIndex      Zero-based index of the 3dRudder
         *
         * \return  The sensor. The unit of 16 bits returned value is given in grams. If 0 , it's possible an error, please check GetLastError for possible error.
         *          .
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
        public static ushort GetSensor(UInt32 nPortNumber, UInt32 nIndex)
        {
            return Import.GetSensor(nPortNumber, nIndex);
        }

        /**
         *
         * \brief   Gets device information of the 3dRudder connected to nPortNumber
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]
         *
         * \return  Null if it fails, else the device information.
         *          .
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
        public static DeviceInformation GetDeviceInformation(UInt32 nPortNumber)
        {            
            IntPtr pPhysicalMaxValue = Import.GetDeviceInformation(nPortNumber);
            if (pPhysicalMaxValue == IntPtr.Zero)              
                return null;

            DeviceInformation sRetPhysicalMaxValue = new DeviceInformation();
            GCHandle pRetPhysicalMaxValue = GCHandle.Alloc(sRetPhysicalMaxValue, GCHandleType.Pinned);
            Import.MemCpy(pRetPhysicalMaxValue.AddrOfPinnedObject(), pPhysicalMaxValue, Marshal.SizeOf(sRetPhysicalMaxValue));
            pRetPhysicalMaxValue.Free();
            return sRetPhysicalMaxValue;
        }

        /**
         *
         * \brief   do a freeze of the 3dRudder value
         * 
         * It may be useful to temporarily desactivate and reactivate the 3dRudder connected to nPortNumber without necessarily removing and replacing the feet. 
         *
         * This makes it possible, for example, to freeze the displacement in the phases when they are not required in the 3D Universe, without risk of drifting,            
         * and to avoid to freeze the user in his initial position, and thus relocate the device or move the legs to relax.
         * 
         * In Freeze mode, the values returned by the __3dRudder__ are identical to those returned when the device waits for the 2nd foot : the outputs are 0. 
         *
         * During an "unfreeze" : 
         *  - The device switches directly to the "InUse" mode, without going through the required immobility step required in standard mode. This makes it possible to freeze the *        displacements and to restore them without latency, for a more fluid operation. 
         *  - The user offsets are recalculated when unfreezing : thus, during the freeze, the user can change his rest position. As a summary, the freeze/unfreeze function allows *       you to reposition yourself without creating unintentional movements in the game. bEnable must be set to 'False' to unfreeze, and to 'True' to freeze.
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nPortNumber The port number of the 3dRudder [0,3]        
         * \param   bEnable     True to enable, false to disable.
         *
         * \return  The possible error code returned by this method. ErrorCode can also be a success.
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
         * \possibleret #Fail
         * \possibleret #Timeout
         *.
         */
        public static ErrorCode SetFreeze(UInt32 nPortNumber, bool bEnable)
        {
            return Import.SetFreeze(nPortNumber, bEnable);            
        }

        /**
        *
        * \brief    return the frozen status of the device. 
        *
        * Check if the 3dRudder had been frozen by the comment Sdk3dRudder.SetFreeze 
        *
        *    ¤compatible_plateforme Win, PS4¤
        *    ¤compatible_firm From x.4.1.0¤
        *    ¤compatible_sdk From 1.91¤     
        * \param    nPortNumber The port number of the 3dRudder [0,3]
        *
        * \return   frozen status
        *
        */
        public static bool IsFrozen(UInt32 nPortNumber)
        {
            return Import.IsFrozen(nPortNumber);
        }

        /**
         *
         * \brief   Gets the last error
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \return  The last error. The possible error code returned by this method. ErrorCode can also be a success. \n \n \n <b> Possible error codes : </b>
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
         * \possibleret #Fail
         * \possibleret #Timeout
         *      .
         */
        public static ErrorCode GetLastError()
        {
            return Import.GetLastSdkError();
        }

        /** 
         *
         * \brief Translates the code to human readable value.
         *
         *  If the error code isn't defined, GetErrorText return "Other : Default".
         *
         * | Code | Value ||
         * | --- | --- ||
         * | ErrorCode.Success | Success ||
         * | ErrorCode.NotSupported | Not Supported ||
         * | ErrorCode.Other| Other ||
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \param   nError  The error code to be translated
         *
         *
         * \return A char* which describe the code. \n \n \n <b> Possible error codes : </b>
         *
         * \possibleret #Success
         * \possibleret #NotConnected
         * \possibleret #Fail
         * \possibleret #IncorrectCommand
         * \possibleret #DeviceNotSupported
         * \possibleret #NotSupported
         * \possibleret #Timeout
         * \possibleret #FirmwareNeedToBeUpdated
         * \possibleret #NotInitialized
         * \possibleret #NotReady
         * \possibleret #Other
         *          .
         */
        public static string GetErrorText(ErrorCode nError)
        {
            return Marshal.PtrToStringAnsi(Import.GetErrorText(nError)); 
        }

        /**
         *
         * \brief   The SDK manages events.
         *
         * Set Event must be called before Init. To use it, you should add delegate to EventRudder. 
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \attention It's important to use it before the Init() function
         *
         * \param   OnConnect  If non-null, the event for connect.
         * \param   OnDisconnect  If non-null, the event for disconnect.
         * \param   OnEndSound  If non-null, the event for end sound.
         *                          .
         */
        public static void SetEvent(CallbackTypeConnect OnConnect, CallbackTypeConnect OnDisconnect, CallbackTypeConnect OnEndSound)
        {
            Import.Set3dREvent(OnConnect,OnDisconnect,OnEndSound);
        }

        /**
         *
         * \brief   The SDK get events message.
         *
         * GetEventMessage must be called each frame. 
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         *  \return A EventData
         *                          .
         */
        public static EventData GetEventMessage()
        {
            IntPtr eventMsg = Import.GetEventMessage();
            EventData eventData = null;
            if (eventMsg != IntPtr.Zero)
            {
                eventData = new EventData();
                GCHandle pRetPhysicalMaxValue = GCHandle.Alloc(eventData, GCHandleType.Pinned);
                Import.MemCpy(pRetPhysicalMaxValue.AddrOfPinnedObject(), eventMsg, Marshal.SizeOf(eventData));
                pRetPhysicalMaxValue.Free();
            }
            return eventData;
        }

        /**
         *
         * \brief   Calculates the curve value. Used when you're using your own response curve for each axes of the 3dRudder.
         *
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \param   fDeadZone   The dead zone.
         * \param   fxSat       The effects sat.
         * \param   fExp        The exponent.
         * \param   fValue      The raw value.
         *
         * \return  The calculated curve value.
         *          .
         */
        public static float CalcCurveValue(float fDeadZone, float fxSat,  float fExp, float fValue)
        {
            return Import.CalcCurveValue(fDeadZone, fxSat, fExp, fValue);
        }

        /**
         *
         * \brief   Calculates the curve value for a normalized value with the non symmetrical pitch actived.
         *
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \param   nPortNumber     The port number of the 3dRudder [0,3]
         * \param   fNormalizedV    The normalized value.
         * \param   sCurve          The curve.         
         *
         * \return  The calculated curve value.
         *          .
         */
        public static float CalcNonSymmetricalPitch(UInt32 nPortNumber, float fNormalizedV, Curve sCurve)
        {
            GCHandle pCurve = GCHandle.Alloc(sCurve, GCHandleType.Pinned);
            float result = Import.CalcNonSymmetricalPitch(nPortNumber, fNormalizedV, pCurve.AddrOfPinnedObject());
            pCurve.Free();
            return result;
        }

        /**
         *
         * \brief   Stop all Threads of the SDK and will free the sdk from memory.
         *
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \return  The possible error code returned by this method. ErrorCode can also be a success. \n \n \n <b> Possible error codes : </b>
         *
         * \possibleret #Success
         * \possibleret #NotInitialized
         * \possibleret #NotReady
         *          .
         */
        public static ErrorCode Stop()
        {
            ErrorCode stop = Import.StopSDK();
            Debug.LogFormat("[3dRudder] SDK Stop: {0}[{1}]", stop, GetErrorText(stop));
            return stop;
        }

        /**
         *
         * \brief   Return the axes value on Unity vector3 format (X = Left/Right, Y = Up/Down, Z = Forward/Backward)
         *
         *
         * ¤compatible_plateforme Win, PS4¤
         * ¤compatible_firm From x.4.x.x¤
         * ¤compatible_sdk From 2.00¤
         *
         * \param   sAxis     The axes value         
         *
         * \return  The axes value on Unity vector3 format.
         *          .
         */
        public static Vector3 GetAxes3D(AxesValue sAxis)
        {            
            return new Vector3(sAxis.Get(Axes.LeftRight), sAxis.Get(Axes.UpDown), sAxis.Get(Axes.ForwardBackward));
        }
#endregion
        
#region DLL
        private class Import
        {
            [DllImport("i3DR")]
            public static extern UInt32 Init();

            [DllImport("i3DR")]
            public static extern ushort GetSDKVersion();

            [DllImport("i3DR")]
            public static extern UInt32 GetNumberOfConnectedDevice();

            [DllImport("i3DR")]
            public static extern bool IsDeviceConnected(UInt32 nPortNumber);

            [DllImport("i3DR")]
            public static extern ushort GetFWVersion(UInt32 nPortNumber);

            [DllImport("i3DR")]
            public static extern ErrorCode HideSystemDevice(UInt32 nPortNumber, bool bHide);
            [DllImport("i3DR")]
            public static extern bool IsSystemDeviceHidden(UInt32 nPortNumber);

            [DllImport("i3DR")]
            public static extern ErrorCode PlaySnd(UInt32 nPortNumber, ushort nFrequency, ushort nDuration);

            [DllImport("i3DR")]
            public static extern ErrorCode PlaySndEx1(UInt32 nPortNumber, UInt32 nSize, IntPtr pTones, bool bAddToPlayedList);

            [DllImport("i3DR")]
            public static extern ErrorCode PlaySndEx2(UInt32 nPortNumber, IntPtr sTones, bool bAddToPlayedList);

            [DllImport("i3DR")]
            public static extern ErrorCode GetUserOffset(UInt32 nPortNumber, IntPtr pAxisValue);

            [DllImport("i3DR")]
            public static extern ErrorCode GetAxes(UInt32 nPortNumber, IntPtr pAxesParam, IntPtr pAxisValue, CalcCurveValue_t pCalcCurve);

            [DllImport("i3DR")]
            public static extern Status GetStatus(UInt32 nPortNumber);

            [DllImport("i3DR")]
            public static extern ushort GetSensor(UInt32 nPortNumber, UInt32 nIndex);

            [DllImport("i3DR")]
            public static extern IntPtr GetDeviceInformation(UInt32 nPortNumber);

            [DllImport("i3DR")]
            public static extern ErrorCode SetFreeze(UInt32 nPortNumber, bool bEnable);

            [DllImport("i3DR")]
            public static extern bool IsFrozen(UInt32 nPortNumber);

            [DllImport("i3DR")]
            public static extern IntPtr GetErrorText(ErrorCode nError);

            [DllImport("i3DR")]
            public static extern ErrorCode GetLastSdkError();

            [DllImport("i3DR")]
            public static extern void Set3dREvent(CallbackTypeConnect OnConnect, CallbackTypeConnect OnDisconnect, CallbackTypeConnect OnEndSound);

            [DllImport("i3DR")]
            public static extern void InitQueueEvent();

            [DllImport("i3DR")]
            public static extern IntPtr GetEventMessage();

            [DllImport("i3DR")]
            public static extern float CalcCurveValue(float fDeadZone, float fxSat, float fExp, float fValue);

            [DllImport("i3DR")]
            public static extern float CalcNonSymmetricalPitch(UInt32 nPortNumber, float fNormalizedV, IntPtr pCurve);

            [DllImport("i3DR")]
            public static extern void MemCpy(IntPtr pDest, IntPtr pSrc, int nSize);

            [DllImport("i3DR")]
            public static extern ErrorCode StopSDK();

            [DllImport("i3DR")]
            public static extern void EndSDK();
        }
#endregion
    }

    /**
     * \class   EventRudder
     *
     * \brief   An event. You have to AddEvent before initializing the SDK.
     */
    public class EventRudder
    {

        /**
         * static delegate Action<uint> EventRudder.OnConnect(uint nPortNumber)
         * Event Sent when a 3dRudder is connected. The 3dRudder is identified by the nPortNumber.
         */
        public static Action<uint> OnConnectEvent = null;
        /**
         * static delegate Action<uint> EventRudder.OnDisconnect(uint nPortNumber)
         * Event Sent when a 3dRudder is disconnected. The 3dRudder is identified by the nPortNumber.
         */
        public static Action<uint> OnDisconnectEvent = null;
        /**
         * static delegate Action<uint> EventRudder.OnEndSound(uint nPortNumber)
         * Receives an event at the end of the sound. The 3dRudder is identified by the nPortNumber.
         */
        public static Action<uint> OnEndSoundEvent = null;

        [AOT.MonoPInvokeCallback(typeof(CallbackTypeConnect))]
        public static void OnConnect(uint nPortNumber)
        {
            if (OnConnectEvent != null)
                OnConnectEvent(nPortNumber);
        }

        [AOT.MonoPInvokeCallback(typeof(CallbackTypeConnect))]
        public static void OnDisconnect(uint nPortNumber)
        {
            if (OnDisconnectEvent != null)
                OnDisconnectEvent(nPortNumber);
        }
        [AOT.MonoPInvokeCallback(typeof(CallbackTypeConnect))]
        public static void OnEndSound(uint nPortNumber)
        {
            if (OnEndSoundEvent != null)
                OnEndSoundEvent(nPortNumber);
        }

        public void Dispose()
        {
            OnConnectEvent = null;
            OnDisconnectEvent = null;
            OnEndSoundEvent = null;
        }
    }

    /**
    * \class   EventData
    *
    * \brief   Information about the device.
    *
    */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class EventData
    {
        public enum TypeEVent
        {
            OnConnect,
            OnDisconnect,
            OnEndSound
        };
        /// TypeEvent
        public TypeEVent typeEvent;
        /// PortNumber
        public UInt32 portNumber;
    }

    /**
     * \class   DeviceInformation
     *
     * \brief   Information about the device.
     *
     */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class DeviceInformation
    {
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName0;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName1;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName2;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName3;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName4;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName5;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName6;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName7;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName8;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName9;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName10;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName11;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName12;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName13;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName14;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName15;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName16;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName17;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName18;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName19;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName20;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName21;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName22;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName23;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName24;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName25;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName26;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName27;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName28;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName29;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName30;
        [MarshalAs(UnmanagedType.U1)]
        byte m_sDeviceName31;

        [MarshalAs(UnmanagedType.U4)]
        uint m_nSerialNumber;

        [MarshalAs(UnmanagedType.R4)]
        float m_fMaxRoll;
        [MarshalAs(UnmanagedType.R4)]
        float m_fMaxPitch;
        [MarshalAs(UnmanagedType.R4)]
        float m_fMaxYaw;

        [MarshalAs(UnmanagedType.R4)]
        float m_fUserRoll;
        [MarshalAs(UnmanagedType.R4)]
        float m_fUserPitch;
        [MarshalAs(UnmanagedType.R4)]
        float m_fUserYaw;

        [MarshalAs(UnmanagedType.R4)]
        float m_DefaultRoll2YawCompensation;

        /**
         *
         * \brief   Default constructor
         */
        public DeviceInformation()
        {            
        }

        /**
         *
         * \brief   Get model's name of the 3dRudder
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \return  Null if it fails, else the name. The default name is “3dRudder 1”
         *          .
         */
        public string GetName()
        {
            byte[] sDeviceName = new byte[32];
            sDeviceName[0] = m_sDeviceName0;
            sDeviceName[1] = m_sDeviceName1;
            sDeviceName[2] = m_sDeviceName2;
            sDeviceName[3] = m_sDeviceName3;
            sDeviceName[4] = m_sDeviceName4;
            sDeviceName[5] = m_sDeviceName5;
            sDeviceName[6] = m_sDeviceName6;
            sDeviceName[7] = m_sDeviceName7;
            sDeviceName[8] = m_sDeviceName8;
            sDeviceName[9] = m_sDeviceName9;
            sDeviceName[10] = m_sDeviceName10;
            sDeviceName[11] = m_sDeviceName11;
            sDeviceName[12] = m_sDeviceName12;
            sDeviceName[13] = m_sDeviceName13;
            sDeviceName[14] = m_sDeviceName14;
            sDeviceName[15] = m_sDeviceName15;
            sDeviceName[16] = m_sDeviceName16;
            sDeviceName[17] = m_sDeviceName17;
            sDeviceName[18] = m_sDeviceName18;
            sDeviceName[19] = m_sDeviceName19;
            sDeviceName[20] = m_sDeviceName20;
            sDeviceName[21] = m_sDeviceName21;
            sDeviceName[22] = m_sDeviceName22;
            sDeviceName[23] = m_sDeviceName23;
            sDeviceName[24] = m_sDeviceName24;
            sDeviceName[25] = m_sDeviceName25;
            sDeviceName[26] = m_sDeviceName26;
            sDeviceName[27] = m_sDeviceName27;
            sDeviceName[28] = m_sDeviceName28;
            sDeviceName[29] = m_sDeviceName29;
            sDeviceName[30] = m_sDeviceName30;
            sDeviceName[31] = m_sDeviceName31;

            return System.Text.Encoding.Default.GetString(sDeviceName);
        }

        /**
         *
         * \brief   Gets serial number
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         *
         * \return  The serial number. By default the number is 0x00010000
         *          .
         */
        public UInt32 GetSerialNumber()  {	return m_nSerialNumber; 	}

        /**
         *
         * \brief   Sets user roll
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nVal    The value.
         *                  .
         */
        public void SetUserRoll(float nVal) { m_fUserRoll = nVal; }
        /**
         *
         * \brief   Sets user pitch
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nVal    The value.
         *                  .
         */
        public void SetUserPitch(float nVal) { m_fUserPitch = nVal; }
        /**
         *
         * \brief   Sets user yaw
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nVal    The value.
         *
         */
        public void SetUserYaw(float nVal) { m_fUserYaw = nVal; }
        /**
         *
         * \brief   Gets maximum roll. 
         * \brief   Get physical specification of the 3dRudder (angle °)
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \return  The maximum roll. 3dRudder R5 Edition | 3dRudder for PS4™ : 18°
         */
        public float GetMaxRoll()  { return m_fMaxRoll; }
        /**
         *
         * \brief   Gets maximum pitch.
         *  Get physical specification of the 3dRudder (angle °)
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \return  The maximum pitch. 3dRudder R5 Edition | 3dRudder for PS4™ : 18°
         *          .
         */
        public float GetMaxPitch()  { return m_fMaxPitch; }
        /**
         *
         * \brief   Gets maximum yaw.
         *      Get physical specification of the 3dRudder (angle °)
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \return  The maximum yaw. 3dRudder R5 Edition | 3dRudder for PS4™ : 25°
         *          .
         */
        public float GetMaxYaw()  { return m_fMaxYaw; }
        /**
         *
         * \brief   Gets user roll
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \return  The user roll.
         *          .
         */
		public float GetUserRoll()  { return m_fUserRoll; }
        /**
         *
         * \brief   Gets user pitch
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \return  The user pitch.
         *          .
         */
		public float GetUserPitch()  { return m_fUserPitch; }
        /**
         *
         * \brief   Gets user yaw
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \return  The user yaw.
         *          .
         */
		public float GetUserYaw()  { return m_fUserYaw; }
        /**
         *
         * \brief   Gets default roll 2 yaw compensation
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \return  The default roll 2 yaw compensation. By default the value is 0.15
         *          .
         */
        public float GetDefaultRoll2YawCompensation() { return m_DefaultRoll2YawCompensation; }
    }

    /**
     * \class   AxesValue
     *
     * \brief   The axes value.
     */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class AxesValue : IDisposable
    {
        [MarshalAs(UnmanagedType.R4)]
        float m_Axes1;
        [MarshalAs(UnmanagedType.R4)]
        float m_Axes2;
        [MarshalAs(UnmanagedType.R4)]
        float m_Axes3;
        [MarshalAs(UnmanagedType.R4)]
        float m_Axes4;


        ~AxesValue()
        {
            this.Dispose();
        }

        /**
         *
         * \brief   Default constructor
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         */
        public AxesValue()
        {
            m_Axes1 = 0.0f;
            m_Axes2 = 0.0f;
            m_Axes3 = 0.0f;
            m_Axes4 = 0.0f;

        }

        /**
         *
         * \brief   Gets a float using the given n axis
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nAxis   The axis to get.
         *
         * \return  The value of the specific axe
         *          .
         */
        public float Get(Axes nAxis)
        {
            switch(nAxis)
            {
                case Axes.LeftRight:
                    return m_Axes1;
                case Axes.ForwardBackward:
                    return m_Axes2;
                case Axes.UpDown:
                    return m_Axes3;
                case Axes.Rotation:
                    return m_Axes4;
            }
            return 0;
        }
               
        public virtual void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    /**
     * \class   Curve
     *
     * \brief   A curve object.
     */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class Curve : IDisposable
    {
        /// DeadZone
        public float DeadZone;
        /// Effect Saturation
        public float XSat;
        /// Exponent
        public float Exp;

        /**
         *
         * \brief   Default constructor
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         */
        public Curve()
        {
            DeadZone = 0.0f;
            XSat = 1.0f;
            Exp = 1.0f;
        }

        /**
         *
         * \brief   Constructor
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   fxSat       The effects sat.
         * \param   fDeadZone   The dead zone.
         * \param   fExp        The exponent.
         *                      .
         * \return  Void
         */
        public Curve(float fxSat, float fDeadZone,  float fExp)
        {
            DeadZone = fDeadZone;
            XSat = fxSat;
            Exp = fExp;
        }

        ~Curve()
        {
            this.Dispose();
        }

        /**
         *
         * \brief   Calculates the curve value.
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   fValue  The value.
         *
         * \return  The calculated curve value.
         *          .
         */
        public virtual float CalcCurveValue(float fValue)
        {            
            return Sdk3dRudder.CalcCurveValue(DeadZone, XSat, Exp, fValue);
        }

        public virtual void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    /**
     * \class   IAxesParam
     *
     * \brief   The axes parameter. This allow you to define the settings of each axis of the 3dRudder.
     */
    [System.Serializable]
    public abstract class IAxesParam : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private Curve[] m_Curve;
        [HideInInspector]
        /// processing roll 2 yaw compensation
        public float Roll2YawCompensation;
        [HideInInspector]
        /// if this object is non Symmetrical
        public bool NonSymmetrical;

        /**
         *
         * \brief   Constructor. 
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         */
        public IAxesParam()
        {
            m_Curve = new Curve[(int)Axes.MaxAxes];
            for (int i = 0; i < (int)Axes.MaxAxes; i++)
                m_Curve[i] = null;

            Roll2YawCompensation = 0.15f;
            NonSymmetrical = false;
        }



#if UNITY_EDITOR        
        public virtual void DrawParam()
        {

        }

        protected float MapRangeValue(float value, float aMin, float aMax, float bMin, float bMax)
        {
            float normal = Mathf.InverseLerp(aMin, aMax, value);
            return Mathf.Lerp(bMin, bMax, normal);
        }

        public virtual void DrawCurve(IAxesParam axesParamNormalized, Axes axe, uint portNumber, float maxRange = 1.0f)
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            Material mat = new Material(shader);
            int curveSampling = 300;
            
            // curves representations
            Rect rect = GUILayoutUtility.GetRect(0, 0, 100, 10);
            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(rect);
                GL.PushMatrix();

                GL.Clear(true, false, Color.black);
                mat.SetPass(0);

                // background
                GL.Begin(GL.QUADS);
                    GL.Color(new Color(0.3f, 0.3f, 0.3f, 1));
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(rect.width, 0, 0);
                    GL.Vertex3(rect.width, rect.height, 0);
                    GL.Vertex3(0, rect.height, 0);
                GL.End();

                // grid
                GL.Begin(GL.LINES);
                    GL.Color(new Color(0.1f, 0.1f, 0.1f, 1));
                    float middle = (rect.width / 2);
                    GL.Vertex3(middle, 0, 0);
                    GL.Vertex3(middle, rect.height, 0);
                    GL.Vertex3(0, rect.height / 2, 0);
                    GL.Vertex3(rect.width, rect.height / 2, 0);
                GL.End();

                // draw deadzone quad
                GL.Begin(GL.QUADS);
                    GL.Color(new Color(0.0f, 0.0f, 0.6f, 0.25f));
                    this.UpdateParam(portNumber);
                    ns3dRudder.Curve curve = this.GetCurve(axe);
                    float DeadZone = MapRangeValue(curve.DeadZone, 0.0f, 1.0f, 0.0f, rect.width);
                    GL.Vertex3(middle - (DeadZone / 2), 0, 0);
                    GL.Vertex3(middle + (DeadZone / 2), 0, 0);
                    GL.Vertex3(middle + (DeadZone / 2), rect.height, 0);
                    GL.Vertex3(middle - (DeadZone / 2), rect.height, 0);
                GL.End();

                //draw rudder info line
                AxesValue axes = new AxesValue();
                ErrorCode error = Sdk3dRudder.GetAxes(portNumber, axesParamNormalized, axes);
                if (error != ErrorCode.Success)
                    Debug.LogFormat("[3dRudder] Error GetAxes: {0}", Sdk3dRudder.GetErrorText(error));
                
                float valueNormalized = axes.Get(axe);
                // tricks for non symmetrical pitch
                if (NonSymmetrical && axe == Axes.ForwardBackward)
                {
                    valueNormalized = Sdk3dRudder.CalcNonSymmetricalPitch(portNumber, valueNormalized, curve);
                    //Debug.Log(valueNormalized);
                }
                GL.Begin(GL.LINES);
                    GL.Color(new Color(0.8f, 0.0f, 0.0f));
                    float xRudder = MapRangeValue(valueNormalized, -1.0f, 1.0f, 0.0f, rect.width);
                    GL.Vertex3(xRudder, 0, 0);
                    GL.Vertex3(xRudder, rect.height, 0);
                GL.End();

                // draw curve
#if UNITY_5_6_OR_NEWER
                GL.Begin(GL.LINE_STRIP);
#else
                GL.Begin(GL.LINES);
#endif
                for (float i = -maxRange; i < maxRange; i += ((2.0f * maxRange) / curveSampling))
                {                   
                    float val = Sdk3dRudder.CalcCurveValue(curve.DeadZone, curve.XSat, curve.Exp, i);
                    float xValue = 0.0f;
                    float yValue = 0.0f;

                    xValue = MapRangeValue(i, -1.0f, 1.0f, 0.0f, rect.width);               
                    // quick fix for sign error here !
                    yValue = MapRangeValue(val * -1, -1.0f, 1.0f, 0.0f, rect.height);

                    GL.Color(new Color(0.0f, 0.8f, 0.0f));
                    GL.Vertex3(xValue, yValue, 0.0f);
                }
                GL.End();

                // draw satured left zone
                float x = MapRangeValue(curve.XSat, -1.0f, 1.0f, 0.0f, rect.width);                
                GL.Begin(GL.QUADS);
                    GL.Color(new Color(1.0f, 0.2f, 0.1f, 0.25f));
                    GL.Vertex3(0, 0, 0);
                    GL.Vertex3(rect.width - x, 0, 0);
                    GL.Vertex3(rect.width - x, rect.height, 0);
                    GL.Vertex3(0, rect.height, 0);
                GL.End();
                // draw satured right zone
                GL.Begin(GL.QUADS);
                    GL.Color(new Color(1.0f, 0.2f, 0.1f, 0.25f));
                    GL.Vertex3(x, 0, 0);
                    GL.Vertex3(rect.width, 0, 0);
                    GL.Vertex3(rect.width, rect.height, 0);
                    GL.Vertex3(x, rect.height, 0);
                GL.End();

                GL.PopMatrix();
                
                GUI.Label(new Rect(0, 0, 100, 50), string.Format("x:{0:0.00}/y:{1:0.00}", Mathf.Clamp(valueNormalized, -1.0f, 1.0f), Sdk3dRudder.CalcCurveValue(curve.DeadZone, curve.XSat, curve.Exp, valueNormalized)));
                GUI.EndClip();
            }            
        }
#endif
        /**
         *
         * \brief   virtual pure UpdateParam
         *
         * give the parameteres used to calculated the axes.
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nPortNumber The device number.
         *
         * \return  the error code depending of the usage.
         *          .
         */
        public virtual ErrorCode UpdateParam(uint nPortNumber) { return ErrorCode.Success; }

        /**
         *
         * \brief   Gets a curve
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nCurve  The curve.
         *
         * \return  Null if it fails, else the curve.
         *          .
         */
        public Curve GetCurve(Axes nCurve)
		{
			if (nCurve >= Axes.LeftRight && nCurve<Axes.MaxAxes)
				return m_Curve[(int)nCurve];
			else
				return null;
		}

        /**
         *
         * \brief   Sets a curve.
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param           nCurve  The curve.
         * \param [in,out]  pCurve  If non-null, the curve.
         *                          .
         */
        public void SetCurve(Axes nCurve, Curve pCurve)
        {
            if (nCurve >= Axes.LeftRight && nCurve < Axes.MaxAxes)
                m_Curve[(int)nCurve] = pCurve;
        }

        /**
         *
         * \brief   Sets all curves.
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param           curves  The curve array.
         *                          .
         */
        public void SetCurves(Curve[] curves)
        {
            if (curves.Length == (int)Axes.MaxAxes)
                m_Curve = curves;
        }

        /**
         *
         * \brief   Disable the upDown of the 3dRudder.
         *
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \attention  When you call NoUpdown, the value of upDown stay at 0.0
         *
         */
        public void NoUpDown()
        {
            SetCurve(Axes.UpDown, null);
        }

        [AOT.MonoPInvokeCallback(typeof(CalcCurveValue_t))]
        public static float CalcCurveValueCb(IntPtr pCurve, float fVal)
        {
            var sCurve = (Curve)GCHandle.FromIntPtr(pCurve).Target;            
            return sCurve.CalcCurveValue(fVal);
        }
    } 
 
    [StructLayout(LayoutKind.Sequential/*, Pack = 4*/)]
    public struct InternalAxesParam
    {        
        [MarshalAs(UnmanagedType.R4)]
        public float m_fDeadZone1;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fDeadZone2;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fDeadZone3;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fDeadZone4;

        [MarshalAs(UnmanagedType.R4)]
        public float m_fxSat1;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fxSat2;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fxSat3;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fxSat4;

        [MarshalAs(UnmanagedType.R4)]
        public float m_fExp1;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fExp2;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fExp3;
        [MarshalAs(UnmanagedType.R4)]
        public float m_fExp4;

        [MarshalAs(UnmanagedType.U8)]
        public ulong m_handleCurveObject1;
        [MarshalAs(UnmanagedType.U8)]
        public ulong m_handleCurveObject2;
        [MarshalAs(UnmanagedType.U8)]
        public ulong m_handleCurveObject3;
        [MarshalAs(UnmanagedType.U8)]
        public ulong m_handleCurveObject4;        

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 m_bValidatedAxes1;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 m_bValidatedAxes2;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 m_bValidatedAxes3;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 m_bValidatedAxes4;

        [MarshalAs(UnmanagedType.R4)]
        public float m_fRoll2YawCompensation;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 m_bNonSymmetrical;

    }

    /**
     * \class   Tone
     *
     * \brief   __Containe one tone__
     *          
     *          Usable in Sdk3dRudder.PlaySndEx
     */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Tone : IDisposable
    {
        /// Gets the frequency
        public ushort Frequency
        {
            get; private set;
        }
        /// Gets duration of tone
        public ushort DurationOfTone
        {
            get; private set;
        }
        /// Gets pause after tone
        public ushort PauseAfterTone
        {
            get; private set;
        }

        ~Tone()
        {
            this.Dispose();
        }

        /**
         *
         * \brief   Default constructor
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         */
        public Tone() 
        {
            Frequency = 440;
            DurationOfTone = 500 / 10;
            PauseAfterTone = 500 / 10;
        }

        /**
         *
         * \brief   Constructor
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   nFrequency      The frequency of the sound to be played
         * \param   nDurationOfTone The duration of the played tone
         * \param   nPauseAfterTone The pause before playing the next tones
         *
         * \return  Void
         */
        public Tone(ushort nFrequency, ushort nDurationOfTone, ushort nPauseAfterTone)
        {
            Frequency = nFrequency;
            DurationOfTone = nDurationOfTone;
            PauseAfterTone = nPauseAfterTone;
        }

        public virtual void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    /**
    * \class   SmoothMovement
    *
    * \brief   __Smooth the movement__
    *          
    *          
    */    
    [Serializable]
    public class SmoothMovement
    {
        public bool Enable;
        [Range(0.1f, 1)]
        public float Smoothness;
        float CurrentSpeed;

        public SmoothMovement()
        {
            Enable = false;
            Smoothness = 0.15f;
            CurrentSpeed = 0.0f;
        }
        /**
         *
         * \brief   ComputeSpeed
         *
         *    ¤compatible_plateforme Win, PS4¤
         *    ¤compatible_firm From x.4.x.x¤
         *    ¤compatible_sdk From 2.00¤
         *
         * \param   input       The input value
         * \param   smoothness  The smoothness factor
         * \param   deltatime   The deltatime
         *
         * \return  Void
         */
        public float ComputeSpeed(float input, float deltatime)
        {
            float speedTarget = input; // m/s            
            float acceleration = (speedTarget - CurrentSpeed) / Smoothness; // m/s²            
            CurrentSpeed = CurrentSpeed + (acceleration * deltatime); // m/s            
            return CurrentSpeed;
        }
    };
}