using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PopulateUI : MonoBehaviour
{
    public TextMeshProUGUI lobbyName;
    public TextMeshProUGUI startButtonText;
    private CurrentLobby currentLobby;

    public GameObject playerCardPrefab;
    public GameObject playerListContainer;

    private void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        PopulateUIElements();
        InvokeRepeating(nameof(UpdateLobby), 1.1f, 2f);
    }

    void PopulateUIElements()
    {
        lobbyName.text = currentLobby.currentLobby.Name;
        CleanerContainer();
        foreach (Player player in currentLobby.currentLobby.Players)
        {
            CreatePlayerCard(player);
        }
    }

    void CreatePlayerCard(Player player)
    {
        GameObject card = Instantiate(playerCardPrefab, Vector3.zero, Quaternion.identity);
        GameObject text = card.transform.GetChild(2).gameObject;
        text.GetComponent<TextMeshProUGUI>().text = player.Id;
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
            }
        }
    }
}
