using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;
using Unity.Services.Vivox;

public class PopulateUI : NetworkBehaviour
{
    public TextMeshProUGUI playerName;
    public int playerCount = 1;

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

    public bool setReady = true;
    bool isHost;

    RelayManager relayManager;

    public TMP_Text lobbyCode;
    public TMP_Text lobbyCode2nd;
    public TMP_InputField setLobbyName;
    public TextMeshProUGUI lobbyNameText;
    public TMP_Dropdown maxPlayers;
    public Toggle isLobbyPrivate;
    public TMP_Dropdown ghost;
    public TMP_Dropdown wizard;
    public TMP_Dropdown lover;
    public TMP_Dropdown hunter;

    public List<string> playerIDs = new List<string>();

    public List<PlayerCardVivox> rosterList = new List<PlayerCardVivox>();

    private void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        relayManager = GameObject.Find("RelayManager").GetComponent<RelayManager>();
        lobbyCode.text = currentLobby.currentLobby.LobbyCode;
        lobbyCode2nd.text = lobbyCode.text;
        PopulateUIElements();
        InvokeRepeating(nameof(UpdateLobby), 1.1f, 2f);
        BindSessionEvents(true);
    }

    private void BindSessionEvents(bool doBind)
    {
        if (doBind)
        {
            VivoxService.Instance.ParticipantAddedToChannel += onParticipantAddedToChannel;
            VivoxService.Instance.ParticipantRemovedFromChannel += onParticipantRemovedFromChannel;
        }
        else
        {
            VivoxService.Instance.ParticipantAddedToChannel -= onParticipantAddedToChannel;
            VivoxService.Instance.ParticipantRemovedFromChannel -= onParticipantRemovedFromChannel;
        }
    }

    private void onParticipantAddedToChannel(VivoxParticipant participant)
    {
        Debug.Log("BiriGeldi");
        foreach (Player item in currentLobby.currentLobby.Players)
        {
            if (item.Id == participant.PlayerId)
            {
                PlayerCardVivox newRosterItem = GameObject
                    .Find(item.Id)
                    .gameObject.GetComponent<PlayerCardVivox>();

                newRosterItem.SetupRosterItem(participant);
                rosterList.Add(newRosterItem);
            }
        }
    }

    private void onParticipantRemovedFromChannel(VivoxParticipant participant)
    {
        Debug.Log("GittiEleman");
        PlayerCardVivox rosterItemToRemove = rosterList.FirstOrDefault(
            p => p.Participant.PlayerId == participant.PlayerId
        );
        rosterList.Remove(rosterItemToRemove);
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
            playerCount++;
            CreatePlayerCard(player);
        }
        if (currentLobby.currentLobby.Players.Any(p => p.Id == currentLobby.thisPlayer.Id))
        {
            if (currentLobby.currentLobby.HostId == currentLobby.thisPlayer.Id)
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
    }

    void CreatePlayerCard(Player player)
    {
        GameObject card = Instantiate(playerCardPrefab, Vector3.zero, Quaternion.identity);
        card.name = player.Id;
        GameObject text = card.transform.GetChild(3).gameObject;
        GameObject countText = card.transform.GetChild(0).gameObject;
        countText.GetComponent<TextMeshProUGUI>().text = "#" + playerCount;
        text.GetComponent<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;
        var recTransform = card.GetComponent<RectTransform>();
        recTransform.SetParent(playerListContainer.transform);
        if (isHost)
        {
            if (
                text.GetComponent<TextMeshProUGUI>().text
                == currentLobby.thisPlayer.Data["PlayerName"].Value
            )
            {
                card.transform.GetChild(4).gameObject.SetActive(false);
            }
            card.transform
                .GetChild(4)
                .GetComponent<Button>()
                .onClick.AddListener(
                    delegate
                    {
                        KickLobby(player);
                    }
                );
        }
        else
        {
            card.transform.GetChild(4).gameObject.SetActive(false);
        }
    }

    async void UpdateLobby()
    {
        currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(
            currentLobby.currentLobby.Id
        );
        if (!currentLobby.currentLobby.Players.Any(p => p.Id == currentLobby.thisPlayer.Id))
        {
            JoinLobby.ReturnMainMenu();
        }

        PopulateUIElements();
    }

    public void RefreshSettingsPanel()
    {
        setLobbyName.text = "";
        lobbyNameText.text = lobbyName.text;
        maxPlayers.options[maxPlayers.value].text = Convert.ToString(
            currentLobby.currentLobby.MaxPlayers
        );
        isLobbyPrivate.isOn = currentLobby.currentLobby.IsPrivate;
        ghost.options[ghost.value].text = ghostCount.text;
        hunter.options[hunter.value].text = hunterCount.text;
        wizard.options[wizard.value].text = wizardCount.text;
        lover.options[lover.value].text = loverCount.text;
    }

    public async void SetNewValueForLobby()
    {
        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.MaxPlayers = Convert.ToInt32(maxPlayers.options[maxPlayers.value].text);
            string villager = Convert.ToString(
                options.MaxPlayers
                    - (
                        Convert.ToInt32(hunter.options[hunter.value].text)
                        + Convert.ToInt32(wizard.options[wizard.value].text)
                        + Convert.ToInt32(lover.options[lover.value].text)
                        + Convert.ToInt32(hunter.options[hunter.value].text)
                    )
            );
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
            if (setLobbyName.text != "")
            {
                options.Name = setLobbyName.text;
            }
            options.IsPrivate = isLobbyPrivate.isOn;
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

    private void CleanerContainer()
    {
        if (playerListContainer is not null && playerListContainer.transform.childCount > 0)
        {
            foreach (Transform item in playerListContainer.transform)
            {
                Destroy(item.gameObject);
                playerCount = 1;
            }
        }
    }

    //ButtonEvents

    public void StartGame()
    {
        if (isHost)
        {
            int totalReadyCount = 0;
            foreach (Player player in currentLobby.currentLobby.Players)
            {
                totalReadyCount += Convert.ToInt32(player.Data["readyCount"].Value);
            }
            if (totalReadyCount == currentLobby.currentLobby.Players.Count)
            {
                JoinLobby.LoadGame();
                relayManager.OnHostClick();
            }
        }
        else if (!isHost && setReady == true)
        {
            SetReady();
        }
    }

    public async void SetReady()
    {
        setReady = false;
        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();
            options.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "readyCount",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "1")
                }
            };
            currentLobby.currentLobby = await Lobbies.Instance.UpdatePlayerAsync(
                currentLobby.currentLobby.Id,
                currentLobby.thisPlayer.Id,
                options
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void ExitLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(
                currentLobby.currentLobby.Id,
                currentLobby.thisPlayer.Id
            );
            Destroy(currentLobby);
            Destroy(relayManager);
            JoinLobby.ReturnMainMenu();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void KickLobby(Player player)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.currentLobby.Id, player.Id);
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

    public void CopyToLobbyCode()
    {
        TextEditor te = new TextEditor();
        te.text = lobbyCode.text;
        te.SelectAll();
        te.Copy();
    }
}
