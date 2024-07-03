using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    Rigidbody rb;
    DefaultPlayerActions InputActions;
    public float speed;
    public float turnSpeed;
    public Vector2 minMaxRotationX;
    public Transform camTransform;
    float cameraAngle;

    CharacterController cc;

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
        rb = GetComponent<Rigidbody>();
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
        Vector2 moveDir = InputActions.Player.Move.ReadValue<Vector2>();
        Vector2 lookInput = InputActions.Player.Look.ReadValue<Vector2>();
        if (IsServer && IsLocalPlayer)
        {
            Move(moveDir);
            Rotate(lookInput);
        }
        else if (IsLocalPlayer)
        {
            RequestMoveServerRpc(moveDir, lookInput);
        }
    }

    void Move(Vector2 moveInput)
    {
        Vector3 movement = moveInput.x * camTransform.right + moveInput.y * camTransform.forward;
        movement.y = 0;
        cc.Move(movement * speed * Time.deltaTime);
    }

    void Rotate(Vector2 lookInput)
    {
        transform.RotateAround(
            transform.position,
            transform.up,
            lookInput.x * turnSpeed * Time.deltaTime
        );
        RotateCamera(lookInput.y);
    }

    void RotateCamera(float lookInputY)
    {
        cameraAngle = Vector3.SignedAngle(
            transform.forward,
            camTransform.forward,
            camTransform.right
        );
        float cameraRotationAmount = lookInputY * turnSpeed * Time.deltaTime;
        float newCameraAngle = cameraAngle - cameraRotationAmount;
        if (newCameraAngle <= minMaxRotationX.x && newCameraAngle >= minMaxRotationX.y)
        {
            camTransform.RotateAround(
                camTransform.position,
                camTransform.right,
                -lookInputY * turnSpeed * Time.deltaTime
            );
        }
    }

    [ServerRpc]
    void RequestMoveServerRpc(Vector2 moveInput, Vector2 lookInput)
    {
        Move(moveInput);
        Rotate(lookInput);
    }
}
