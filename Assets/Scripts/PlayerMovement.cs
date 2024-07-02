using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    // NetworkVariable with default permissions: readable by everyone, writable by server
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    DefaultPlayerActions InputActions;
    Vector3 moveDir;
    public float speed = 5f;

    private void Awake()
    {
        InputActions = new DefaultPlayerActions();
        InputActions.Player.Enable();
    }



    void Update()
    {
        if (IsOwner)
        {
            HandleMovement();
        
        }    
            // Non-owners update their position based on the server value
            transform.position = Position.Value;
        
    }

    void HandleMovement()
    {
        moveDir = InputActions.Player.Move.ReadValue<Vector2>();
        Vector3 newPos = transform.position + new Vector3(moveDir.x, 0f, moveDir.y) * speed ;

        // Update local position for smooth client-side movement
        transform.position = newPos;

        if (NetworkManager.Singleton.IsServer)
        {
            Position.Value = newPos;
        }
        else
        {
            Debug.Log("Client is requesting move to new position: " + newPos);
            RequestMoveServerRpc(newPos);
        }
    }

    [ServerRpc]
    void RequestMoveServerRpc(Vector3 newPos, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("Server received move request to new position: " + newPos);
        Position.Value = newPos;
    }


}