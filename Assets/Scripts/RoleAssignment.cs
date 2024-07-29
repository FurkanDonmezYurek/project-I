using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public enum PlayerRole
{
    Unassigned,
    Wizard,
    Villager,
    Lover,
    HeadHunter,
    Hunter,
    Ghost,
    AlphaGhost
}

public class RoleAssignment : NetworkBehaviour
{
    public NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(
        PlayerRole.Unassigned
    );

    public bool usedSkill = false;
    public bool isVekil = false;
    public bool isDead = false;

    public int[] roleCountList = new int[5];

    private CurrentLobby currentLobby;

    public GameObject[] npcArray = new GameObject[5];
    
    private void Awake()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        transform.name = currentLobby.thisPlayer.Data["PlayerName"].Value;
    }

    private void Start()
    {
        if (IsServer && IsOwner)
        {
            Invoke("GetLobbyData", 10f);
            //bunu 10sn yaptim
        }
        // Add a listener to the NetworkVariable to handle changes
        role.OnValueChanged += OnRoleChanged;

        npcArray = GameObject.FindGameObjectsWithTag("NPC");

    }
    
    //NPC Method
    public void NPCRequest()
    {
        for (int i = 0; i < npcArray.Length ; i++)
        {
            npcArray[i].GetComponent<NPCManager>().FieldOfViewCheck(this.gameObject);
        }
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
            case PlayerRole.Wizard:
                GetComponent<Wizard>().enabled = true;
                break;
            case PlayerRole.Villager:
                GetComponent<Villager>().enabled = true;
                break;
            case PlayerRole.Lover:
                GetComponent<Lover>().enabled = true;
                break;
            case PlayerRole.Hunter:
                GetComponent<Hunter>().enabled = true;
                break;
            case PlayerRole.Ghost:
                GetComponent<Ghost>().enabled = true;
                break;
            case PlayerRole.AlphaGhost:
                GetComponent<AlphaGhost>().enabled = true;
                break;
            case PlayerRole.HeadHunter:
                GetComponent<HeadHunter>().enabled = true;
                break;
        }
    }

    private void RemoveAllRoleComponents()
    {
        Debug.Log("Disabling all role components");

        GetComponent<Hunter>().enabled = false;
        GetComponent<Villager>().enabled = false;
        GetComponent<Ghost>().enabled = false;
        GetComponent<AlphaGhost>().enabled = false;
        GetComponent<Wizard>().enabled = false;
        GetComponent<Lover>().enabled = false;
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
                        roleAssignment.AssignRole(PlayerRole.Ghost);
                        break;
                    case 1:
                        roleAssignment.AssignRole(PlayerRole.Hunter);
                        break;
                    case 2:
                        roleAssignment.AssignRole(PlayerRole.Villager);
                        break;
                    case 3:
                        roleAssignment.AssignRole(PlayerRole.Lover);
                        break;
                    case 4:
                        roleAssignment.AssignRole(PlayerRole.Wizard);
                        break;
                }
                roleCountList[roleClass]--;
                Debug.Log($"Assigned role {roleAssignment.role.Value} to player {player.name}");
            }
        }
    }
}
