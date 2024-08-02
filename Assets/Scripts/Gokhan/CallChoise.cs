using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CallChoice : NetworkBehaviour
{
    //[SerializeField] private Transform meetingPoint;
    // [SerializeField] private AudioClip bellSound;
    // private AudioSource audioSource;
    
    private Voting voting;
    private PlayerMovement pl_movement;

    private void Start()
    {
        voting = FindObjectOfType<Voting>();
        //audioSource = GetComponent<AudioSource>();
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

            if (networkObject != null && networkObject.CompareTag("CallButton"))
            {
                ReportServerRpc();
            }
            
        }
    }

    [ServerRpc]
    private void ReportServerRpc()
    {
        voting.CallMeeting();
        //RingBellClientRpc();
    }

    // [ClientRpc]
    // private void RingBellClientRpc()
    // {
    //     audioSource.PlayOneShot(bellSound);
    // }
}