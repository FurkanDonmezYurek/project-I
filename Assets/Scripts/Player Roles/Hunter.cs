using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hunter : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    private PlayerMovement pl_movement;

    private void Start()
    {
        roleAssignment = GetComponent<RoleAssignment>();
        pl_movement = GetComponent<PlayerMovement>();

        if (roleAssignment == null)
        {
            Debug.LogError("RoleAssignment script not found on the player!");
        }
        else
        {
            Debug.Log("Avci role assigned and script initialized.");
        }
    }

    private void Update()
    {
        Debug.Log($"Avci Update: IsLocalPlayer: {IsLocalPlayer}, IsOwner: {IsOwner}");

        if (
            IsLocalPlayer
            && roleAssignment.role.Value == PlayerRole.Hunter
            && Input.GetKeyDown(KeyCode.K)
        )
        {
            Debug.Log("K key pressed. Attempting to find target to kill.");

            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );

            if (networkObject != null)
            {
                ulong targetId = networkObject.OwnerClientId;
                Debug.Log($"Target found: {networkObject.name} with ID {targetId}");
                KillPlayerServerRpc(targetId);
            }
            else
            {
                Debug.Log("No target found to kill.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void KillPlayerServerRpc(ulong targetId)
    {
        Debug.Log(
            $"Server received: {gameObject.name} wants to kill the player with ID {targetId}"
        );

        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject netObj = spawnedObject.Value;
            if (netObj.OwnerClientId == targetId)
            {
                RoleAssignment targetRoleAssignment = netObj.GetComponent<RoleAssignment>();
                if (targetRoleAssignment != null)
                {
                    if (targetRoleAssignment.role.Value == PlayerRole.Villager)
                    {
                        Debug.Log($"{netObj.name} is Koylu. Avci will be demoted to Koylu.");
                        roleAssignment.AssignRoleServerRpc(PlayerRole.Villager);
                    }
                    Debug.Log($"Target object found on server: {netObj.name}");
                    KillPlayerClientRpc(new NetworkObjectReference(netObj));
                }
                targetRoleAssignment.isDead = true;
                return;
            }
        }
        Debug.Log("Target object not found on server.");
    }

    [ClientRpc]
    private void KillPlayerClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            
            //for test purposes
            Renderer targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                Debug.Log($"Changing target's color to red: {targetObject.name}");
                //killed animation
                targetRenderer.material.color = Color.yellow;
            }
            else
            {
                Debug.Log("Renderer component not found on target.");
            }
        }
        else
        {
            Debug.Log("Failed to get NetworkObject from NetworkObjectReference.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestServerRpc()
    {
        Debug.Log($"Server received: {gameObject.name} called TestServerRpc.");
    }
}
