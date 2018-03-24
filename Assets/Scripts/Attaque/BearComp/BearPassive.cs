using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearPassive : MonoBehaviour {

    private int hitStack = 0;


    public void IncrementHitStack(List<GameObject> hitEnemies)
    {
        hitStack++;
        Debug.Log("Current hit stack : " + hitStack);

        if(hitStack >= Constants.WARBEAR_HITSTACK)
        {
            Debug.Log("Hit stack reset!");
            hitStack = 0;

            foreach(GameObject enemy in hitEnemies)
            {
                Debug.Log("Enemy stun by Warbear passive : " + enemy.name);
                PlayerController enemyController = enemy.GetComponent<PlayerController>();

                if (PhotonNetwork.connected == true)
                {
                    enemyController.photonView.RPC("Stun", PhotonTargets.All, Constants.WARBEAR_STUN_DURATION);
                }
                else
                {
                    StartCoroutine("OfflineStun", enemyController);
                }
            }
        }
    }

    private IEnumerator OfflineStun(PlayerController enemyController)
    {
        // prive translation, rotation du perso et compétences du perso
        enemyController.SetMobile(false);
        enemyController.SetActiveCompetence(false);

        // Attend la duree demandé
        yield return new WaitForSeconds(Constants.WARBEAR_STUN_DURATION);

        // autorise translation, rotation du perso et compétences du perso
        enemyController.SetMobile(true);
        enemyController.SetActiveCompetence(true);
    }
}
