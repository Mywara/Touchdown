using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PUNTutorial
{
    public class GameManager : Photon.PunBehaviour
    {

        public static GameManager instance;
        public static GameObject localPlayer;
        public static float teamIwantToJoin = 0;

        private string characterToLoad = "Pirate";
        private string levelToLoad = "RandomMap";
        private int[][] seeds = null;

        void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            instance = this;
            //PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.automaticallySyncScene = false;

            // Défige le curseur 
            Cursor.lockState = CursorLockMode.None;
            // Rend le curseur visible
            Cursor.visible = true;
        }

        void Start()
        {
            PhotonNetwork.ConnectUsingSettings("Version_1.20");
        }

        public void JoinGame()
        {
            //PhotonNetwork.LoadLevel(levelToLoad);

            if (PhotonNetwork.isMasterClient)
            {
                photonView.RPC("LoadLevel", PhotonTargets.AllViaServer, levelToLoad);
            }

            /* RoomOptions ro = new RoomOptions();
             ro.MaxPlayers = 6;
             PhotonNetwork.JoinOrCreateRoom("Default Room", ro, null);*/
        }

        /*
        public override void OnJoinedRoom()
        {
            
            if (PhotonNetwork.isMasterClient)
            {
                //PhotonNetwork.LoadLevel(levelToLoad);
                photonView.RPC("LoadLevel", PhotonTargets.AllViaServer,levelToLoad);
            }
            
        }
        */

        [PunRPC]
        public void LoadLevel(string theLevelToLoad)
        {
            PhotonNetwork.LoadLevel(theLevelToLoad);
        }
        //
        
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "EndScene" && scene.name != "StartScene")
            { //level 2 is end level, photonView don't exist
                if (!PhotonNetwork.inRoom) return;
                if (levelToLoad.Equals("RandomMap") && PhotonNetwork.isMasterClient)
                {
                    int nbPlayerMaxInRoom = PhotonNetwork.room.MaxPlayers;
                    int nbLigne = 5;
                    int nbColonne = 3;
                    switch (nbPlayerMaxInRoom)
                    {
                        case 1:
                            {
                                nbLigne = 3;
                                nbColonne = 1;
                                break;
                            }
                        case 2:
                            {
                                nbLigne = 3;
                                nbColonne = 1;
                                break;
                            }
                        case 3:
                            {
                                nbLigne = 3;
                                nbColonne = 2;
                                break;
                            }
                        case 4:
                            {
                                nbLigne = 3;
                                nbColonne = 2;
                                break;
                            }
                        case 5:
                            {
                                nbLigne = 4;
                                nbColonne = 2;
                                break;
                            }
                        case 6:
                            {
                                nbLigne = 4;
                                nbColonne = 2;
                                break;
                            }
                        case 7:
                            {
                                nbLigne = 4;
                                nbColonne = 3;
                                break;
                            }
                        case 8:
                            {
                                nbLigne = 4;
                                nbColonne = 3;
                                break;
                            }
                        case 9:
                            {
                                nbLigne = 5;
                                nbColonne = 3;
                                break;
                            }
                        case 10:
                            {
                                nbLigne = 5;
                                nbColonne = 3;
                                break;
                            }
                        default:
                            {
                                nbLigne = 5;
                                nbColonne = 3;
                                break;
                            }
                    }
                    RoomManager.instance.photonView.RPC("SetMapSize", PhotonTargets.AllBuffered, nbLigne, nbColonne);
                    seeds = RoomManager.instance.GenerateSeed();
                    RoomManager.instance.photonView.RPC("GenerateMap", PhotonTargets.AllBufferedViaServer, seeds);
                }
            }
        }
        

        //Méthode pour faire spawn le joueur dans la partie != de respawn
        //ici on creer le joueur pour la première fois
        public void SpawnPlayerInTheGame()
        {
            //localPlayer = PhotonNetwork.Instantiate("TempPlayer", new Vector3(0, 0.5f, 0), Quaternion.identity, 0);
            localPlayer = PhotonNetwork.Instantiate(characterToLoad, new Vector3(0, 20f, 0), Quaternion.identity, 0);
            localPlayer.SetActive(false);
            if(teamIwantToJoin == 0)
            {
                RoomManager.instance.photonView.RPC("AutoJoinTeam", PhotonTargets.AllBufferedViaServer, PhotonNetwork.player.ID, localPlayer.GetPhotonView().viewID);
            }
            else if (teamIwantToJoin == 1)
            {
                RoomManager.instance.photonView.RPC("JoinTeam1", PhotonTargets.AllBufferedViaServer, PhotonNetwork.player.ID, localPlayer.GetPhotonView().viewID);
            }
            else if (teamIwantToJoin == 2)
            {
                RoomManager.instance.photonView.RPC("JoinTeam2", PhotonTargets.AllBufferedViaServer, PhotonNetwork.player.ID, localPlayer.GetPhotonView().viewID);
            }
            RoomManager.instance.photonView.RPC("RespawnPlayer", PhotonTargets.AllViaServer, PhotonNetwork.player.ID, 0.0F);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void ToLobby()
        {
            PhotonNetwork.LoadLevel("Lobby");
        }

        public string CharacterToLoad
        {
            get
            {
                return this.characterToLoad;
            }
            set
            {
                this.characterToLoad = value;
            }
        }

        public string LevelToLoad
        {
            get
            {
                return this.levelToLoad;
            }
            set
            {
                this.levelToLoad = value;
            }
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.LoadLevel("Lobby");
        }
    }
}
