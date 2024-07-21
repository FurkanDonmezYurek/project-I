using Unity.Services.Vivox;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCardVivox : MonoBehaviour
{
    public VivoxParticipant Participant;

    public Image ChatStateImage;
    public Sprite MutedImage;
    public Sprite SpeakingImage;
    public Sprite NotSpeakingImage;

    public void SetupRosterItem(VivoxParticipant participant)
    {
        Participant = participant;
        UpdateChatStateImage();
        Participant.ParticipantMuteStateChanged += UpdateChatStateImage;
        Participant.ParticipantSpeechDetected += UpdateChatStateImage;
    }

    private void UpdateChatStateImage()
    {
        /// Update the UI of the game to the state of the participant
        if (Participant.IsMuted)
        {
            ChatStateImage.sprite = MutedImage;
            ChatStateImage.gameObject.transform.localScale = Vector3.one;
        }
        else
        {
            if (Participant.SpeechDetected)
            {
                ChatStateImage.sprite = SpeakingImage;
                ChatStateImage.gameObject.transform.localScale = Vector3.one;
            }
            else
            {
                ChatStateImage.sprite = NotSpeakingImage;
            }
        }
    }
}
