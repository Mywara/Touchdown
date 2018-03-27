using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaunchGameWhenReady : Photon.PUNBehaviour {

    public Button readyForNewPhase;
    public GameObject piratePreview;
    public GameObject undeadPreview;
    public GameObject warBearPreview;
    public Transform team1List;
    public Transform team2List;
    public GameObject playerNamePrefab;
    public Button joinTeam1;
    public Button joinTeam2;
    public Button resetTeam;

    private Dictionary<int, GameObject> playerNameList = new Dictionary<int, GameObject>();
    private int nbMaxPlayer;
    private int nbPlayerReady;
    private bool iAmReady = false;
    private PUNTutorial.GameManager gameManager;
    private bool gameLaunch = false;

    private void Awake()
    {
        //on va chercher le nombre de joueur max dans la room
        if (PhotonNetwork.connected)
        {
            nbMaxPlayer = PhotonNetwork.room.MaxPlayers;
        }
        //on initialise le bouton a rouge
        readyForNewPhase.GetComponent<Image>().color = Color.red;
        UpdateReadyButtonText();
        gameManager = PUNTutorial.GameManager.instance;
    }

    // Use this for initialization
    void Start()
    {
        piratePreview.SetActive(false);
        undeadPreview.SetActive(false);
        warBearPreview.SetActive(false);
        joinTeam1.onClick.AddListener(delegate { ButtonToSwitchTeam(1, PhotonNetwork.player.ID, PlayerPrefs.GetString("PlayerName")); });
        joinTeam2.onClick.AddListener(delegate { ButtonToSwitchTeam(2, PhotonNetwork.player.ID, PlayerPrefs.GetString("PlayerName")); });
        resetTeam.onClick.AddListener(delegate { ResetTeam(); });
    }

    // Update is called once per frame
    void Update()
    {
        //si on est pas le masterclient, on ne fait rien
        if(!PhotonNetwork.isMasterClient)
        {
            return;
        }
        //Si tous les joueurs sont rdy, on charge le niveau
        if(nbPlayerReady == nbMaxPlayer && !gameLaunch)
        {
            gameLaunch = true;
            //PUNTutorial.GameManager.instance.JoinGame();
            photonView.RPC("LoadLevel", PhotonTargets.AllViaServer, gameManager.LevelToLoad);
        }
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

    public void SelectCharacterPirate()
    {
       gameManager.CharacterToLoad = "Pirate";
        piratePreview.SetActive(true);
        undeadPreview.SetActive(false);
        warBearPreview.SetActive(false);
    }

    public void SelectCharacterUndeath()
    {
        gameManager.CharacterToLoad = "Undeath";
        piratePreview.SetActive(false);
        undeadPreview.SetActive(true);
        warBearPreview.SetActive(false);
    }

    public void SelectCharacterWarBear()
    {
        gameManager.CharacterToLoad = "War_Bear";
        piratePreview.SetActive(false);
        undeadPreview.SetActive(false);
        warBearPreview.SetActive(true);
    }

    public void SelectLevelScene1()
    {
        gameManager.LevelToLoad = "Scene1";
    }

    public void SelectLevelRandomMap()
    {
        gameManager.LevelToLoad = "RandomMap";
    }

    [PunRPC]
    public void LoadLevel(string theLevelToLoad)
    {
        PhotonNetwork.LoadLevel(theLevelToLoad);
    }

    private void ButtonToSwitchTeam(int newTeam, int playerID, string playerName)
    {
        photonView.RPC("SwitchTeam", PhotonTargets.AllBuffered, newTeam, playerID, playerName);
        SwitchTeamIWantToJoin(newTeam);
    }

    [PunRPC]
    private void SwitchTeam(int newTeam, int playerID, string playerName)
    {
        //Debug.Log("switch team, newteam : " + newTeam);
        if (!playerNameList.ContainsKey(playerID))
        {
            GameObject textCreated = Instantiate(playerNamePrefab, Vector3.zero, Quaternion.identity);
            textCreated.GetComponent<Text>().text = playerName;
            playerNameList.Add(playerID, textCreated);
        }
        if(newTeam == 1)
        {
            JoinTeam1(playerNameList[playerID]);
        }
        else if (newTeam == 2)
        {
            JoinTeam2(playerNameList[playerID]);
        }
    }

    private void JoinTeam1(GameObject playerNameinListInstance)
    {
        //Debug.Log("Joined Team 1");
        playerNameinListInstance.transform.position = Vector3.zero;
        playerNameinListInstance.transform.SetParent(team1List);      
    }

    private void JoinTeam2(GameObject playerNameinListInstance)
    {
        //Debug.Log("Joined Team 2");
        playerNameinListInstance.transform.position = Vector3.zero;
        playerNameinListInstance.transform.SetParent(team2List);
    }

    private void SwitchTeamIWantToJoin(int teamNumber)
    {
        if(teamNumber == 1)
        {
            PUNTutorial.GameManager.teamIwantToJoin = 1;
        }
        else if (teamNumber == 2)
        {
            PUNTutorial.GameManager.teamIwantToJoin = 2;
        }
    }

    private void ResetTeam()
    {
        playerNameList[PhotonNetwork.player.ID].transform.position = Vector3.zero;
        playerNameList[PhotonNetwork.player.ID].transform.SetParent(null);
        PUNTutorial.GameManager.teamIwantToJoin = 0;
    }
}
