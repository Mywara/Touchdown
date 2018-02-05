using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour {


	void Update () {
        transform.LookAt(2*  this.transform.position - Camera.main.transform.position);
	}
}
