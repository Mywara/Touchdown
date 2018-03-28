using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthTag : Photon.PUNBehaviour
{

    public GameObject tagVisualisationPrefab;
    public GameObject tagPrefab;
    private GameObject tagVisualisation;
    private GameObject healthTag;
    public bool readyToPlace = false;
    private int nbChargesMax = 1;
    public int nbCharges = 1;
    private AutoAttaqueRanged autoAttaqueRanged;
    //cooldown voulu + temps d'activation du pièges
    public float cooldown = 20;
    public bool startCoroutineGetACharge = false;
    public Text nbtrap;
    public Text cd;
    [HideInInspector]
    public bool inGame = true;

    // Use this for initialization
    void Start()
    {
        autoAttaqueRanged = this.transform.GetComponent<AutoAttaqueRanged>();

    }

    // Update is called once per frame
    void Update()
    {
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
        if (inGame)
        {
            if (startCoroutineGetACharge)
            {
                StartCoroutine(GetaCharges(cooldown));
                startCoroutineGetACharge = false;
            }

            //désactive l'utilisation de la compétence sans poser le piege
            if (readyToPlace && Input.GetKeyDown(KeyCode.C))
            {
                Destroy(tagVisualisation);
                readyToPlace = false;
            }
            //active la previsualisation pour placer le piege
            if (Input.GetKeyDown(KeyCode.C) && !readyToPlace && nbCharges > 0)
            {
                RaycastHit hit;
                Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit);
                if (hit.transform.tag == "Floor")
                {
                    tagVisualisation = Instantiate(tagVisualisationPrefab, hit.point, Quaternion.identity);
                }
                readyToPlace = true;
                if(autoAttaqueRanged != null)
                {
                    autoAttaqueRanged.inModePlacing = true;
                }

            }
            //pose le piege (si des charges sont disponible)
            if (readyToPlace && Input.GetMouseButtonDown(0) && nbCharges > 0)
            {
                healthTag = PhotonNetwork.Instantiate(tagPrefab.name, new Vector3(tagVisualisation.transform.position.x, tagVisualisation.transform.position.y + 0.1f, tagVisualisation.transform.position.z), Quaternion.identity, 0);

                readyToPlace = false;
                //Destroy(healthTag.gameObject.GetComponent<PGlaceMovement>());
                photonView.RPC("DestroyMovementScript", PhotonTargets.All, "PGlaceMovement");
                if (autoAttaqueRanged != null)
                {
                    autoAttaqueRanged.inModePlacing = false;
                }
                photonView.RPC("SetMyOwner", PhotonTargets.All, this.photonView.viewID, healthTag.GetPhotonView().viewID);
                nbCharges--;
            }
        }
        else // en mode intermanche
        {
            //désactive l'utilisation de la compétence sans poser le piege
            if (readyToPlace && Input.GetKeyDown(KeyCode.C))
            {
                Destroy(tagVisualisation);
                readyToPlace = false;
            }
            //active la previsualisation pour placer le piege
            if (Input.GetKeyDown(KeyCode.C) && !readyToPlace && nbCharges > 0)
            {
                RaycastHit hit;
                //on instancie la ou est la souris
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
                if (this.transform.GetComponent<PlayerController>().team == 1 && hit.point.z < 0)
                {
                    tagVisualisation = Instantiate(tagVisualisationPrefab, hit.point, Quaternion.identity);
                    tagVisualisation.GetComponent<PGlaceMovement>().Owner = this.transform.gameObject;
                    tagVisualisation.GetComponent<PGlaceMovement>().inGame = false;
                    readyToPlace = true;
                }
                if (this.transform.GetComponent<PlayerController>().team == 2 && hit.point.z > 0)
                {
                    tagVisualisation = Instantiate(tagVisualisationPrefab, hit.point, Quaternion.identity);
                    tagVisualisation.GetComponent<PGlaceMovement>().Owner = this.transform.gameObject;
                    tagVisualisation.GetComponent<PGlaceMovement>().inGame = false;
                    readyToPlace = true;
                }
            }
            //pose le piege (si des charges sont disponible)
            if (readyToPlace && Input.GetMouseButtonDown(0) && nbCharges > 0)
            {
                healthTag = PhotonNetwork.Instantiate(tagPrefab.name, new Vector3(tagVisualisation.transform.position.x, tagVisualisation.transform.position.y + 0.1f, tagVisualisation.transform.position.z), Quaternion.identity, 0);
                readyToPlace = false;
                //Destroy(healthTag.gameObject.GetComponent<PGlaceMovement>());
                photonView.RPC("DestroyMovementScript", PhotonTargets.All, "PGlaceMovement");
                if (autoAttaqueRanged != null)
                {
                    autoAttaqueRanged.inModePlacing = false;
                }
                photonView.RPC("SetMyOwner", PhotonTargets.All, this.photonView.viewID, healthTag.GetPhotonView().viewID);
                nbCharges--;

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

            if (coolDown <= 0 && nbCharges < nbChargesMax)
            {
                nbCharges++;
                nbtrap.text = "" + nbCharges;
            }
            yield return null;
        }

    }

    [PunRPC]
    void SetMyOwner(int idt, int idTrap)
    {
        GameObject owner = PhotonView.Find(idt).gameObject;
        healthTag.GetComponent<HealthTagTrigger>().Owner = owner;
        Debug.Log("Owner : " + owner.name + " , This : " + PUNTutorial.GameManager.localPlayer.name);
        if (owner.GetComponent<PlayerController>().Team != PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team)
        {
            Debug.Log("render to false");
            healthTag.GetComponent<Renderer>().enabled = false;
        }
    }

    [PunRPC]
    void DestroyMovementScript(string scriptName)
    {
        Destroy(healthTag.gameObject.GetComponent(scriptName));
        Destroy(tagVisualisation);
    }

    public void SwitchPlayerMode()
    {
        inGame = !inGame;
    }
}
