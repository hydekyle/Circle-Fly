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
    public TMP_Text scoreText;
    public TMP_Text maxScore;
    [Header("Configuration")]
    public int totalCoins = 30;
    [HideInInspector]
    public bool isGameStarted = false;
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
        IncreaseDifficulty();
        ScoreLevelTime();
        SpawnCoins();
        levelStartedTime = Time.time;
    }

    void IncreaseDifficulty()
    {
        if (enemy.bulletSpeed < 2f) enemy.bulletSpeed += 0.1f;
        if (enemy.bulletCooldown > 0.3f) enemy.bulletCooldown -= 0.05f;
    }

    void ScoreLevelTime()
    {
        var timePassed = (Time.time - levelStartedTime);
        var timeScore = Mathf.Clamp(1000 - (int)timePassed * 10, 0, 1000);
        score += timeScore;
    }

    public void AddCoin()
    {
        activeCoins--;
        score += 10;
        scoreText.text = score.ToString();
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

    public void EndGame()
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
        SceneManager.LoadScene(0);
    }
}
