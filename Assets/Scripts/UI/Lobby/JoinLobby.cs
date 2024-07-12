using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
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
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            Debug.Log("Join Lobby Done:" + lobby.Name);

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;
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
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            DontDestroyOnLoad(this);
            GetComponent<CurrentLobby>().currentLobby = lobby;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
