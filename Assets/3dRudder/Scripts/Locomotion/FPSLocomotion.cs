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
     * \class   FPSLocomotion
     *
     * \brief   The FPS locomotion. This allow you to move as an FPS game.
     */
    public class FPSLocomotion : ILocomotion
    {
        // Angle per second for the rotation
        public float AnglePerSecond = 90.0f;
        // If translate in local or world direction
        public bool LocalTranslation = true;        

        protected Vector3 translation;
        protected float angle;
        protected Transform trans;

        // Use this for initialization
        void Start()
        {
            translation = Vector3.zero;
            angle = 0.0f;
            trans = transform;
        }

        // Vector4 X = Left/Right, Y = Up/Down, Z = Forward/Backward, W = Rotation
        public override void UpdateAxes(Controller3dRudder controller3dRudder, Vector4 axesWithFactor)
        {
            // Translation XYZ           
            translation = axesWithFactor;
            // Rotation
            angle = axesWithFactor.w; ;
        }

        // Update is called once per frame
        void Update()
        {
            // Translate
            trans.Translate(translation * Time.deltaTime, LocalTranslation ? Space.Self : Space.World);
            // Rotate
            trans.Rotate(0, angle * AnglePerSecond * Time.deltaTime, 0);
        }
    }
}