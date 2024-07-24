using System.Collections;
using System.Collections.Generic;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageObjectUI : MonoBehaviour
{
    public TMP_Text PlayerName;
    public TMP_Text MessageText;
    private TMP_Text playerCount;

    public void SetTextMessage(VivoxMessage message, bool deleted = false)
    {
        playerCount = gameObject.transform
            .GetChild(0)
            .gameObject.transform.GetChild(2)
            .gameObject.GetComponent<TextMeshProUGUI>();
        GameObject playerCard = GameObject.Find(message.SenderPlayerId);
        playerCount.text = playerCard.transform
            .GetChild(0)
            .gameObject.GetComponent<TextMeshProUGUI>()
            .text;
        if (message.FromSelf)
        {
            MessageText.alignment = TextAlignmentOptions.MidlineRight;
            MessageText.text = string.Format(
                $"{message.MessageText} :\n<color=#5A5A5A><size=8>{message.ReceivedTime}</size></color>"
            );
            PlayerName.alignment = TextAlignmentOptions.MidlineRight;
            PlayerName.text = string.Format($"<color=blue>{message.SenderDisplayName} </color>");
        }
        else
        {
            MessageText.alignment = TextAlignmentOptions.MidlineLeft;
            MessageText.text = string.IsNullOrEmpty(message.ChannelName)
                ? string.Format(
                    $": {message.MessageText}\n<color=#5A5A5A><size=8>{message.ReceivedTime}</size></color>"
                ) // DM
                : string.Format(
                    $": {message.MessageText}\n<color=#5A5A5A><size=8>{message.ReceivedTime}</size></color>"
                ); // Channel Message

            PlayerName.alignment = TextAlignmentOptions.MidlineLeft;
            PlayerName.text = string.IsNullOrEmpty(message.ChannelName)
                ? string.Format($"<color=purple>{message.SenderDisplayName} </color>")
                : string.Format($"<color=purple>{message.SenderDisplayName} </color>");
        }
    }
}
