using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;  // أضف هذا
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("عناصر الواجهة")]
    public TMP_Text scoreText;    // بدل Text
    public TMP_Text throwsText;   // بدل Text
    public TMP_Text timerText;    // بدل Text

    private int score = 0;
    private int throwsCount = 0;
    private float timeRemaining = 60f;
    private bool hasStarted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreText();
        UpdateThrowsText();
        UpdateTimerText();
    }

    void OnEnable() => PlaceHoop.onPlacedObject += StartGame;
    void OnDisable() => PlaceHoop.onPlacedObject -= StartGame;

    void StartGame()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            timeRemaining = 60f;
            StartCoroutine(Timer());
        }
    }

    IEnumerator Timer()
    {
        while (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerText();
            yield return null;
        }
        SceneManager.LoadScene("Main");
    }

    public void IncrementScore()
    {
        score++;
        UpdateScoreText();
    }

    public void IncrementThrows()
    {
        throwsCount++;
        UpdateThrowsText();
    }

    public void ResetGame()
    {
        StopAllCoroutines();
        score = 0;
        throwsCount = 0;
        timeRemaining = 60f;
        hasStarted = false;
        UpdateScoreText();
        UpdateThrowsText();
        UpdateTimerText();
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main");
    }


    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void UpdateThrowsText()
    {
        if (throwsText != null)
            throwsText.text = "Throws: " + throwsCount;
    }

    void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining);
    }
}
