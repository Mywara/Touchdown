using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaunchGameWhenReady : Photon.PUNBehaviour {

    public Button readyForNewPhase;
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
    }

    public void SelectCharacterUndeath()
    {
        gameManager.CharacterToLoad = "Undeath";
    }

    public void SelectCharacterWarBear()
    {
        gameManager.CharacterToLoad = "War_Bear";
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
}
