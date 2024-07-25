using System.Threading.Tasks;
#if AUTH_PACKAGE_PRESENT
using Unity.Services.Authentication;
#endif
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyScreenUI : MonoBehaviour
{
    public Button LogoutButton;
    EventSystem m_EventSystem;
    CurrentLobby currentLobby;

    void Start()
    {
        currentLobby = GameObject.Find("LobbyManager").GetComponent<CurrentLobby>();
        m_EventSystem = EventSystem.current;
        if (!m_EventSystem)
        {
            Debug.LogError("Unable to find EventSystem object.");
        }
        VivoxService.Instance.LoggedIn += OnUserLoggedIn;

        LogoutButton.onClick.AddListener(() =>
        {
            LogoutOfVivoxServiceAsync();
        });
    }

    void OnDestroy()
    {
        VivoxService.Instance.LoggedIn -= OnUserLoggedIn;

        LogoutButton.onClick.RemoveAllListeners();
    }

    Task JoinLobbyChannel()
    {
        string LobbyChannelName = currentLobby.currentLobby.Name;
        return VivoxService.Instance.JoinGroupChannelAsync(
            LobbyChannelName,
            ChatCapability.TextAndAudio
        );
    }

    async void LogoutOfVivoxServiceAsync()
    {
        LogoutButton.interactable = false;

        await VivoxService.Instance.LogoutAsync();
#if AUTH_PACKAGE_PRESENT
        AuthenticationService.Instance.SignOut();
#endif
    }

    async void OnUserLoggedIn()
    {
        await JoinLobbyChannel();
        LogoutButton.interactable = true;
        m_EventSystem.SetSelectedGameObject(LogoutButton.gameObject, null);
    }
}
