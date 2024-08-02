using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class PlayerNameSetter : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            Debug.Log("isOwner true");
            if (IsHost)
            {
                Debug.Log("isHost");
                SetNameServerRpc(PlayerPrefs.GetString("PlayerName"));
            }
            else
            {
                Debug.Log("not host");
                SendNameToServer();
            }
        }
        else
        {
            Debug.Log("not owner");
        }
    }

    [ServerRpc]
    public void SetNameServerRpc(string playerName)
    {
        Debug.Log("Server received name request: " + playerName);
        transform.name = playerName;
        UpdateNameClientRpc(playerName);
    }

    [ClientRpc]
    private void UpdateNameClientRpc(string playerName)
    {
        Debug.Log("Client received name update: " + playerName);
        transform.name = playerName;
    }

    private void SendNameToServer()
    {
        string playerName = PlayerPrefs.GetString("PlayerName");
        if (!string.IsNullOrEmpty(playerName))
        {
            SetNameServerRpc(playerName);
        }
        else
        {
            Debug.LogError("Player name is not set in PlayerPrefs");
        }
    }
}