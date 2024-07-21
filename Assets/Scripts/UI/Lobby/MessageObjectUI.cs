using System.Collections;
using System.Collections.Generic;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageObjectUI : MonoBehaviour
{
    public TMP_Text MessageText;

    public void SetTextMessage(VivoxMessage message, bool deleted = false)
    {
        if (message.FromSelf)
        {
            MessageText.alignment = TextAlignmentOptions.MidlineRight;
            MessageText.text = string.Format(
                $"{message.MessageText} :<color=blue>{message.SenderDisplayName} </color>\n<color=#5A5A5A><size=8>{message.ReceivedTime}</size></color>"
            );
        }
        else
        {
            MessageText.alignment = TextAlignmentOptions.MidlineLeft;
            MessageText.text = string.IsNullOrEmpty(message.ChannelName)
                ? string.Format(
                    $"<color=purple>{message.SenderDisplayName} </color>: {message.MessageText}\n<color=#5A5A5A><size=8>{message.ReceivedTime}</size></color>"
                ) // DM
                : string.Format(
                    $"<color=green>{message.SenderDisplayName} </color>: {message.MessageText}\n<color=#5A5A5A><size=8>{message.ReceivedTime}</size></color>"
                ); // Channel Message
        }
    }
}
