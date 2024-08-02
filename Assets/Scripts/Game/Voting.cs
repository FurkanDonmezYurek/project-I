using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Voting : NetworkBehaviour
{
    [SerializeField] private ReactorTimer reactorTimer;
    [SerializeField] private GameObject votingUIPrefab;
    public Dictionary<ulong, ulong> votes = new Dictionary<ulong, ulong>(); // voter, voted
    private bool isVotingInProgress = false;
    private float votingDuration = 45.0f;
    private float votingTimer = 0.0f;
    public static Voting Instance;

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
            Debug.Log("Time " + votingTimer);
            if (votingTimer <= 0.0f)
            {
                EndVotingServerRpc();
            }
        }
    }

    public void CallMeeting()
    {
        if (isVotingInProgress)
        {
            Debug.LogWarning("Voting is already in progress.");
            return;
        }
        isVotingInProgress = true;
        votingTimer = votingDuration;

        Debug.Log("Calling meeting and starting voting process.");
        ShowVotingUI();
    }

    private void ShowVotingUI()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                var roleAssignment = player.GetComponent<RoleAssignment>();
                if (player && !roleAssignment.isDead)
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
    }

    [ClientRpc]
    private void ShowVotingUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Showing voting screen on client");
        var votingUIInstance = Instantiate(votingUIPrefab);
        votingUIInstance.GetComponent<VotingUI>().enabled = true;
        votingUIInstance.SetActive(true);
    }

    public void CastVote(ulong voterId, ulong targetId)
    {
        Debug.Log($"CastVote called: voterId = {voterId}, targetId = {targetId}");

        if (votes.ContainsKey(voterId))
        {
            votes[voterId] = targetId;
        }
        else
        {
            votes.Add(voterId, targetId);
        }
        Debug.Log("Voter " + voterId + " voted for " + targetId);
        Debug.Log("Current votes:");
        foreach (var vote in votes)
        {
            Debug.Log("Voter: " + vote.Key + ", Voted for: " + vote.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndVotingServerRpc()
    {
        if (!IsServer) return;

        ulong mostVotedPlayer = CalculateMostVotedPlayer();
        if (mostVotedPlayer != 0)
        {
            Debug.Log("Most voted player: " + mostVotedPlayer);
            KillPlayer(mostVotedPlayer);
        }

        isVotingInProgress = false;
        reactorTimer.ResumeReactorServerRpc();
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

        ulong mostVotedPlayer = 0;
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
        return mostVotedPlayer;
    }

    private void KillPlayer(ulong playerId)
    {
        Debug.Log("Attempting to kill player: " + playerId);

        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(playerId))
        {
            var player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
            if (player != null)
            {
                Debug.Log("Player object found for: " + playerId);
                var roleAssignment = player.GetComponent<RoleAssignment>();
                roleAssignment.isDead = true;
                Debug.Log($"Player {playerId} was killed.");
                AnnounceDeathClientRpc(playerId, roleAssignment.role.Value);
            }
            else
            {
                Debug.Log("Player object is null for: " + playerId);
            }
        }
        else
        {
            Debug.Log("Player ID not found in connected clients: " + playerId);
        }
    }

    [ClientRpc]
    private void AnnounceDeathClientRpc(ulong playerId, PlayerRole role)
    {
        Debug.Log($"Player {playerId} ({role}) was killed.");
    }

    public void ResetVotingState()
    {
        isVotingInProgress = false;
        votes.Clear();  // Clear votes for the next round
        HideVotingUI();
        Debug.Log("Voting process has ended for this call/round.");
    }

    private void HideVotingUI()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
                if (player && !player.GetComponent<RoleAssignment>().isDead)
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
    }

    [ClientRpc]
    private void HideVotingUIClientRpc(ClientRpcParams clientRpcParams = default)
    {
        var votingUIInstance = FindObjectOfType<VotingUI>();
        if (votingUIInstance != null)
        {
            votingUIInstance.gameObject.SetActive(false);
        }
    }
}