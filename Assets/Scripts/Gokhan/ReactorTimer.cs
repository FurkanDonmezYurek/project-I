using UnityEngine;
using UnityEngine.UI; // UI elemanlarýný kullanmak için gerekli
using Unity.Netcode; // Netcode için gerekli
using TMPro;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


public class ReactorTimer : NetworkBehaviour
{

    // Geri sayýmýn gösterileceði Text nesnesi
    [SerializeField] public TextMeshProUGUI announcementTextUI;
    [SerializeField] public GameObject gameOverPanelUI;
    public float startTimer; // Örneðin 60 saniye (1 dakika)

    [Header("Time Control")]
    [SerializeField] public float taskCompletedOnTime;
    [SerializeField] public float SabotageTaskOnTime;

    // Geri sayýmýn þu anki deðeri (saniye cinsinden)
    private float countDownTime = 0.0f;

    public float leftMinute;
    public float leftSec;

    public string announcement;

    private void Start()
    {
    
    // Baþlangýç süresi (saniye cinsinden)
    taskCompletedOnTime = 10.0f;
    SabotageTaskOnTime = 20.0f;

    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Herkese açýk bir RPC ile geri sayýmý baþlat (sadece sunucu tarafýndan çaðrýlýr)
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        // Yeni bir istemci baðlandýðýnda geri sayýmý baþlat
        if (IsServer)
        {
            CountDownStartServerRpc();
        }
    }

    [ServerRpc] // Herkese açýk bir RPC
    void CountDownStartServerRpc()
    {
        // Geri sayýmý baþlat
        countDownTime = startTimer;

        // Geri sayým metnini güncelle
        UpdateCountDownText();
    }

    void Update()
    {
        // Her saniye geri sayýmý bir saniye azalt
        countDownTime -= Time.deltaTime;

        // Geri sayým sýfýra ulaþtýðýnda oyunu bitir (isteðe baðlý)
        if (countDownTime <= 0.0f)
        {
            // Oyun bitti!
            // Sunucuya özel kodlarý çalýþtýr
            if (NetworkManager.Singleton.IsServer)
            {
                // Fizik kurallarýný durdur
                countDownTime = 0.0f;
                gameOverPanelUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0;
            }
        }
        // Geri sayým metnini güncelle
        UpdateCountDownText();

        if (IsServer)
        {
            // Geri sayým metnini formatla ve göster
            if (leftMinute == 2.0f)
            {
                announcementTextUI.text = "Yaklaþýk 2 dk kaldý!";
            }
            else if (leftMinute == 1.0f)
            {
                announcementTextUI.text = "1 dk'dan az zaman kaldý!";
            }
        }
    }

    // Geri sayým metnini dakika ve saniye olarak günceller
    void UpdateCountDownText()
    {
        int minute = Mathf.FloorToInt(countDownTime / 60.0f); // Dakikalarý hesapla
        leftMinute = minute;

        int second = Mathf.RoundToInt(countDownTime % 60.0f); // Saniye hesapla
        leftSec = second;

        if (countDownTime <= 30.0f)
        {
            announcementTextUI.text = string.Format("{0:00}:{1:00}", minute, second);
        }
    }

    // Geri sayýmý artýrmak için bir fonksiyon
    public void SabotageTimer()
    {
        // Herkese açýk bir RPC ile geri sayýmý 10 saniye artýr (sadece sunucu tarafýndan çaðrýlýr)
        if (IsServer)
        {
            CountDownIncreaseServerRpc(SabotageTaskOnTime); // Artýrýlacak saniye miktarýný parametre olarak gönder
        }
    }

    [ServerRpc] // Herkese açýk bir RPC
    void CountDownIncreaseServerRpc(float IncreasingSecond)
    {
        countDownTime += IncreasingSecond;
        UpdateCountDownText();
    }

    // Geri sayýmý azaltmak için bir fonksiyon
    public void QuickTimer()
    {
        // Herkese açýk bir RPC ile geri sayýmý 5 saniye azalt (sadece sunucu tarafýndan çaðrýlýr)
        if (IsServer)
        {
            CountDownDecreaseServerRpc(taskCompletedOnTime); // Azaltýlacak saniye miktarýný parametre olarak gönder
        }
    }
    [ServerRpc] // Herkese açýk bir RPC
    void CountDownDecreaseServerRpc(float DecreasingTime)
    {
        countDownTime -= DecreasingTime;
    }

}