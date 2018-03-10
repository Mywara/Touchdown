﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Photon.PunBehaviour {

    static public RoomManager instance;
    public float respawnTime = 5;
    public float respawnHeight = 2;
    public GameObject respawnTeam1;
    public GameObject respawnTeam2;

    private List<GameObject> team1 = new List<GameObject>();
    private List<GameObject> team2 = new List<GameObject>();
    //public List<GameObject> team1 = new List<GameObject>();
    //public List<GameObject> team2 = new List<GameObject>();
    public Dictionary<int, GameObject> allPlayer = new Dictionary<int, GameObject>();
    private bool friendlyFire = false;

    void Awake()
    {

        if (instance != null && instance != this)
        {
            Debug.Log("there is already a RoomManager");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }
    [PunRPC]
    public void RespawnPlayer(int playerID, float timeBeforeRespawn)
    {
        bool needRespawn = true;
        GameObject player = allPlayer[playerID];
        //Debug.Log("player found : " + player.name);
        player.SetActive(false);
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
        return MapGeneration.instance.GenerateSeed5x5();
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
        MapGeneration.instance.FiveByFiveGeneration(seeds);
        MapGeneration mapGen = MapGeneration.instance;
        int sizeMultiplicatorZ = (2 * mapGen.NbLigne -1) + 2;
        int sizeMultiplicatorX = (2 * mapGen.NbColonne + 1) + 2;
        Boundary.instance.SetSize(sizeMultiplicatorX * 5, 20, sizeMultiplicatorZ * 5);
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
}
