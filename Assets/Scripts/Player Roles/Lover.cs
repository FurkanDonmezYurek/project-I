using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Lover : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    private PlayerMovement pl_movement;
    private Animator Animator;

    public ulong loverId = ulong.MaxValue;

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
            Debug.Log("Asik role assigned and script initialized.");
        }

        Animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (
            IsLocalPlayer
            && Input.GetKeyDown(KeyCode.L)
            && !roleAssignment.usedSkill
            && roleAssignment.role.Value == PlayerRole.Lover
        )
        {
            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );

            Debug.Log("L key pressed. Attempting to find target to make in love.");

            if (networkObject != null)
            {
                ulong targetId = networkObject.GetComponent<NetworkObject>().OwnerClientId;
                Debug.Log($"Target found: {networkObject.name} with ID {targetId}");
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
        Debug.Log(
            $"Server received: {gameObject.name} wants to make the player with ID {targetId} in love."
        );
        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject netObj = spawnedObject.Value;
            if (netObj.OwnerClientId == targetId)
            {
                loverId = targetId;
                Debug.Log(
                    $"Target object found on server: {netObj.name} is now in love with {gameObject.name}"
                );
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
                Animator.SetTrigger("LoverSkill");
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
