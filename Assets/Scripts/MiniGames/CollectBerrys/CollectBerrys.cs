using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectBerrys : MonoBehaviour
{
     public Image[] berries;  // Böğürtlen Resmi
    public Text scoreText;   // Score
    public float gameTime = 30.0f;

    private int score = 0;
    private int totalBerries = 5;  // Toplam Böğürtlen sayısı
    private float timer;
    private bool gameEnded = false;

    void Start()
    {
        timer = gameTime;
        PlaceBerriesRandomly();
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

                // Add Button component and set up onClick event
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
        if (!gameEnded)
        {
            berry.gameObject.SetActive(false);
            score++;
            UpdateScoreText();

            // End game if all berries are collected
            if (score >= totalBerries)
            {
                EndGame();
            }
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

        // Close the game screen
        gameObject.SetActive(false);
    }
}


