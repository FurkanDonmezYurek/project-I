using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

public class CallChoise : NetworkBehaviour
{
    [Header("Player UI View")]
    //[SerializeField] public GameObject callButtonUI;

    [SerializeField] private Transform meetingPoint; 
    private Voting voting;

    private PlayerMovement pl_movement;
    private void Start()
    {
        voting = FindObjectOfType<Voting>();
    }

    private void Update()
    {
        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.R))
        {
            var networkObject = ObjectRecognizer.Recognize(
                pl_movement.camTransform,
                pl_movement.recognizeDistance,
                pl_movement.layerMask
            );
            if (networkObject != null)
            {
                ReportServerRpc();
            }
            
        }
    }

    [ServerRpc]
    private void ReportServerRpc()
    {
        voting.CallMeeting();
    }
}
