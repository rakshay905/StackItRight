using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class StackGameManager : MonoBehaviour
{
    public static StackGameManager Instance;

    public GameObject blockPrefab;
    public Transform stackParent;

    private GameObject lastBlock;
    private float blockHeight = 0.2f;
    // private float moveRange = 3f;

    public float blockHeightFixed = 0.2f;
    // public float blockHeightFixed = 0.2f;
    public float blockDepthFixed = 2f;

    public TextMeshProUGUI scoreText;
    private int score = 0;

    public CameraFollow cameraFollow;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    private bool isGameOver = false;
    private int highScore;

    private GameObject lastSafeBlock;
    private bool hasContinued = false;

    private GameObject currentMovingBlock;

    public Camera mainCamera;

    [SerializeField]
    Color[] backgroundColors;

    private int colorIndex = 0;

    public float smoothSpeed = 3.5f;

    private bool isPaused = false;

    public GameObject pausePanel;

    // public GameObject pauseButton;
    public GameObject topRightButtons;   // parent
    public GameObject pauseButton;
    public GameObject settingsButton;

    public TextMeshProUGUI soundText;
    public TextMeshProUGUI vibrationText;
    public Button soundButton;
    public Button vibrationButton;

    Color onColor = new Color(0.2f, 0.8f, 0.2f);   // green
    Color offColor = new Color(0.85f, 0.2f, 0.2f); // red

    public static bool InputLocked = false;

    public TextMeshProUGUI bestScoreText;

    CanvasGroup gameOverCanvasGroup;

    Vector3 scoreBaseScale;
    bool scoreAnimating = false;

    public GameObject watchAdButton;   // invisible button
    public GameObject restartButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateBackgroundColor();
        gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        gameOverPanel.transform.localScale = Vector3.one * 0.9f;
        gameOverCanvasGroup.alpha = 0;
        gameOverPanel.SetActive(false);

        UpdateAudioUI();

        // pauseButton.SetActive(true);
        topRightButtons.SetActive(true);

        scoreBaseScale = scoreText.transform.localScale;

        gameOverPanel.SetActive(false);

        // mainCamera.backgroundColor = backgroundColors[0];

        highScore = PlayerPrefs.GetInt("HIGH_SCORE", 0);

        bestScoreText.text = "Best: "  + FormatScore(highScore);
        scoreText.text = "Score: 0";

        lastBlock = GameObject.Find("BaseBlock");
        cameraFollow.SetTarget(lastBlock.transform);
        SpawnNewBlock();
    }

    public void PlaceBlock(GameObject currentBlock)
    {
        Debug.Log("dsdsdsddddsdsdsd " + blockHeightFixed);
        float offsetX = currentBlock.transform.position.x - lastBlock.transform.position.x;
        float overlap = lastBlock.transform.localScale.x - Mathf.Abs(offsetX);

        // if (overlap <= 0)
        // {
        //     GameOver();
        //     return;
        // }

        if (Mathf.Abs(offsetX) < 0.05f)
        {
            overlap = lastBlock.transform.localScale.x;
            currentBlock.transform.position = new Vector3(
                lastBlock.transform.position.x,
                currentBlock.transform.position.y,
                currentBlock.transform.position.z
            );
            AddScore(30);
            bool vibrationOn = AudioManager.Instance.vibrationOn;
            if (vibrationOn) Handheld.Vibrate();
            AudioManager.Instance.Play(AudioManager.Instance.perfectClip);

            currentBlock.transform.localScale += Vector3.up * 0.1f;
            Invoke(nameof(ResetScale), 0.1f);

        }
        else if (overlap > 0)
        {
            AddScore(10);
            AudioManager.Instance.Play(AudioManager.Instance.placeClip);
        }
        else
        {
            GameOver();
            AudioManager.Instance.Play(AudioManager.Instance.failClip);
            return;
        }

        // Resize current block
        currentBlock.transform.localScale = new Vector3(
            overlap,
            blockHeightFixed,
            blockDepthFixed
        );


        // Reposition after cut
        currentBlock.transform.position = new Vector3(
            lastBlock.transform.position.x + offsetX / 2,
            currentBlock.transform.position.y,
            currentBlock.transform.position.z
        );

        lastBlock = currentBlock;

        // UPDATE CAMERA TARGET HERE ✅
        cameraFollow.SetTarget(currentBlock.transform);

        lastSafeBlock = currentBlock;

        // SpawnNewBlock();
        StartCoroutine(SpawnNextBlockDelayed());
    }

    void SpawnNewBlock()
    {
        Vector3 pos = lastBlock.transform.position;
        pos.y += blockHeight;

        // GameObject newBlock = Instantiate(blockPrefab, pos, Quaternion.identity, stackParent);
        currentMovingBlock = Instantiate(blockPrefab, pos, Quaternion.identity, stackParent);
        GameObject newBlock = currentMovingBlock;
        newBlock.transform.localScale = new Vector3(
            lastBlock.transform.localScale.x, // only X shrinks
            blockHeightFixed,                  // Y stays SAME
            blockDepthFixed                    // Z stays SAME
        );

        Renderer rend = newBlock.GetComponent<Renderer>();
        rend.material.color = Random.ColorHSV(
            0f, 1f,
            0.6f, 0.9f,
            0.6f, 1f
        );

        BlockController bc = newBlock.GetComponent<BlockController>();
        // bc.moveRange = moveRange;
        // bc.moveRange += score * 0.02f;
    }

    // void GameOver()
    // {
    //     isGameOver = true;
    //     gameOverText.gameObject.SetActive(true);
    // }

    // void GameOver()
    // {
    //     if (!hasContinued)
    //     {
    //         isGameOver = true;
    //         gameOverText.text = "GAME OVER\nWatch Ad to Continue";
    //         gameOverText.gameObject.SetActive(true);
    //     }
    //     else
    //     {
    //         FinalGameOver();
    //     }
    // }

    void GameOver()
    {
        isGameOver = true;
        // pauseButton.SetActive(false);
        topRightButtons.SetActive(false);

        // gameOverPanel.SetActive(true);
        StartCoroutine(ShowGameOverPanel());

        // 🔥 UPDATE & SAVE FIRST
        UpdateHighScore();

        if (!hasContinued)
        {
            // watchAdButton.SetActive(true);
            // restartButton.SetActive(false);
            bool rewardedReady = AdMobManager.Instance.IsRewardedReady();

            watchAdButton.SetActive(rewardedReady);
            restartButton.SetActive(!rewardedReady);

            gameOverText.text =
                "GAME OVER\n\n" +
                "SCORE: " + FormatScore(score) + "\n" +
                "BEST: "  + FormatScore(highScore) + "\n";
                // "Watch Ad to Continue";
        }
        else
        {
            FinalGameOver();
        }
    }


    void AddScore(int value)
    {
        score += value;
        scoreText.text = "Score: " + FormatScore(score);

        StartCoroutine(ScorePop());

        // if (score % 5 == 0)
        // {
        //     UpdateBackgroundColor();
        // }

        if (score > highScore)
        {
            // 🔥 update top UI instantly
            highScore = score;
            PlayerPrefs.SetInt("HIGH_SCORE", highScore);
            PlayerPrefs.Save();
            bestScoreText.text = "Best: "  + FormatScore(highScore);
        }

    }

    void Update()
    {
        if (!isGameOver) return;

        // if (Input.GetMouseButtonDown(0))
        // {
        //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // }

        if (currentMovingBlock != null)
        {
            Destroy(currentMovingBlock.GetComponent<BlockController>());
        }

        // if (Input.GetMouseButtonDown(0))
        // {
        //     if (!hasContinued)
        //     {
        //         ShowRewardedAd();
        //     }
        //     else
        //     {
        //         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                TogglePause();
        }


    }

    // void FinalGameOver()
    // {
    //     isGameOver = true;
    //     gameOverText.text = "GAME OVER\nTap to Restart";
    //     gameOverText.gameObject.SetActive(true);
    // }

    public void FinalGameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        // pauseButton.SetActive(false);
        topRightButtons.SetActive(false);

        bool newBest = score >= highScore;

        // 🔥 ENSURE UPDATED
        UpdateHighScore();

        watchAdButton.SetActive(false);
        restartButton.SetActive(true);
        gameOverText.text =
            "GAME OVER\n\n" +
            "SCORE: " + score + "\n" +
            (newBest ? "\nNEW BEST!\n\n" : "") +
            "BEST: "  + FormatScore(highScore) + "\n";
            // "Tap to Restart";

        // AdMobManager.Instance.TryShowInterstitial();
        StartCoroutine(ShowInterstitialDelayed());
    }

    IEnumerator ShowInterstitialDelayed()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        AdMobManager.Instance.TryShowInterstitial();
    }



    // void ShowRewardedAd()
    // {
    //     Debug.Log("Showing rewarded ad...");

    //     // TEMP: simulate ad success
    //     Invoke(nameof(OnRewardedAdSuccess), 1.5f);
    // }

    public void ShowRewardedAd()
    {
        if (!AdMobManager.Instance.IsRewardedReady())
        return;

        InputLocked = true; // 🔒 stop any double input
        AdMobManager.Instance.ShowRewardedAd();
    }

    // public void OnRewardedAdSuccess()
    // {
    //     hasContinued = true;
    //     isGameOver = false;

    //     // 🔥 HIDE GAME OVER UI
    //     gameOverPanel.SetActive(false);
    //     // pauseButton.SetActive(true); // 🔥 BACK TO GAME
    //     topRightButtons.SetActive(true);

    //     // 🔥 DESTROY FAILED MOVING BLOCK
    //     if (currentMovingBlock != null)
    //     {
    //         Destroy(currentMovingBlock);
    //         currentMovingBlock = null;
    //     }

    //     // 🔥 RESTORE LAST SAFE BLOCK
    //     lastBlock = lastSafeBlock;

    //     // 🔥 UPDATE CAMERA
    //     cameraFollow.SetTarget(lastBlock.transform);

    //     // 🔥 SPAWN ONE NEW BLOCK ONLY
    //     SpawnNewBlock();

    //     scoreText.text = "Score: " + FormatScore(score);
    //     bestScoreText.text = "Best: "  + FormatScore(highScore);

    // }

    public void OnRewardedAdSuccess()
    {
        hasContinued = true;

        isGameOver = false;
        InputLocked = false;

        gameOverPanel.SetActive(false);
        topRightButtons.SetActive(true);

        // 🔥 Destroy failed moving block (if any)
        if (currentMovingBlock != null)
        {
            Destroy(currentMovingBlock);
            currentMovingBlock = null;
        }

        // 🔥 Restore last safe block
        lastBlock = lastSafeBlock;

        // 🔥 Update camera
        cameraFollow.SetTarget(lastBlock.transform);

        // 🔥 VERY IMPORTANT
        Time.timeScale = 1f;

        // 🔥 Spawn a fresh moving block
        SpawnNewBlock();
    }

    // void UpdateBackgroundColor()
    // {
    //     colorIndex++;

    //     if (colorIndex >= backgroundColors.Length)
    //         colorIndex = 0;

    //     mainCamera.backgroundColor = backgroundColors[colorIndex];
    // }
    void UpdateBackgroundColor()
    {
        if (backgroundColors.Length == 0) return;

        int index = Random.Range(0, backgroundColors.Length);
        mainCamera.backgroundColor = backgroundColors[index];
    }



    void ResetScale()
    {
        lastBlock.transform.localScale = new Vector3(
            lastBlock.transform.localScale.x,
            blockHeightFixed,
            blockDepthFixed
        );
    }

    public void TogglePause()
    {
        InputLocked = true; // 🔥 ADD THIS FIRST

        if (isGameOver) return;
        AudioManager.Instance.Play(AudioManager.Instance.buttonClip);

        InputLocked = true;   // 🔒 lock input immediately

        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        // pauseButton.SetActive(!isPaused); // 🔥 HIDE WHEN PAUSED
        topRightButtons.SetActive(!isPaused); // 🔥 hide both buttons
        Time.timeScale = isPaused ? 0f : 1f;

        // unlock input after a short delay
        StartCoroutine(UnlockInput());
    }

    IEnumerator UnlockInput()
    {
        yield return new WaitForSecondsRealtime(0.15f);
        InputLocked = false;
    }

    public void ResumeGame()
    {
        isPaused = false;
        AudioManager.Instance.Play(AudioManager.Instance.buttonClip);
        pausePanel.SetActive(false);
        // pauseButton.SetActive(true); // 🔥 SHOW AGAIN
        topRightButtons.SetActive(true); // 🔥 show again
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        UpdateBackgroundColor();
        Time.timeScale = 1f;
        AudioManager.Instance.Play(AudioManager.Instance.buttonClip);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HIGH_SCORE", highScore);
            PlayerPrefs.Save();

            // 🔥 update top UI instantly
            bestScoreText.text = "Best: "  + FormatScore(highScore);
        }
    }

    IEnumerator ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);

        float t = 0f;
        float duration = 0.25f;

        Vector3 startScale = Vector3.one * 0.9f;
        Vector3 endScale = Vector3.one;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;

            gameOverCanvasGroup.alpha = Mathf.Lerp(0, 1, p);
            gameOverPanel.transform.localScale = Vector3.Lerp(startScale, endScale, p);

            yield return null;
        }

        gameOverCanvasGroup.alpha = 1;
        gameOverPanel.transform.localScale = endScale;
    }

    IEnumerator ScorePop()
    {
        if (scoreAnimating) yield break;
        scoreAnimating = true;

        float growTime = 0.12f;
        float shrinkTime = 0.15f;

        Vector3 startScale = scoreBaseScale;
        Vector3 peakScale = scoreBaseScale * 1.12f;

        float t = 0f;

        // 🔹 GROW (ease out)
        while (t < growTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / growTime);
            p = Mathf.Sin(p * Mathf.PI * 0.5f); // ease-out

            scoreText.transform.localScale = Vector3.Lerp(startScale, peakScale, p);
            yield return null;
        }

        t = 0f;

        // 🔹 SHRINK BACK (ease in)
        while (t < shrinkTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / shrinkTime);
            p = 1f - Mathf.Cos(p * Mathf.PI * 0.5f); // ease-in

            scoreText.transform.localScale = Vector3.Lerp(peakScale, startScale, p);
            yield return null;
        }

        scoreText.transform.localScale = startScale;
        scoreAnimating = false;
    }

    public void OpenSettings()
    {
        if (isGameOver) return;

        InputLocked = true;

        isPaused = true;
        Time.timeScale = 0f;

        pausePanel.SetActive(true);
        topRightButtons.SetActive(false);

        StartCoroutine(UnlockInput());
    }

    void UpdateAudioUI()
    {
        // SOUND
        bool soundOn = AudioManager.Instance.soundOn;
        soundText.text = "Sound: " + (soundOn ? "ON" : "OFF");
        soundButton.image.color = soundOn ? onColor : offColor;

        // VIBRATION
        bool vibrationOn = AudioManager.Instance.vibrationOn;
        vibrationText.text = "Vibration: " + (vibrationOn ? "ON" : "OFF");
        vibrationButton.image.color = vibrationOn ? onColor : offColor;
    }

    public void OnSoundButton()
    {
        AudioManager.Instance.soundOn = !AudioManager.Instance.soundOn;
        PlayerPrefs.SetInt("SOUND_ON", AudioManager.Instance.soundOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateAudioUI();
        AudioManager.Instance.Play(AudioManager.Instance.buttonClip);
    }

    public void OnVibrationButton()
    {
        AudioManager.Instance.vibrationOn = !AudioManager.Instance.vibrationOn;
        PlayerPrefs.SetInt("VIBRATION_ON", AudioManager.Instance.vibrationOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateAudioUI();
        AudioManager.Instance.Vibrate();
    }

    IEnumerator SpawnNextBlockDelayed()
    {
        yield return new WaitForSeconds(0.12f);
        SpawnNewBlock();
    }

    string FormatScore(int value)
    {
        if (value < 1000)
            return value.ToString();

        if (value < 10000)
        {
            float v = Mathf.Floor(value / 10f) / 100f; // 🔥 1020 → 1.02
            return v.ToString("0.##") + "k";
        }

        if (value < 100000)
            return Mathf.Floor(value / 100f).ToString("0.#") + "k";

        if (value < 1000000)
            return (value / 1000).ToString() + "k";

        return (value / 1000000f).ToString("0.##") + "M";
    }

    public void LockInputImmediately()
    {
        InputLocked = true;
    }

}
