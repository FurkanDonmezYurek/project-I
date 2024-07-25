using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectRecognizer
{
    public static NetworkObject Recognize(Transform startPos, float distance, LayerMask layerMask)
    {
        Ray ray = new Ray(startPos.position, startPos.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distance, layerMask))
        {
            if (
                hit.collider.gameObject.TryGetComponent<NetworkObject>(
                    out NetworkObject networkObject
                )
            )
            {
                // //is it player?
                // if (networkObject.IsPlayerObject)
                // {
                //     Debug.Log("Player");
                // }
                //
                // //is it Object?
                // if (networkObject.IsSceneObject == true)
                // {
                //     Debug.Log("Object");
                // }


                return networkObject;
            }
        }

        return null;
    }
}
