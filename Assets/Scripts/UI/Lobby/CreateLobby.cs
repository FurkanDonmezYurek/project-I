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

public class CreateLobby : MonoBehaviour
{
    public TMP_InputField lobbyName;
    public TMP_Dropdown maxPlayers;
    public Toggle isLobbyPrivate;

    async void Start()
    {
        //for SignIn
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateLobbyFunc()
    {
        string lobbyname = lobbyName.text;
        int maxplayers = Convert.ToInt32(maxPlayers.options[maxPlayers.value].text);
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = isLobbyPrivate.isOn;
        options.Player = new Player(AuthenticationService.Instance.PlayerId);

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname, maxplayers, options);

        DontDestroyOnLoad(this);
        GetComponent<CurrentLobby>().currentLobby = lobby;
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
