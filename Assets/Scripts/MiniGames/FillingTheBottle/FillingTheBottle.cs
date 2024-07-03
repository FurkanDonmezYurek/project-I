using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillingTheBottle : MonoBehaviour
{
     public Image[] waterBottles;    
    public Text scoreText;           
    public float gameTime = 30.0f;  
    private int score = 0;          
    private int totalBottles = 5;   
    private float timer;            
    private bool gameEnded = false; 

    void Start()
    {
        timer = gameTime;
        PlaceWaterBottlesRandomly();
        UpdateScoreText();
    }

    void Update()
    {
        if (!gameEnded)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                EndGame();
            }
        }
    }

    void PlaceWaterBottlesRandomly()
    {
        for (int i = 0; i < waterBottles.Length; i++)
        {
            Image bottle = waterBottles[i];
            if (bottle != null)
            {
                float x = Random.Range(0f, Screen.width);
                float y = Random.Range(0f, Screen.height);
                bottle.rectTransform.position = new Vector3(x, y, 0);
                bottle.gameObject.SetActive(true);

                // Button componentini ekleyip onClick eventini ayarla
                Button bottleButton = bottle.gameObject.GetComponent<Button>();
                if (bottleButton == null)
                {
                    bottleButton = bottle.gameObject.AddComponent<Button>();
                }
                bottleButton.onClick.AddListener(() => OnBottleClick(bottle));
            }
        }
    }

    void OnBottleClick(Image bottle)
    {
        if (!gameEnded)
        {
            // Simüle edilmiş su dolumu işlemi
            StartCoroutine(FillBottleCoroutine(bottle));
        }
    }

    IEnumerator FillBottleCoroutine(Image bottle)
    {
        float fillTime = 2.0f;  // Su dolum süresi (örneğin 2 saniye)
        float elapsedTime = 0f;

        while (elapsedTime < fillTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Su dolumu tamamlandı
        bottle.fillAmount = 1f;  // 1.0 = %100 dolum
        score++;
        UpdateScoreText();

        // Eğer tüm su şişelerini bulduysa oyunu bitir
        if (score >= totalBottles)
        {
            EndGame();
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        Debug.Log("Game Over! Total Score: " + score);

        // Canvas ekranını kapat
        gameObject.SetActive(false);
    }
}
