using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LoadManager : NetworkBehaviour
{
    CurrentLobby currentLobby;
    RelayManager relayManager;
    public bool gameStarted = false;
    public GameObject loadingPanel;
    RoleAssignment roleAssignment;
    public GameObject playerPrefab;

    // public List<Transform> spawnPoints;

    // public GameObject gameUi;

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

    // Spawn Noktasi
    // public override void OnNetworkSpawn()
    // {
    //     if (IsServer)
    //     {
    //         NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedClientRpc;
    //     }
    // }

    // [ClientRpc]
    // private void OnClientConnectedClientRpc(ulong clientId)
    // {
    //     Transform spawnPoint = GetRandomSpawnPoint();
    //     spawnPoints.Remove(spawnPoint);
    //     NetworkObject playerInstance = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(
    //         clientId
    //     );
    //     playerInstance.transform.position = spawnPoint.position;
    // }

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
