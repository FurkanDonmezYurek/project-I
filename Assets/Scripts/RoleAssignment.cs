using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum PlayerRole
{
    Unassigned,
    Buyucu,
    Koylu,
    Asik,
    BaşAvcı,
    Avci,
    Hayalet,
    AlphaHayalet
}

public class RoleAssignment : NetworkBehaviour
{
    public NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(PlayerRole.Unassigned);

    public bool usedSkill = false;
    public bool isDead = false;

    public int[] roleCountList = new int[5];

    private CurrentLobby currentLobby;

    private void Start()
    {
        Debug.Log($"RoleAssignment Start: IsServer: {IsServer}, IsOwner: {IsOwner}, IsLocalPlayer: {IsLocalPlayer}");

        if (IsServer && IsOwner)
        {
            Invoke("GetLobbyData", 5f);
            //RemoveAllRoleComponents();
        }

        // Add a listener to the NetworkVariable to handle changes
        role.OnValueChanged += OnRoleChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AssignRoleServerRpc(PlayerRole newRole)
    {
        AssignRole(newRole);
    }

    private void AssignRole(PlayerRole newRole)
    {
        role.Value = newRole;
        Debug.Log($"Server received: {gameObject.name} is assigned to {newRole} role");
        EnableRelevantRoleScript(newRole);
    }

    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        Debug.Log($"Role changed: {gameObject.name} was {oldRole}, now {newRole}");
        EnableRelevantRoleScript(newRole);
    }

    private void EnableRelevantRoleScript(PlayerRole newRole)
    {
        RemoveAllRoleComponents();

        Debug.Log($"Enabling role script for {newRole}");


        switch (newRole)
        {
            case PlayerRole.Buyucu:
                GetComponent<Buyucu>().enabled = true;
                break;
            case PlayerRole.Koylu:
                GetComponent<Koylu>().enabled = true;
                break;
            case PlayerRole.Asik:
                GetComponent<Asik>().enabled = true;
                break;
            case PlayerRole.Avci:
                GetComponent<Avci>().enabled = true;
                break;
            case PlayerRole.Hayalet:
                GetComponent<Hayalet>().enabled = true;
                break;
            case PlayerRole.AlphaHayalet:
                GetComponent<AlphaHayalet>().enabled = true;
                break;
            case PlayerRole.BaşAvcı:
                GetComponent<HeadHunter>().enabled = true;
                break;
        }
    }

    private void RemoveAllRoleComponents()
    {
        Debug.Log("Disabling all role components");

        GetComponent<Avci>().enabled = false;
        GetComponent<Koylu>().enabled = false;
        GetComponent<Hayalet>().enabled = false;
        GetComponent<AlphaHayalet>().enabled = false;
        GetComponent<Buyucu>().enabled = false;
        GetComponent<Asik>().enabled = false;
        GetComponent<HeadHunter>().enabled = false;
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && role.Value != PlayerRole.Unassigned)
        {
            Debug.Log($"Editor: {gameObject.name} is assigned to {role.Value} role");
            EnableRelevantRoleScript(role.Value);
        }
    }

    private void GetLobbyData()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        roleCountList[0] = Convert.ToInt32(currentLobby.currentLobby.Data["ghost"].Value);
        roleCountList[1] = Convert.ToInt32(currentLobby.currentLobby.Data["hunter"].Value);
        roleCountList[2] = Convert.ToInt32(currentLobby.currentLobby.Data["villager"].Value);
        roleCountList[3] = Convert.ToInt32(currentLobby.currentLobby.Data["lover"].Value);
        roleCountList[4] = Convert.ToInt32(currentLobby.currentLobby.Data["wizard"].Value);

        Debug.Log("Lobby data received. Assigning roles to players.");

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            RoleAssignment roleAssignment = player.GetComponent<RoleAssignment>();
            int roleClass = UnityEngine.Random.Range(0, roleCountList.Length);
            if (roleCountList[roleClass] != 0)
            {
                switch (roleClass)
                {
                    case 0:
                        roleAssignment.AssignRole(PlayerRole.Hayalet);
                        GetComponent<Hayalet>().enabled = true;
                        break;
                    case 1:
                        roleAssignment.AssignRole(PlayerRole.Avci);
                        GetComponent<Avci>().enabled = true;
                        break;
                    case 2:
                        roleAssignment.AssignRole(PlayerRole.Koylu);
                        GetComponent<Koylu>().enabled = true;
                        break;
                    case 3:
                        roleAssignment.AssignRole(PlayerRole.Asik);
                        GetComponent<Asik>().enabled = true;
                        break;
                    case 4:
                        roleAssignment.AssignRole(PlayerRole.Buyucu);
                        GetComponent<Buyucu>().enabled = true;
                        break;
                }
                roleCountList[roleClass]--;
                Debug.Log($"Assigned role {roleAssignment.role.Value} to player {player.name}");
            }
        }
    }
}