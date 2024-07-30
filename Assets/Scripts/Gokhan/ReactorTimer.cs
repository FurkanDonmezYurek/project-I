using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ReactorTimer : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI announcementTextUI;
    [SerializeField] private GameObject gameOverPanelUI;
    public float startTimer;

    [Header("Time Control")]
    [SerializeField] public float taskCompletedOnTime;
    [SerializeField] public float SabotageTaskOnTime;

    private float countDownTime = 0.0f;
    private bool isPaused = false;

    public float leftMinute;
    public float leftSec;
    public string announcement;

    private void Start()
    {
        taskCompletedOnTime = 10.0f;
        SabotageTaskOnTime = 20.0f;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            CountDownStartServerRpc();
        }
    }

    [ServerRpc]
    void CountDownStartServerRpc()
    {
        countDownTime = startTimer;
        UpdateCountDownText();
    }

    void Update()
    {
        if (!isPaused)
        {
            countDownTime -= Time.deltaTime;

            if (countDownTime <= 0.0f)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    countDownTime = 0.0f;
                    gameOverPanelUI.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    Time.timeScale = 0;
                }
            }

            UpdateCountDownText();

            if (IsServer)
            {
                if (leftMinute == 2.0f)
                {
                    announcementTextUI.text = "Yaklaşık 2 dk kaldı!";
                }
                else if (leftMinute == 1.0f)
                {
                    announcementTextUI.text = "1 dk'dan az zaman kaldı!";
                }
            }
        }
    }

    void UpdateCountDownText()
    {
        int minute = Mathf.FloorToInt(countDownTime / 60.0f);
        leftMinute = minute;

        int second = Mathf.RoundToInt(countDownTime % 60.0f);
        leftSec = second;

        if (countDownTime <= 30.0f)
        {
            announcementTextUI.text = string.Format("{0:00}:{1:00}", minute, second);
        }
    }

    public void SabotageTimer()
    {
        if (IsServer)
        {
            CountDownIncreaseServerRpc(SabotageTaskOnTime);
        }
    }

    [ServerRpc]
    void CountDownIncreaseServerRpc(float IncreasingSecond)
    {
        countDownTime += IncreasingSecond;
        UpdateCountDownText();
    }

    [ServerRpc]
    void CountDownDecreaseServerRpc(float DecreasingTime)
    {
        countDownTime -= DecreasingTime;
    }

    [ServerRpc]
    public void PauseReactorServerRpc()
    {
        isPaused = true;
    }

    [ServerRpc]
    public void ResumeReactorServerRpc()
    {
        isPaused = false;
    }
}
