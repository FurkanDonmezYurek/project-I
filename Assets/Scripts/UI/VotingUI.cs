using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class VotingUI : NetworkBehaviour
{
    private Voting voting;
    public GameObject voteUI;
    public GameObject voteUIPanel;
    CurrentLobby currentLobby;
    public List<GameObject> playerListAlive;
    public GameObject playerCardVote;
    public GameObject playerCardContainer;
    private ulong selectedPlayerId;

    private void Start()
    {
        voting = FindObjectOfType<Voting>();
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
    }

    private void Update()
    {
        if (!playerCardContainer.activeSelf)
        {
            if (playerCardContainer is not null && playerCardContainer.transform.childCount > 0)
            {
                foreach (Transform item in playerCardContainer.transform)
                {
                    Destroy(item.gameObject);
                }
            }
        }
    }

    public void AddPlayerVoteCard()
    {
        foreach (Player player in currentLobby.currentLobby.Players)
        {
            GameObject playerObject = GameObject.Find(player.Data["PlayerName"].Value);
            RoleAssignment roleAssignment = playerObject.GetComponent<RoleAssignment>();
            if (!roleAssignment.isDead)
            {
                playerListAlive.Add(playerObject);
                GameObject card = Instantiate(playerCardVote, Vector3.zero, Quaternion.identity);
                GameObject text = card.transform.GetChild(2).gameObject;
                text.GetComponent<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;
                var recTransform = card.GetComponent<RectTransform>();
                recTransform.SetParent(playerCardContainer.transform);
                NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
                card.transform
                    .GetChild(0)
                    .gameObject.GetComponent<Button>()
                    .onClick.AddListener(
                        delegate
                        {
                            OnPlayerCardClicked(networkObject.OwnerClientId);
                        }
                    );
            }
        }
    }

    public void OnPlayerCardClicked(ulong playerId)
    {
        selectedPlayerId = playerId;
        Debug.Log("Selected player: " + selectedPlayerId);
    }

    public void OnVoteButtonClicked()
    {
        ulong voterId = NetworkManager.Singleton.LocalClientId;
        voting.CastVote(voterId, selectedPlayerId);
        Debug.Log("Voter " + voterId + " voted for " + selectedPlayerId);
        //gameObject.SetActive(false);
    }

    public void OnEndVotingButtonClicked()
    {
        // voting.EndVotingServerRpc();
        // voting.ResetVotingState();
        //gameObject.SetActive(false);
    }

    //kim oldu kim kaldi gormek icin
    public void ShowAnnouncement(string message)
    {
        var announcementText = transform.Find("AnnouncementText").GetComponent<TextMeshProUGUI>();
        announcementText.text = message;

        var announcementPanel = transform.Find("AnnouncementPanel").gameObject;
        announcementPanel.SetActive(true);

        StartCoroutine(HideAnnouncementAfterDelay(5.0f));
    }

    private IEnumerator HideAnnouncementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        var announcementPanel = transform.Find("AnnouncementPanel").gameObject;
        announcementPanel.SetActive(false);
    }
    //    public void OnVoteButtonClicked(ulong targetId)
    //    {
    //        ulong voterId = NetworkManager.Singleton.LocalClientId;
    //        voting.CastVote(voterId, targetId);
    //    }
    //
    //    public void OnEndVotingButtonClicked()
    //    {
    //        voting.EndVotingServerRpc();
    //        voting.ResetVotingState();
    //    }
}
