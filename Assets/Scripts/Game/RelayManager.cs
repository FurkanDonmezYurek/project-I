using UnityEngine;
using System;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;

public class RelayManager : MonoBehaviour
{
    RelayHostData hostData;
    RelayJointData joinData;

    CurrentLobby currentLobby;

    void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
    }

    public async void OnHostClick()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(
            currentLobby.currentLobby.MaxPlayers
        );
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
        hostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(hostData.AllocationID);
        SetJoinCode();
    }

    public async void OnJoinClick()
    {
        var allocation = await RelayService.Instance.JoinAllocationAsync(
            currentLobby.currentLobby.Data["joinCode"].Value
        );
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

    public async void SetJoinCode()
    {
        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "joinCode",
                    new DataObject(DataObject.VisibilityOptions.Member, hostData.JoinCode)
                }
            };
            currentLobby.currentLobby = await Lobbies.Instance.UpdateLobbyAsync(
                currentLobby.currentLobby.Id,
                options
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
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
