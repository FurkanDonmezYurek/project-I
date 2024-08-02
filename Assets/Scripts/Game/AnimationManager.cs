using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AnimationManager : NetworkBehaviour
{
    private Animator animator;
    private PlayerRole currentRole;
    private RoleAssignment _roleAssignment;

    private static readonly int RoleID = Animator.StringToHash("RoleID");

    private static readonly Dictionary<PlayerRole, int> roleIndices = new Dictionary<PlayerRole, int>
    {
        { PlayerRole.Lover, 0 },
        { PlayerRole.Ghost, 1 },
        { PlayerRole.AlphaGhost,1},
        { PlayerRole.Hunter, 2 },
        { PlayerRole.HeadHunter,2}
    };

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found!");
           
        }

        _roleAssignment = GetComponent<RoleAssignment>();


    }

  

    public void UpdateAnimationState()
    {
        if (animator != null)
        {
            if (roleIndices.TryGetValue(_roleAssignment.role.Value, out int index))
            {
                animator.SetInteger(RoleID, index);
                Debug.Log($"Animator parameter updated: RoleID set to {index} for role {currentRole}");
            }
            else
            {
                animator.SetInteger(RoleID, -1);  // Varsayılan değer
                Debug.LogWarning($"Role {currentRole} is not recognized. RoleID set to default.");
            }
        }
    } 
    
}