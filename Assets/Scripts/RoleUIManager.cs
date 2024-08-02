using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class RoleUIManager : MonoBehaviour
{
    public GameObject potionPanel;
    public GameObject skillPanel;
    public GameObject changePanel;
    public GameObject taskPanel;

    public GameObject player1;
    public GameObject player2;
    public TextMeshProUGUI roleText;

    private RoleAssignment roleAssignment;

    private void Start()
    {
        HideAllPanels();

        // Find the RoleAssignment component on the player object
        roleAssignment = GetComponent<RoleAssignment>();

        // Find UI elements at runtime
        //player1 = GameObject.Find("AssigneePlayerPanel");
        //player2 = GameObject.Find("Nick");
        //potionPanel = GameObject.Find("PotionPanel");
        //skillPanel = GameObject.Find("SkillPanel");
        //changePanel = GameObject.Find("ChangePanel");
        //taskPanel = GameObject.Find("TaskPanel");
        //roleText = GameObject.Find("RoleText").GetComponent<TextMeshProUGUI>();

        if (roleAssignment != null)
        {
            roleAssignment.role.OnValueChanged += OnRoleChanged;
            OnRoleChanged(PlayerRole.Unassigned, roleAssignment.role.Value); // Initialize role text on start
        }
    }

    private void OnDestroy()
    {
        if (roleAssignment != null)
        {
            roleAssignment.role.OnValueChanged -= OnRoleChanged;
        }
    }

    private void OnRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        UpdateRoleUI(newRole);
        UpdateRoleText(newRole);
    }

    public void UpdateRoleUI(PlayerRole role)
    {
        HideAllPanels();

        // Show panel based on the new role
        switch (role)
        {
            case PlayerRole.Wizard:
                player1.SetActive(true);
                player2.SetActive(true);
                taskPanel.SetActive(true);
                break;
            case PlayerRole.Villager:
                player1.SetActive(true);
                player2.SetActive(true);
                taskPanel.SetActive(true);
                break;
            case PlayerRole.Lover:
                player1.SetActive(true);
                player2.SetActive(true);
                taskPanel.SetActive(true);
                potionPanel.SetActive(true);
                break;
            case PlayerRole.HeadHunter:
                player1.SetActive(true);
                player2.SetActive(true);
                skillPanel.SetActive(true);
                taskPanel.SetActive(true);
                break;
            case PlayerRole.Hunter:
                player1.SetActive(true);
                player2.SetActive(true);
                taskPanel.SetActive(true);
                skillPanel.SetActive(true);
                break;
            case PlayerRole.Ghost:
                player1.SetActive(true);
                player2.SetActive(true);
                skillPanel.SetActive(true);
                changePanel.SetActive(true);
                break;
            case PlayerRole.AlphaGhost:
                player1.SetActive(true);
                player2.SetActive(true);
                skillPanel.SetActive(true);
                changePanel.SetActive(true);
                break;
            case PlayerRole.Unassigned:
            default:
                // Do nothing or show a default UI
                break;
        }
    }

    public void UpdateRoleText(PlayerRole role)
    {
        if (roleText != null)
        {
            roleText.text = role.ToString();
        }
    }

    private void HideAllPanels()
    {
        if (potionPanel != null) potionPanel.SetActive(false);
        if (skillPanel != null) skillPanel.SetActive(false);
        if (changePanel != null) changePanel.SetActive(false);
        if (taskPanel != null) taskPanel.SetActive(false);
        if (player1 != null) player1.SetActive(false);
        if (player2 != null) player2.SetActive(false);
    }
}
