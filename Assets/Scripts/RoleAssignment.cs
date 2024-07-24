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
    public NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(
        PlayerRole.Unassigned
    );

    public bool usedSkill = false;
    public bool isDead = false;

    public int[] roleCountList = new int[5];

    CurrentLobby currentLobby;

    private void Start()
    {
        if (IsServer && IsOwner)
        {
            Invoke("GetLobbyData", 5f);
        }

        // Add a listener to the NetworkVariable to handle changes
        // role.OnValueChanged += OnRoleChanged;
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

        switch (newRole)
        {
            case PlayerRole.Koylu:
                gameObject.AddComponent<Koylu>();
                break;
            case PlayerRole.Hayalet:
                gameObject.AddComponent<Hayalet>();
                break;
            case PlayerRole.AlphaHayalet:
                gameObject.AddComponent<AlphaHayalet>();
                break;
            case PlayerRole.Buyucu:
                gameObject.AddComponent<Buyucu>();
                break;
            case PlayerRole.Asik:
                gameObject.AddComponent<Asik>();
                break;
            case PlayerRole.Avci:
                gameObject.AddComponent<Avci>();
                break;
            case PlayerRole.BaşAvcı:
                gameObject.AddComponent<HeadHunter>();
                break;
        }
    }

    private void RemoveAllRoleComponents()
    {
        Destroy(GetComponent<Koylu>());
        Destroy(GetComponent<Hayalet>());
        Destroy(GetComponent<AlphaHayalet>());
        Destroy(GetComponent<Buyucu>());
        Destroy(GetComponent<Asik>());
        Destroy(GetComponent<Avci>());
        Destroy(GetComponent<HeadHunter>());
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && role.Value != PlayerRole.Unassigned)
        {
            Debug.Log($"Editor: {gameObject.name} is assigned to {role.Value} role");
            EnableRelevantRoleScript(role.Value);
        }
    }

    void GetLobbyData()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        roleCountList[0] = Convert.ToInt32(currentLobby.currentLobby.Data["ghost"].Value);
        roleCountList[1] = Convert.ToInt32(currentLobby.currentLobby.Data["hunter"].Value);
        roleCountList[2] = Convert.ToInt32(currentLobby.currentLobby.Data["villager"].Value);
        roleCountList[3] = Convert.ToInt32(currentLobby.currentLobby.Data["lover"].Value);
        roleCountList[4] = Convert.ToInt32(currentLobby.currentLobby.Data["wizard"].Value);
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
                        break;
                    case 1:
                        roleAssignment.AssignRole(PlayerRole.Avci);
                        break;
                    case 2:
                        roleAssignment.AssignRole(PlayerRole.Koylu);
                        break;
                    case 3:
                        roleAssignment.AssignRole(PlayerRole.Asik);
                        break;
                    case 4:
                        roleAssignment.AssignRole(PlayerRole.Buyucu);
                        break;
                }
                roleCountList[roleClass]--;
            }
            else
            {
                return;
            }
        }
    }
}
