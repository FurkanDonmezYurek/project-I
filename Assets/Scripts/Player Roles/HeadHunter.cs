using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HeadHunter : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    private PlayerMovement pl_movement;

    public ulong vekilId = ulong.MaxValue;
    public bool isDead;

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
    }

    private void Update()
    {
        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.H))
        {
            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );

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

        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.V))
        {
            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );

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
                if (targetRoleAssignment != null)
                {
                    if (targetRoleAssignment.role.Value == PlayerRole.Koylu)
                    {
                        Debug.Log($"{netObj.name} is Koylu. Head Hunter will be demoted to Koylu.");
                        roleAssignment.AssignRoleServerRpc(PlayerRole.Koylu);
                    }
                    if (targetRoleAssignment.role.Value == PlayerRole.Asik)
                    {
                        Asik asikComponent = netObj.GetComponent<Asik>();
                        if (asikComponent != null && asikComponent.loverId != ulong.MaxValue)
                        {
                            KillPlayerServerRpc(asikComponent.loverId);
                            Debug.Log($"Asik {netObj.name} killed. Their lover will also die.");
                        }
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
        if (isDead && vekilId != ulong.MaxValue)
        {
            foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
            {
                NetworkObject netObj = spawnedObject.Value;
                if (netObj.OwnerClientId == vekilId && !netObj.GetComponent<Koylu>().isDead)
                {
                    Debug.Log($"Vekil {netObj.name} will be promoted to Head Hunter.");
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
            targetRoleAssignment.AssignRoleServerRpc(PlayerRole.Avci);
            Debug.Log($"Vekil {targetObject.name} is now a Head Hunter.");
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
                targetObject.GetComponent<Koylu>().isVekil = true;
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
