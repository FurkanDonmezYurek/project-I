using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hayalet : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    private PlayerMovement pl_movement;

    private HeadHunter headHunter;
    
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
            Debug.Log("Hayalet role assigned and script initialized.");
        }
    }

    private void Update()
    {
        if (IsLocalPlayer && Input.GetMouseButtonDown(0))
        {
            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );
            
            Debug.Log("E key pressed. Attempting to find target to kill.");
            
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
        Debug.Log($"Server received: {gameObject.name} wants to kill the player with ID {targetId}");
        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject netObj = spawnedObject.Value;
            if (netObj.OwnerClientId == targetId)
            {
                RoleAssignment targetRoleAssignment = netObj.GetComponent<RoleAssignment>();
                if (targetRoleAssignment != null)
                {
                    if (targetRoleAssignment.role.Value == PlayerRole.Asik)
                    {
                        Asik asikComponent = netObj.GetComponent<Asik>();
                        if (asikComponent != null && asikComponent.loverId != ulong.MaxValue)
                        {
                            KillPlayerServerRpc(asikComponent.loverId);
                            Debug.Log($"Asik {netObj.name} killed. Their lover will also die.");
                        }
                    }
                    
                    //make the vekil of the headhunter a hunter, i hope so...
                    if (targetRoleAssignment.role.Value == PlayerRole.BaşAvcı)
                    {
                        headHunter = targetRoleAssignment.gameObject.GetComponent<HeadHunter>();
                        headHunter.isDead = true;
                        headHunter.MakeVekilHunterServerRpc();
                    }
                    
                    Debug.Log($"Target object found on server: {netObj.name}");
                    KillPlayerClientRpc(new NetworkObjectReference(netObj));
                }
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
            Renderer targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.color = Color.black;
                Debug.Log($"Hayalet killed {targetObject.name}.");
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
    
}