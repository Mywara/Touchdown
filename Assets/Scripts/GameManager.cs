using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PUNTutorial
{
    public class GameManager : Photon.PunBehaviour
    {

        public static GameManager instance;
        public static GameObject localPlayer;

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
            PhotonNetwork.ConnectUsingSettings("Version_1.0");
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
            localPlayer = PhotonNetwork.Instantiate("TempPlayer", new Vector3(0, 0.5f, 0), Quaternion.identity, 0);
        }
    }
}
