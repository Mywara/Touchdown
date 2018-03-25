using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PGlace : Photon.PUNBehaviour
{

    public GameObject trapVisualisationPrefab;
    public GameObject trapPrefab;
    private GameObject trapVisualisation;
    private GameObject trap;
    public bool readyToPlace = false;
    public int nbCharges = 3;
    //private AutoAttaqueRanged autoAttaqueRanged;
    //cooldown voulu + temps d'activation du pièges
    public float cooldown = 20;
    public bool startCoroutineGetACharge = false;
    private int idtrap = 0;
    GameObject[] traps = new GameObject[3];
    public Text nbtrap;
    public Text cd;
    [HideInInspector]
    public bool inGame = true;

    // Use this for initialization
    void Start () {
        //autoAttaqueRanged = this.transform.GetComponent<AutoAttaqueRanged>();

    }
	
	// Update is called once per frame
	void Update () {
        if (!photonView.isMine)
        {
            return;
        }
        /*
        if (Input.GetKeyDown(KeyCode.L))
        {
            inGame = !inGame;
        }
        */
        if (inGame) { 
            if (startCoroutineGetACharge)
            {
                StartCoroutine(GetaCharges(cooldown));
                startCoroutineGetACharge = false;
            }

            //désactive l'utilisation de la compétence sans poser le piege
            if (readyToPlace && Input.GetKeyDown(KeyCode.X))
            {
            
                Destroy(trapVisualisation);
                readyToPlace = false;
                //autoAttaqueRanged.inModePlacing = false;
            }
            //active la previsualisation pour placer le piege
                if (Input.GetKeyDown(KeyCode.X) && !readyToPlace)
            {
                RaycastHit hit;
                Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)),out hit);
                if(hit.transform.tag == "Floor")
                {
                    trapVisualisation = Instantiate(trapVisualisationPrefab,hit.point,Quaternion.identity);
                    
                }
                readyToPlace = true;
                //autoAttaqueRanged.inModePlacing = true;
            }
            //pose le piege (si des charges sont disponible)
            if (readyToPlace && Input.GetMouseButtonDown(0) && nbCharges > 0)
            {
            
                                 
                trap = PhotonNetwork.Instantiate(trapPrefab.name, trapVisualisation.transform.position, Quaternion.identity, 0);
                photonView.RPC("DestroyTrap", PhotonTargets.All, idtrap);
            
                traps[idtrap % 3] = trap;
                idtrap++;
                readyToPlace = false;
                Destroy(trapVisualisation);
                //autoAttaqueRanged.inModePlacing = false;
                photonView.RPC("SetOwner", PhotonTargets.All, this.photonView.viewID, trap.GetPhotonView().viewID);
                       
            }
        }
        else // en mode intermanche
        {
            //désactive l'utilisation de la compétence sans poser le piege
            if (readyToPlace && Input.GetKeyDown(KeyCode.X))
            {
                Destroy(trapVisualisation);
                readyToPlace = false;
                //autoAttaqueRanged.inModePlacing = false;
            }
            //active la previsualisation pour placer le piege
            
            if (Input.GetKeyDown(KeyCode.X) && !readyToPlace)
            {

                RaycastHit hit;
                //on instancie la ou est la souris
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
                if(this.transform.GetComponent<PlayerController>().team == 1 && hit.point.z < 0)
                {
                    trapVisualisation = Instantiate(trapVisualisationPrefab, hit.point, Quaternion.identity);
                    trapVisualisation.GetComponent<PGlaceMovement>().Owner = this.transform.gameObject;
                    trapVisualisation.GetComponent<PGlaceMovement>().inGame = false;
                    readyToPlace = true;
                    //autoAttaqueRanged.inModePlacing = true;
                }
                if (this.transform.GetComponent<PlayerController>().team == 2 && hit.point.z > 0)
                {
                    trapVisualisation = Instantiate(trapVisualisationPrefab, hit.point, Quaternion.identity);
                    trapVisualisation.GetComponent<PGlaceMovement>().Owner = this.transform.gameObject;
                    trapVisualisation.GetComponent<PGlaceMovement>().inGame = false;
                    readyToPlace = true;
                    //autoAttaqueRanged.inModePlacing = true;
                }

            }
            //pose le piege (si des charges sont disponible)
            if (readyToPlace && Input.GetMouseButtonDown(0) && nbCharges > 0)
            {
                trap = PhotonNetwork.Instantiate(trapPrefab.name, trapVisualisation.transform.position, Quaternion.identity, 0);
                photonView.RPC("DestroyTrap", PhotonTargets.All, idtrap);
                traps[idtrap % 3] = trap;
                idtrap++;
                readyToPlace = false;
                Destroy(trapVisualisation);
                //autoAttaqueRanged.inModePlacing = false;
                photonView.RPC("SetOwner", PhotonTargets.All, this.photonView.viewID, trap.GetPhotonView().viewID);

            }
        }
    }


    public IEnumerator GetaCharges(float coolDown)
    {
        while (coolDown > 0)
        {
            coolDown -= Time.deltaTime;
            if (nbCharges == 0)
            {
                cd.text = "" + coolDown;
            }
            
            if (coolDown <= 0)
            {
                nbCharges++;
                nbtrap.text = "" + nbCharges;
            }
            yield return null;
        }

    }

    [PunRPC]
    void DestroyTrap(int idtrap)
    {
        Destroy(traps[idtrap % 3]);
    }

    [PunRPC]
    void SetOwner(int idt, int idTrap)
    {
        GameObject owner = PhotonView.Find(idt).gameObject;
        GameObject trap = PhotonView.Find(idTrap).gameObject;
        trap.GetComponent<PGlaceTrigger>().Owner = owner;
        Debug.Log("Owner : " + owner.name + " , This : " + PUNTutorial.GameManager.localPlayer.name);
        if(owner.GetComponent<PlayerController>().Team != PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team)
        {
            Debug.Log("render to false");
            trap.GetComponent<Renderer>().enabled = false;
        }
    }

    public void SwitchPlayerMode()
    {
        inGame = !inGame;
    }
}
