using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        axeButton.onClick.AddListener(OnAxeClick);
        UpdateScoreText();
    }

    void OnAxeClick()
    {
        chopCount++;
        if (chopCount >= chopsToCut)
        {
            chopCount = 0;
            score++;
            UpdateScoreText();
            // Burada odun kesildi animasyonu veya efekti ekleyebilirsin
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString();
    }
}
