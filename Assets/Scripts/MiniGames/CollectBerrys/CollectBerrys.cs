using System.Collections; // IEnumerator için gerekli
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;


public class CollectBerrys : MonoBehaviour
{
    public Image[] berries;  // Böğürtlen Resimleri
    public Text scoreText;   // Skor
    public Text taskCompleteText; // Görev Tamamlandı Metni
    public float gameTime = 30.0f; // Oyun Süresi
    public GameObject GameObject;

    
    
    private TaskManager TaskManager;
    

    private int score = 0;
    private int totalBerries = 5;  // Toplam Böğürtlen Sayısı
    private float timer;
    private bool gameEnded = false;

    void Start()
    {
        timer = gameTime;
        PlaceBerriesRandomly();
        UpdateScoreText();
        TaskManager = gameObject.transform.parent.gameObject.GetComponent<TaskManager>();
        
        // Görev Tamamlandı metninin başlangıçta gizli olduğundan emin ol
        if (taskCompleteText != null)
        {
            taskCompleteText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Task Complete Text is not assigned.");
        }
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
        foreach (Image berry in berries)
        {
            if (berry != null)
            {
                float x = Random.Range(0f, Screen.width);
                float y = Random.Range(0f, Screen.height);
                berry.rectTransform.position = new Vector3(x, y, 0);
                berry.gameObject.SetActive(true);

                Button berryButton = berry.gameObject.GetComponent<Button>();
                if (berryButton == null)
                {
                    berryButton = berry.gameObject.AddComponent<Button>();
                }

                berryButton.onClick.RemoveAllListeners();
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

            // Bütün böğürtlenler toplandı mı kontrol et
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

        // Bütün böğürtlenler toplandıysa görev tamamlandı metnini göster
        if (score >= totalBerries)
        {
            if (taskCompleteText != null)
            {
                taskCompleteText.gameObject.SetActive(true);
                // Eğer metni belli bir süre göstermek istersen aşağıdaki Coroutine'i kullanabilirsin:
                // StartCoroutine(ShowTaskCompleteText());
                GameObject.gameObject.SetActive(false);
                TaskManager.GameEnded();


            }
        }

        // Diğer oyun elemanlarını devre dışı bırakabilirsin
        // gameObject.SetActive(false); Bu satırı kaldırarak tüm oyun nesnelerinin görünmesini sağlayabilirsin
    }

    // Bu Coroutine, taskCompleteText gösterildikten sonra bir süre bekleyebilir.
    IEnumerator ShowTaskCompleteText()
    {
        taskCompleteText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2); // 2 saniye bekle
        taskCompleteText.gameObject.SetActive(false);
    }
}
