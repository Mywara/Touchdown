using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PUNTutorial
{
    public class GameManager : Photon.PunBehaviour
    {

        public static GameManager instance;
        public static GameObject localPlayer;

        private string characterToLoad = "Pirate";

        void Awake()
        {
            
            if (instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            instance = this;
            PhotonNetwork.automaticallySyncScene = true;
        }

        void Start()
        {
            PhotonNetwork.ConnectUsingSettings("Version_1.1");
        }

        public void JoinGame()
        {
            RoomOptions ro = new RoomOptions();
            ro.MaxPlayers = 6;
            PhotonNetwork.JoinOrCreateRoom("Default Room", ro, null);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Joined Room");
            if (PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.LoadLevel("Scene1");
            }
        }
        
        void OnLevelWasLoaded(int levelNumber)
        {
            if (!PhotonNetwork.inRoom) return;
            //localPlayer = PhotonNetwork.Instantiate("TempPlayer", new Vector3(0, 0.5f, 0), Quaternion.identity, 0);
            localPlayer = PhotonNetwork.Instantiate(characterToLoad, new Vector3(0, 2f, 0), Quaternion.identity, 0);

          //  RoomManager.instance.photonView.RPC("AutoJoinTeam", PhotonTargets.AllBufferedViaServer, localPlayer.GetPhotonView().viewID);
           // RoomManager.instance.photonView.RPC("RespawnPlayer", PhotonTargets.AllViaServer, localPlayer.GetPhotonView().viewID, 5.0f);       
        }
        

        public void SelectCharacterPirate()
        {
            characterToLoad = "Pirate";
        }

        public void SelectCharacterUndeath()
        {
            characterToLoad = "Undeath";
        }

        public void SelectCharacterWarBear()
        {
            characterToLoad = "War_Bear";
        }
    }
}
