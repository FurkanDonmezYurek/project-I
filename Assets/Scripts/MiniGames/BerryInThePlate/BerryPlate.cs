using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BerryPlate : MonoBehaviour
{
    public Image[] berries; // Böğürtlen Image öğeleri
    public Text scoreText; // Skoru gösterecek Text öğesi
    public Image bowl; // Tabak için Image öğesi
    public float gameTime = 30.0f;

    private int score = 0;
    private int totalBerries = 5; // Toplam böğürtlen sayısı
    private float timer;
    private bool gameEnded = false;
    private bool placingBerries = false;

    void Start()
    {
        timer = gameTime;
        PlaceBerriesRandomly();
        UpdateScoreText();
        bowl.gameObject.SetActive(false); // Tabağı başlangıçta gizle
    }

    void Update()
    {
        if (!gameEnded && !placingBerries)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                EndGame();
            }
        }
    }

    void PlaceBerriesRandomly()
    {
        for (int i = 0; i < berries.Length; i++)
        {
            Image berry = berries[i];
            if (berry != null)
            {
                float x = Random.Range(0f, Screen.width);
                float y = Random.Range(0f, Screen.height);
                berry.rectTransform.position = new Vector3(x, y, 0);
                berry.gameObject.SetActive(true);

                // Button bileşenini ekle ve onClick olayını ayarla
                Button berryButton = berry.gameObject.GetComponent<Button>();
                if (berryButton == null)
                {
                    berryButton = berry.gameObject.AddComponent<Button>();
                }

                berryButton.onClick.AddListener(() => OnBerryClick(berry));
            }
        }
    }

    void OnBerryClick(Image berry)
    {
        if (!gameEnded && !placingBerries)
        {
            berry.gameObject.SetActive(false);
            score++;
            UpdateScoreText();

            // Tüm böğürtlenler toplandıysa, böğürtlenleri tabağa koyma aşamasına geç
            if (score >= totalBerries)
            {
                placingBerries = true;
                ShowBowl();
            }
        }
    }

    void ShowBowl()
    {
        bowl.gameObject.SetActive(true);
        Debug.Log("Tüm böğürtlenler toplandı! Onları tabağa yerleştirin.");
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Skor: " + score.ToString();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        Debug.Log("Oyun Bitti! Toplam Skor: " + score);

        // Oyun ekranını kapat
        gameObject.SetActive(false);
    }

    public void PlaceBerryInBowl()
    {
        if (placingBerries)
        {
            score++; // Böğürtlenleri tabağa yerleştirirken skoru güncelle veya istediğiniz başka bir işlemi yapın
            Debug.Log("Böğürtlen tabağa kondu!");
            // Böğürtlenleri tabağa yerleştirmek için ek mantık
        }
    }
}
