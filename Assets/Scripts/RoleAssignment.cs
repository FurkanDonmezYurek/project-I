using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum PlayerRole
{
    Unassigned,
    Buyucu,
    Koylu,
    Asik,
    Avci,
    Hayalet
}

public class RoleAssignment : NetworkBehaviour
{
    public NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(PlayerRole.Unassigned);

    private void Start()
    {
        if (IsServer)
        {
            AssignRole((PlayerRole)Random.Range(1, System.Enum.GetValues(typeof(PlayerRole)).Length));
        }

        // Add a listener to the NetworkVariable to handle changes
        role.OnValueChanged += OnRoleChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AssignRoleServerRpc(PlayerRole newRole)
    {
        AssignRole(newRole);
    }

    private void AssignRole(PlayerRole newRole)
    {
        role.Value = newRole;
        Debug.Log($"Server received: {gameObject.name} is assigned to {newRole} role");
        EnableRelevantRoleScript(newRole);
    }

    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        Debug.Log($"Role changed: {gameObject.name} was {oldRole}, now {newRole}");
        EnableRelevantRoleScript(newRole);
    }

    private void EnableRelevantRoleScript(PlayerRole newRole)
    {
        // Disable all role scripts initially
        GetComponent<Hayalet>().enabled = false;
        GetComponent<Buyucu>().enabled = false;
        GetComponent<Asik>().enabled = false;
        GetComponent<Avci>().enabled = false;

        // Enable only the relevant script based on the assigned role
        switch (newRole)
        {
            case PlayerRole.Hayalet:
                GetComponent<Hayalet>().enabled = true;
                break;
            case PlayerRole.Buyucu:
                GetComponent<Buyucu>().enabled = true;
                break;
            case PlayerRole.Asik:
                GetComponent<Asik>().enabled = true;
                break;
            case PlayerRole.Avci:
                GetComponent<Avci>().enabled = true;
                break;
            default:
                break;
        }
    }

    private void OnValidate()
    {
        // This method allows you to change the role in the Unity Editor and see the changes immediately.
        if (!Application.isPlaying && role.Value != PlayerRole.Unassigned)
        {
            Debug.Log($"Editor: {gameObject.name} is assigned to {role.Value} role");
        }
    }

    private void OnDestroy()
    {
        // Remove the listener when the object is destroyed to avoid memory leaks
        role.OnValueChanged -= OnRoleChanged;
    }
}
