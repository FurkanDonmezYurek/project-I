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
    public Vector2 minMaxRotationX;
    public Transform camTransform;

    [SerializeField]
    public float recognizeDistance;

    float turnSpeed;
    float cameraAngle;

    [SerializeField]
    public LayerMask layerMask;

    CharacterController cc;

    [SerializeField]
    NetworkMovementComponent playerMovement;
    bool taskStarted = false;
    GameObject taskObject;

    RoleAssignment roleAssignment;

    //GameManager gManager;
    public ulong loverId = 999;
    public ulong proxyId = 999;
    public bool isDead = false;

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
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Awake()
    {
        InputActions = new DefaultPlayerActions();
        InputActions.Player.Enable();
    }

    public void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            var networkObject = ObjectRecognizer.Recognize(
                camTransform,
                recognizeDistance,
                layerMask
            );

            if (networkObject != null)
            {
                taskObject = networkObject.gameObject;

                // if (networkObject.IsPlayerObject)
                // {
                //     Debug.Log("Player " + networkObject.NetworkObjectId);
                // }
                // else
                if (
                    networkObject.IsSceneObject == true
                    && networkObject.gameObject.transform.tag == "Task"
                )
                {
                    TaskManager.RunTask(taskObject);
                    taskStarted = true;
                }
            }
        }

        if (taskStarted && taskObject != null)
        {
            if (Vector3.Distance(transform.position, taskObject.transform.position) > 5f)
            {
                TaskManager.RunTask(taskObject);
                taskObject = null;
                taskStarted = false;
            }
        }
    }

    // void RotateCamera(float lookInputY)
    // {
    //     cameraAngle = Vector3.SignedAngle(
    //         transform.forward,
    //         camTransform.forward,
    //         camTransform.right
    //     );
    //     float cameraRotationAmount = lookInputY * turnSpeed * Time.deltaTime;
    //     float newCameraAngle = cameraAngle - cameraRotationAmount;
    //     if (newCameraAngle <= minMaxRotationX.x && newCameraAngle >= minMaxRotationX.y)
    //     {
    //         camTransform.RotateAround(
    //             camTransform.position,
    //             camTransform.right,
    //             -lookInputY * turnSpeed * Time.deltaTime
    //         );
    //     }
    // }
}
