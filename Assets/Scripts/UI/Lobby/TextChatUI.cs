using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using Unity.Services.Vivox.AudioTaps;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEditor.VersionControl;

public class TextChatUI : MonoBehaviour
{
    public GameObject MessageObject;
    public GameObject ChatContentObj;

    public TMP_InputField chatInputField;
    CurrentLobby currentLobby;

    private void Awake()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        VivoxService.Instance.ChannelMessageReceived += onChannelMessageReceived;
    }

    public void SendMessageButton()
    {
        string channelName = currentLobby.currentLobby.Id;
        SendMessageAsync(channelName, chatInputField.text);
    }

    private async void SendMessageAsync(string channelName, string message)
    {
        if (string.IsNullOrEmpty(chatInputField.text))
        {
            return;
        }

        await VivoxService.Instance.SendChannelTextMessageAsync(channelName, message);
        chatInputField.text = string.Empty;
    }

    // private void BindSessionEvents(bool doBind)
    // {
    //     VivoxService.Instance.ChannelMessageReceived += onChannelMessageReceived;
    // }

    private void onChannelMessageReceived(VivoxMessage message)
    {
        string messageText = message.MessageText;
        string senderID = message.SenderPlayerId;
        string senderDisplayName = message.SenderDisplayName;
        string messageChannel = message.ChannelName;
        AddMessageToChat(message);
    }

    void AddMessageToChat(VivoxMessage message)
    {
        var newMessageObj = Instantiate(MessageObject, ChatContentObj.transform);
        var newMessageTextObject = newMessageObj.GetComponent<MessageObjectUI>();

        newMessageTextObject.SetTextMessage(message);
    }
}
