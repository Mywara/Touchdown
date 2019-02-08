﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ns3dRudder
{
	public class Cutom3dRudderScript : ILocomotion
	{
		private Vector4 axes;
        private Transform trans;

		// Use this for initialization
		void Start()
		{
            axes = Vector4.zero;
            trans = transform;
		}

		// Vector4 X = Left/Right, Y = Up/Down, Z = Forward/Backward, W = Rotation
		public override void UpdateAxes(Controller3dRudder controller3dRudder, Vector4 axesWithFactor)
		{
            // save axesWithFactor like you want
            axes = axesWithFactor;
		}

		// Update is called once per frame
		void Update()
		{
        }
	}
}