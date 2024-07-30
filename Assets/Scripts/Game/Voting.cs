using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Voting : NetworkBehaviour
{
    [SerializeField]
    private ReactorTimer reactorTimer;

    [SerializeField]
    private GameObject votingUIPanel;
    private VotingUI votingUI;

    private Dictionary<ulong, ulong> votes = new Dictionary<ulong, ulong>(); // voter, voted
    private bool isVotingInProgress = false;

    // public int numOfPlayers = 0;
    public Vector3 meetingArea;
    CurrentLobby currentLobby;

    private void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        GameObject playerSelf = GameObject.Find(currentLobby.thisPlayer.Data["PlayerName"].Value);
        if (playerSelf != null)
        {
            votingUIPanel = votingUI.voteUIPanel;
        }

        // numOfPlayers = votingUI.playerListAlive.Count;
        // Debug.Log("Voting Start: Number of players: " + numOfPlayers);
    }

    public void CallMeeting()
    {
        if (isVotingInProgress)
            return;
        isVotingInProgress = true;
        Debug.Log("Meeting Called. Voting started.");

        // reactorTimer.PauseReactorServerRpc();
        MovePlayersToMeetingArea();
        ShowVotingUI();
    }

    private void MovePlayersToMeetingArea()
    {
        foreach (var player in FindObjectsOfType<PlayerMovement>())
        {
            player.transform.position = meetingArea;
            Debug.Log("Moved player to meeting area: " + player.name);
        }
    }

    private void ShowVotingUI()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                ShowVotingUIClientRpc(
                    new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new List<ulong> { clientId }
                        }
                    }
                );
                Debug.Log("Showing voting UI to client: " + clientId);
            }
        }
    }

    [ClientRpc]
    private void ShowVotingUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        votingUIPanel.SetActive(true);
        Debug.Log("Voting UI shown on client.");
    }

    public void CastVote(ulong voterId, ulong targetId)
    {
        if (votes.ContainsKey(voterId))
        {
            votes[voterId] = targetId;
            Debug.Log($"Voter {voterId} changed vote to {targetId}");
        }
        else
        {
            votes.Add(voterId, targetId);
            Debug.Log($"Voter {voterId} voted for {targetId}");
        }
    }

    [ServerRpc]
    public void EndVotingServerRpc()
    {
        if (!IsServer)
            return;

        Debug.Log("Ending voting process.");
        ulong mostVotedPlayer = CalculateMostVotedPlayer();
        if (mostVotedPlayer != 0) // Check if a player is actually voted out
        {
            KillPlayer(mostVotedPlayer);
        }

        isVotingInProgress = false;
        // reactorTimer.ResumeReactorServerRpc();
        ResetVotingState();
    }

    private ulong CalculateMostVotedPlayer()
    {
        Dictionary<ulong, int> voteCounts = new Dictionary<ulong, int>();

        foreach (var vote in votes.Values)
        {
            if (voteCounts.ContainsKey(vote))
            {
                voteCounts[vote]++;
            }
            else
            {
                voteCounts.Add(vote, 1);
            }
        }

        ulong mostVotedPlayer = 0;
        int maxVotes = 0;
        int alivePlayersCount = GetAlivePlayersCount();

        foreach (var vote in voteCounts)
        {
            if (vote.Value > maxVotes)
            {
                maxVotes = vote.Value;
                mostVotedPlayer = vote.Key;
            }
        }

        if (maxVotes <= alivePlayersCount / 2)
        {
            mostVotedPlayer = 0;
        }

        Debug.Log($"Most voted player: {mostVotedPlayer} with {maxVotes} votes.");
        return mostVotedPlayer;
    }

    private int GetAlivePlayersCount()
    {
        int alivePlayersCount = 0;
        foreach (var player in FindObjectsOfType<RoleAssignment>())
        {
            if (!player.isDead)
            {
                alivePlayersCount++;
            }
        }
        Debug.Log("Alive players count: " + alivePlayersCount);
        return alivePlayersCount;
    }

    private void KillPlayer(ulong playerId)
    {
        var player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
        if (player != null)
        {
            player.GetComponent<RoleAssignment>().isDead = true;
            Debug.Log($"Player {playerId} was killed.");
        }
    }

    public void ResetVotingState()
    {
        isVotingInProgress = false;
        votes.Clear();
        Debug.Log("Voting state reset.");
        HideVotingUI();
    }

    private void HideVotingUI()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                HideVotingUIClientRpc(
                    new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new List<ulong> { clientId }
                        }
                    }
                );
                Debug.Log("Hiding voting UI from client: " + clientId);
            }
        }
    }

    [ClientRpc]
    private void HideVotingUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        votingUIPanel.SetActive(false);
        Debug.Log("Voting UI hidden on client.");
    }
}
