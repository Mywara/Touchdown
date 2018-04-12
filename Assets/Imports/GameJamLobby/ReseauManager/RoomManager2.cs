using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager2 : Photon.PunBehaviour {
    
    static public RoomManager2 instance;

    public Text roomActvies;
    public Text roomNameText;
    public Text numberText;
    public string LevelToLoad = "SceneTest";

    /*
    public void Start()
    {
        PhotonNetwork.ConnectUsingSettings("Version_1.20");
    }
    */
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        //PhotonNetwork.automaticallySyncScene = true;
        RefreshRoom();

        // Défige le curseur 
        Cursor.lockState = CursorLockMode.None;
        // Rend le curseur visible
        Cursor.visible = true;
    }


    public void Update()
    {
  
    }

    public void CreateRoom()
    {
        string name = roomNameText.text;
        RoomOptions ro = new RoomOptions();

        int i = 0;
        string player_nbr = numberText.text;
        System.Int32.TryParse(numberText.text, out i);
        if (i == 0 || !char.IsDigit(player_nbr, 0))
        {
            i = 1;
        }
        else if (i > 20)
        {
            i = 20;
        }
        ro.MaxPlayers = (byte)i;
        PhotonNetwork.JoinOrCreateRoom(name, ro, TypedLobby.Default);
    }

    public void JoinRoom()
    {
        string name = roomNameText.text;
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 4;
        PhotonNetwork.JoinRoom(name);
    }

    public override void OnJoinedRoom()
    {
        /*
         * //if auotSyncScene is on, just chang the scene on the master, then photon auto sync the scene
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel("StartScene");
        }
        */
        PhotonNetwork.LoadLevel("StartScene");
    }


    public void RefreshRoom()
    {
        roomActvies.text = "Active Rooms";
        foreach (RoomInfo game in PhotonNetwork.GetRoomList())
        {
            roomActvies.text = roomActvies.text + "\n Room name : " + game.Name + "  Player :  " + game.PlayerCount + "/" + game.MaxPlayers;
        }
    }
}
