using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class CreateLobby : MonoBehaviour
{
    public TMP_InputField lobbyName;
    public TMP_Dropdown maxPlayers;
    public Toggle isLobbyPrivate;

    public TMP_Dropdown ghost;
    public TMP_Dropdown wizard;
    public TMP_Dropdown lover;
    public TMP_Dropdown hunter;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        //for SignIn
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobbyFunc()
    {
        string lobbyname = lobbyName.text;
        int maxplayers = Convert.ToInt32(maxPlayers.options[maxPlayers.value].text);
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = isLobbyPrivate.isOn;
        options.Player = new Player(AuthenticationService.Instance.PlayerId);
        options.Player.Data = new Dictionary<string, PlayerDataObject>()
        {
            {
                "PlayerName",
                new PlayerDataObject(
                    PlayerDataObject.VisibilityOptions.Public,
                    PlayerPrefs.GetString("PlayerName")
                )
            },
            { "readyCount", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "1") }
        };

        string villager = Convert.ToString(
            maxplayers
                - (
                    Convert.ToInt32(hunter.options[hunter.value].text)
                    + Convert.ToInt32(wizard.options[wizard.value].text)
                    + Convert.ToInt32(lover.options[lover.value].text)
                    + Convert.ToInt32(hunter.options[hunter.value].text)
                )
        );
        //lobby Role data
        options.Data = new Dictionary<string, DataObject>()
        {
            {
                "ghost",
                new DataObject(
                    DataObject.VisibilityOptions.Public,
                    ghost.options[ghost.value].text,
                    DataObject.IndexOptions.S1
                )
            },
            {
                "wizard",
                new DataObject(
                    DataObject.VisibilityOptions.Public,
                    wizard.options[wizard.value].text,
                    DataObject.IndexOptions.S2
                )
            },
            {
                "lover",
                new DataObject(
                    DataObject.VisibilityOptions.Public,
                    lover.options[lover.value].text,
                    DataObject.IndexOptions.S3
                )
            },
            {
                "hunter",
                new DataObject(
                    DataObject.VisibilityOptions.Public,
                    hunter.options[hunter.value].text,
                    DataObject.IndexOptions.S4
                )
            },
            {
                "villager",
                new DataObject(
                    DataObject.VisibilityOptions.Public,
                    villager,
                    DataObject.IndexOptions.S5
                )
            }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname, maxplayers, options);

        DontDestroyOnLoad(this);
        GetComponent<CurrentLobby>().currentLobby = lobby;
        GetComponent<CurrentLobby>().thisPlayer = options.Player;

        JoinLobby.LoadLobbyRoom();
        Debug.Log("Create Lobby Done");

        StartCoroutine(PingLobbyCoroutine(lobby.Id, 15f));
    }

    IEnumerator PingLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}
