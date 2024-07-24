using UnityEngine;
using UnityEngine.UI; // UI elemanlar�n� kullanmak i�in gerekli
using Unity.Netcode; // Netcode i�in gerekli
using TMPro;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


public class ReactorTimer : NetworkBehaviour
{

    // Geri say�m�n g�sterilece�i Text nesnesi
    [SerializeField] public TextMeshProUGUI announcementTextUI;
    [SerializeField] public GameObject gameOverPanelUI;
    public float startTimer; // �rne�in 60 saniye (1 dakika)

    [Header("Time Control")]
    [SerializeField] public float taskCompletedOnTime;
    [SerializeField] public float SabotageTaskOnTime;

    // Geri say�m�n �u anki de�eri (saniye cinsinden)
    private float countDownTime = 0.0f;

    public float leftMinute;
    public float leftSec;

    public string announcement;

    private void Start()
    {
    
    // Ba�lang�� s�resi (saniye cinsinden)
    taskCompletedOnTime = 10.0f;
    SabotageTaskOnTime = 20.0f;

    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Herkese a��k bir RPC ile geri say�m� ba�lat (sadece sunucu taraf�ndan �a�r�l�r)
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        // Yeni bir istemci ba�land���nda geri say�m� ba�lat
        if (IsServer)
        {
            CountDownStartServerRpc();
        }
    }

    [ServerRpc] // Herkese a��k bir RPC
    void CountDownStartServerRpc()
    {
        // Geri say�m� ba�lat
        countDownTime = startTimer;

        // Geri say�m metnini g�ncelle
        UpdateCountDownText();
    }

    void Update()
    {
        // Her saniye geri say�m� bir saniye azalt
        countDownTime -= Time.deltaTime;

        // Geri say�m s�f�ra ula�t���nda oyunu bitir (iste�e ba�l�)
        if (countDownTime <= 0.0f)
        {
            // Oyun bitti!
            // Sunucuya �zel kodlar� �al��t�r
            if (NetworkManager.Singleton.IsServer)
            {
                // Fizik kurallar�n� durdur
                countDownTime = 0.0f;
                gameOverPanelUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0;
            }
        }
        // Geri say�m metnini g�ncelle
        UpdateCountDownText();

        if (IsServer)
        {
            // Geri say�m metnini formatla ve g�ster
            if (leftMinute == 2.0f)
            {
                announcementTextUI.text = "Yakla��k 2 dk kald�!";
            }
            else if (leftMinute == 1.0f)
            {
                announcementTextUI.text = "1 dk'dan az zaman kald�!";
            }
        }
    }

    // Geri say�m metnini dakika ve saniye olarak g�nceller
    void UpdateCountDownText()
    {
        int minute = Mathf.FloorToInt(countDownTime / 60.0f); // Dakikalar� hesapla
        leftMinute = minute;

        int second = Mathf.RoundToInt(countDownTime % 60.0f); // Saniye hesapla
        leftSec = second;

        if (countDownTime <= 30.0f)
        {
            announcementTextUI.text = string.Format("{0:00}:{1:00}", minute, second);
        }
    }

    // Geri say�m� art�rmak i�in bir fonksiyon
    public void SabotageTimer()
    {
        // Herkese a��k bir RPC ile geri say�m� 10 saniye art�r (sadece sunucu taraf�ndan �a�r�l�r)
        if (IsServer)
        {
            CountDownIncreaseServerRpc(SabotageTaskOnTime); // Art�r�lacak saniye miktar�n� parametre olarak g�nder
        }
    }

    [ServerRpc] // Herkese a��k bir RPC
    void CountDownIncreaseServerRpc(float IncreasingSecond)
    {
        countDownTime += IncreasingSecond;
        UpdateCountDownText();
    }

    // Geri say�m� azaltmak i�in bir fonksiyon
    public void QuickTimer()
    {
        // Herkese a��k bir RPC ile geri say�m� 5 saniye azalt (sadece sunucu taraf�ndan �a�r�l�r)
        if (IsServer)
        {
            CountDownDecreaseServerRpc(taskCompletedOnTime); // Azalt�lacak saniye miktar�n� parametre olarak g�nder
        }
    }
    [ServerRpc] // Herkese a��k bir RPC
    void CountDownDecreaseServerRpc(float DecreasingTime)
    {
        countDownTime -= DecreasingTime;
    }

}