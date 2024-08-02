using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Ghost : NetworkBehaviour
{
    private RoleAssignment roleAssignment;
    private PlayerMovement pl_movement;
    private HeadHunter headHunter;
    private Animator Animator;

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
            Debug.Log("Hayalet role assigned and script initialized.");
            StartPotionMechanicCoroutine();
        }

        Animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (IsLocalPlayer && !roleAssignment.isDead.Value && Input.GetMouseButtonDown(0) &&
            roleAssignment.role.Value == PlayerRole.Ghost && canKill)
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
               // ResetPotionMechanicCoroutine();
            }
            else
            {
                Debug.Log("No target found to kill.");
            }
        }

        if (IsLocalPlayer && !roleAssignment.isDead.Value && Input.GetKeyDown(KeyCode.P) &&
            roleAssignment.role.Value == PlayerRole.Ghost)
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
                if (targetRoleAssignment != null && !targetRoleAssignment.isDead.Value)
                {
                    targetRoleAssignment.isDead.Value = true; 
                    if (targetRoleAssignment.role.Value == PlayerRole.Lover)
                    {
                        Lover asikComponent = netObj.GetComponent<Lover>();
                        if (asikComponent != null && asikComponent.loverId != ulong.MaxValue)
                        {
                            KillPlayerServerRpc(asikComponent.loverId);
                            Debug.Log($"Asik {netObj.name} killed. Their lover will also die.");
                        }
                    }
                    // Make the vekil of the headhunter a hunter
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
                Animator.SetTrigger("GhostAttack");
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
            roleAssignment.NPCRequest();

            Renderer targetRenderer = targetObject.GetComponentInChildren<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.color = Color.black;
                //targetObject.gameObject.GetComponent<Animator>().SetBool("IsDead",true); ///////not so sure -_-
                //targetObject.gameObject.SetActive(false);
                Debug.Log($"Ghost killed {targetObject.name}.");
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
        Debug.Log("Kill skill is on cooldown.");
        yield return new WaitForSeconds(cooldownTime);
        canKill = true; 
        Debug.Log("Kill skill is ready to use again.");
    }

    private void StartPotionMechanicCoroutine()
    {
        if (potionMechanicCoroutine != null)
        {
            StopCoroutine(potionMechanicCoroutine);
        }
        potionMechanicCoroutine = StartCoroutine(PotionMechanicCoroutine());
    }

    private void ResetPotionMechanicCoroutine()
    {
        if (potionMechanicCoroutine != null)
        {
            StopCoroutine(potionMechanicCoroutine);
        }
        RegainHumanAppearance();
        potionMechanicCoroutine = StartCoroutine(PotionMechanicCoroutine());
    }

    private IEnumerator PotionMechanicCoroutine()
    {
        yield return new WaitForSeconds(potionMechanicTime);
        LoseHumanAppearance();
    }

    private void LoseHumanAppearance()
    {
        Renderer ghostRenderer = GetComponentInChildren<Renderer>();
        if (ghostRenderer != null)
        {
            ghostRenderer.material.color = Color.grey; 
            Debug.Log("Hayalet lost its human appearance.");
            Animator.SetBool("ChangeGhost", true);
        }
        else
        {
            Debug.Log("Renderer not found on Hayalet.");
        }
    }
    private void RegainHumanAppearance()
    {
        Renderer ghostRenderer = GetComponentInChildren<Renderer>();
        if (ghostRenderer != null)
        {
            ghostRenderer.material.color = Color.red; 
            Debug.Log("Hayalet regained its human appearance.");
        }
        else
        {
            Debug.Log("Renderer not found on Hayalet.");
        }
    }


}