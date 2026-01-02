using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;


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

    public static bool InputLocked = false;

    public TextMeshProUGUI bestScoreText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameOverPanel.SetActive(false);

        mainCamera.backgroundColor = backgroundColors[0];

        highScore = PlayerPrefs.GetInt("HIGH_SCORE", 0);

        bestScoreText.text = "Best: " + highScore;
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
            AddScore(3);

            currentBlock.transform.localScale += Vector3.up * 0.1f;
            Invoke(nameof(ResetScale), 0.1f);

        }
        else if (overlap > 0)
        {
            AddScore(1);
        }
        else
        {
            GameOver();
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

        SpawnNewBlock();
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

        gameOverPanel.SetActive(true);

        // 🔥 UPDATE & SAVE FIRST
        UpdateHighScore();

        if (!hasContinued)
        {
            gameOverText.text =
                "GAME OVER\n\n" +
                "SCORE: " + score + "\n" +
                "BEST: " + highScore + "\n\n" +
                "Watch Ad to Continue";
        }
        else
        {
            FinalGameOver();
        }
    }


    void AddScore(int value)
    {
        score += value;
        scoreText.text = "Score: " + score.ToString();

        if (score % 5 == 0)
        {
            UpdateBackgroundColor();
        }

        if (score > highScore)
        {
            // 🔥 update top UI instantly
            highScore = score;
            PlayerPrefs.SetInt("HIGH_SCORE", highScore);
            PlayerPrefs.Save();
            bestScoreText.text = "Best: " + highScore;
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

        if (Input.GetMouseButtonDown(0))
        {
            if (!hasContinued)
            {
                ShowRewardedAd();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

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

    void FinalGameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);

        bool newBest = score >= highScore;

        // 🔥 ENSURE UPDATED
        UpdateHighScore();

        gameOverText.text =
            "GAME OVER\n\n" +
            "SCORE: " + score + "\n" +
            (newBest ? "\nNEW BEST!\n\n" : "") +
            "BEST: " + highScore + "\n\n" +
            "Tap to Restart";
    }


    void ShowRewardedAd()
    {
        Debug.Log("Showing rewarded ad...");

        // TEMP: simulate ad success
        Invoke(nameof(OnRewardedAdSuccess), 1.5f);
    }

    // void OnRewardedAdSuccess()
    // {
    //     hasContinued = true;
    //     isGameOver = false;
    //     gameOverText.gameObject.SetActive(false);

    //     // 🔥 DESTROY FAILED MOVING BLOCK
    //     if (currentMovingBlock != null)
    //     {
    //         Destroy(currentMovingBlock);
    //         currentMovingBlock = null;
    //     }

    //     // Restore correct reference
    //     lastBlock = lastSafeBlock;

    //     // Update camera
    //     cameraFollow.SetTarget(lastBlock.transform);

    //     // Spawn ONLY ONE fresh block
    //     SpawnNewBlock();
    // }

    void OnRewardedAdSuccess()
    {
        hasContinued = true;
        isGameOver = false;

        // 🔥 HIDE GAME OVER UI
        gameOverPanel.SetActive(false);

        // 🔥 DESTROY FAILED MOVING BLOCK
        if (currentMovingBlock != null)
        {
            Destroy(currentMovingBlock);
            currentMovingBlock = null;
        }

        // 🔥 RESTORE LAST SAFE BLOCK
        lastBlock = lastSafeBlock;

        // 🔥 UPDATE CAMERA
        cameraFollow.SetTarget(lastBlock.transform);

        // 🔥 SPAWN ONE NEW BLOCK ONLY
        SpawnNewBlock();

        scoreText.text = "Score: " + score.ToString();
        bestScoreText.text = "Best: " + highScore;

    }



    void UpdateBackgroundColor()
    {
        colorIndex++;

        if (colorIndex >= backgroundColors.Length)
            colorIndex = 0;

        mainCamera.backgroundColor = backgroundColors[colorIndex];
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
        if (isGameOver) return;

        InputLocked = true;   // 🔒 lock input immediately

        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
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
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
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
            bestScoreText.text = "Best: " + highScore;
        }
    }



}
