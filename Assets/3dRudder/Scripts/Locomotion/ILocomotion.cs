using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* \namespace	ns3dRudder
*
*/
namespace ns3dRudder
{
	/**
	 * \class	ILocomotion
	 *
	 * \brief	The interface for locomotion. This allow you to get the axes of the 3dRudder.
	 */
    public abstract class ILocomotion : MonoBehaviour
    {
    	/**
		*
		* \brief Update the axes with a factor (Speed)
		*
		* ¤compatible_plateforme Win, PS4¤
		* ¤compatible_firm From x.4.x.x¤
		* ¤compatible_sdk From 2.00¤
		*
		* \param	controller3dRudder	The access to controller.
		* \param	axesWithFactor	The Unity.Vector4 with (X = Left/Right, Y = Up/Down, Z = Forward/Backward, W = Rotation)
		*
		*/
        public abstract void UpdateAxes(Controller3dRudder controller3dRudder, Vector4 axesWithFactor);
    }
}