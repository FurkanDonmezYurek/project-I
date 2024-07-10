using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Koylu : MonoBehaviour
{
    private RoleAssignment roleAssignment;
    
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