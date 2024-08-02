using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HeadHunter : NetworkBehaviour
{
    public RoleAssignment roleAssignment;
    private PlayerMovement pl_movement;
    private Animator Animator;

    public ulong vekilId = ulong.MaxValue;

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
            Debug.Log("Head Hunter role assigned and script initialized.");
        }

        Animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (IsLocalPlayer && !roleAssignment.isDead.Value && Input.GetMouseButtonDown(0) && roleAssignment.role.Value == PlayerRole.HeadHunter)
        {
            var networkObject = ObjectRecognizer.Recognize(pl_movement.camTransform, pl_movement.recognizeDistance, pl_movement.layerMask);

            Debug.Log("H key pressed. Attempting to find target to kill.");

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

        if (IsLocalPlayer && !roleAssignment.isDead.Value && Input.GetKeyDown(KeyCode.X) && !roleAssignment.usedSkill && roleAssignment.role.Value == PlayerRole.HeadHunter)
        {
            var networkObject = ObjectRecognizer.Recognize(pl_movement.camTransform, pl_movement.recognizeDistance, pl_movement.layerMask);

            Debug.Log("V key pressed. Attempting to find target to make 'vekil'.");

            if (networkObject != null)
            {
                ulong targetId = networkObject.GetComponent<NetworkObject>().OwnerClientId;
                Debug.Log($"Target found: {networkObject.name} with ID {targetId}");
                VekilServerRpc(targetId);
            }
            else
            {
                Debug.Log("No target found to make 'vekil'.");
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
                if (targetRoleAssignment != null && !targetRoleAssignment.isDead.Value) 
                {
                    if (targetRoleAssignment.role.Value == PlayerRole.Villager)
                    {
                        Debug.Log($"{netObj.name} is Koylu. Head Hunter will be demoted to Koylu.");
                        roleAssignment.AssignRoleServerRpc(PlayerRole.Villager);
                    }
                    if (targetRoleAssignment.role.Value == PlayerRole.Lover)
                    {
                        Lover asikComponent = netObj.GetComponent<Lover>();
                        if (asikComponent != null && asikComponent.loverId != ulong.MaxValue)
                        {
                            KillPlayerServerRpc(asikComponent.loverId);
                            Debug.Log($"Asik {netObj.name} killed. Their lover will also die.");
                        }
                    }
                    Debug.Log($"Target object found on server: {netObj.name}");
                    targetRoleAssignment.isDead.Value = true;
                    targetRoleAssignment.UpdateIsDeadClientRpc(true);
                    KillPlayerClientRpc(new NetworkObjectReference(netObj));
                }
                else
                {
                    Debug.Log($"Target {netObj.name} is already dead.");
                }
                return;
            }
        }
        Debug.Log("Target object not found on server.");
    }

    [ServerRpc(RequireOwnership = false)]
    private void VekilServerRpc(ulong targetId)
    {
        Debug.Log($"Server received: {gameObject.name} wants to make the player with ID {targetId} its vekil.");
        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject netObj = spawnedObject.Value;
            if (netObj.OwnerClientId == targetId)
            {
                vekilId = targetId;
                Debug.Log($"Target object found on server: {netObj.name} is 'vekil' of {gameObject.name}");
                VekilClientRpc(new NetworkObjectReference(netObj));
                return;
            }
        }
        Debug.Log("Target object not found on server.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void MakeVekilHunterServerRpc()
    {
        if (roleAssignment.isDead.Value && vekilId != ulong.MaxValue)
        {
            foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
            {
                NetworkObject netObj = spawnedObject.Value;
                if (netObj.OwnerClientId == vekilId && !netObj.GetComponent<RoleAssignment>().isDead.Value 
                    && netObj.GetComponent<RoleAssignment>().role.Value != PlayerRole.Ghost && 
                    netObj.GetComponent<RoleAssignment>().role.Value != PlayerRole.AlphaGhost)
                {
                    Debug.Log($"Vekil {netObj.name} will be promoted to Hunter.");
                    MakeVekilHunterClientRpc(new NetworkObjectReference(netObj));
                    return;
                }
            }
        }
        Debug.Log("Vekil not found or Head Hunter is not dead.");
    }

    [ClientRpc]
    private void MakeVekilHunterClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            RoleAssignment targetRoleAssignment = targetObject.GetComponent<RoleAssignment>();
            targetRoleAssignment.AssignRoleServerRpc(PlayerRole.Hunter);
            Debug.Log($"Vekil {targetObject.name} is now a Hunter.");
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
                targetRenderer.material.color = Color.yellow;
                Debug.Log($"Head hunter killed {targetObject.name}.");
                Animator.SetTrigger("HunterSkill");
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
    private void VekilClientRpc(NetworkObjectReference target)
    {
        if (target.TryGet(out NetworkObject targetObject))
        {
            Renderer targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetObject.GetComponent<RoleAssignment>().isVekil = true;
                targetRenderer.material.color = Color.blue;
                Debug.Log($"Head hunter made {targetObject.name} vekil.");
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
