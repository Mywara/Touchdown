using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomManager : Photon.PunBehaviour {

    static public RoomManager instance;
    public float respawnTime = 5;
    public float respawnHeight = 2;
    public GameObject respawnTeam1;
    public GameObject respawnTeam2;
    public float waitForStartTime = 10F;
    public float stratPhaseTime = 90F;

    private List<GameObject> team1 = new List<GameObject>();
    private List<GameObject> team2 = new List<GameObject>();
    //public List<GameObject> team1 = new List<GameObject>();
    //public List<GameObject> team2 = new List<GameObject>();
    public Dictionary<int, GameObject> allPlayer = new Dictionary<int, GameObject>();
    private bool friendlyFire = false;

    public Button readyForNewPhase;

    public GameObject NonePlacingZoneTeam1;
    public GameObject NonePlacingZoneTeam2;

    private int nbMaxPlayer;
    private int nbPlayerReady = 0;
    private bool iAmReady = false;
    private bool allPlayerHaveGeneratedMap = false;
    private int nbPlayerHaveGeneratedMap = 0;
    private bool waitForStart = false;
    private bool stratPhase = false;
    private bool playPhase = false;
    private bool gameStarted = false;
    private float startTimePhase = 0.0f;
    private float gameTimerValueSaved;

    void Awake()
    {

        if (instance != null && instance != this)
        {
            Debug.Log("there is already a RoomManager");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        //on va chercher le nombre de joueur max dans la room
        if (PhotonNetwork.connected)
        {
            nbMaxPlayer = PhotonNetwork.room.MaxPlayers;
        }
        //on initialise le bouton a rouge
        readyForNewPhase.GetComponent<Image>().color = Color.red;
        readyForNewPhase.gameObject.SetActive(false);
        NonePlacingZoneTeam1.GetComponent<Renderer>().enabled = false;
        NonePlacingZoneTeam2.GetComponent<Renderer>().enabled = false;
    }

    // Use this for initialization
    void Start() {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name.Equals("Scene1"))
        {
            PUNTutorial.GameManager.instance.SpawnPlayerInTheGame();
            if (PhotonNetwork.isMasterClient)
            {
                photonView.RPC("StartGamePhase", PhotonTargets.AllViaServer);
            }
            Debug.Log("Scene1 detected, spawn the player, no need to wait for other player to load map");
        }
    }

    // Update is called once per frame
    void Update() {
        //Si tous les joueurs on chargé la carte, et que notre joueur n'a pas déjà été spawn, on le spawn
        if(!allPlayerHaveGeneratedMap && nbPlayerHaveGeneratedMap == nbMaxPlayer)
        {
            Debug.Log("All player have generated the map");
            PUNTutorial.GameManager.instance.SpawnPlayerInTheGame();
            allPlayerHaveGeneratedMap = true;
            if (PhotonNetwork.isMasterClient)
            {
                photonView.RPC("StartGamePhase", PhotonTargets.AllViaServer);
            }
        }
        if(PhotonNetwork.isMasterClient)
        {
            if (waitForStart)
            {
                if (Time.time > startTimePhase + waitForStartTime || nbPlayerReady == nbMaxPlayer)
                {
                    waitForStart = false;
                    photonView.RPC("ResetPhase", PhotonTargets.AllViaServer);
                    photonView.RPC("StratPhase", PhotonTargets.AllViaServer);
                }
            }
            else if (stratPhase)
            {
                if (Time.time > startTimePhase + stratPhaseTime || nbPlayerReady == nbMaxPlayer)
                {
                    stratPhase = false;
                    photonView.RPC("ResetPhase", PhotonTargets.AllViaServer);
                    photonView.RPC("PlayPhase", PhotonTargets.AllViaServer);
                }
            }
            else if (playPhase)
            {

            }
        } 
    }

    //on lance la phase de départ, on attend quelque sec avant le lancer la partie
    [PunRPC]
    private void StartGamePhase()
    {
        Debug.Log("Starting phase");
        waitForStart = true;
        //on note quand la phase a commencé
        startTimePhase = Time.time;
        //On lance le timer avec la valeur de la phase d'attente
        if (PhotonNetwork.isMasterClient)
        {
            Timer.instance.photonView.RPC("StartCustomCountdownTime", PhotonTargets.AllViaServer, waitForStartTime);
        }
    }

    //on lance la phase de stratégie
    [PunRPC]
    private void StratPhase()
    {
        Debug.Log("Strategic phase");
        //on respawn tous les joueurs
        RoomManager.instance.photonView.RPC("RespawnPlayer", PhotonTargets.All, PhotonNetwork.player.ID, 0.0F);
        //PUNTutorial.GameManager.localPlayer.SetActive(false);
        readyForNewPhase.gameObject.SetActive(true);
        waitForStart = false;
        playPhase = false;
        stratPhase = true;
        //on note quand la phase a commencé
        startTimePhase = Time.time;
        //On note la valeur du timer actuel (le timer de la parti) pour le relancer plus tard, on lui fait 'pause'
        //On lance le timer avec la valeur de la phase d'attente
        gameTimerValueSaved = Timer.instance.GameTimerSaved;
        if (PhotonNetwork.isMasterClient)
        {
            Timer.instance.photonView.RPC("StartCustomCountdownTime", PhotonTargets.AllViaServer, stratPhaseTime);
        }
        //On active le mode stratégie
        SwitchPlayerMode();
        if(NonePlacingZoneTeam1 != null && NonePlacingZoneTeam2 != null)
        {
            int playerTeam = PUNTutorial.GameManager.localPlayer.GetComponent<PlayerController>().Team;
            if(playerTeam == 1)
            {
                NonePlacingZoneTeam1.SetActive(true);
            }
            if (playerTeam == 2)
            {
                NonePlacingZoneTeam2.SetActive(true);
            }
        }
    }

    //on lance la phase de jeu (manche)
    [PunRPC]
    private void PlayPhase()
    {
        Debug.Log("Playing phase");
        RoomManager.instance.photonView.RPC("RespawnPlayer", PhotonTargets.All, PhotonNetwork.player.ID, 0.0F);
        readyForNewPhase.gameObject.SetActive(false);
        stratPhase = false;
        playPhase = true;
        //on note quand la phase a commencé
        startTimePhase = Time.time;
        //On enlève le mode stratégie
        if (NonePlacingZoneTeam1 != null && NonePlacingZoneTeam2 != null)
        {
            NonePlacingZoneTeam1.SetActive(false);
            NonePlacingZoneTeam2.SetActive(false);
        }
        SwitchPlayerMode();
        //si la partie n'est pas commencé on lance le timer de la partie avec le temps max
        if (gameStarted == false)
        {
            gameStarted = true;
            if(PhotonNetwork.isMasterClient)
            {
                Timer.instance.photonView.RPC("StartCountdownTime", PhotonTargets.AllViaServer);
            }
        }
        else
        {
            //sinon on reprend le timer de la parti où il s'etait arreté
            if (PhotonNetwork.isMasterClient)
            {
                Timer.instance.photonView.RPC("StartCustomCountdownTime", PhotonTargets.AllViaServer, gameTimerValueSaved);
            }
        } 
    }

    //but marqué, on respawn les joueurs, et on lance la phase de stratégie
    public void GoalMarked()
    {
        playPhase = false;
        if (PhotonNetwork.isMasterClient)
        {
            photonView.RPC("StratPhase", PhotonTargets.AllViaServer);
        }
    }

    public bool IsInPlayPhase()
    {
        return playPhase;
    }

    public bool IsInWaitForStartPhase()
    {
        return waitForStart;
    }

    [PunRPC]
    public void RespawnPlayer(int playerID, float timeBeforeRespawn)
    {
        bool needRespawn = true;
        GameObject player = allPlayer[playerID];
        //Debug.Log("player found : " + player.name);
        if(player.GetActive())
        {
            player.SetActive(false);
        }
        PlayerController playerControllerScript = player.GetComponent<PlayerController>();
        while (needRespawn)
        {
            if (playerControllerScript != null)
            {
                GameObject[] spawnSlots = null;
                if (playerControllerScript.Team == 1)
                {
                    //Debug.Log("Player is in team one, will respawn in team 1 spawn");
                    SpawnPoint spawnPointScript = respawnTeam1.GetComponent<SpawnPoint>();
                    if (spawnPointScript != null)
                    {
                        spawnSlots = spawnPointScript.SpawnSlotsAvailiable;
                    }
                    else
                    {
                        Debug.Log("Respawn point does not have a SpawnPoint script");
                    }
                }
                else if (playerControllerScript.Team == 2)
                {
                    SpawnPoint spawnPointScript = respawnTeam2.GetComponent<SpawnPoint>();
                    //Debug.Log("Player is in team two, will respawn in team 2 spawn");
                    if (spawnPointScript != null)
                    {
                        spawnSlots = spawnPointScript.SpawnSlotsAvailiable;
                    }
                    else
                    {
                        Debug.Log("Respawn point does not have a SpawnPoint script");
                    }
                }
                else
                {
                    Debug.Log("Error Player have no team");
                }
                if (spawnSlots != null && spawnSlots.Length != 0)
                {
                    int randromInt = Random.Range(0, spawnSlots.Length);
                    GameObject spawn = spawnSlots[randromInt];
                    Vector3 tpToSpawnPos = spawn.transform.position;
                    tpToSpawnPos.y += respawnHeight;
                    player.transform.position = tpToSpawnPos;
                    player.transform.rotation = spawn.transform.rotation;
                    StartCoroutine(RespawnCoroutine(player, timeBeforeRespawn));
                    needRespawn = false;
                }
                else
                {
                    Debug.Log("Error no spawn slot avaiable");
                }
            }
            else
            {
                Debug.Log("player don't have PlayerController script");
            }
        }
    }
    [PunRPC]
    public void RespawnPlayer(int playerID)
    {
        this.RespawnPlayer(playerID, this.respawnTime);
    }

    private IEnumerator RespawnCoroutine(GameObject player, float timeBeforRespawn)
    {
        yield return new WaitForSeconds(timeBeforRespawn);

        //reset la vie au dessus du perso en reseau
        Debug.Log(player.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value);
        player.GetComponent<PUNTutorial.HealthScript>().photonView.RPC("ResetHealth", PhotonTargets.All);
        Debug.Log(player.GetComponent<PUNTutorial.HealthScript>().HealthSlider.value);
        //reset la vie du client local en haut a gauche
        player.GetComponent<PUNTutorial.HealthScript2>().ResetHealth();
        //on reactive le player
        player.SetActive(true);
        //Debug.Log("Player: "+ player.name + " respawned");
    }

    [PunRPC]
    public void JoinTeam1(int playerID, int playerViewID)
    {
        GameObject player = PhotonView.Find(playerViewID).gameObject;
        PlayerController playerControllerScript = player.GetComponent<PlayerController>();
        if (playerControllerScript != null)
        {
            playerControllerScript.Team = 1;
            this.team1.Add(player);
            this.allPlayer.Add(playerID, player);
            //Debug.Log("Player : " + player.name + " Joined team 1");

            if(playerControllerScript.photonView.isMine)
            {
                GameObject.Find("GlobalUI").GetComponent<ScoreUpdate>().SetScoreColor(1);
            }
        }
        else
        {
            Debug.Log("Error in joining team, player don't have PlayerController script");
        }
    }

    [PunRPC]
    public void JoinTeam2(int playerID, int playerViewID)
    {
        GameObject player = PhotonView.Find(playerViewID).gameObject;
        PlayerController playerControllerScript = player.GetComponent<PlayerController>();
        if (playerControllerScript != null)
        {
            playerControllerScript.Team = 2;
            this.team2.Add(player);
            this.allPlayer.Add(playerID, player);
            //Debug.Log("Player : "+ player.name + " Joined team 2");

            if (playerControllerScript.photonView.isMine)
            {
                GameObject.Find("GlobalUI").GetComponent<ScoreUpdate>().SetScoreColor(2);
                Debug.Log("RoomManager - SCORE COLOR CHANGED : 2");
            }
        }
        else
        {
            Debug.Log("Error in joining team, player d'ont have PlayerController script");
        }
    }

    [PunRPC]
    public void AutoJoinTeam(int playerID, int playerViewID)
    {
        if (this.team1.Count <= this.team2.Count)
        {
            JoinTeam1(playerID, playerViewID);
        }
        else
        {
            JoinTeam2(playerID, playerViewID);
        }
    }

    [PunRPC]
    public void RemoveFromTeam(int playerID)
    {
        GameObject player = allPlayer[playerID];
        PlayerController playerControllerScript = player.GetComponent<PlayerController>();
        if (playerControllerScript != null)
        {
            int playerTeam = playerControllerScript.team;
            if(playerTeam == 1)
            {
                this.team1.Remove(player);
            }
            else if (playerTeam == 2)
            {
                this.team2.Remove(player);
            }
        }
        else
        {
            Debug.Log("Error in leaving team, player d'ont have PlayerController script");
        }
    }

    public bool FriendlyFire
    {
        get
        {
            return this.friendlyFire;
        }
    }

    public void FriendlyFireOn()
    {
        this.friendlyFire = true;
    }

    public void FriendlyFireOff()
    {
        this.friendlyFire = false;
    }

    [PunRPC]
    public int[][] GenerateSeed()
    {
        //return MapGeneration.instance.GenerateSeed3x3();
        //return MapGeneration.instance.GenerateSeed5x5();
        return MapGeneration.instance.GenerateSeed11x11();
    }

    [PunRPC]
    public void SetMapSize(int nombLigne, int nombColonne)
    {
        MapGeneration mapGen = MapGeneration.instance;
        mapGen.NbLigne = nombLigne;
        mapGen.NbColonne = nombColonne;
    }

    [PunRPC]
    public void GenerateMap(int[][] seeds)
    {
        //Debug.Log("GenerateMap, seed length : " +seeds.Length);
        //MapGeneration.instance.ThreeByThreeGeneration(seeds);
        //MapGeneration.instance.FiveByFiveGeneration(seeds);
        MapGeneration.instance.ElevenByElevenGeneration(seeds);
        MapGeneration mapGen = MapGeneration.instance;
        int sizeMultiplicatorZ = (2 * mapGen.NbLigne -1) + 2;
        int sizeMultiplicatorX = (2 * mapGen.NbColonne + 1) + 2;
        Boundary.instance.SetSize(sizeMultiplicatorX * 11, 20, sizeMultiplicatorZ * 11);
        if(NonePlacingZoneTeam1 != null && NonePlacingZoneTeam2 != null)
        {
            //On set la taille des zones ou on ne peut poser de piège par rapport à la taille de la map
            NonePlacingZoneTeam1.transform.localScale = new Vector3(sizeMultiplicatorX * 11, 5, mapGen.NbLigne * 11);
            NonePlacingZoneTeam2.transform.localScale = new Vector3(sizeMultiplicatorX * 11, 5, mapGen.NbLigne * 11);
            //On set leurs positions : partie haute / basse de la map
            NonePlacingZoneTeam1.transform.position = new Vector3(0, 2, (mapGen.NbLigne * 11 / 2));
            NonePlacingZoneTeam2.transform.position = new Vector3(0, 2, -(mapGen.NbLigne * 11 / 2));
            //on les desactives, et on activera quand il le faut
            NonePlacingZoneTeam1.SetActive(false);
            NonePlacingZoneTeam2.SetActive(false);

            NonePlacingZoneTeam1.GetComponent<Renderer>().enabled = true;
            NonePlacingZoneTeam2.GetComponent<Renderer>().enabled = true;
        }
        //On notifie a tout le monde que l'on a chargé la carte
        photonView.RPC("NewPlayerHaveGenMap", PhotonTargets.All);
    }
    [PunRPC]
    public void LoadLevel(string levelToLoad)
    {
        PhotonNetwork.LoadLevel(levelToLoad);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);
        Debug.Log("Player photon disconnected");
        RemoveFromTeam(otherPlayer.ID);
    }

    //méthode pour incrémenter le compteur de nombre de joueur ayant chargé la map
    [PunRPC]
    private void NewPlayerHaveGenMap()
    {
        Debug.Log("new player have generated the map");
        nbPlayerHaveGeneratedMap++;
    }


    public void Ready()
    {
        if (iAmReady)
        {
            iAmReady = false;
            if (PhotonNetwork.connected)
            {
                photonView.RPC("NewPlayerUnRdy", PhotonTargets.AllViaServer);
            }
            else
            {
                NewPlayerUnRdy();
            }
            if (readyForNewPhase != null)
            {
                readyForNewPhase.GetComponent<Image>().color = Color.red;
            }
            else
            {
                Debug.Log("Party manage miss the button ready for new phase");
            }
        }
        else
        {
            iAmReady = true;
            //Debug.Log("New player ready");
            //augmente le nombre de joueur pret pour la prochaine phase (en reseau et en local)
            if (PhotonNetwork.connected)
            {
                photonView.RPC("NewPlayerRdy", PhotonTargets.AllViaServer);
            }
            else
            {
                NewPlayerRdy();
            }
            //modification visuel du bouton pret -> rouge = pas pret / vert = pret
            if (readyForNewPhase != null)
            {
                readyForNewPhase.GetComponent<Image>().color = Color.green;
            }
            else
            {
                Debug.Log("Party manage miss the button ready for new phase");
            }
        }
    }


    [PunRPC]
    private void NewPlayerRdy()
    {
        nbPlayerReady++;
        //Debug.Log("nb player ready / max player : " + nbPlayerReady + " / " + nbMaxPlayer);
        UpdateReadyButtonText();
    }

    [PunRPC]
    private void NewPlayerUnRdy()
    {
        nbPlayerReady--;
        //Debug.Log("nb player ready / max player : " + nbPlayerReady + " / " + nbMaxPlayer);
        UpdateReadyButtonText();
    }

    //reset la parametre au changement de phase
    [PunRPC]
    private void ResetPhase()
    {
        iAmReady = false;
        nbPlayerReady = 0;
        readyForNewPhase.GetComponent<Image>().color = Color.red;
        UpdateReadyButtonText();
    }

    private void UpdateReadyButtonText()
    {
        if (readyForNewPhase != null)
        {
            readyForNewPhase.GetComponentInChildren<Text>().text = "Ready ( " + nbPlayerReady + " / " + nbMaxPlayer + " )";
        }
        else
        {
            Debug.Log("No ready for next phase button");
        }
    }

    //Change entre mode normale et mode stratégique
    [PunRPC]
    private void SwitchPlayerMode()
    {
        //on choppe le joueur local
        GameObject localPlayer = PUNTutorial.GameManager.localPlayer;
        //changement sur le script PlayerController
        localPlayer.GetComponent<PlayerController>().SwitchPlayerMode();
        //changement sur le script PGlace
        localPlayer.GetComponent<PGlace>().SwitchPlayerMode();
        //changement sur le script HealthTag
        localPlayer.GetComponent<HealthTag>().SwitchPlayerMode();
        //changement sur le script CameraFollow
        Camera.main.transform.parent.GetComponent<CameraFollow>().SwitchPlayerMode();
    }
}
