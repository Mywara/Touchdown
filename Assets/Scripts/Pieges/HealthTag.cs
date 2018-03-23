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


            healthTag = PhotonNetwork.Instantiate(tagPrefab.name, tagVisualisation.transform.position, Quaternion.identity, 0);
            PhotonNetwork.Destroy(healthTag);

            readyToPlace = false;
            Destroy(tagVisualisation);
            autoAttaqueRanged.inModePlacing = false;
            photonView.RPC("SetOwner", PhotonTargets.All, this.photonView.viewID, healthTag.GetPhotonView().viewID);

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
}
