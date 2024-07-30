using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class VotingUI : MonoBehaviour
{
    private Voting voting;
    public GameObject voteUI;

    private void Start()
    {
        voting = FindObjectOfType<Voting>();
    }

    public void OnVoteButtonClicked(ulong targetId)
    {
        ulong voterId = NetworkManager.Singleton.LocalClientId;
        voting.CastVote(voterId, targetId);
    }

    public void OnEndVotingButtonClicked()
    {
        voting.EndVotingServerRpc();
        voting.ResetVotingState();
        voteUI.SetActive(false);
    }
}