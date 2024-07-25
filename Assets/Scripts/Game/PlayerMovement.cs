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
        Cursor.lockState = CursorLockMode.Locked;
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
            Cursor.lockState = CursorLockMode.Locked;
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            var networkObject = ObjectRecognizer.Recognize(
                camTransform,
                recognizeDistance,
                layerMask
            );

            if (networkObject != null)
            {
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
    }
}
