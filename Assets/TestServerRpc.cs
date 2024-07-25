using UnityEngine;
using Unity.Netcode;

public class TestServerRpc : NetworkBehaviour
{
    private void Update()
    {
        if (IsLocalPlayer && Input.GetKeyDown(KeyCode.T))
        {
            TestRpcServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestRpcServerRpc()
    {
        Debug.Log("Server received TestRpc call.");
    }
}