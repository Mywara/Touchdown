using System.Collections;
using UnityEngine;
using ns3dRudder;

/**
* \class    Controller3dRudder
*
* \brief    The MonoBehaviour for the 3dRudder controller
*/
[HelpURL("https://3drudder-dev.com/docs/3drudder-documentations/3drudder-sdk-unity/")]
public class Controller3dRudder : MonoBehaviour
{
    // Port number of 3dRudder
    public uint PortNumber = 0;
    
    /// Enable/Disable forward backward axis
    public bool CanForward = true;
    /// Speed factor for forward backward axis
    public float SpeedForwardBackward = 1f;
    /// Enable/Disable left right axis
    public bool CanStrafe = true;
    /// Speed factor for left right axis
    public float SpeedLeftRight = 1f;
    /// Enable/Disable up down axis
    public bool CanUpDown = false;
    /// Speed factor for up down axis
    public float SpeedUpDown = 1f;
    /// Enable/Disable rotation axis
    public bool CanRotate = true;
    /// Speed factor for rotation axis
    public float SpeedRotation = 1f;
    /// Axes Param
    public ns3dRudder.IAxesParam axesParam;

    /// Smooth Movement
    public SmoothMovement SmoothForwardBackward;
    public SmoothMovement SmoothLeftRight;
    public SmoothMovement SmoothUpDown;
    public SmoothMovement SmoothRotation;

    // Mode
    protected ILocomotion locomotion;
    protected AxesValue axes;
    protected Vector4 axesWithSpeed = Vector4.zero;

    public AxesValue GetAxesValue() { return axes; }
    public Vector4 GetAxesWithSpeed() { return axesWithSpeed; }


    private void Reset()
    {
        axesParam = ScriptableObject.CreateInstance<AxesParamDefault>();
    }

    // Use this for initialization
    IEnumerator Start()
    {
        while (Manager3dRudder.Instance.IsInitialized == false)
        {
            yield return null;
        }
        axes = new AxesValue();
        locomotion = gameObject.GetComponent<ILocomotion>();
        if (locomotion == null)
        {            
            locomotion = gameObject.AddComponent<FPSLocomotion>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get info
        GetAxes();
    }

    void GetAxes()
    {      
        // Get state of Controller with port number : portNumber            
        ns3dRudder.ErrorCode error = Sdk3dRudder.GetAxes(PortNumber, axesParam, axes);
        if (error == ns3dRudder.ErrorCode.Success)
        {
            if (CanStrafe)
                axesWithSpeed.x = axes.Get(Axes.LeftRight) * SpeedLeftRight;
            if (CanUpDown)
                axesWithSpeed.y = axes.Get(Axes.UpDown) * SpeedUpDown;
            if (CanForward)
                axesWithSpeed.z = axes.Get(Axes.ForwardBackward) * SpeedForwardBackward;
            if (CanRotate)
                axesWithSpeed.w = axes.Get(Axes.Rotation) * SpeedRotation;

            // Smooth
            if (SmoothLeftRight.Enable)
                axesWithSpeed.x = SmoothLeftRight.ComputeSpeed(axesWithSpeed.x, Time.deltaTime);
            if (SmoothUpDown.Enable)
                axesWithSpeed.y = SmoothUpDown.ComputeSpeed(axesWithSpeed.y, Time.deltaTime);
            if (SmoothForwardBackward.Enable)
                axesWithSpeed.z = SmoothForwardBackward.ComputeSpeed(axesWithSpeed.z, Time.deltaTime);
            if (SmoothRotation.Enable)
                axesWithSpeed.w = SmoothRotation.ComputeSpeed(axesWithSpeed.w, Time.deltaTime);
        }

        if (locomotion != null)
            locomotion.UpdateAxes(this, axesWithSpeed);
    }
}
