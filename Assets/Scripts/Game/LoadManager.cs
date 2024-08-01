using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LoadManager : NetworkBehaviour
{
    CurrentLobby currentLobby;
    RelayManager relayManager;
    public bool gameStarted = false;
    public GameObject loadingPanel;
    RoleAssignment roleAssignment;
    public GameObject playerPrefab;
    
    public List<Transform> spawnPoints;
    public GameObject gameUi;

    void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        relayManager = GameObject.Find("RelayManager").GetComponent<RelayManager>();
        loadingPanel = GameObject.Find("LoadingCanvas");

        if (currentLobby.thisPlayer.Id == currentLobby.currentLobby.HostId)
        {
            Debug.Log("Current player is the host.");
            relayManager.OnHostClick();
        }
        else
        {
            Debug.Log("Current player is joining.");
            relayManager.OnJoinClick();
        }
    }
    
    // public override void OnNetworkSpawn()
    // {
    //     NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    // }
    //
    // private void OnClientConnected(ulong clientId)
    // {
    //     foreach (Player player in currentLobby.currentLobby.Players)
    //     {
    //         Debug.Log(clientId.ToString());
    //         Debug.Log(player.Data["RelayClientId"].Value); 
    //         if (clientId.ToString() == player.Data["RelayClientId"].Value)
    //         {
    //             NetworkObject netObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
    //             netObj.transform.name = player.Data["PlayerName"].Value;
    //             Debug.Log( "name: "+netObj.transform.name);
    //         }
    //     }
    // }
    //
    // private Transform GetRandomSpawnPoint()
    // {
    //     int index = Random.Range(0, spawnPoints.Count);
    //     return spawnPoints[index];
    // }

    private void Update()
    {
        if (IsHost)
        {
            roleAssignment = GameObject
                .Find(currentLobby.thisPlayer.Data["PlayerName"].Value)
                .GetComponent<RoleAssignment>();

            if (
                NetworkManager.Singleton.ConnectedClientsList.Count
                == currentLobby.currentLobby.Players.Count
            )
            {
                if (!gameStarted)
                {
                    Debug.Log("All clients are connected. Starting the game.");
                    gameStarted = true;
                    roleAssignment.GetLobbyData();
                    loadingPanel.SetActive(false);
                    loadGameClientRpc();
                }
            }
        }
    }

    [ClientRpc]
    public void loadGameClientRpc()
    {
        Debug.Log("Game is starting for all clients.");
        gameStarted = true;
        loadingPanel.SetActive(false);
    }
}