using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VideoController : MonoBehaviour
{
    public float waitTime=5f;
    private float skipTime = 10f;
    public GameObject skipButton;
    void Start()
    {
        StartCoroutine(WaitForIntro());

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            StartCoroutine(ShowSkipButton());
        }
    }

    IEnumerator WaitForIntro()
    {
        yield return new WaitForSeconds(waitTime);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    IEnumerator ShowSkipButton()
    {
        yield return new WaitForSeconds(skipTime);

        skipButton.SetActive(true); 
    }

    public void skipping()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
