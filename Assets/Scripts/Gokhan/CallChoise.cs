using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CallChoise : NetworkBehaviour
{
    [Header("Player UI View")]
    [SerializeField] public GameObject callButtonUI;

    private RoleAssignment roleAssignment;

    private void OnTriggerEnter(Collider other)
    {
        PlayerRole playerSelfRole = other.GetComponent<RoleAssignment>().role.Value;

        //Tetikleyiciye Giren Nesne Oyuncu ise
        if (playerSelfRole != null)
        {
            callButtonUI.SetActive(true);
            print("Oyuncu Algýlandý");
        }

    }
}
