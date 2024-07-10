using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Asik : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    public ulong loverId = ulong.MaxValue;

    private void Start()
    {
        roleAssignment = GetComponent<RoleAssignment>();
        if (roleAssignment == null)
        {
            Debug.LogError("RoleAssignment script not found on the player!");
        }
        else
        {
            Debug.Log("Asik role assigned and script initialized.");
        }
    }

    private void Update()
    {
        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("L key pressed. Attempting to find target to make in love.");
            GameObject target = FindTarget();
            if (target != null)
            {
                ulong targetId = target.GetComponent<NetworkObject>().OwnerClientId;
                Debug.Log($"Target found: {target.name} with ID {targetId}");
                MakeInLoveServerRpc(targetId);
            }
            else
            {
                Debug.Log("No target found to make in love.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MakeInLoveServerRpc(ulong targetId)
    {
        Debug.Log($"Server received: {gameObject.name} wants to make the player with ID {targetId} in love.");
        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject netObj = spawnedObject.Value;
            if (netObj.OwnerClientId == targetId)
            {
                loverId = targetId;
                Debug.Log($"Target object found on server: {netObj.name} is now in love with {gameObject.name}");
                MakeInLoveClientRpc(new NetworkObjectReference(netObj));
                return;
            }
        }
        Debug.Log("Target object not found on server.");
    }

    [ClientRpc]
    private void MakeInLoveClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            Renderer targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.color = Color.magenta;
                Debug.Log($"Asik made {targetObject.name} in love.");
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
            RoleAssignment targetRoleAssignment = player.GetComponent<RoleAssignment>();
            if (targetRoleAssignment != null && player.GetComponent<NetworkObject>().OwnerClientId != NetworkObject.OwnerClientId)
            {
                return player;
            }
        }
        return null;
    }
}
