using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using GameFramework.Network.Movement;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    DefaultPlayerActions InputActions;
    public Transform camTransform;

    [SerializeField]
    public float recognizeDistance;

    [SerializeField]
    public LayerMask layerMask;

    [SerializeField]
    NetworkMovementComponent playerMovement;
    bool taskStarted = false;
    GameObject taskObject;

    private Voting voting;

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cvm =
            camTransform.gameObject.GetComponent<CinemachineVirtualCamera>();

        if (IsOwner)
        {
            cvm.Priority = 1;
        }
        else
        {
            cvm.Priority = 0;
        }
    }

    private void Start()
    {
        // Cursor.lockState = CursorLockMode.Locked;
        voting = FindObjectOfType<Voting>();
    }

    private void Awake()
    {
        InputActions = new DefaultPlayerActions();
        InputActions.Player.Enable();
    }

    public void Update()
    {
        if (taskStarted)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            taskObject = null;
            // Cursor.lockState = CursorLockMode.Locked;
            Vector2 movementInput = InputActions.Player.Move.ReadValue<Vector2>();
            Vector2 lookInput = InputActions.Player.Look.ReadValue<Vector2>();
            if (IsClient && IsLocalPlayer)
            {
                playerMovement.ProcessLocalPlayerMovement(movementInput, lookInput);
            }
            else
            {
                playerMovement.ProcessSimulatedPlayerMovement();
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            var networkObject = ObjectRecognizer.Recognize(
                camTransform,
                recognizeDistance,
                layerMask
            );
            if (networkObject != null)
            {
                if (networkObject.gameObject.transform.tag == "NPC")
                {
                    GameObject text = networkObject.gameObject.GetComponent<NPCManager>().textObj;
                    if (text.activeSelf)
                    {
                        text.SetActive(false);
                    }
                    else
                    {
                        text.SetActive(true);
                    }
                }
                if (taskObject == null)
                {
                    taskObject = networkObject.gameObject;
                }
                if (
                    networkObject.IsSceneObject == true
                    && networkObject.gameObject.transform.tag == "Task"
                    && taskObject == networkObject.gameObject
                )
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    taskStarted = !taskStarted;
                    TaskManager.RunTask(taskObject, taskStarted);
                }
            }
        }

        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.R))
        {
            ReportServerRpc();
        }
    }

    [ServerRpc]
    private void ReportServerRpc()
    {
        voting.CallMeeting();
    }
}
