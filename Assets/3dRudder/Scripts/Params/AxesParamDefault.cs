using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \namespace    ns3dRudder
*
*/
namespace ns3dRudder
{
    /**
    * \class    AxesParamDefault
    *
    * \brief    The axes parameter default.
    */
    //[CreateAssetMenu(fileName = "Default", menuName = "3dRudder/AxesParamDefault", order = 1)]
    public class AxesParamDefault : IAxesParam
    {
        /// Default value for deadzone
        public static float DEFAULT_DEADZONE = 0.25f;
        public static float DEFAULT_XSAT = 0.7f;
        public static float DEFAULT_DEADZONE_ROTATION = 0.15f;

        /**
        *
        * \brief    Constructor
        *
        *
        * ¤compatible_plateforme Win, PS4¤
        * ¤compatible_firm From x.4.x.x¤
        * ¤compatible_sdk From 2.00¤
        *
        *                       .
        */
        public AxesParamDefault() : base()
        {
            SetCurves(GetDefaultNonNormalizedCurves());
            NonSymmetrical = true;
        }

        /**
        *
        * \brief    Update curves data
        *
        *    ¤compatible_plateforme Win, PS4¤
        *    ¤compatible_firm From x.4.x.x¤
        *    ¤compatible_sdk From 2.00¤
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
        */
        public override ErrorCode UpdateParam(uint nPortNumber)
        {
            ErrorCode nError = ErrorCode.Success;
            DeviceInformation Info = Sdk3dRudder.GetDeviceInformation(nPortNumber);
            if (Info != null)
            {
                float XSatLR = Info.GetUserRoll() / Info.GetMaxRoll();
                float XSatFB = Info.GetUserPitch() / Info.GetMaxPitch();
                float XSatR = Info.GetUserYaw() / Info.GetMaxYaw();

                Curve curve = GetCurve(Axes.LeftRight);
                curve.XSat = DEFAULT_XSAT * XSatLR;
                curve.DeadZone = DEFAULT_DEADZONE * curve.XSat;
                
                curve = GetCurve(Axes.ForwardBackward);
                curve.XSat = DEFAULT_XSAT * XSatFB;
                curve.DeadZone = DEFAULT_DEADZONE * curve.XSat;

                curve = GetCurve(Axes.Rotation);
                curve.XSat = XSatR;
                curve.DeadZone = DEFAULT_DEADZONE_ROTATION * curve.XSat; ;

                Roll2YawCompensation = Info.GetDefaultRoll2YawCompensation();
            }
            else
                nError = Sdk3dRudder.GetLastError();
            return nError;
        }

        /**
        *
        * \brief    Update curves data
        *
        *    ¤compatible_plateforme Win, PS4¤
        *    ¤compatible_firm From x.4.x.x¤
        *    ¤compatible_sdk From 2.00¤
        *                       .
        * \return   The default curves non normalized. 
        *
        */
        public static Curve[] GetDefaultNonNormalizedCurves()
        {
            Curve[] result = new Curve[4];

            result[(int)Axes.ForwardBackward] = new Curve(DEFAULT_XSAT, DEFAULT_DEADZONE, 1.0f);
            result[(int)Axes.LeftRight] = new Curve(DEFAULT_XSAT, DEFAULT_DEADZONE, 1.0f);
            result[(int)Axes.UpDown] = new Curve(0.6f, 0.1f, 2.0f);
            result[(int)Axes.Rotation] = new Curve(1.0f, DEFAULT_DEADZONE_ROTATION, 1.0f);

            return result;
        }
    }
}