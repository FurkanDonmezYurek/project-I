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

public class JoinLobby : MonoBehaviour
{
    public GameObject QuickJoinPanel;

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
                }
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            Debug.Log("Join Lobby Done:" + lobby.Name);

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;
            GetComponent<CurrentLobby>().thisPlayer = options.Player;
            LoadLobbyRoom();

            // JoinRelay(lobby.LobbyCode);
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
                }
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;
            GetComponent<CurrentLobby>().thisPlayer = options.Player;
            LoadLobbyRoom();

            // JoinRelay(lobby.LobbyCode);

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public static void ReturnMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public static void LoadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}