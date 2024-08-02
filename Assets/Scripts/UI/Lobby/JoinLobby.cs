using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class JoinLobby : MonoBehaviour
{
    public GameObject QuickJoinPanel;
    public TMP_InputField lobbyCodeArea;

    public async void JoinLobbyWithLobbyId(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
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
                {
                    "readyCount",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "0")
                },
                {"RelayClientId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "")}   
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            Debug.Log("Join Lobby Done:" + lobby.Name);
            CreateLobby.LoginToVivoxAsync();

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;
            GetComponent<CurrentLobby>().thisPlayer = options.Player;
            LoadLobbyRoom();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinLobbyWithLobbyCode()
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
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
                {
                    "readyCount",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "0")
                },
                {"RelayClientId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "")}   
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(
                lobbyCodeArea.text,
                options
            );
            Debug.Log("Join Lobby Done:" + lobby.Name);
            CreateLobby.LoginToVivoxAsync();

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;
            GetComponent<CurrentLobby>().thisPlayer = options.Player;
            LoadLobbyRoom();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            options.Player = new Player(AuthenticationService.Instance.PlayerId);
            options.Player.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "PlayerName",
                    new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Public,
                        PlayerPrefs.GetString("PlayerName")
                    )
                },{
                    "readyCount",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "0")
                },
                {"RelayClientId", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "")}   
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            CreateLobby.LoginToVivoxAsync();

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;
            GetComponent<CurrentLobby>().thisPlayer = options.Player;
            LoadLobbyRoom();

            if (lobby != null)
            {
                QuickJoinPanel.SetActive(false);
            }
            Debug.Log("Join Lobby Done:" + lobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public static void LoadLobbyRoom()
    {
        SceneManager.LoadScene(3);
        Debug.Log("Lobby");
    }

    public static void ReturnMainMenu()
    {
        Debug.Log("ReturnMenu");
        SceneManager.LoadScene(2);
    }

    public static void LoadGame()
    {
        Debug.Log("LoadGame");
        SceneManager.LoadScene(4);
    }
}
