using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class RelayManager : MonoBehaviour
{
    private string playerID;
    public TextMeshProUGUI idText;
    public TextMeshProUGUI joinText;
    public TMP_Dropdown playerCount;
    public TMP_InputField joinInputField;
    RelayHostData hostData;
    RelayJointData joinData;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        SignIn();
    }

    async void SignIn()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerID = AuthenticationService.Instance.PlayerId;
        idText.text = playerID;
    }

    public async void OnHostClick()
    {
        int maxPlayerCount = Convert.ToInt32(playerCount.options[playerCount.value].text);
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayerCount);
        hostData = new RelayHostData()
        {
            IPv4Adress = allocation.RelayServer.IpV4,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            Key = allocation.Key,
        };
        Debug.Log("Allocate Complete" + hostData.AllocationID);
        hostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(hostData.AllocationID);

        joinText.text = hostData.JoinCode;

        UnityTransport transport =
            NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

        transport.SetRelayServerData(
            hostData.IPv4Adress,
            hostData.Port,
            hostData.AllocationIDBytes,
            hostData.Key,
            hostData.ConnectionData
        );

        NetworkManager.Singleton.StartHost();
    }

    public async void OnJoinClick()
    {
        var allocation = await RelayService.Instance.JoinAllocationAsync(joinInputField.text);
        Debug.Log("ANAN");
        joinData = new RelayJointData()
        {
            IPv4Adress = allocation.RelayServer.IpV4,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            ConnectionData = allocation.ConnectionData,
            HostConnectionData = allocation.HostConnectionData,
            Key = allocation.Key,
        };

        UnityTransport transport =
            NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

        transport.SetRelayServerData(
            joinData.IPv4Adress,
            joinData.Port,
            joinData.AllocationIDBytes,
            joinData.Key,
            joinData.ConnectionData,
            joinData.HostConnectionData
        );

        NetworkManager.Singleton.StartClient();
    }
}

public struct RelayHostData
{
    public string JoinCode;
    public string IPv4Adress;
    public ushort Port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] Key;
}

public struct RelayJointData
{
    public string IPv4Adress;
    public ushort Port;
    public Guid AllocationID;
    public byte[] AllocationIDBytes;
    public byte[] ConnectionData;
    public byte[] HostConnectionData;
    public byte[] Key;
}
