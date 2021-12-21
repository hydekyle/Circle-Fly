using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;

public class Enemy : MonoBehaviour
{
    public EZObjectPool poolBullets;
    List<Transform> cannons = new List<Transform>();
    public float bulletInitialSpeed = 0.6f, bulletInitialCooldown = 0.8f;
    public float bulletSpeedDifficultByLevel = 0.1f, bulletCooldownDifficultByLevel = 0.04f;
    public float bulletMaxSpeed = 4f, bulletMaxCooldown = 0.2f;
    float bulletSpeed, bulletCooldown, timeLastBullet;
    bool isActive = false;

    void Awake()
    {
        foreach (Transform cannon in transform.Find("Cannons")) cannons.Add(cannon);
        bulletSpeed = bulletInitialSpeed;
        bulletCooldown = bulletInitialCooldown;
    }

    public void AwakeEnemy()
    {
        GameManager.Instance.levelNumber.OnChanged += LevelChanged;
        transform.gameObject.SetActive(true);
        timeLastBullet = Time.time + bulletCooldown;
        isActive = true;
    }

    public void SleepEnemy()
    {
        isActive = false;
    }

    public void LevelChanged()
    {
        SetDifficulty(GameManager.Instance.levelNumber.Value);
    }

    public void SetDifficulty(int level)
    {
        bulletSpeed = Mathf.Clamp(bulletInitialSpeed + bulletSpeedDifficultByLevel * level, bulletInitialSpeed, bulletMaxSpeed);
        bulletCooldown = Mathf.Clamp(bulletInitialCooldown - bulletCooldownDifficultByLevel * level, bulletMaxCooldown, bulletInitialCooldown);
    }

    void FixedUpdate()
    {
        if (!isActive) return;
        if (Time.time > timeLastBullet + bulletCooldown) ShotRandom();
    }

    void ShotRandom()
    {
        if (poolBullets.TryGetNextObject(cannons[Random.Range(0, cannons.Count)].position, transform.rotation, out GameObject bullet))
        {
            var direction = bullet.transform.position - transform.position;
            bullet.GetComponent<Rigidbody2D>().velocity = direction.normalized * bulletSpeed;
            timeLastBullet = Time.time;
        }
    }
}
