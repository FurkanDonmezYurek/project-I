using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Voting : NetworkBehaviour
{
    [SerializeField] private ReactorTimer reactorTimer;
    [SerializeField] private GameObject votingUIPanel; 

    private Dictionary<ulong, ulong> votes = new Dictionary<ulong, ulong>(); // voter, voted
    private bool isVotingInProgress = false;

    public int numOfPlayers = 0;
    public Vector3 meetingArea;
    private void Start()
    {
        numOfPlayers = FindObjectsOfType<RoleAssignment>().Length;
    }

    private void Update()
    {
        numOfPlayers = FindObjectsOfType<RoleAssignment>().Length;
    }

    public void CallMeeting()
    {
        if (isVotingInProgress) return;
        isVotingInProgress = true;

        reactorTimer.PauseReactorServerRpc();
        MovePlayersToMeetingArea();
        ShowVotingUI();
    }

    private void MovePlayersToMeetingArea()
    {
        foreach (var player in FindObjectsOfType<PlayerMovement>())
        {
            player.transform.position = meetingArea; 
        }
    }

    private void ShowVotingUI()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                ShowVotingUIClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { clientId }
                    }
                });
            }
        }
    }

    [ClientRpc]
    private void ShowVotingUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        votingUIPanel.SetActive(true); 
    }

    public void CastVote(ulong voterId, ulong targetId)
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

    [ServerRpc]
    public void EndVotingServerRpc()
    {
        if (!IsServer) return;

        ulong mostVotedPlayer = CalculateMostVotedPlayer();
        if (mostVotedPlayer != 0) // Check if a player is actually voted out
        {
            KillPlayer(mostVotedPlayer);
        }

        isVotingInProgress = false;
        reactorTimer.ResumeReactorServerRpc();
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
        HideVotingUI();
    }

    private void HideVotingUI()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                HideVotingUIClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { clientId }
                    }
                });
            }
        }
    }

    [ClientRpc]
    private void HideVotingUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        votingUIPanel.SetActive(false); 
    }
}