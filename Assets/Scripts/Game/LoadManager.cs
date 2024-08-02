using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerInfoItem
{
    public string Name { get; set; }
    public ulong ClientID { get; set; }

    public PlayerInfoItem(string name, ulong clientId)
    {
        Name = name;
        ClientID = clientId;
    }
}

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

    public List<PlayerInfoItem> playerInfoItems = new List<PlayerInfoItem>();

    public string playerInfoJson = "";

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

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (ulong _) =>
        {
            playerInfoItems = GameObject
                .FindGameObjectsWithTag("Player")
                .Select(
                    elm =>
                        new PlayerInfoItem(
                            name: elm.gameObject.name,
                            clientId: elm.gameObject.GetComponent<NetworkObject>().OwnerClientId
                        )
                )
                .ToList();

            playerInfoJson = "";
            string json = JsonConvert.SerializeObject(playerInfoItems);
            playerInfoJson = json;

            UpdateStrClientRpc(playerInfoJson);
        };
    }

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

    [ClientRpc]
    void UpdateStrClientRpc(string newValue)
    {
        playerInfoJson = newValue;

        Debug.Log("Client JSON:");
        Debug.Log(playerInfoJson);

        var list =
            (List<PlayerInfoItem>)
                JsonConvert.DeserializeObject(playerInfoJson, typeof(List<PlayerInfoItem>));
        playerInfoItems = list;
        var playerObjectList = GameObject.FindGameObjectsWithTag("Player");
        foreach (var info in playerInfoItems)
        {
            foreach (var obj in playerObjectList)
            {
                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                if (netObj.OwnerClientId == info.ClientID)
                {
                    obj.name = info.Name;
                }
            }
        }

        // Debug.Log("liste oldu mu.");
        // Debug.Log(tempStr);
    }
}
