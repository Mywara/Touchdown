﻿using System.Collections;
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
            autoAttaqueRanged.inModePlacing = false;
        }
        //active la previsualisation pour placer le piege
        if (Input.GetKeyDown(KeyCode.C) && !readyToPlace)
        {
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit);
            if (hit.transform.tag == "Floor")
            {
                tagVisualisation = Instantiate(tagVisualisationPrefab, hit.point, Quaternion.identity);
            }
            readyToPlace = true;
            autoAttaqueRanged.inModePlacing = true;
        }
        //pose le piege (si des charges sont disponible)
        if (readyToPlace && Input.GetMouseButtonDown(0) && nbCharges > 0)
        {


            healthTag = PhotonNetwork.Instantiate(tagPrefab.name, new Vector3(tagVisualisation.transform.position.x, tagVisualisation.transform.position.y + 0.1f, tagVisualisation.transform.position.z), Quaternion.identity, 0);

            readyToPlace = false;
            //Destroy(healthTag.gameObject.GetComponent<PGlaceMovement>());
            photonView.RPC("DestroyMovementScript", PhotonTargets.All, "PGlaceMovement");
            autoAttaqueRanged.inModePlacing = false;
            photonView.RPC("SetMyOwner", PhotonTargets.All, this.photonView.viewID, healthTag.GetPhotonView().viewID);
            nbCharges--;

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
}
