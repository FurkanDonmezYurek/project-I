using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;

public class PlayerName : MonoBehaviour
{
    public TMP_InputField playerName;
    public TMP_Text playerNameMenu;
    public GameObject playerProfile;
    public TMP_Text warningMessage;

    void Start()
    {
        if (PlayerPrefs.GetString("PlayerName") == "")
        {
            playerProfile.SetActive(true);
        }
        else
        {
            playerNameMenu.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    void Update() { }

    public void SetPlayerName()
    {
        if (playerName.text != "")
        {
            PlayerPrefs.SetString("PlayerName", playerName.text);
            playerNameMenu.text = PlayerPrefs.GetString("PlayerName");
            playerProfile.SetActive(false);

            AnalyticsService.Instance.RecordEvent(
                new CustomEvent("PlayerNameChanged") { { "PlayerName", playerNameMenu.text } }
            );
        }
        else
        {
            warningMessage.gameObject.SetActive(true);
        }
    }
}
