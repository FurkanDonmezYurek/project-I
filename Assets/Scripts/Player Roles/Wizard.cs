using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Wizard : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    
    private void Start()
    {
        roleAssignment = GetComponent<RoleAssignment>();
        if (roleAssignment == null)
        {
            Debug.LogError("RoleAssignment script not found on the player!");
        }
        else
        {
            Debug.Log("Buyucu role assigned and script initialized.");
        }
    }

    private void Update()
    {
        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.R) && roleAssignment.role.Value == PlayerRole.Wizard)
        {
            Debug.Log("R key pressed. Attempting to find target to revive.");
            GameObject target = FindTarget();
            if (target != null && target != gameObject)
            {
                ulong targetId = target.GetComponent<NetworkObject>().OwnerClientId;
                Debug.Log($"Target found: {target.name} with ID {targetId}");
                RevivePlayerServerRpc(targetId);
            }
            else
            {
                Debug.Log("No target found or target is self.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RevivePlayerServerRpc(ulong targetId)
    {
        Debug.Log($"Server received: {gameObject.name} wants to kill the player with ID {targetId}");
        
        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject netObj = spawnedObject.Value;
            if (netObj.OwnerClientId == targetId)
            {
                Debug.Log($"Target object found on server: {netObj.name}");
                RevivePlayerClientRpc(new NetworkObjectReference(netObj));
                return;
            }
        }
        Debug.Log("Target object not found on server.");
    }

    [ClientRpc]
    private void RevivePlayerClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            Renderer targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.color = Color.green;
                Debug.Log($"Buyucu revived {targetObject.name} successfully.");
            }
            else
            {
                Debug.Log("Target renderer not found.");
            }
        }
        else
        {
            Debug.Log("Target object not found on client.");
        }
    }

    private GameObject FindTarget()
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player != gameObject) 
            {
                RoleAssignment targetRoleAssignment = player.GetComponent<RoleAssignment>();
                if (targetRoleAssignment != null)
                {
                    Debug.Log($"Checking player {player.name} with role {targetRoleAssignment.role.Value}");
                    if (targetRoleAssignment.role.Value == PlayerRole.Villager) 
                    {
                        Debug.Log($"Target player {player.name} found.");
                        return player;
                    }
                }
            }
        }
        Debug.Log("No valid target found.");
        return null;
    }
}
