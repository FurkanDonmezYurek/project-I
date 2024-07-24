using System;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Services.Vivox;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LoginScreenUI : MonoBehaviour
{
    const int k_DefaultMaxStringLength = 15;

    int m_PermissionAskedCount;
    CurrentLobby currentLobby;

    void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        LoginToVivoxService();
    }

    bool IsMicPermissionGranted()
    {
        bool isGranted = Permission.HasUserAuthorizedPermission(Permission.Microphone);
        return isGranted;
    }

    void AskForPermissions()
    {
        string permissionCode = Permission.Microphone;
        m_PermissionAskedCount++;
        Permission.RequestUserPermission(permissionCode);
    }

    bool IsPermissionsDenied()
    {
        return m_PermissionAskedCount == 1;
    }

    void LoginToVivoxService()
    {
        if (IsMicPermissionGranted())
        {
            // The user authorized use of the microphone.
            LoginToVivox();
        }
        else
        {
            // We do not have the needed permissions.
            // Ask for permissions or proceed without the functionality enabled if they were denied by the user
            if (IsPermissionsDenied())
            {
                m_PermissionAskedCount = 0;
                LoginToVivox();
            }
            else
            {
                AskForPermissions();
            }
        }
    }

    async void LoginToVivox()
    {
        string playerId = currentLobby.thisPlayer.Id;
        await VivoxVoiceManager.Instance.InitializeAsync(playerId);
        var loginOptions = new LoginOptions()
        {
            DisplayName = PlayerPrefs.GetString("PlayerName"),
            ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond
        };
        await VivoxService.Instance.LoginAsync(loginOptions);
    }
}
