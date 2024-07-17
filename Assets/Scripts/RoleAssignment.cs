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
    BaşAvcı,
    Avci,
    Hayalet,
    AlphaHayalet
}

public class RoleAssignment : NetworkBehaviour
{
    public NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(PlayerRole.Unassigned);
    
    public bool usedSkill = false;
    public bool isDead = false;
    
    private void Start()
    {
        if (IsServer)
        {
            AssignRole(
                (PlayerRole)Random.Range(1, System.Enum.GetValues(typeof(PlayerRole)).Length)
            );
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
        RemoveAllRoleComponents();

        switch (newRole)
        {
            case PlayerRole.Koylu:
                gameObject.AddComponent<Koylu>();
                break;
            case PlayerRole.Hayalet:
                gameObject.AddComponent<Hayalet>();
                break;
            case PlayerRole.AlphaHayalet:
                gameObject.AddComponent<AlphaHayalet>();
                break;
            case PlayerRole.Buyucu:
                gameObject.AddComponent<Buyucu>();
                break;
            case PlayerRole.Asik:
                gameObject.AddComponent<Asik>();
                break;
            case PlayerRole.Avci:
                gameObject.AddComponent<Avci>();
                break;
            case PlayerRole.BaşAvcı:
                gameObject.AddComponent<HeadHunter>();
                break;
        }
    }

    private void RemoveAllRoleComponents()
    {
        Destroy(GetComponent<Koylu>());
        Destroy(GetComponent<Hayalet>());
        Destroy(GetComponent<AlphaHayalet>());
        Destroy(GetComponent<Buyucu>());
        Destroy(GetComponent<Asik>());
        Destroy(GetComponent<Avci>());
        Destroy(GetComponent<HeadHunter>());
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && role.Value != PlayerRole.Unassigned)
        {
            Debug.Log($"Editor: {gameObject.name} is assigned to {role.Value} role");
            EnableRelevantRoleScript(role.Value);
        }
    }

    private void OnDestroy()
    {
        // Remove the listener when the object is destroyed to avoid memory leaks
        role.OnValueChanged -= OnRoleChanged;
    }
}