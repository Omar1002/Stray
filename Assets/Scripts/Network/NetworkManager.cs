﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class NetworkManager : Photon.PunBehaviour {

	const string VERSION = "v0.0.1";

	public string roomName = "Stray";
    public int m_maxPlayersPerRoom = 5;

    public int roomNumber;
    

	public GameObject playerPrefabName;
	public Camera camera;
	public Transform[] spawnPoints;
    public int m_playersInRoom = 0;
    //public Transform spawnPoint1;

    private GameObject m_persistentData;

    //Persistent data transferral variables (Used to get the users room name)


    void Awake()
    {
        if (!GameObject.Find("PersistentDataGO"))
        {
            m_persistentData = new GameObject("PersistentDataGO");
            m_persistentData.AddComponent<PersistentData>();
        }
        else
        {
            GameObject.Find("PersistentDataGO");
        }
    }


	void Start () 
	{
		PhotonNetwork.ConnectUsingSettings (VERSION);//Connect to the lobby using the given settings
	}




    void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        PhotonNetwork.JoinRandomRoom();
    }


   

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("OnPhotonRandomJoinFailed");
        PhotonNetwork.CreateRoom(null, new RoomOptions(){maxPlayers = (byte)m_maxPlayersPerRoom},TypedLobby.Default);
    }




	void OnJoinedRoom()
	{
		//When you join the room, you create a clone of the Player prefab, and enable the Character Controller, FirstPersonController, Camera and Audio Listener.
		//This is done to keep each individual clone independent if being run on the same machine (Excellent for playtesting and debugging)
		Debug.Log ("Room joined");

        m_playersInRoom = PhotonNetwork.playerList.Length - 1;

        GameObject myPlayer = PhotonNetwork.Instantiate (playerPrefabName.name, spawnPoints[m_playersInRoom].position, spawnPoints[m_playersInRoom].rotation, 0);

        //GameObject NPC = PhotonNetwork.Instantiate(NPCname.name, spawnPoint1.position, spawnPoint1.rotation, 0);

		//The following components are disabled by default, to insure clone independence on a single machine.
		myPlayer.GetComponent <CharacterController> ().enabled = true;//Turn on the character controller
		myPlayer.GetComponent <FirstPersonController> ().enabled = true;//Turn on the FirstPersonController script.
		myPlayer.GetComponentInChildren<Camera>().enabled = true;//Turn on the camera
		myPlayer.GetComponentInChildren<AudioListener> ().enabled = true;//Turn on the Audio Listener

	}

}
