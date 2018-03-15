using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGeneration : Photon.PUNBehaviour {

    static public MapGeneration instance;
    /*
    public GameObject[] threeByThreeBlocks;
    public GameObject threeByThreeBase;
    public GameObject threeByThreeCenter;
    public GameObject threeByThreeSpawn;
    public GameObject[] fiveByFiveBlocks;
    public GameObject fiveByFiveBase;
    public GameObject fiveByFiveCenter;
    public GameObject fiveByFiveSpawn;
    */
    public GameObject[] elevenByElevenBlocks;
    public GameObject elevenByElevenBase;
    public GameObject elevenByElevenCenter;
    public GameObject elevenByElevenSpawn;
    public GameObject crystal;

    /*
    public Button threeByThreeGene;
    public Button fiveByFiveGene;
    public Slider ligneSlider;
    public Slider colonneSlider;
    public Text slidersValue;
    public Button threeByThreeGeneWithSeed;
    public Button fiveByFiveGeneWithSeed;
    public Button generateSeed;
    */

    private Random rnd;
    private int threeTabLength;
    private int fiveTabLength;
    private int elevenTabLength;
    private List<GameObject> instances;
    private int nbLigne;
    private int nbColonne;
    /*
    private int[][] seed3x3;
    private int[][] seed5x5;
    */

    void Awake()
    {

        if (instance != null && instance != this)
        {
            Debug.Log("there is already a RoomManager");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        nbColonne = 0;
        nbLigne = 2;
        /*
        threeTabLength = threeByThreeBlocks.Length;
        fiveTabLength = fiveByFiveBlocks.Length;
        */
        elevenTabLength = elevenByElevenBlocks.Length;
        instances = new List<GameObject>();
    }

    void Start () {
        //nbLigne += 2;
        //seed3x3 = null;
        //seed5x5 = null;
        /*
        threeByThreeGene.onClick.AddListener(ButtonThreeByClicked);
        fiveByFiveGene.onClick.AddListener(ButtonFiveByClicked);
        threeByThreeGeneWithSeed.onClick.AddListener(ButtonThreeWithSeedByClicked);
        fiveByFiveGeneWithSeed.onClick.AddListener(ButtonFiveWithSeedByClicked);
        generateSeed.onClick.AddListener(ButtonSeedByClicked);
        ligneSlider.wholeNumbers = true;
        ligneSlider.minValue = 0;
        ligneSlider.maxValue = 10;
        ligneSlider.onValueChanged.AddListener(delegate { LigneSliderValueChanged(); });
        colonneSlider.wholeNumbers = true;
        colonneSlider.minValue = 0;
        colonneSlider.maxValue = 10;
        colonneSlider.onValueChanged.AddListener(delegate { ColonneSliderValueChanged(); });
        RefeshText();
        */
    }
	
	// Update is called once per frame
	void Update () {
    }
    /*
    private void RefeshText()
    {
        slidersValue.text = "Nb ligne : " + nbLigne + " / Nb colonne : " + nbColonne;
    }

    private void LigneSliderValueChanged()
    {
        nbLigne = (int)ligneSlider.value +2;
        RefeshText();
    }

    private void ColonneSliderValueChanged()
    {
        nbColonne = (int)colonneSlider.value;
        RefeshText();
    }
    
    private void ButtonThreeByClicked()
    {
        DestoryAllBlocks();
        ThreeByThreeGeneration();
    }

    private void ButtonFiveByClicked()
    {
        DestoryAllBlocks();
        FiveByFiveGeneration();
    }

    private void ButtonThreeWithSeedByClicked()
    {
        DestoryAllBlocks();
        ThreeByThreeGeneration(seed3x3);
    }

    private void ButtonFiveWithSeedByClicked()
    {
        DestoryAllBlocks();
        FiveByFiveGeneration(seed5x5);
    }

    private void ButtonSeedByClicked()
    {
        seed3x3 = GenerateSeed3x3();
        seed5x5 = GenerateSeed5x5();
        Debug.Log("seed3x3 length : " + seed3x3.Length);
        Debug.Log("seed5x5 length : " + seed5x5.Length);
    }

    private void ThreeByThreeGeneration()
    {
        GameObject center = Instantiate(threeByThreeCenter, Vector3.zero, Quaternion.identity);
        instances.Add(center);
        for(int i=0; i<nbLigne; i++)
        {
            for(int j=-nbColonne; j<=nbColonne; j++)
            {
                if((i != 0 || j != 0) && (i != 0 || j < 0))
                {
                    if (i==(nbLigne-1) && j==0)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 3;
                        spawnPos.z += i * 3;
                        GameObject posBase = Instantiate(threeByThreeBase, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 3;
                        spawnPosSym.z -= i * 3;
                        GameObject posBase2 = Instantiate(threeByThreeBase, spawnPosSym, posBase.transform.rotation);
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                    }
                    else if (i == (nbLigne - 1) && j == nbColonne)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 3;
                        spawnPos.z += i * 3;
                        GameObject posBase = Instantiate(threeByThreeSpawn, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 3;
                        spawnPosSym.z -= i * 3;
                        GameObject posBase2 = Instantiate(threeByThreeSpawn, spawnPosSym, posBase.transform.rotation);
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                    }
                    else
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 3;
                        spawnPos.z += i * 3;
                        int randomPrefabNum = Random.Range(0, threeTabLength);
                        GameObject block = Instantiate(threeByThreeBlocks[randomPrefabNum], spawnPos, Quaternion.identity);
                        instances.Add(block);
                        int randomRot = Random.Range(0, 5);
                        block.transform.Rotate(0, 90 * randomRot, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 3;
                        spawnPosSym.z -= i * 3;
                        GameObject block2 = Instantiate(threeByThreeBlocks[randomPrefabNum], spawnPosSym, block.transform.rotation);
                        instances.Add(block2);
                        block2.transform.Rotate(0, 180, 0);
                    }
                }
            }
        }
    }

    private void FiveByFiveGeneration()
    {
        GameObject center = Instantiate(fiveByFiveCenter, Vector3.zero, Quaternion.identity);
        instances.Add(center);
        for (int i = 0; i < nbLigne; i++)
        {
            for (int j = -nbColonne; j <= nbColonne; j++)
            {
                if ((i != 0 || j != 0) && (i != 0 || j < 0))
                {
                    if (i == (nbLigne - 1) && j == 0)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 5;
                        spawnPos.z += i * 5;
                        GameObject posBase = Instantiate(fiveByFiveBase, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 5;
                        spawnPosSym.z -= i * 5;
                        GameObject posBase2 = Instantiate(fiveByFiveBase, spawnPosSym, posBase.transform.rotation);
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                    }
                    else if (i == (nbLigne - 1) && j == nbColonne)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 5;
                        spawnPos.z += i * 5;
                        GameObject posBase = Instantiate(fiveByFiveSpawn, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 5;
                        spawnPosSym.z -= i * 5;
                        GameObject posBase2 = Instantiate(fiveByFiveSpawn, spawnPosSym, posBase.transform.rotation);
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                    }
                    else
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 5;
                        spawnPos.z += i * 5;
                        int randomPrefabNum = Random.Range(0, fiveTabLength);
                        GameObject block = Instantiate(fiveByFiveBlocks[randomPrefabNum], spawnPos, Quaternion.identity);
                        instances.Add(block);
                        int randomRot = Random.Range(0, 5);
                        block.transform.Rotate(0, 90 * randomRot, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 5;
                        spawnPosSym.z -= i * 5;
                        GameObject block2 = Instantiate(fiveByFiveBlocks[randomPrefabNum], spawnPosSym, block.transform.rotation);
                        instances.Add(block2);
                        block2.transform.Rotate(0, 180, 0);
                    }
                }
            }
        }
    }
    */
    
    private void DestoryAllBlocks()
    {
        foreach(GameObject block in instances)
        {
            Destroy(block);
        }
        instances.Clear();
    }
    /*
    public int[][] GenerateSeed3x3()
    {
        int nbNombreToGenerate = (nbLigne) * (2 * nbColonne + 1) - (nbColonne + 3);
        int[][] seeds = new int[nbNombreToGenerate][];
        for (int i = 0; i < nbNombreToGenerate; i++)
        {
            int randomPrefabNum = Random.Range(0, threeTabLength);
            int randomRot = Random.Range(0, 5);
            seeds[i] = new int[] { randomPrefabNum, randomRot };
        }
        return seeds;
    }

    public int[][] GenerateSeed5x5()
    {
        int nbNombreToGenerate = (nbLigne) * (2 * nbColonne + 1) - (nbColonne + 3);
        int[][] seeds = new int[nbNombreToGenerate][];
        for (int i = 0; i < nbNombreToGenerate; i++)
        {
            int randomPrefabNum = Random.Range(0, fiveTabLength);
            int randomRot = Random.Range(0, 5);
            seeds[i] = new int[] { randomPrefabNum, randomRot };
        }
        return seeds;
    }
    */
    public int[][] GenerateSeed11x11()
    {
        int nbNombreToGenerate = (nbLigne) * (2 * nbColonne + 1) - (nbColonne + 3);
        int[][] seeds = new int[nbNombreToGenerate][];
        for (int i = 0; i < nbNombreToGenerate; i++)
        {
            int randomPrefabNum = Random.Range(0, elevenTabLength);
            int randomRot = Random.Range(0, 5);
            seeds[i] = new int[] { randomPrefabNum, randomRot };
        }
        return seeds;
    }
    /*
    [PunRPC]
    public void ThreeByThreeGeneration(int[][] seeds)
    {
        if(seeds == null || seeds.Length == 0)
        {
            Debug.Log("Error : no seed");
            return;
        }

        int cmpt = 0;
        GameObject center = Instantiate(threeByThreeCenter, Vector3.zero, Quaternion.identity);
        instances.Add(center);
        for (int i = 0; i < nbLigne; i++)
        {
            for (int j = -nbColonne; j <= nbColonne; j++)
            {
                if ((i != 0 || j != 0) && (i != 0 || j < 0))
                {
                    if (i == (nbLigne - 1) && j == 0)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 3;
                        spawnPos.z += i * 3;
                        GameObject posBase = Instantiate(threeByThreeBase, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 3;
                        spawnPosSym.z -= i * 3;
                        GameObject posBase2 = Instantiate(threeByThreeBase, spawnPosSym, posBase.transform.rotation);
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                    }
                    else if (i == (nbLigne - 1) && j == nbColonne)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 3 + 1;
                        spawnPos.z += i * 3 + 1;
                        GameObject posBase = Instantiate(threeByThreeSpawn, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        RoomManager.instance.respawnTeam2 = posBase;
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 3 + 1;
                        spawnPosSym.z -= i * 3 + 1;
                        GameObject posBase2 = Instantiate(threeByThreeSpawn, spawnPosSym, posBase.transform.rotation);
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                        RoomManager.instance.respawnTeam1 = posBase2;
                    }
                    else
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 3;
                        spawnPos.z += i * 3;
                        int randomPrefabNum = seeds[cmpt][0];
                        GameObject block = Instantiate(threeByThreeBlocks[randomPrefabNum], spawnPos, Quaternion.identity);
                        instances.Add(block);
                        int randomRot = seeds[cmpt][1];
                        block.transform.Rotate(0, 90 * randomRot, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 3;
                        spawnPosSym.z -= i * 3;
                        GameObject block2 = Instantiate(threeByThreeBlocks[randomPrefabNum], spawnPosSym, block.transform.rotation);
                        instances.Add(block2);
                        block2.transform.Rotate(0, 180, 0);
                        cmpt++;
                    }
                }
            }
        }
    }
    */
    /*
    [PunRPC]
    public void FiveByFiveGeneration(int[][] seeds)
    {
        if (seeds == null || seeds.Length == 0)
        {
            Debug.Log("Error : no seed");
            return;
        }

        int cmpt = 0;
        GameObject center = Instantiate(fiveByFiveCenter, Vector3.zero, Quaternion.identity);
        if (PhotonNetwork.isMasterClient)
        {
            Vector3 crystalStartingPosition = center.transform.position;
            crystalStartingPosition.y += 1f;

            GameObject crystalInstance = PhotonNetwork.InstantiateSceneObject("Crystal", crystalStartingPosition, Quaternion.identity, 0, null);

            crystalInstance.GetComponent<Crystal>().photonView.RPC("SetStartingPosition", PhotonTargets.AllBuffered, crystalStartingPosition);
        }
        instances.Add(center);
        for (int i = 0; i < nbLigne; i++)
        {
            for (int j = -nbColonne; j <= nbColonne; j++)
            {
                if ((i != 0 || j != 0) && (i != 0 || j < 0))
                {
                    if (i == (nbLigne - 1) && j == 0)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 5;
                        spawnPos.z += i * 5;
                        GameObject posBase = Instantiate(fiveByFiveBase, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        posBase.transform.Find("Goal").tag = "GoalD";
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 5;
                        spawnPosSym.z -= i * 5;
                        GameObject posBase2 = Instantiate(fiveByFiveBase, spawnPosSym, posBase.transform.rotation);
                        posBase2.transform.Find("Goal").tag = "GoalG";
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                    }
                    else if (i == (nbLigne - 1) && j == nbColonne)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 5 + 1;
                        spawnPos.z += i * 5 + 1;
                        GameObject posBase = Instantiate(fiveByFiveSpawn, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        RoomManager.instance.respawnTeam2 = posBase;
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 5 + 1;
                        spawnPosSym.z -= i * 5 + 1;
                        GameObject posBase2 = Instantiate(fiveByFiveSpawn, spawnPosSym, posBase.transform.rotation);
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                        RoomManager.instance.respawnTeam1 = posBase2;
                    }
                    else
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 5;
                        spawnPos.z += i * 5;
                        int randomPrefabNum = seeds[cmpt][0];
                        GameObject block = Instantiate(fiveByFiveBlocks[randomPrefabNum], spawnPos, Quaternion.identity);
                        instances.Add(block);
                        int randomRot = seeds[cmpt][1];
                        block.transform.Rotate(0, 90 * randomRot, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 5;
                        spawnPosSym.z -= i * 5;
                        GameObject block2 = Instantiate(fiveByFiveBlocks[randomPrefabNum], spawnPosSym, block.transform.rotation);
                        instances.Add(block2);
                        block2.transform.Rotate(0, 180, 0);
                        cmpt++;
                    }
                }
            }
        }
    }
    */

    [PunRPC]
    public void ElevenByElevenGeneration(int[][] seeds)
    {
        if (seeds == null || seeds.Length == 0)
        {
            Debug.Log("Error : no seed");
            return;
        }

        int cmpt = 0;
        GameObject center = Instantiate(elevenByElevenCenter, Vector3.zero, Quaternion.identity);
        if (PhotonNetwork.isMasterClient)
        {
            Vector3 crystalStartingPosition = center.transform.position;
            crystalStartingPosition.y += 1f;

            GameObject crystalInstance = PhotonNetwork.InstantiateSceneObject("Crystal", crystalStartingPosition, Quaternion.identity, 0, null);

            crystalInstance.GetComponent<Crystal>().photonView.RPC("SetStartingPosition", PhotonTargets.AllBuffered, crystalStartingPosition);
        }
        instances.Add(center);
        for (int i = 0; i < nbLigne; i++)
        {
            for (int j = -nbColonne; j <= nbColonne; j++)
            {
                if ((i != 0 || j != 0) && (i != 0 || j < 0))
                {
                    if (i == (nbLigne - 1) && j == 0)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 11;
                        spawnPos.z += i * 11;
                        GameObject posBase = Instantiate(elevenByElevenBase, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        posBase.transform.Find("Goal").tag = "GoalD";
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 11;
                        spawnPosSym.z -= i * 11;
                        GameObject posBase2 = Instantiate(elevenByElevenBase, spawnPosSym, posBase.transform.rotation);
                        posBase2.transform.Find("Goal").tag = "GoalG";
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                    }
                    else if (i == (nbLigne - 1) && j == nbColonne)
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 11;
                        spawnPos.z += i * 11;
                        GameObject posBase = Instantiate(elevenByElevenSpawn, spawnPos, Quaternion.identity);
                        instances.Add(posBase);
                        posBase.transform.Rotate(0, 180, 0);
                        RoomManager.instance.respawnTeam2 = posBase;
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 11;
                        spawnPosSym.z -= i * 11;
                        GameObject posBase2 = Instantiate(elevenByElevenSpawn, spawnPosSym, posBase.transform.rotation);
                        instances.Add(posBase2);
                        posBase2.transform.Rotate(0, 180, 0);
                        RoomManager.instance.respawnTeam1 = posBase2;
                    }
                    else
                    {
                        Vector3 spawnPos = center.transform.position;
                        spawnPos.x += j * 11;
                        spawnPos.z += i * 11;
                        int randomPrefabNum = seeds[cmpt][0];
                        GameObject block = Instantiate(elevenByElevenBlocks[randomPrefabNum], spawnPos, Quaternion.identity);
                        instances.Add(block);
                        int randomRot = seeds[cmpt][1];
                        block.transform.Rotate(0, 90 * randomRot, 0);
                        Vector3 spawnPosSym = center.transform.position;
                        spawnPosSym.x -= j * 11;
                        spawnPosSym.z -= i * 11;
                        GameObject block2 = Instantiate(elevenByElevenBlocks[randomPrefabNum], spawnPosSym, block.transform.rotation);
                        instances.Add(block2);
                        block2.transform.Rotate(0, 180, 0);
                        cmpt++;
                    }
                }
            }
        }
    }

    public int NbLigne
    {
        get
        {
            return this.nbLigne;
        }
        set
        {
            this.nbLigne = value;
        }
    }

    public int NbColonne
    {
        get
        {
            return this.nbColonne;
        }
        set
        {
            this.nbColonne = value;
        }
    }
}
