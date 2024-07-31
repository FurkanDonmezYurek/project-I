using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class WoodChopper : MonoBehaviour
{
    public Image woodImage;
    public Button axeButton;
    public Text scoreText;

    private int score = 0;
    private int chopCount = 0;
    private int chopsToCut = 5;

    public GameObject BG;
    public GameObject TaskComplete;
    public GameObject ScoreBar;

    void Start()
    {
        axeButton.onClick.AddListener(OnAxeClick);
        UpdateScoreText();
        BG.gameObject.SetActive(true);
        TaskComplete.gameObject.SetActive(false);
        ScoreBar.gameObject.SetActive(true);
    }

   

    public void OnAxeClick()
    {
        chopCount++;
        if (chopCount >= chopsToCut)
        {
            chopCount = 0;
            score++;
            UpdateScoreText();
            if (score == 10)
            {
                EndGame();
            }
            // Burada odun kesildi animasyonu veya efekti ekleyebilirsin
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Kesilen Odun: " + score.ToString();
    }

    void EndGame()
    {
        if (score == 10)
        {
            BG.gameObject.SetActive(true);
            TaskComplete.gameObject.SetActive(true);
            axeButton.gameObject.SetActive(false);
            woodImage.gameObject.SetActive(false);
            scoreText.gameObject.SetActive(false);
            ScoreBar.gameObject.SetActive(false);
            
        }
    }
}
