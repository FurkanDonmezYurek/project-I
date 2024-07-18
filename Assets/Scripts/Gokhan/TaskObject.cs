using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TaskObject : NetworkBehaviour
{
    [Header("Player UI View")]
    [SerializeField] public GameObject imposterSabotageUI;
    [SerializeField] public GameObject otherTaskUI;

    private RoleAssignment roleAssignment;

    private void OnTriggerEnter(Collider other)
    {
        PlayerRole playerSelfRole = other.GetComponent<RoleAssignment>().role.Value;

        //Tetikleyiciye Giren Nesne Imposter ise
        if (playerSelfRole == PlayerRole.Hayalet)
        {
            imposterSabotageUI.SetActive(true);
            otherTaskUI.SetActive(false);
            print("Imposter algýlandý");
        }
        //Tetikleyiciye Giren Nesne Baþkabiþey ise
        else
        {
            otherTaskUI.SetActive(true);
            imposterSabotageUI.SetActive(false);
            print("Diðerleri algýlandý");
        }

    }
}
