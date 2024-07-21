using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AlphaHayalet : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    private PlayerMovement pl_movement;
    private HeadHunter headHunter;

    //private bool usedTransformAbility = false;
    
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
            Debug.Log("Alpha Hayalet role assigned and script initialized.");
        }
    }

    private void Update()
    {
        if (IsLocalPlayer && Input.GetMouseButtonDown(0) && roleAssignment.role.Value == PlayerRole.AlphaHayalet)
        {
            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );

            Debug.Log("Left mouse button clicked. Attempting to find target to kill.");

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

        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.T) && !roleAssignment.usedSkill && roleAssignment.role.Value == PlayerRole.AlphaHayalet)
        {
            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );

            Debug.Log("T key pressed. Attempting to find target to transform into Hayalet.");

            if (networkObject != null) 
            {
                ulong targetId = networkObject.OwnerClientId;
                Debug.Log($"Target found: {networkObject.name} with ID {targetId}");
                TransformToHayaletServerRpc(targetId);
                roleAssignment.usedSkill = true;
            }
            else
            {
                Debug.Log("No target found to transform.");
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

                    if (targetRoleAssignment.role.Value == PlayerRole.BaşAvcı)
                    {
                        headHunter = targetRoleAssignment.gameObject.GetComponent<HeadHunter>();
                        headHunter.roleAssignment.isDead = true;
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

    [ServerRpc(RequireOwnership = false)]
    private void TransformToHayaletServerRpc(ulong targetId)
    {
        Debug.Log($"Server received: {gameObject.name} wants to transform the player with ID {targetId} into a Hayalet");
        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject netObj = spawnedObject.Value;
            if (netObj.OwnerClientId == targetId)
            {
                RoleAssignment targetRoleAssignment = netObj.GetComponent<RoleAssignment>();
                if (targetRoleAssignment != null && targetRoleAssignment.role.Value == PlayerRole.Koylu)
                {
                    Debug.Log($"Target object found on server: {netObj.name} will be transformed into a Hayalet.");
                    TransformToHayaletClientRpc(new NetworkObjectReference(netObj));
                }
                return;
            }
        }
        Debug.Log("Target object not found on server.");
    }

    [ClientRpc]
    private void TransformToHayaletClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            RoleAssignment targetRoleAssignment = targetObject.GetComponent<RoleAssignment>();
            targetRoleAssignment.AssignRoleServerRpc(PlayerRole.Hayalet);
            Renderer targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.color = Color.grey;
                Debug.Log($"Alpha Hayalet transformed {targetObject.name} into a Hayalet.");
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

    [ClientRpc]
    private void KillPlayerClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            Renderer targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.color = Color.black;
                Debug.Log($"Alpha Hayalet killed {targetObject.name}.");
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
