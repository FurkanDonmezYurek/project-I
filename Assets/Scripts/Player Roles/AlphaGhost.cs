using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AlphaGhost : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    private PlayerMovement pl_movement;
    private HeadHunter headHunter;
    private Animator animator;

    private float cooldownTime = 5f;
    private bool canKill = true;
    private float potionMechanicTime = 30f;
    private Coroutine potionMechanicCoroutine;

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
            StartPotionMechanicCoroutine();
        }

        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (IsLocalPlayer && !roleAssignment.isDead.Value && Input.GetMouseButtonDown(0) &&
            roleAssignment.role.Value == PlayerRole.AlphaGhost && canKill)
        {
            var networkObject = ObjectRecognizer.Recognize(pl_movement.camTransform, pl_movement.recognizeDistance,
                pl_movement.layerMask);

            Debug.Log("Mouse button pressed. Attempting to find target to kill.");

            if (networkObject != null)
            {
                ulong targetId = networkObject.OwnerClientId;
                Debug.Log($"Target found: {networkObject.name} with ID {targetId}");
                KillPlayerServerRpc(targetId);
                StartCoroutine(KillCooldown());
                //ResetPotionMechanicCoroutine();  ************ yanlis yerde ondan kullanilmiyor ********
            }
            else
            {
                Debug.Log("No target found to kill.");
            }
        }

        if (
            IsLocalPlayer
            && Input.GetKeyDown(KeyCode.X)
            && !roleAssignment.usedSkill
            && roleAssignment.role.Value == PlayerRole.AlphaGhost
        )
        {
            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );

            Debug.Log("X key pressed. Attempting to find target to transform into Hayalet.");

            if (networkObject != null)
            {
                ulong targetId = networkObject.OwnerClientId;
                Debug.Log($"Target found: {networkObject.name} with ID {targetId}");
                TransformToHayaletServerRpc(targetId);
            }
            else
            {
                Debug.Log("No target found to transform.");
            }
        }
        if (IsLocalPlayer && !roleAssignment.isDead.Value && Input.GetKeyDown(KeyCode.P) &&
            roleAssignment.role.Value == PlayerRole.AlphaGhost)
        {
            Debug.Log("Potion used.");
            ResetPotionMechanicCoroutine();
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
                    if (targetRoleAssignment.role.Value == PlayerRole.Lover)
                    {
                        Lover asikComponent = netObj.GetComponent<Lover>();
                        if (asikComponent != null && asikComponent.loverId != ulong.MaxValue)
                        {
                            KillPlayerServerRpc(asikComponent.loverId);
                            Debug.Log($"Asik {netObj.name} killed. Their lover will also die.");
                        }
                    }

                    if (targetRoleAssignment.role.Value == PlayerRole.HeadHunter)
                    {
                        headHunter = targetRoleAssignment.gameObject.GetComponent<HeadHunter>();
                        headHunter.roleAssignment.isDead.Value = true;
                        headHunter.MakeVekilHunterServerRpc();
                    }

                    Debug.Log($"Target object found on server: {netObj.name}");
                    targetRoleAssignment.isDead.Value = true;
                    KillPlayerClientRpc(new NetworkObjectReference(netObj));
                    targetRoleAssignment.UpdateIsDeadClientRpc(true);
                    ResetPotionMechanicCoroutine();
                }
                
                return;
            }
        }
        Debug.Log("Target object not found on server.");
    }

    [ServerRpc(RequireOwnership = false)]
    private void TransformToHayaletServerRpc(ulong targetId)
    {
        Debug.Log(
            $"Server received: {gameObject.name} wants to transform the player with ID {targetId} into a Hayalet"
        );
        foreach (var spawnedObject in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject netObj = spawnedObject.Value;
            if (netObj.OwnerClientId == targetId)
            {
                RoleAssignment targetRoleAssignment = netObj.GetComponent<RoleAssignment>();
                if (
                    targetRoleAssignment != null
                    && targetRoleAssignment.role.Value == PlayerRole.Villager
                )
                {
                    Debug.Log(
                        $"Target object found on server: {netObj.name} will be transformed into a Hayalet."
                    );
                    roleAssignment.usedSkill = true; //mark that skill is already used
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
            targetRoleAssignment.AssignRoleServerRpc(PlayerRole.Ghost);
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
            roleAssignment.NPCRequest();
            //rendereri sil
            Renderer targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.color = Color.black;
                targetObject.gameObject.GetComponent<Animator>().SetBool("IsDead", true);
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

    private IEnumerator KillCooldown()
    {
        canKill = false;
        yield return new WaitForSeconds(cooldownTime);
        canKill = true;
    }

    private void StartPotionMechanicCoroutine()
    {
        potionMechanicCoroutine = StartCoroutine(PotionMechanicCoroutine());
    }

    private void ResetPotionMechanicCoroutine()
    {
        if (potionMechanicCoroutine != null)
        {
            StopCoroutine(potionMechanicCoroutine);
        }

        StartPotionMechanicCoroutine();
    }

    private IEnumerator PotionMechanicCoroutine()
    {
        yield return new WaitForSeconds(potionMechanicTime);
        LoseHumanAppearance();
    }

    private void LoseHumanAppearance()
    {
        Renderer alphaGhostRenderer = GetComponentInChildren<Renderer>();
        if (alphaGhostRenderer != null)
        {
            alphaGhostRenderer.material.color = Color.grey;
            Debug.Log("Alpha Hayalet lost its human appearance.");
            animator.SetBool("ChangeGhost", true);
        }
        else
        {
            Debug.Log("Renderer not found on Alpha Hayalet.");
        }
    }
}