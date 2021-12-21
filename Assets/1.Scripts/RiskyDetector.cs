using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class RiskyDetector : MonoBehaviour
{
    public Animator hotScore;
    public TMP_Text hotScoreText;
    public GameObject playerGO;
    byte bullets = 0;
    float wowCooldown = 1f;
    float lastTimeWow = -1f;

    async void AddRiskyScore()
    {
        await UniTask.DelayFrame(20);
        if (playerGO.activeSelf && Time.time > lastTimeWow + wowCooldown)
        {
            var addScore = 100 * GameManager.Instance.levelNumber.Value;
            hotScoreText.text = "Risky!\n+" + addScore;
            hotScore.transform.position = GameManager.Instance.playerT.transform.position / 2;
            hotScore.Play(0);
            GameManager.Instance.AddScore(addScore);
            GameManager.Instance.audioSource.PlayOneShot(GameManager.Instance.wowRisky);
            lastTimeWow = Time.time;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet")) bullets++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (bullets == 2) AddRiskyScore();
            bullets--;
        }
    }
}
