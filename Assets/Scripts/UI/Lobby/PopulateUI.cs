using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Netcode;
using System;

public class PopulateUI : NetworkBehaviour
{
    public TextMeshProUGUI playerName;
    public int playerCount = 0;

    public GameObject privImage;
    public TextMeshProUGUI lobbyName;
    public TextMeshProUGUI ghostCount;
    public TextMeshProUGUI hunterCount;
    public TextMeshProUGUI wizardCount;
    public TextMeshProUGUI loverCount;
    public TextMeshProUGUI villagerCount;

    public TextMeshProUGUI startButtonText;
    private CurrentLobby currentLobby;

    public GameObject playerCardPrefab;
    public GameObject playerListContainer;
    Player hostPlayer;

    public int readyCount = 0;
    public bool setReady = true;
    bool isHost;

    RelayManager relayManager;

    private void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        relayManager = GameObject.Find("RelayManager").GetComponent<RelayManager>();
        PopulateUIElements();
        InvokeRepeating(nameof(UpdateLobby), 1.1f, 2f);
    }

    void PopulateUIElements()
    {
        if (!currentLobby.currentLobby.IsPrivate)
        {
            privImage.SetActive(false);
        }
        playerName.text = currentLobby.thisPlayer.Data["PlayerName"].Value;
        lobbyName.text = currentLobby.currentLobby.Name;
        ghostCount.text = currentLobby.currentLobby.Data["ghost"].Value;
        hunterCount.text = currentLobby.currentLobby.Data["hunter"].Value;
        wizardCount.text = currentLobby.currentLobby.Data["wizard"].Value;
        loverCount.text = currentLobby.currentLobby.Data["lover"].Value;
        villagerCount.text = currentLobby.currentLobby.Data["villager"].Value;

        CleanerContainer();

        foreach (Player player in currentLobby.currentLobby.Players)
        {
            CreatePlayerCard(player);
            playerCount++;
            if (player.Id == currentLobby.currentLobby.HostId)
            {
                hostPlayer = player;
            }
        }

        if (hostPlayer.Id == currentLobby.thisPlayer.Id)
        {
            startButtonText.text = "Başlat";
            isHost = true;

            SetJoinCode();
        }
        else
        {
            startButtonText.text = "Hazır";
            isHost = false;
            if (!isHost && currentLobby.currentLobby.Data["joinCode"].Value != "")
            {
                JoinLobby.LoadGame();
                relayManager.OnJoinClick();
            }
        }
    }

    void CreatePlayerCard(Player player)
    {
        GameObject card = Instantiate(playerCardPrefab, Vector3.zero, Quaternion.identity);
        GameObject text = card.transform.GetChild(2).gameObject;
        GameObject countText = card.transform.GetChild(0).gameObject;
        countText.GetComponent<TextMeshProUGUI>().text = "#" + playerCount;
        text.GetComponent<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;
        var recTransform = card.GetComponent<RectTransform>();
        recTransform.SetParent(playerListContainer.transform);
    }

    async void UpdateLobby()
    {
        currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(
            currentLobby.currentLobby.Id
        );

        PopulateUIElements();
    }

    private void CleanerContainer()
    {
        if (playerListContainer is not null && playerListContainer.transform.childCount > 0)
        {
            foreach (Transform item in playerListContainer.transform)
            {
                Destroy(item.gameObject);
                playerCount = 0;
            }
        }
    }

    //ButtonEvents

    public void StartGame()
    {
        if (isHost)
        {
            JoinLobby.LoadGame();
            relayManager.OnHostClick();
        }

        // if (isHost)
        // {
        //     if (
        //         Convert.ToInt32(currentLobby.currentLobby.Data["readyCount"].Value)
        //         == playerCount - 1
        //     )
        //     {

        //     }
        // }
        // else if (!isHost && setReady == true)
        // {
        //     readyCount++;
        //     setReady = false;
        // }
    }

    public async void ExitLobby()
    {
        try
        {
            //Ensure you sign-in before calling Authentication Instance
            //See IAuthenticationService interface
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.currentLobby.Id, playerId);
            JoinLobby.ReturnMainMenu();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void SetJoinCode()
    {
        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>()
            {
                { "joinCode", new DataObject(DataObject.VisibilityOptions.Member, "") }
            };
            currentLobby.currentLobby = await Lobbies.Instance.UpdateLobbyAsync(
                currentLobby.currentLobby.Id,
                options
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
