using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameManager gameManager;
    public SpriteRenderer rendererPlayer;
    public float orbitRadius;
    public float rotationSpeed = 0.5f;
    public int dir = 1;
    public float rotationBaseTime;
    public float maxY = 3f;
    public float minY = 1f;
    bool actionPressed = false;
    public float speedMovementY = 2f;

    void Start()
    {
        orbitRadius = maxY;
        rendererPlayer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coin")) GetCoin(other.gameObject);
        else if (other.CompareTag("Bullet")) BulletHit(other.gameObject);
    }

    void GetCoin(GameObject coin)
    {
        coin.SetActive(false);
        gameManager.AddCoin();
    }

    void BulletHit(GameObject bullet)
    {
        gameManager.EndGame();
    }

    void Update()
    {
        if (!gameManager.isGameStarted) return;
        if (Input.GetMouseButtonDown(0))
        {
            dir *= -1;
            actionPressed = true;
            //rendererPlayer.flipY = dir < 0 ? true : false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            actionPressed = false;
        }
        orbitRadius = Mathf.Lerp(orbitRadius, actionPressed ? minY : maxY, Time.deltaTime * speedMovementY);
        MoveAroundOrbit();
    }

    void MoveAroundOrbit()
    {
        rotationBaseTime += Time.deltaTime * dir;
        transform.position = new Vector2(Mathf.Sin(rotationBaseTime * rotationSpeed * Mathf.PI) * orbitRadius,
                                        Mathf.Cos(rotationBaseTime * rotationSpeed * Mathf.PI) * orbitRadius);
        if (dir == 1)
            transform.rotation = Quaternion.Euler(0, 180, rotationBaseTime * (rotationSpeed * 180));
        else
            transform.rotation = Quaternion.Euler(0, 0, rotationBaseTime * (rotationSpeed * -180));
    }
}