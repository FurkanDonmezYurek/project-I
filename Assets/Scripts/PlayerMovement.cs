using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerMovement : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    DefaultPlayerActions InputActions;
    Vector3 moveDir;
    public float speed;

    Vector2 mousePos;
    public float mouseSens;
    NetworkObject playerObject;

    private void Awake()
    {
        InputActions = new DefaultPlayerActions();
        InputActions.Player.Enable();
    }

    void Start()
    {
        // Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (IsOwner)
        {
            Move();
        }
    }

    void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Position.Value = PlayerPos();
        }
        else
        {
            RequestMoveServerRpc();
        }
    }

    [ServerRpc]
    void RequestMoveServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Position.Value = PlayerPos();
    }

    Vector3 PlayerPos()
    {
        moveDir = InputActions.Player.Move.ReadValue<Vector2>();
        this.transform.position += new Vector3(moveDir.x, 0f, moveDir.y) * speed;
        Vector3 newPos = transform.position;
        return newPos;
    }

    private void FixedUpdate() { }
}
