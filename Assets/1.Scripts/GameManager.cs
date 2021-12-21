using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using EZObjectPools;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityObservables;
using UnityEngine.Events;

[System.Serializable]
public class ObservableInt : Observable<int> { }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("References")]
    public Transform playerT;
    public EZObjectPool poolCoins;
    public Enemy enemy;
    public TMP_Text scoreText, maxScore, timeBonusText;
    public GameObject buttonReward;
    public Animator timeBonusAnimator;
    public AudioSource audioSource;
    public AudioClip coinGrab, jiggleLevelCompleted, wowRisky, playerDie, playSound;
    [Header("Configuration")]
    public int totalCoins = 30;
    public ObservableInt levelNumber = new ObservableInt() { Value = 1 };
    public UnityEvent onStartGame;
    [HideInInspector]
    public bool isGameStarted = false;
    Player player;
    int activeCoins = 0;
    int score = 0;
    public float levelStartedTime;

    void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
    }

    void Start()
    {
        levelNumber.OnChanged += OnLevelChanged;
        player = playerT.GetComponent<Player>();
        if (PlayerPrefs.HasKey("Score"))
        {
            var score = PlayerPrefs.GetInt("Score");
            maxScore.text = "MAX SCORE: \n " + score.ToString();
        }
        AdManager.ShowBannerAD();
        if (AdManager.gameCount > 0 && AdManager.isShowingAds) buttonReward.SetActive(true);
    }

    void OnLevelChanged()
    {
        if (activeCoins == 0)
        {
            ScoreTimeBonus();
            SpawnCoins();
        }
    }

    async void GameStart()
    {
        audioSource.PlayOneShot(playSound);
        AdManager.gameCount++;
        SpawnCoins();
        enemy.AwakeEnemy();
        await UniTask.DelayFrame(10);
        levelStartedTime = Time.time;
        isGameStarted = true;
    }

    async void ScoreTimeBonus()
    {
        var timePassed = (Time.time - levelStartedTime);
        levelStartedTime = Time.time;
        await UniTask.DelayFrame(50);
        if (playerT.gameObject.activeSelf)
        {
            audioSource.PlayOneShot(jiggleLevelCompleted);
            var timeScore = Mathf.Clamp(1450 - (int)timePassed * 50, 0, 1000);
            if (timeScore > 0)
            {
                timeBonusText.text = "Time Bonus!\n+" + timeScore;
                timeBonusAnimator.transform.position = playerT.position / 2;
                timeBonusAnimator.Play(0);
                AddScore(timeScore);
            }
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
        if (activeCoins == 0) levelNumber.Value++;
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
        SaveScore();
        playerT.gameObject.SetActive(false);
        audioSource.PlayOneShot(playerDie);
        enemy.SleepEnemy();
        await UniTask.DelayFrame(10);
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
        await SceneManager.LoadSceneAsync(1);
        AdManager.ShowInterstitialAD();
    }

    void SaveScore()
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
    }

    public void ButtonShowReward()
    {
        AdManager.ShowRewardAD();
        buttonReward.SetActive(false);
    }

    public void ButtonPlay()
    {
        if (Time.timeSinceLevelLoad < 0.1f) return; //Avoid missclicks
        onStartGame.Invoke();
        GameStart();
    }

}
