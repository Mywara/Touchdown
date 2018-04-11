using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PGlace : Photon.PUNBehaviour
{
    public GameObject trapVisualisationPrefab;
    public GameObject trapPrefab;
    public GameObject trapVisualisation;
    private GameObject trap;
    public bool readyToPlace = false;
    public int nbCharges = 3;
    public int nbChargesV = 3;
    private AutoAttaqueRanged autoAttaqueRanged;
    //cooldown voulu + temps d'activation du pièges
    public float cooldown = 20;
    public bool startCoroutineGetACharge = false;
    private int idtrap = 0;
    GameObject[] traps = new GameObject[3];
    public Text nbtrap;
    public Text cd;
    private GameObject cdGreyMask;
    private GameObject cdTimer;
    [HideInInspector]
    public bool inGame = true;
    public bool IcetrapVisualisation = false;

    // Use this for initialization
    void Start ()
    {
        autoAttaqueRanged = this.transform.GetComponent<AutoAttaqueRanged>();
        cdGreyMask = transform.Find("PlayerHealth/IceTrapIcon/CooldownGreyMask").gameObject;
        cdTimer = transform.Find("PlayerHealth/IceTrapIcon/cdIce").gameObject;
        cdGreyMask.SetActive(false);
        cdTimer.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (!photonView.isMine)
        {
            return;
        }
        if (inGame) { 
            if (startCoroutineGetACharge)
            {
                StartCoroutine(GetaCharges(cooldown));
                startCoroutineGetACharge = false;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {    
                //désactive l'utilisation de la compétence sans poser le piege
                if (readyToPlace)
                {
                    DesactivateTrap();
                }
                //active la previsualisation pour placer le piege
                else
                {
                    RaycastHit hit;
                    if (this.transform.root.GetComponent<HealthTag>().HealtrapVisualisation == true)
                    {
                        this.transform.root.GetComponent<HealthTag>().DesactiveTrap();
                        trapVisualisation = Instantiate(trapVisualisationPrefab, this.transform.root.GetComponent<HealthTag>().tagVisualisation.transform.position, Quaternion.identity);
                    }
                    else
                    {
                        Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit);
                        if (hit.transform.tag == "Floor")
                        {
                            trapVisualisation = Instantiate(trapVisualisationPrefab, hit.point, Quaternion.identity);
                        }
                    }
                    readyToPlace = true;
                    IcetrapVisualisation = true;
                    if (autoAttaqueRanged != null)
                    {
                        autoAttaqueRanged.inModePlacing = true;
                    }
                }
            }
         
            //pose le piege (si des charges sont disponible)
            if (readyToPlace && Input.GetMouseButtonDown(1) && nbCharges > 0)
            {
                PutTrap();
            }
        }
        else // en mode intermanche
        {        
            if (Input.GetKeyDown(KeyCode.X))
            {
                //désactive l'utilisation de la compétence sans poser le piege
                if (readyToPlace)
                {
                    DesactivateTrap();
                }              
                else
                {
                    //active la previsualisation pour placer le piege
                    if (this.transform.root.GetComponent<HealthTag>().HealtrapVisualisation == true)
                    {
                        this.transform.root.GetComponent<HealthTag>().DesactiveTrap();
                    }                  
                    RaycastHit hit;
                    //on instancie la ou est la souris
                    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
                    if ((this.transform.GetComponent<PlayerController>().team == 1 && hit.point.z < 0) || (this.transform.GetComponent<PlayerController>().team == 2 && hit.point.z > 0))
                    {
                        trapVisualisation = Instantiate(trapVisualisationPrefab, hit.point, Quaternion.identity);
                        trapVisualisation.GetComponent<PGlaceMovement>().Owner = this.transform.gameObject;
                        trapVisualisation.GetComponent<PGlaceMovement>().inGame = false;
                        readyToPlace = true;
                        IcetrapVisualisation = true;
                        if (autoAttaqueRanged != null)
                        {
                            autoAttaqueRanged.inModePlacing = true;
                        }
                    }
                }
            }          
            //pose le piege (si des charges sont disponible)
            if (readyToPlace && Input.GetMouseButtonDown(1) && nbCharges > 0)
            {
                PutTrap();
            }
        }
    }


    public IEnumerator GetaCharges(float coolDown)
    {
        while (coolDown > 0)
        {
            coolDown -= Time.deltaTime;
            if (nbChargesV == 0)
            {
                // Show cooldown time and mask
                cdGreyMask.SetActive(true);
                cdTimer.SetActive(true);

                cd.text = "" + coolDown;
            }
            
            if (coolDown <= 0)
            {
                nbChargesV++;
                nbCharges++;
                nbtrap.text = "" + nbChargesV;

                coolDown = 0f;
                cd.text = coolDown.ToString();

                // Hide cooldown time and mask
                cdGreyMask.SetActive(false);
                cdTimer.SetActive(false);
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

    public void DesactivateTrap()
    {
        Destroy(trapVisualisation);
        readyToPlace = false;
        if (autoAttaqueRanged != null)
        {
            autoAttaqueRanged.inModePlacing = false;
        }
        IcetrapVisualisation = false;
    }

    public void PutTrap()
    {
        
        trap = PhotonNetwork.Instantiate(trapPrefab.name, trapVisualisation.transform.position, Quaternion.identity, 0);
        photonView.RPC("DestroyTrap", PhotonTargets.All, idtrap);
        traps[idtrap % 3] = trap;
        idtrap++;
        IcetrapVisualisation = false;
        readyToPlace = false;
        Destroy(trapVisualisation);
        if (autoAttaqueRanged != null)
        {
            autoAttaqueRanged.inModePlacing = false;
        }
        photonView.RPC("SetOwner", PhotonTargets.All, this.photonView.viewID, trap.GetPhotonView().viewID);
        if (nbChargesV > 0)
        {
            this.nbChargesV--;
        }

        this.nbtrap.text = "" +this.nbChargesV;
    }
}
