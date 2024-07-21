using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using TMPro;

public class VivoxUI : MonoBehaviour
{
    CurrentLobby currentLobby;

    private void Awake()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
    }

    private void Start()
    {
        JoinEchoChannelAsync();
    }

    public async void JoinEchoChannelAsync()
    {
        string channelToJoin = currentLobby.currentLobby.Id;
        await VivoxService.Instance.JoinEchoChannelAsync(
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
