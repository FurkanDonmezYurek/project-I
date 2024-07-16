using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RoleManagement : NetworkBehaviour
{
    public void AssignRole(GameObject player, PlayerRole newRole)
    {
        if (IsServer)
        {
            RoleAssignment roleAssignment = player.GetComponent<RoleAssignment>();
            if (roleAssignment != null)
            {
                Debug.Log($"Assigning role {newRole} to player {player.name}");
                roleAssignment.AssignRoleServerRpc(newRole);
            }
            else
            {
                Debug.LogError($"RoleAssignment script not found on {player.name}!");
            }
        }
        else
        {
            Debug.LogWarning("Only the server can assign roles.");
        }
    }
}