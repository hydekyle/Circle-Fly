using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using EZObjectPools;
using Cysharp.Threading.Tasks;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerT;
    public EZObjectPool poolCoins;
    public Enemy enemy;
    public TMP_Text scoreText, maxScore, timeBonusText;
    public Animator timeBonusAnimator;
    public AudioSource audioSource;
    public AudioClip coinGrab, jiggleLevelCompleted, wowRisky, playerDie;
    [Header("Configuration")]
    public int totalCoins = 30;
    [HideInInspector]
    public bool isGameStarted = false;
    [HideInInspector]
    public int levelNumber = 1;
    Player player;
    int activeCoins = 0;
    int score = 0;
    float levelStartedTime;

    void Start()
    {
        player = playerT.GetComponent<Player>();
        if (PlayerPrefs.HasKey("Score"))
        {
            var score = PlayerPrefs.GetInt("Score");
            maxScore.text = "MAX SCORE: \n " + score.ToString();
        }
    }

    public void GameStart()
    {
        maxScore.gameObject.SetActive(false);
        SpawnCoins();
        enemy.AwakeEnemy();
        isGameStarted = true;
    }

    void LevelCompleted()
    {
        levelNumber++;
        IncreaseDifficulty();
        ScoreTimeBonus();
        SpawnCoins();
        levelStartedTime = Time.time;
        audioSource.PlayOneShot(jiggleLevelCompleted);
    }

    void IncreaseDifficulty()
    {
        enemy.LevelUp();
    }

    void ScoreTimeBonus()
    {
        var timePassed = (Time.time - levelStartedTime);
        var timeScore = Mathf.Clamp(1000 - (int)timePassed * 30, 0, 1000);
        if (timeScore > 0)
        {
            timeBonusText.text = "Time Bonus!\n+" + timeScore;
            timeBonusAnimator.transform.position = playerT.position;
            timeBonusAnimator.Play(0);
            AddScore(timeScore);
        }
    }

    public void AddScore(int addScore)
    {
        score += addScore;
        scoreText.text = score.ToString();
    }

    public void AddCoin()
    {
        audioSource.PlayOneShot(coinGrab);
        AddScore(10);
        activeCoins--;
        if (activeCoins == 0) LevelCompleted();
    }

    void SpawnCoins()
    {
        var ang = 360 / totalCoins;
        var radius = player.maxY;
        for (var i = 1; i < totalCoins + 1; i++)
        {
            var posX = radius * Mathf.Sin(ang * i * Mathf.Deg2Rad);
            var posY = radius * Mathf.Cos(ang * i * Mathf.Deg2Rad);
            var coinPos = new Vector2(posX, posY);
            poolCoins.TryGetNextObject(coinPos, transform.rotation);
            activeCoins++;
        }
        radius = player.minY;
        for (var i = 1; i < (totalCoins / 2) + 1; i++)
        {
            var posX = radius * Mathf.Sin(ang * i * Mathf.Deg2Rad * 2);
            var posY = radius * Mathf.Cos(ang * i * Mathf.Deg2Rad * 2);
            var coinPos = new Vector2(posX, posY);
            poolCoins.TryGetNextObject(coinPos, transform.rotation);
            activeCoins++;
        }
    }

    public async void EndGame()
    {
        if (PlayerPrefs.HasKey("Score"))
        {
            var maxScore = PlayerPrefs.GetInt("Score");
            if (score > maxScore) PlayerPrefs.SetInt("Score", score);
        }
        else
        {
            PlayerPrefs.SetInt("Score", score);
        }
        playerT.gameObject.SetActive(false);
        audioSource.PlayOneShot(playerDie);
        await UniTask.DelayFrame(30);
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
        SceneManager.LoadScene(0);
    }
}
