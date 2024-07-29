using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class NPCManager : NetworkBehaviour
{
    public float radius;

    [Range(0, 360)]
    public float angle;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeePlayer;

    public string victum = "";
    public string killer = "";
    public string[] witnesses;

    ApiManager apiManager;

    public NetworkVariable<string> aiPrompt = new NetworkVariable<string>();
    public TMP_Text localAiPrompt;

    private void Awake()
    {
        localAiPrompt.text = "Hava Ne Kadar Güzel Yhaaa";
        if (IsHost)
        {
            aiPrompt.Value = localAiPrompt.text;
        }
    }

    private void Start()
    {
        apiManager = GetComponent<ApiManager>();

        // StartCoroutine(FOVRoutine());
    }

    // private IEnumerator FOVRoutine()
    // {
    //     WaitForSeconds wait = new WaitForSeconds(0.2f);

    //     while (true)
    //     {
    //         yield return wait;
    //         FieldOfViewCheck();
    //     }
    // }

    public async void FieldOfViewCheck(GameObject playerSelf)
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform[] targetArray = new Transform[rangeChecks.Length];
            Vector3[] directionTargetArray = new Vector3[rangeChecks.Length];
            for (int i = 0; i < rangeChecks.Length; i++)
            {
                targetArray.SetValue(rangeChecks[i].transform, i);
                Vector3 direction = (targetArray[i].position - transform.position).normalized;
                directionTargetArray.SetValue(direction, i);

                if (Vector3.Angle(transform.forward, directionTargetArray[i]) < angle / 2)
                {
                    float distanceToTarget = Vector3.Distance(
                        transform.position,
                        targetArray[i].position
                    );
                    if (
                        !Physics.Raycast(
                            transform.position,
                            directionTargetArray[i],
                            distanceToTarget,
                            obstructionMask
                        )
                    )
                    {
                        // canSeePlayer = true;
                        Debug.Log(targetArray[i].gameObject.name);
                        if (
                            targetArray[i].gameObject.TryGetComponent(
                                out RoleAssignment roleAssignment
                            )
                        )
                        {
                            if (roleAssignment.isDead)
                            {
                                victum = targetArray[i].name;
                            }
                            if (!roleAssignment.isDead && !roleAssignment.usedSkill)
                            {
                                witnesses.SetValue(targetArray[i].name, 0);
                            }

                            killer = playerSelf.transform.name;
                        }
                    }
                    else
                    {
                        // canSeePlayer = false;
                        return;
                    }
                }
                else
                {
                    // canSeePlayer = false;
                    return;
                }
            }
        }

        if (victum != "" && killer != "")
        {
            localAiPrompt.text = await apiManager.QueryGemini(killer, victum, witnesses);
            if (IsHost)
            {
                aiPrompt.Value = localAiPrompt.text;
            }
            else
            {
                AiPromptServerRpc();
            }
        }
        else
        {
            return;
        }
        // if (rangeChecks.Length != 0)
        // {
        //     Transform target = rangeChecks[0].transform;
        //     Vector3 directionToTarget = (target.position - transform.position).normalized;

        //     if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
        //     {
        //         float distanceToTarget = Vector3.Distance(transform.position, target.position);
        //         if (
        //             !Physics.Raycast(
        //                 transform.position,
        //                 directionToTarget,
        //                 distanceToTarget,
        //                 obstructionMask
        //             )
        //         )
        //         {
        //             canSeePlayer = true;
        //             if (target.gameObject.TryGetComponent(out RoleAssignment roleAssignment))
        //             {
        //                 if (roleAssignment.isDead)
        //                 {
        //                     victum = target.name;
        //                 }
        //                 if (roleAssignment.usedSkill)
        //                 {
        //                     killer = target.name;
        //                 }
        //                 if (!roleAssignment.isDead && !roleAssignment.usedSkill)
        //                 {
        //                     witnesses.SetValue(target.name, 0);
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             canSeePlayer = false;
        //         }
        //     }
        //     else
        //     {
        //         canSeePlayer = false;
        //     }
        // }
        // else if (canSeePlayer)
        // {
        //     canSeePlayer = false;
        // }
    }

    [ServerRpc]
    public void AiPromptServerRpc()
    {
        aiPrompt.Value = localAiPrompt.text;
    }
}
