using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

    public GameObject crys;

    private void OnTriggerEnter(Collider target)
    {
        if (PhotonNetwork.connected == true && !PhotonNetwork.isMasterClient)
        {
            return;
        }


        crys = GameObject.FindGameObjectWithTag("Crystal");

        //Debug.Log(target.transform.root.gameObject.name);

        if (target.transform.root.gameObject.tag == "Player"
            && crys.GetComponent<Crystal>().isHeld == true
            && crys.GetComponent<Crystal>().playerHolding == target.transform.root.gameObject)
        {
            //Le cristal doit être reset si le but est marqué il faut donc que le butteur soit de l'equipe adverse au but atteint
            if(this.tag == "GoalG" && target.gameObject.GetComponent<PlayerController>().team == 0 ||
               this.tag == "GoalD" && target.gameObject.GetComponent<PlayerController>().team == 1)
            {
                //Debug.Log("Crystal is in goal collider OK!");

                crys.GetComponent<Crystal>().photonView.RPC("LeaveOnGround", PhotonTargets.All);
                //Debug.Log("Crystal left on ground");

                crys.GetComponent<Crystal>().photonView.RPC("ResetCrystalPosition", PhotonTargets.All);
                //Debug.Log("Crystal reset");
            }
        }

        
    }

}
