using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    // NetworkVariable with default permissions: readable by everyone, writable by server
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    DefaultPlayerActions InputActions;
    public float speed;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        InputActions = new DefaultPlayerActions();
        InputActions.Player.Enable();
    }

    public void Update()
    {
        Vector2 moveDir = InputActions.Player.Move.ReadValue<Vector2>();
        if (IsServer && IsLocalPlayer)
        {
            Move(moveDir);
        }
        else if (IsLocalPlayer)
        {
            RequestMoveServerRpc(moveDir);
        }
    }

    void Move(Vector2 moveInput)
    {
        Vector3 newPos = new Vector3(moveInput.x, 0f, moveInput.y);

        rb.MovePosition(transform.position + newPos * Time.deltaTime * speed);
        // rb.Move(Player.transform.position + newPos * Time.deltaTime * speed, Quaternion.identity);
    }

    [ServerRpc]
    void RequestMoveServerRpc(Vector2 moveInput)
    {
        Move(moveInput);
    }
}
