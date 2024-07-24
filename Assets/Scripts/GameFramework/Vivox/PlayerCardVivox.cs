using System.Linq;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardVivox : MonoBehaviour
{
    public VivoxParticipant Participant;

    public Image ChatStateImage;
    public Sprite MutedImage;
    public Sprite SpeakingImage;
    public Sprite NotSpeakingImage;
    CurrentLobby currentLobby;
    bool isMuted;

    private void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
    }

    public void SetupRosterItem(VivoxParticipant participant)
    {
        Participant = participant;
        // ChatStateImage = gameObject.transform.GetChild(1).GetComponent<Image>();
        UpdateChatStateImage();
        Participant.ParticipantMuteStateChanged += OnParticipantMuteStateChanged;
        Participant.ParticipantSpeechDetected += OnParticipantSpeechDetected;
    }

    private void OnDestroy()
    {
        if (Participant != null)
        {
            Participant.ParticipantMuteStateChanged -= OnParticipantMuteStateChanged;
            Participant.ParticipantSpeechDetected -= OnParticipantSpeechDetected;
        }
    }

    private void OnParticipantMuteStateChanged()
    {
        UpdateChatStateImage();
    }

    private void OnParticipantSpeechDetected()
    {
        UpdateChatStateImage();
    }

    private void UpdateChatStateImage()
    {
        // Update the UI of the game to the state of the participant
        if (Participant.IsMuted)
        {
            isMuted = true;
            ChatStateImage.sprite = MutedImage;
        }
        else
        {
            isMuted = false;
            if (Participant.SpeechDetected)
            {
                ChatStateImage.sprite = SpeakingImage;
            }
            else
            {
                ChatStateImage.sprite = NotSpeakingImage;
            }
        }
        ChatStateImage.gameObject.transform.localScale = Vector3.one;
    }

    public void MuteAndUnmuteButton()
    {
        string playerId = gameObject.name;
        if (isMuted)
        {
            Debug.Log("Unmute");
            VivoxService.Instance.ActiveChannels[currentLobby.currentLobby.Id]
                .Where(participant => participant.PlayerId == playerId)
                .First()
                .UnmutePlayerLocally();
        }
        else
        {
            Debug.Log("Mute");
            VivoxService.Instance.ActiveChannels[currentLobby.currentLobby.Id]
                .Where(participant => participant.PlayerId == playerId)
                .First()
                .MutePlayerLocally();
        }
    }
}
