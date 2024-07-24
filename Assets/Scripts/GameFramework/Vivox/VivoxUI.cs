using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class VivoxUI : MonoBehaviour
{
    CurrentLobby currentLobby;
    public List<PlayerCardVivox> rosterList = new List<PlayerCardVivox>();

    private void Awake()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
    }

    private void Start()
    {
        JoinGroupChannelAsync();
    }

    void MutePlayerLocally(string PlayerId)
    {
        string ChannelName = currentLobby.currentLobby.Id;
        VivoxService.Instance.ActiveChannels[ChannelName]
            .Where(participant => participant.PlayerId == PlayerId)
            .First()
            .MutePlayerLocally();
    }

    void UnmutePlayerLocally(string PlayerId)
    {
        string ChannelName = currentLobby.currentLobby.Id;
        VivoxService.Instance.ActiveChannels[ChannelName]
            .Where(participant => participant.PlayerId == PlayerId)
            .First()
            .UnmutePlayerLocally();
    }

    // private void BindSessionEvents(bool doBind)
    // {
    //     if (doBind)
    //     {
    //         VivoxService.Instance.ParticipantAddedToChannel += onParticipantAddedToChannel;
    //         VivoxService.Instance.ParticipantRemovedFromChannel += onParticipantRemovedFromChannel;
    //     }
    //     else
    //     {
    //         VivoxService.Instance.ParticipantAddedToChannel -= onParticipantAddedToChannel;
    //         VivoxService.Instance.ParticipantRemovedFromChannel -= onParticipantRemovedFromChannel;
    //     }
    // }

    // private void onParticipantAddedToChannel(VivoxParticipant participant)
    // {
    //     ///RosterItem is a class intended to store the participant object, and reflect events relating to it into the game's UI.
    //     ///It is a sample of one way to use these events, and is detailed just below this snippet.
    //     PlayerCardVivox newRosterItem = new PlayerCardVivox();
    //     newRosterItem.SetupRosterItem(participant);
    //     rosterList.Add(newRosterItem);
    // }

    // private void onParticipantRemovedFromChannel(VivoxParticipant participant)
    // {
    //     PlayerCardVivox rosterItemToRemove = rosterList.FirstOrDefault(
    //         p => p.Participant.PlayerId == participant.PlayerId
    //     );
    //     rosterList.Remove(rosterItemToRemove);
    // }

    public async void JoinGroupChannelAsync()
    {
        string channelToJoin = currentLobby.currentLobby.Id;
        await VivoxService.Instance.JoinGroupChannelAsync(
            channelToJoin,
            ChatCapability.TextAndAudio
        );
    }

    public async void LeaveEchoChannelAsync()
    {
        string channelToLeave = currentLobby.currentLobby.Id;
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    }

    public async void LogoutOfVivoxAsync()
    {
        await VivoxService.Instance.LogoutAsync();
    }
}
