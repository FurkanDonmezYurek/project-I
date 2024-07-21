using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Koylu : NetworkBehaviour
{
    private RoleAssignment roleAssignment;

    public bool isVekil = false;
    public bool isDead = false;

    private void Start()
    {
        roleAssignment = GetComponent<RoleAssignment>();
        if (roleAssignment == null)
        {
            Debug.LogError("RoleAssignment script not found on the player!");
        }
        else
        {
            Debug.Log("Koylu role assigned and script initialized.");
        }
    }
}