using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameOverManager : NetworkBehaviour
{
    public float timeRemaining = 20.0f;

    private void Update()
    {
        timeRemaining -= Time.deltaTime;
        if (IsServer)
        {
            CheckGameOver();
        }
    }

    private void CheckGameOver()
    {
        if (timeRemaining <= 0)
        {
            EndGame("Reactor timer finished!");
            return;
        }

        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        int aliveCount = 0;
        int ghostCount = 0;
        int alphaGhostCount = 0;
        int villagerCount = 0;

        foreach (GameObject player in players)
        {
            RoleAssignment roleAssignment = player.GetComponent<RoleAssignment>();
            if (roleAssignment != null && !roleAssignment.isDead.Value)
            {
                aliveCount++;
                if (roleAssignment.role.Value == PlayerRole.Ghost)
                {
                    ghostCount++;
                }
                else if (roleAssignment.role.Value == PlayerRole.AlphaGhost)
                {
                    alphaGhostCount++;
                }
                else if (roleAssignment.role.Value == PlayerRole.Villager)
                {
                    villagerCount++;
                }
            }
        }

        if (aliveCount == 1)
        {
            EndGame("Only one player is alive!");
            return;
        }

        if (ghostCount + alphaGhostCount == aliveCount || ghostCount + alphaGhostCount == 0)
        {
            EndGame("All alive players are ghosts/alpha ghosts or all ghosts/alpha ghosts are dead!");
            return;
        }

        if (villagerCount == 0)
        {
            EndGame("All villagers are dead!");
            return;
        }
    }

    private void EndGame(string reason)
    {
        Debug.Log("Game Over: " + reason);
        GameOverClientRpc(reason);
        // Add additional server-side game over logic here, such as stopping the game or announcing the winner.
        JoinLobby.ReturnMainMenu();
    }

    [ClientRpc]
    private void GameOverClientRpc(string reason)
    {
        JoinLobby.ReturnMainMenu();
        // Implement your client-side game over logic here
        Debug.Log("Game Over: " + reason);
        print("GAME IS OVER");
    }
}
