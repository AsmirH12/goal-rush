using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const float HORIZONTAL_BOUND = 37;
    private const float VERTICAL_BOUND = 65;

    private bool isActive = true;
    public bool canScoreGoal = true;
    private GameObject currentMenu;

    private int playerScore;
    private int opposingScore;

    public GameObject ballPrefab;
    public GameObject currentBall;
    public GameObject playerRef;
    public GameObject opposingPlayerRef;
    public GameObject opposingGoalieRef;
    public GameObject menuPrefab;
    public GameObject menuUI;
    public GameObject inGameText;

    public TextMeshProUGUI gameTimerText;
    public TextMeshProUGUI[] goalTextLetters;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI savedText;
    public TextMeshProUGUI goalScoredText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI winnerText;

    private bool didCallGameOver = false;

    private float secondsElapsed;

    void Start()
    {
        currentBall = GameObject.Find("Ball");
        playerRef = GameObject.Find("Player");
        opposingPlayerRef = GameObject.Find("Opposing Player");
        opposingGoalieRef = GameObject.Find("Opposing Goalie");

        secondsElapsed = 0;
        playerScore = 0;
        opposingScore = 0;

        SetPlayersMoveStatus(false);

        ShowMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if(Mathf.Abs(currentBall.transform.position.z) > HORIZONTAL_BOUND || Mathf.Abs(currentBall.transform.position.x) > VERTICAL_BOUND)
        {
            currentBall = Instantiate(ballPrefab, new Vector3(0, 5, 0), ballPrefab.transform.rotation);
        }

        if(isActive)
        {
            secondsElapsed += Time.deltaTime * 90; // Game lasts 1 minute
            gameTimerText.text = string.Format("{0:00}:{1:00}", System.Math.Truncate(secondsElapsed / 60), secondsElapsed % 60);
        }

        if(secondsElapsed > 5400 && !didCallGameOver)
        {
            gameTimerText.text = "90:00";
            StartCoroutine(GameOver());
            didCallGameOver = true;
        }
    }

    public IEnumerator GoalScored(bool didPlayerScore)
    {
        canScoreGoal = false;

        if(didPlayerScore)
        {
            playerScore++;
        } else
        {
            opposingScore++;
        }

        Time.timeScale = 0.15f;

        opposingGoalieRef.GetComponent<Goalie>().canReceiveBall = false;

        playerRef.GetComponent<PlayerController>().canReceiveBall = false;
        playerRef.GetComponent<PlayerController>().hasBall = false;

        opposingPlayerRef.GetComponent<OpposingPlayer>().canReceiveBall = false;
        opposingPlayerRef.GetComponent<OpposingPlayer>().hasBall = false;

        UpdateScoreText();

        for(int i = 0; i < goalTextLetters.Length; i++)
        {
            goalTextLetters[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f * Time.timeScale);
        }

        goalScoredText.text = didPlayerScore ? "You Scored!" : "Opposing Team Scored!";
        goalScoredText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1 * Time.timeScale);

        for(int i = 0; i < 3; i++)
        {
            foreach (TextMeshProUGUI letter in goalTextLetters)
            {
                letter.gameObject.SetActive(false);
            }
            
            yield return new WaitForSeconds(0.25f * Time.timeScale);

            foreach (TextMeshProUGUI letter in goalTextLetters)
            {
                letter.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(0.25f * Time.timeScale);
        }

        foreach (TextMeshProUGUI letter in goalTextLetters)
        {
            letter.gameObject.SetActive(false);
        }

        goalScoredText.gameObject.SetActive(false);

        Time.timeScale = 1;

        if (didPlayerScore)
        {
            playerRef.transform.position = new Vector3(-45, playerRef.transform.position.y, 0);
            opposingPlayerRef.transform.position = new Vector3(5, opposingPlayerRef.transform.position.y, 0);
        } else
        {
            playerRef.transform.position = new Vector3(-5, playerRef.transform.position.y, 0);
            opposingPlayerRef.transform.position = new Vector3(45, opposingPlayerRef.transform.position.y, 0);
        }

        currentBall.transform.position = new Vector3(0, 0.8f, 0);
        currentBall.GetComponent<Rigidbody>().velocity = Vector3.zero;

        opposingPlayerRef.GetComponent<OpposingPlayer>().canReceiveBall = true;
        playerRef.GetComponent<PlayerController>().canReceiveBall = true;
        opposingGoalieRef.GetComponent<Goalie>().canReceiveBall = true;

        canScoreGoal = true;
    }

    public IEnumerator GoaliePossess()
    {
        playerRef.GetComponent<PlayerController>().hasBall = false;
        playerRef.GetComponent<PlayerController>().canReceiveBall = false;
        playerRef.GetComponent<PlayerController>().isMoveable = false;

        opposingPlayerRef.GetComponent<OpposingPlayer>().canReceiveBall = false;
        opposingPlayerRef.GetComponent<OpposingPlayer>().hasBall = false;
        opposingPlayerRef.GetComponent<OpposingPlayer>().canMove = false;

        opposingGoalieRef.GetComponent<Goalie>().canReceiveBall = false;

        currentBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
        currentBall.GetComponent<Rigidbody>().isKinematic = true;
        currentBall.transform.position = new Vector3(56, 2.5f, opposingGoalieRef.transform.position.z);

        savedText.gameObject.SetActive(true);

        // Goal Kick
        yield return new WaitForSeconds(3);

        savedText.gameObject.SetActive(false);

        currentBall.GetComponent<Rigidbody>().isKinematic = false;

        playerRef.GetComponent<PlayerController>().canReceiveBall = true;
        playerRef.GetComponent<PlayerController>().isMoveable = true;

        opposingPlayerRef.GetComponent<OpposingPlayer>().canReceiveBall = true;
        opposingPlayerRef.GetComponent<OpposingPlayer>().canMove = true;

        playerRef.transform.position = new Vector3(-5.8f, 0.5f, 3.4f);
        currentBall.transform.position = new Vector3(56, 2.5f, opposingGoalieRef.transform.position.z);

        opposingGoalieRef.GetComponent<Goalie>().canReceiveBall = true;

        float opposingPlayerZPos = Random.Range(-HORIZONTAL_BOUND, HORIZONTAL_BOUND);
        opposingPlayerRef.transform.position = new Vector3(51, 0.8f, opposingGoalieRef.transform.position.z);
    }

    private void UpdateScoreText()
    {
        scoreText.text = playerScore + "-" + opposingScore;
    }

    IEnumerator GameOver()
    {
        isActive = false;

        DisplayWinner();

        SetPlayersMoveStatus(false);

        yield return new WaitForSeconds(5);

        ShowMenu();
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartGame(int difficulty)
    {
        isActive = true;

        playerScore = 0;
        opposingScore = 0;
        UpdateScoreText();

        secondsElapsed = 0;

        Cursor.lockState = CursorLockMode.Locked;

        didCallGameOver = false;

        Destroy(currentMenu);

        inGameText.gameObject.SetActive(true);
        HideTemporaryInGameText();
        menuUI.gameObject.SetActive(false);

        SetPlayersMoveStatus(true);
        canScoreGoal = true;

        // Reset positions
        currentBall.transform.position = new Vector3(0, 0.8f, 0);
        currentBall.GetComponent<Rigidbody>().velocity = Vector3.zero;

        playerRef.transform.position = new Vector3(-20, 0.5f, 0);
        opposingPlayerRef.transform.position = new Vector3(20, 0.5f, 0);

        // Difficulty
        if(difficulty == 1)
        {
            opposingPlayerRef.GetComponent<OpposingPlayer>().runSpeed = 5;
            opposingGoalieRef.GetComponent<Goalie>().speed = 3;
        } else if(difficulty == 2)
        {
            opposingPlayerRef.GetComponent<OpposingPlayer>().runSpeed = 6;
            opposingGoalieRef.GetComponent<Goalie>().speed = 5;
        } else if(difficulty == 3)
        {
            opposingPlayerRef.GetComponent<OpposingPlayer>().runSpeed = 7;
            opposingGoalieRef.GetComponent<Goalie>().speed = 6;
        }
    }

    private void SetPlayersMoveStatus(bool status)
    {
        playerRef.GetComponent<PlayerController>().isMoveable = status;
        opposingPlayerRef.GetComponent<OpposingPlayer>().canMove = status;
    }

    private void DisplayWinner()
    {
        if(playerScore > opposingScore)
        {
            winnerText.text = "You Won!";
            winnerText.color = Color.blue;
        } else if(playerScore < opposingScore)
        {
            winnerText.text = "You Lost";
            winnerText.color = Color.red;
        } else
        {
            winnerText.text = "It Was A Draw!";
            winnerText.color = Color.white;
        }

        gameOverText.gameObject.SetActive(true);
        winnerText.gameObject.SetActive(true);
    }

    private void ShowMenu()
    {
        currentMenu = Instantiate(menuPrefab);
        menuUI.gameObject.SetActive(true);
        inGameText.gameObject.SetActive(false);
    }

    private void HideTemporaryInGameText()
    {
        savedText.gameObject.SetActive(false);
        goalScoredText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        winnerText.gameObject.SetActive(false);
    }
}
