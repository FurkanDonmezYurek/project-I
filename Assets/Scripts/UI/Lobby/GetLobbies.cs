using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class GetLobbies : MonoBehaviour
{
    public GameObject buttonsContainer;
    public GameObject buttonPrefab;

    public async void GetLobbiesFunc()
    {
        CleanerContainer();
        try
        {
            QueryLobbiesOptions options = new();
            options.Count = 25;

            //Filter for open lobbies only
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"
                )
            };

            //Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(asc: false, field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            Debug.LogWarning("Get Lobbies Done Count:" + lobbies.Results.Count);
            foreach (Lobby foundLobby in lobbies.Results)
            {
                CreateLobbyButton(foundLobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void CreateLobbyButton(Lobby lobby)
    {
        var button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
        button.name = lobby.Name + "Button";
        button.GetComponentInChildren<TextMeshProUGUI>().text = lobby.Name;

        var recTransform = button.GetComponent<RectTransform>();
        recTransform.SetParent(buttonsContainer.transform);
        button
            .GetComponent<Button>()
            .onClick.AddListener(
                delegate()
                {
                    LobbyButtonClick(lobby);
                }
            );
    }

    public void LobbyButtonClick(Lobby lobby)
    {
        Debug.Log("Clicked Lobby:" + lobby.Name);
        GetComponent<JoinLobby>().JoinLobbyWithLobbyId(lobby.Id);
    }

    private void CleanerContainer()
    {
        if (buttonsContainer is not null && buttonsContainer.transform.childCount > 0)
        {
            foreach (Transform item in buttonsContainer.transform)
            {
                Destroy(item.gameObject);
            }
        }
    }
}
