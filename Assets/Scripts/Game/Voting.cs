using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Newtonsoft.Json;
using System;
using System.Linq;
using TMPro;

public class Voting : NetworkBehaviour
{
    [SerializeField]
    private ReactorTimer reactorTimer;

    [SerializeField]
    private GameObject votingUIPrefab;
    public Dictionary<ulong, ulong> votes = new Dictionary<ulong, ulong>(); // voter, voted
    private bool isVotingInProgress = false;
    private float votingDuration = 45.0f;
    private float votingTimer = 0.0f;
    public static Voting Instance;
    public int playerCount = -1;
    public TextMeshProUGUI timerText;
    public GameObject callPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isVotingInProgress)
        {
            votingTimer -= Time.deltaTime;

            if (timerText)
            {
                timerText.text = Convert.ToInt32(votingTimer).ToString();
            }

            if (votingTimer <= 0.0f || votes.ToArray().Length == playerCount)
            {
                EndVoting();
            }
        }
    }

    public void CallForMeeting()
    {
        RequestForMeetingServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestForMeetingServerRpc()
    {
        prepareVotingUI();
        prepareVotingUIClientRpc();
    }

    [ClientRpc]
    public void prepareVotingUIClientRpc()
    {
        prepareVotingUI();
    }

    public void prepareVotingUI()
    {
        if (isVotingInProgress)
        {
            Debug.LogWarning("Voting is already in progress.");
            return;
        }
        isVotingInProgress = true;
        votingTimer = votingDuration;

        var players = GameObject.FindGameObjectsWithTag("Player");
        playerCount = Array
            .FindAll(players, (elm) => !elm.gameObject.GetComponent<RoleAssignment>().isDead.Value)
            .Length;

        var myPlayer = Array.Find(
            players,
            elm => elm.gameObject.GetComponent<NetworkObject>().IsOwner
        );

        if (!myPlayer.GetComponent<RoleAssignment>().isDead.Value)
        {
            callPanel = myPlayer.transform.GetChild(0).GetChild(0).Find("CallPanel").gameObject;
            callPanel.SetActive(true);
            timerText = callPanel.transform
                .GetChild(2)
                .GetChild(5)
                .GetChild(1)
                .GetChild(0)
                .GetChild(1)
                .gameObject.GetComponent<TextMeshProUGUI>();

            Debug.Log("Showing voting screen.");
        }
    }

    public void CastVote(ulong voterId, ulong targetId)
    {
        SendVoteToServerRpc(voterId, targetId);
    }

    public void UpdateVoteList(ulong voterId, ulong targetId)
    {
        if (votes.ContainsKey(voterId))
        {
            votes[voterId] = targetId;
        }
        else
        {
            votes.Add(voterId, targetId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendVoteToServerRpc(ulong voterId, ulong targetId)
    {
        UpdateVoteList(voterId, targetId);
        string json = JsonConvert.SerializeObject(votes);
        SendVotesToClientRpc(json);
    }

    [ClientRpc]
    public void SendVotesToClientRpc(string json)
    {
        var list =
            (Dictionary<ulong, ulong>)
                JsonConvert.DeserializeObject(json, typeof(Dictionary<ulong, ulong>));
        votes = list;

        Debug.Log("CLİENMT a gelen json: " + json);
        Debug.Log("Dikşınery " + string.Join(Environment.NewLine, votes));
    }

    public void EndVoting()
    {
        if (!IsServer)
            return;

        ulong mostVotedPlayer = CalculateMostVotedPlayer();
        if (mostVotedPlayer != ulong.MaxValue)
        {
            Debug.Log("Öldün madıfakı: " + mostVotedPlayer);
            KillPlayer(mostVotedPlayer);
            KillPlayerClientRpc(mostVotedPlayer);
        }

        isVotingInProgress = false;
        // reactorTimer.ResumeReactorServerRpc();
        ResetVotingState();
    }

    private ulong CalculateMostVotedPlayer()
    {
        Dictionary<ulong, int> voteCounts = new Dictionary<ulong, int>();

        Debug.Log("Entering CalculateMostVotedPlayer. Current votes count: " + votes.Count);
        foreach (var vote in votes)
        {
            Debug.Log(vote.Key + " voted for " + vote.Value);
            if (voteCounts.ContainsKey(vote.Value))
            {
                voteCounts[vote.Value]++;
            }
            else
            {
                voteCounts.Add(vote.Value, 1);
            }
        }

        ulong mostVotedPlayer = ulong.MaxValue;
        int maxVotes = 0;

        foreach (var vote in voteCounts)
        {
            Debug.Log(vote.Key + " has " + vote.Value + " votes");
            if (vote.Value > maxVotes)
            {
                maxVotes = vote.Value;
                mostVotedPlayer = vote.Key;
            }
        }

        Debug.Log(mostVotedPlayer + " got " + maxVotes + " votes.");

        // TODO: 2 tane oyuncu eşit oy alırsa ne olacak

        return mostVotedPlayer;
    }

    [ClientRpc]
    public void KillPlayerClientRpc(ulong playerId)
    {
        KillPlayer(playerId);
        isVotingInProgress = false;
        // reactorTimer.ResumeReactorServerRpc();
        ResetVotingState();
    }

    private void KillPlayer(ulong playerId)
    {
        var playerList = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in playerList)
        {
            var netObj = player.GetComponent<NetworkObject>();

            if (netObj.OwnerClientId == playerId)
            {
                Debug.Log("Player object found for: " + playerId);
                var roleAssignment = player.GetComponent<RoleAssignment>();
                roleAssignment.isDead.Value = true;
                player.GetComponent<CapsuleCollider>().enabled = false;
                player.transform.GetChild(3).gameObject.SetActive(false);
                Debug.Log($"Player {playerId} was killed.");
                AnnounceDeathClientRpc(playerId, roleAssignment.role.Value);

                callPanel.SetActive(false);
                return;
            }
        }

        Debug.Log("Player ID not found in connected clients: " + playerId);
    }

    [ClientRpc]
    private void AnnounceDeathClientRpc(ulong playerId, PlayerRole role)
    {
        Debug.Log($"Player {playerId} ({role}) was killed.");
    }

    public void ResetVotingState()
    {
        playerCount = -1;
        isVotingInProgress = false;
        votes.Clear(); // Clear votes for the next round
        // HideVotingUI();
        Debug.Log("Voting process has ended for this call/round.");
    }
}
