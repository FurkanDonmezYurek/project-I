using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BerryPlate : MonoBehaviour
{
    public Image[] berries; // Böğürtlen Image öğeleri
    public Text scoreText; // Skoru gösterecek Text öğesi
    public Image bowl; // Tabak için Image öğesi
    public Text taskCompleteText; // TASK COMPLETE mesajı için Text bileşeni
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
        bowl.gameObject.SetActive(true); // Tabağı başlangıçta göster
        taskCompleteText.gameObject.SetActive(false); // TASK COMPLETE mesajını başlangıçta gizle
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

        // Böğürtlenlerin ekrandan kaybolduğunu kontrol et
        if (!placingBerries && AreAllBerriesOutOfScreen())
        {
            ShowTaskComplete();
            CloseBowl();
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
                // DragHandler ekle
                if (berry.gameObject.GetComponent<DragHandler>() == null)
                {
                    DragHandler dragHandler = berry.gameObject.AddComponent<DragHandler>();
                    dragHandler.bowl = bowl; // DragHandler'da tabak referansını ayarla
                }
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
            }
        }
    }

    void CloseBowl()
    {
        bowl.gameObject.SetActive(false);
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

    bool AreAllBerriesOutOfScreen()
    {
        foreach (Image berry in berries)
        {
            if (berry != null && berry.gameObject.activeSelf)
            {
                RectTransform berryRect = berry.rectTransform;
                Vector3[] corners = new Vector3[4];
                berryRect.GetWorldCorners(corners);

                // Ekranın içindeki köşe noktaları
                if (corners[0].x >= 0 && corners[0].x <= Screen.width && 
                    corners[0].y >= 0 && corners[0].y <= Screen.height)
                {
                    return false; // En az bir böğürtlen ekran içinde
                }
            }
        }
        return true; // Tüm böğürtlenler ekran dışında
    }

    void ShowTaskComplete()
    {
        if (taskCompleteText != null)
        {
            taskCompleteText.gameObject.SetActive(true);
            Debug.Log("Görev Tamamlandı!");
        }
    }
}
