using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchLogic : MonoBehaviour
{
    private static MatchLogic Instance;

    public int maxPoints = 3;
    public Text pointsText;
    public GameObject levelCompleteUI;
    private int points = 0;


    private void Start()
    {
        Instance = this;
    }

    void UpdatePotinsText()
    {
        pointsText.text = points + "/" + maxPoints;
        if (points== maxPoints)
        {
            levelCompleteUI.SetActive(true);
        }
    }


    public static void AddPoint()
    {
        AddPoints(1);
    }

    public static void AddPoints(int points)
    {
        Instance.points += points;
        Instance.UpdatePotinsText();
    }
}

