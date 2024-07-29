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

    // public GameObject gameUi;

    void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        relayManager = GameObject.Find("RelayManager").GetComponent<RelayManager>();
        loadingPanel = GameObject.Find("LoadingCanvas");
        if (currentLobby.thisPlayer.Id == currentLobby.currentLobby.HostId)
        {
            relayManager.OnHostClick();
        }
        else
        {
            relayManager.OnJoinClick();
        }
    }

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
                    gameStarted = true;
                    roleAssignment.GetLobbyData();
                    loadingPanel.SetActive(false);
                    loadGameClientRpc();
                }
            }
        }
        // if (NetworkManager.Singleton.IsConnectedClient)
        // {
        //     if (
        //         NetworkManager.Singleton.ConnectedClientsList.Count
        //         != currentLobby.currentLobby.Players.Count
        //     )
        //     {
        //         gameStarted = false;
        //         loadingPanel.SetActive(true);
        //     }
        //     else
        //     {
        //         if (gameStarted == false)
        //         {
        //             gameStarted = true;
        //             roleAssignment.GetLobbyData();
        //             loadingPanel.SetActive(false);
        //         }
        //     }
        // }
    }

    [ClientRpc]
    public void loadGameClientRpc()
    {
        gameStarted = true;
        loadingPanel.SetActive(false);
    }
}
