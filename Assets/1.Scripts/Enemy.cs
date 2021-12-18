using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;

public class Enemy : MonoBehaviour
{
    public EZObjectPool poolBullets;
    List<Transform> cannons = new List<Transform>();
    public float bulletSpeed = 0.6f;
    public float bulletCooldown = 0.8f;
    float timeLastBullet;
    bool isActive = false;
    public float rotationSpeed = 0f;

    void Awake()
    {
        foreach (Transform cannon in transform.Find("Cannons")) cannons.Add(cannon);
    }

    public void AwakeEnemy()
    {
        transform.gameObject.SetActive(true);
        timeLastBullet = Time.time + bulletCooldown; // To avoid bullets at the very start
        isActive = true;
    }

    public void LevelUp()
    {
        if (bulletSpeed < 4f) bulletSpeed += 0.1f;
        if (bulletCooldown > 0.2f) bulletCooldown -= 0.04f;
        var newRotation = Mathf.Abs(rotationSpeed) + 5;
        // if (newRotation % 2 == 0) newRotation *= -1;
        // rotationSpeed = newRotation;
    }

    void Update()
    {
        if (!isActive) return;
        if (Time.time > timeLastBullet + bulletCooldown) ShotRandom();
        transform.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed);
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
